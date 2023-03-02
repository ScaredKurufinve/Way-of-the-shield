using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Items;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.TextTools;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using static Way_of_the_shield.Main;
using System.Reflection.Emit;
using Kingmaker.Designers;

namespace Way_of_the_shield.NewComponents
{
    [HarmonyPatch]
    public class ArrowCatching
    {
        public static BlueprintBuff buff;
        public static BlueprintBuff AoEBuff;
        public static BlueprintArmorEnchantment Enchantment;
        public static RulebookEvent.CustomDataKey ArrowCatchingKey = new("ArrowCatching");

        public static void CreateArrowCatchingEnchantment()
        {
            #region create Arrow Catching effect buff
            BlueprintBuff ArrowCatchingEffectBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("472986c4378d417e8066608452f6560b")),
                name = modName + "_ArrowCatchingEffectBuff",
                FxOnRemove = new(),
                FxOnStart = new(),
                m_DisplayName = new LocalizedString() { Key = "ArrowCatchingEffectBuff_DisplayName" },
                m_Description = new LocalizedString() { Key = "ArrowCatchingEffectBuff_Description" },
                m_DescriptionShort = new LocalizedString() { Key = "ArrowCatchingEffectBuff_ShortDescription" },
                Stacking = StackingType.Stack,
                m_Flags = BlueprintBuff.Flags.StayOnDeath,
            };
            ArrowCatchingEffectBuff.AddToCache();
            buff = ArrowCatchingEffectBuff;
            #endregion
            #region Create ArrowCatchingAreaEffect
            BlueprintAbilityAreaEffect ArrowCatchingAreaEffect = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("9c14a794f68947b0b0f5d5158e421b10")),
                name = modName + "_ArrowCatchingAreaEffect",
                Shape = AreaEffectShape.Cylinder,
                Size = new(5),
                Fx = new(),
                m_TargetType = BlueprintAbilityAreaEffect.TargetType.Ally,
                AffectDead = true,
            };
            ArrowCatchingAreaEffect.AddComponent(
                new AbilityAreaEffectBuff()
                {
                    m_Buff = ArrowCatchingEffectBuff.ToReference<BlueprintBuffReference>(),
                    Condition = new()
                    {
                        Conditions = new Condition[1]
                        {
                            new ContextConditionIsCaster() {Not = true}
                        }
                    }
                });
            ArrowCatchingAreaEffect.AddToCache();
            #endregion
            #region Create ArrowCatchingAoEBuff
            BlueprintBuff ArrowCatchingAoEBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("194bf0bcb7474983b395eea1844a200c")),
                name = modName + "_ArrowCatchingAoEBuff",
                FxOnRemove = new(),
                FxOnStart = new(),
                m_DisplayName = new LocalizedString() { Key = "ArrowCatchingAoEBuff_DisplayName" },
                m_Description = new LocalizedString() { Key = "ArrowCatchingAoEBuff_Description" },
                //m_DescriptionShort = new LocalizedString() { Key = "ArrowCatchingAoEBuff_ShortDescription" },
                Stacking = StackingType.Replace,
                m_Flags = BlueprintBuff.Flags.StayOnDeath
                | BlueprintBuff.Flags.HiddenInUi,
            };
            ArrowCatchingAoEBuff.AddComponent(
                new AddAreaEffect()
                {
                    m_AreaEffect = ArrowCatchingAreaEffect.ToReference<BlueprintAbilityAreaEffectReference>(),
                });
            ArrowCatchingAoEBuff.AddToCache();
            AoEBuff = ArrowCatchingAoEBuff;
            #endregion
            #region Create ArrowCatchingFact

            BlueprintUnitFact ArrowCatchingFact = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("63726ab5d359497291fa2280603ca9d8")),
                name = modName + "_ArrowCatchingFact"
            };
            ArrowCatchingFact.AddComponent(
                new AddFactContextActions()
                {
                    Deactivated = new(),
                    NewRound = new(),
                    Activated = new()
                    {
                        Actions = new GameAction[1]
                        {
                            new ContextActionApplyBuff()
                            {
                                DurationValue = new(),
                                Permanent = true,
                                m_Buff = ArrowCatchingAoEBuff.ToReference<BlueprintBuffReference>(),
                                IsNotDispelable = true,

                            }
                        }
                    }
                });
            ArrowCatchingFact.AddToCache();
            #endregion
            #region Create ArrowCatchingEnchantment
            BlueprintArmorEnchantment ArrowCatchingEnchantment = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("e778c16a7f404d59bca25d42e380c9d0")),
                name = modName + "_ArrowCatchingEnchantment",
                m_EnchantmentCost = 1,
                m_EnchantName = new() { Key = "ArrowCatchingEnchantment_EnchantName" },
                m_Description = new LocalizedString() { Key = "ArrowCatchingEnchantment_Description" }
            };
            ArrowCatchingEnchantment.AddComponent(
                new AddFactToEquipmentWielder()
                {
                    //m_Fact = ArrowCatchingFact.ToReference<BlueprintUnitFactReference>()
                    m_Fact = AoEBuff.ToReference<BlueprintUnitFactReference>()
                });
            ArrowCatchingEnchantment.AddToCache();
            Enchantment = ArrowCatchingEnchantment;
            #endregion
        }

        [HarmonyPatch(typeof(RuleAttackWithWeapon), MethodType.Constructor, new Type[] { typeof(UnitEntityData), typeof(UnitEntityData), typeof(ItemEntityWeapon), typeof(int) })]
        [HarmonyPrefix]
        public static void InsertArrowCatchCheckIntoRuleAttackWithWeapon(ref RuleAttackWithWeapon __instance, UnitEntityData attacker, ref UnitEntityData target, ItemEntityWeapon weapon)
        {
#if DEBUG
            if (Debug.GetValue())
                Comment.Log("Entered InsertArrowCatchCheckIntoRuleAttackWithWeapon"); 
#endif
            if (weapon.Blueprint.VisualParameters.Projectiles.Length == 0) return;
            IEnumerable<Buff> Catching = target.Buffs.Enumerable.Where(b => b.Blueprint == buff);
            if (Catching.Count() == 0
                || target.Buffs.HasFact(AoEBuff)
                || __instance.TryGetCustomData(ArrowCatchingKey, out UnitEntityData originalTarget) && originalTarget is not null) return;
            UnitEntityData catcher;
            foreach (Buff b in Catching)
            {
                if (b.MaybeContext is null)
                {
                    Comment.Log("InsertArrowCatchCheckIntoRuleAttackWithWeapon: No context on the Buff");
                    continue;
                };
                ItemEntity sourceItem = b.MaybeContext?.SourceItem;
                catcher = b.MaybeContext.MaybeCaster ?? sourceItem?.Owner;
                if (catcher is null)
                {
                    Comment.Log("InsertArrowCatchCheckIntoRuleAttackWithWeapon: Can't find the catcher");
                }
                if (!(sourceItem is ItemEntityArmor armor && GameHelper.GetItemEnhancementBonus(armor) >= GameHelper.GetItemEnhancementBonus(weapon))) return;
                if (catcher.State.LifeState == Kingmaker.UnitLogic.UnitLifeState.Conscious
                    && attacker.HasLOS(catcher))
                {
                    __instance.SetCustomData(ArrowCatchingKey, target);
                    target = catcher;
#if DEBUG
                    if (Debug.GetValue())
                        Comment.Log("target is " + target.CharacterName); 
#endif
                    return;
                }
            };

        }

        [HarmonyPatch(typeof(AttackLogMessage), nameof(AttackLogMessage.GetData))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AddArrowCatchingLineIntoAttackLog(IEnumerable<CodeInstruction> instructions)
        {
#if DEBUG
            if (Debug.GetValue())
                Comment.Log("Entered AddArrowCatchingLineIntoAttackLog transpiler"); 
#endif

            List<CodeInstruction> _instructions = instructions.ToList();

            CodeInstruction[] toSearch = new CodeInstruction[]
            {
                new (OpCodes.Ldarg_0),
                new (OpCodes.Ldfld, typeof(AttackLogMessage).GetField(nameof(AttackLogMessage.Message)))
            };

            int index = IndexFinder(_instructions, toSearch);
            if (index == -1)
            {
                Comment.Error("Failed to find this.Message when transpiling AttackLogMessage.GetData) to add Arrow Catching string");
                return instructions;
            };

            
            _instructions.RemoveRange(index - 1, 2);

            CodeInstruction[] toInsert = new CodeInstruction[]
            {
                new (OpCodes.Ldarg_1),
               new CodeInstruction(OpCodes.Call, typeof(ArrowCatching).GetMethod(nameof(CatchOrNoCatch)))
            };

            _instructions.InsertRange(index - 1, toInsert);

            return _instructions;
        }

        public static string CatchOrNoCatch(AttackLogMessage msg, RuleAttackRoll rule)
        {
#if DEBUG
            if (Debug.GetValue())
            {
                Comment.Log("ArrowCatching - rule.TryGetCustomData(ArrowCatchingKey, out UnitEntityData original1) is " + (rule.TryGetCustomData(ArrowCatchingKey, out UnitEntityData original1)));
                Comment.Log("ArrowCatching - original1 is null? " + (original1 is null));
            }  
#endif
            if (rule.RuleAttackWithWeapon is null || !rule.RuleAttackWithWeapon.TryGetCustomData(ArrowCatchingKey, out UnitEntityData original) || original is null) return msg.Message;
            else return (Catch1 + LogHelper.GetUnitName(original) + Catch2);
            //else return string.Format("{0} attacks {1} with {2}, but the projectile veers towards the enchanted shield of {3}",
                   //rule.Initiator.CharacterName, original.CharacterName, rule.Weapon?.Name, rule.Target.CharacterName);
        }

        public static LocalizedString Catch1
        {
            get
            {
                m_Catch1 ??= new() { m_Key = "ArrowCatching_SourceAttacks", m_ShouldProcess = true, Shared = null };
                return m_Catch1;
            }
             
        }

        public static LocalizedString Catch2
        {
            get
            {
                m_Catch2 ??= new() { m_Key = "ArrowCatching_ShieldOfTarget", m_ShouldProcess = true, Shared = null };
                return m_Catch2;
            }

        }

        private static LocalizedString m_Catch1;
        private static LocalizedString m_Catch2;
    }

}
