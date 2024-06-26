﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Kingmaker;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Items.Slots;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UI.MVVM._VM.ServiceWindows.Inventory;
using Kingmaker.UI.MVVM._VM.Slots;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using static Way_of_the_shield.Backstab.RuleCalculateAC_OnTrigger_patch;
using System.Text;

namespace Way_of_the_shield.NewComponents
{
    public static class OffHandParry
    {
        public static BlueprintItemWeapon LightShieldWeapon = ResourcesLibrary.TryGetBlueprint<BlueprintItemWeapon>("62c90581f9892e9468f0d8229c7321c4");


        public class OffHandParryUnitPart : OldStyleUnitPart, ITargetRulebookHandler<RuleAttackRoll>, IInventoryHandler
        {
            public WeaponCategory? Category;
            public bool activated;
            public bool riposte;

            public static bool flag = false;
            public ItemEntityWeapon weapon;
            public bool HasParries;
            public int n = 0;
            public bool Triggered;

            IEnumerator<int> parries;

            public void TryActivate(WeaponCategory category, bool counterattack)
            {
#if DEBUG
                if (Debug.GetValue())
                    Comment.Log($"OffHandParryUnitPart - Trying to activate the part with {category} category.");
#endif
                flag = true;
                ItemEntityWeapon w = Owner?.Body?.SecondaryHand.MaybeWeapon;
                if (w is null) Comment.Warning("OffHandParryUnitPart - Weapon is Null when trying to activate the part on " + Owner.CharacterName);
                if (w?.Blueprint.Type.Category == category)
                {
                    Category = category;
                    weapon = w;
                    parries = UnitAttack.EnumerateAttacks(Rulebook.Trigger(new RuleCalculateAttacksCount(Owner)).Result.SecondaryHand);
                    riposte = counterattack;
                    IEnumerator<int> parries2 = UnitAttack.EnumerateAttacks(Rulebook.Trigger(new RuleCalculateAttacksCount(Owner)).Result.SecondaryHand);
                    n = 0;
                    while (parries2.MoveNext())
                    {
                        n++;
                    }
#if DEBUG
                    if (Debug.GetValue())
                        Comment.Log($"OffHandParryUnitPart - Amount of parries is {n}");
#endif
                    Owner.Unit.Ensure<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>().ShieldDenied.Retain();
                    Owner.Unit.Ensure<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>().ForceDualWieldingPenalties.Retain();
                    activated = true;
                    UnitAttack OldAttack = Owner.Unit.Commands.Attack;
                    OldAttack.m_AllAttacks.RemoveAll(attack => attack.Hand == Owner.Body.SecondaryHand);
                }
#if DEBUG
                if (Debug.GetValue())
                    Comment.Log($"OffHandParryUnitPart - Activated is {activated}"); 
#endif
                flag = false;
                HasParries = parries.MoveNext();
            }
            public void TryDeactivate(WeaponCategory category)
            {
                flag = true;
                if (Owner?.Body?.SecondaryHand.MaybeWeapon?.Blueprint.Type.Category == category)
                {
                    Category = null;
                    parries = null;
                    riposte = false;
                    Owner.Unit.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.ShieldDenied.Release();
                    Owner.Unit.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.ForceDualWieldingPenalties.Release();
                    activated = false;
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log("OffHandParry part is deactivated through TryDeactivate call.");   
#endif
                }
                flag = false;
            }

            public void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
#if DEBUG
                if (Debug.GetValue())
                    Comment.Log($"OffHandParry of {Owner.CharacterName} noticed the incoming attack.  Attacker is {evt.Initiator.CharacterName}, weapon is {evt.Weapon.Name}." +
                        $"Existing parry? {evt.Parry != null}. " +
                        $"Weapon is melee? {evt.Weapon.Blueprint.IsMelee}. " +
                        $"Part activated? {activated}. " +
                        $"Has parries? {HasParries}.");
#endif
                RuleCheckTargetFlatFooted_OnTrigger_patch.rule = evt;
                bool NotSuitable = evt.Parry != null
                        || !evt.Weapon.Blueprint.IsMelee || !activated
                        || !Owner.State.CanAct
                        || Rulebook.Trigger<RuleCheckTargetFlatFooted>(new(evt.Initiator, evt.Target)).IsFlatFooted // code out later
                        || !HasParries;
                RuleCheckTargetFlatFooted_OnTrigger_patch.rule = null;
                if (NotSuitable)
                    return;

                evt.TryParry(Owner, weapon, parries.Current);
                Triggered = true;
#if DEBUG
                if (Debug.GetValue())
                    Comment.Log($"OffHandParry part of {Owner.CharacterName} wishes to parry. The Triggered HasShieldBash is set to {Triggered}"); 
#endif
                ModifiableValue additionalAttackBonus = Owner.Stats.AdditionalAttackBonus;
                int num = evt.Initiator.Descriptor.State.Size - Owner.State.Size;
                if (num > 0)
                {
                    int value = -4 * num;
                    evt.AddTemporaryModifier(additionalAttackBonus.AddModifier(value, ModifierDescriptor.Penalty));
                }

            }
            public void OnEventDidTrigger(RuleAttackRoll evt)
            {
                RuleAttackRoll.ParryData parry = evt.Parry;
                if (parry != null
                    && parry.Initiator == Owner
                    && activated
                    && parry.IsTriggered
                    && Triggered)
                {
                    HasParries = parries.MoveNext();
                    n--;
                    Triggered = false;
#if DEBUG
                    if (Debug.GetValue())
                        Comment.Log($"OffHandParry {parry.Initiator.CharacterName} parried the attack of {evt.Initiator.CharacterName}. Attempted to move the parry enumerator. The Triggered HasShieldBash is now set to {Triggered}."); 
#endif
                    if (evt.Result == AttackResult.Parried && riposte == true)
                        Game.Instance.CombatEngagementController.ForceAttackOfOpportunity(Owner, evt.Initiator, false);
                }
            }
            public void Refresh()
            {
#if DEBUG
                if (Debug.GetValue())
                    Comment.Log($"Inventory refresh is noticed by OffHandParry part of {Owner.CharacterName}."); 
#endif
                if (Owner?.Body?.SecondaryHand.MaybeWeapon == weapon) return;

                Category = null;
                parries = null;
                activated = false;
                riposte = false;
#if DEBUG
                if (Debug.GetValue())
                    Comment.Log($"OffHandParry part of {Owner.CharacterName} is deactivated due to inventory refreshment."); 
#endif
            }

            public void TryEquip(ItemSlotVM slot)
            {

            }
            public void TryDrop(ItemSlotVM slot)
            {

            }

        }


        [AllowedOn(typeof(BlueprintUnitFact), false)]
        [TypeId("ba113232f466438cadd7263f03a536be")]
        public class OffHandParryComponent : UnitFactComponentDelegate


        {
            public WeaponCategory category;
            public bool riposte = false;


            public override void OnActivate()
            {
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log($"OffHandParryComponent of {Owner.CharacterName} is being turned on."); 
#endif
                OffHandParryUnitPart part = Owner.Parts.Ensure<OffHandParryUnitPart>();
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log("OffHandParry part activate state is" + part.activated); 
#endif
                if (part.activated == true) return;
                part.TryActivate(category, riposte);
            }

            public override void OnDeactivate()
            {
#if DEBUG
                if (Debug.GetValue())
                    Comment.Log("OffHandParryComponent of " + Owner.CharacterName + " is being turned off."); 
#endif
                Owner.Parts.Get<OffHandParryUnitPart>()?.TryDeactivate(category);
            }

        }

        [HarmonyPatch(typeof(RuleCalculateAttacksCount), nameof(RuleCalculateAttacksCount.PrepareHandVariables))]
        public static class RuleCalculateAttacksCount_OnTrigger_patch
        {

            [HarmonyPostfix]
            public static void Postfix(RuleCalculateAttacksCount __instance)
            {
                OffHandParryUnitPart part = __instance.Initiator?.Parts.Get<OffHandParryUnitPart>();
                if (part is not null && part.activated && !OffHandParryUnitPart.flag)
                {
                    RuleCalculateAttacksCount.AttacksCount result = __instance.Result.SecondaryHand;
                    result.AdditionalAttacks = 0;
                    result.HasteAttacks = 0;
                    result.PenalizedAttacks = 0;
                }
            }


            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log("RuleCalculateAttacksCount PrepareHandVariables transpiler"); 
#endif
                List<CodeInstruction> _instructions = instructions.ToList();

                CodeInstruction[] toSearch =
                    new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Ldfld, typeof(UnitDescriptor).GetField(nameof(UnitDescriptor.State))),
                        new CodeInstruction(OpCodes.Ldfld, typeof(UnitState).GetField(nameof(UnitState.Features))),
                        new CodeInstruction(OpCodes.Ldfld, typeof(UnitMechanicFeatures).GetField(nameof(UnitMechanicFeatures.ShieldBash))),
                        new CodeInstruction(OpCodes.Call),
                        new CodeInstruction(OpCodes.Brtrue_S)
                    };

                int index = IndexFinder(instructions, toSearch);
                if (index == -1) { return instructions; };

                CodeInstruction[] toInsert =
                    new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Ldsfld, typeof(OffHandParryUnitPart).GetField(nameof(OffHandParryUnitPart.flag))),
                        new CodeInstruction(OpCodes.Brtrue_S, _instructions[index -1].operand)
                    };
                _instructions.InsertRange(index, toInsert);
                _instructions.InsertRange(index - 1,
                    new CodeInstruction[]
                    {
                        new CodeInstruction (OpCodes.Ldarg_0),
                        CodeInstruction.Call(typeof(OffHandParry), nameof(OffHandParry.BucklerOrBash))
                    });

                return _instructions;

            }

//            [HarmonyPatch(nameof(RuleCalculateAttacksCount.AddExtraAttacks))]
//            [HarmonyTranspiler]
//            public static IEnumerable<CodeInstruction> Transpiler2(IEnumerable<CodeInstruction> instructions)
//            {
//#if DEBUG
//                if (Settings.Debug.GetValue())
//                    Comment.Log("Entered RuleCalculateAttacksCount AddExtraAttacks transpiler"); 
//#endif
//                List<CodeInstruction> _instructions = instructions.ToList();

//                CodeInstruction[] toSearch =
//                    new CodeInstruction[]
//                    {
//                        new CodeInstruction(OpCodes.Ldfld, typeof(UnitDescriptor).GetField(nameof(UnitDescriptor.State))),
//                        new CodeInstruction(OpCodes.Ldfld, typeof(UnitState).GetField(nameof(UnitState.Features))),
//                        new CodeInstruction(OpCodes.Ldfld, typeof(UnitMechanicFeatures).GetField(nameof(UnitMechanicFeatures.ShieldBash))),
//                        new CodeInstruction(OpCodes.Call),
//                        new CodeInstruction(OpCodes.Brtrue_S)
//                    };

//                int index = IndexFinder(instructions, toSearch);
//                if (index == -1) { return instructions; };

//                _instructions.InsertRange(index - 1,
//                    new CodeInstruction[]
//                    {
//                        new CodeInstruction (OpCodes.Ldarg_0),
//                        CodeInstruction.Call(typeof(OffHandParry), nameof(OffHandParry.BucklerOrBash))
//                    });

//                return _instructions;

//            }

            [HarmonyPatch(typeof(HandSlot), nameof(HandSlot.MaybeWeapon), MethodType.Getter)]
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler4(IEnumerable<CodeInstruction> instructions)
            {
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log("Entered HandSlot MaybeWeapon getter transpiler"); 
#endif
                List<CodeInstruction> _instructions = instructions.ToList();

                CodeInstruction[] toSearch =
                    new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Ldfld, typeof(UnitDescriptor).GetField(nameof(UnitDescriptor.State))),
                        new CodeInstruction(OpCodes.Ldfld, typeof(UnitState).GetField(nameof(UnitState.Features))),
                        new CodeInstruction(OpCodes.Ldfld, typeof(UnitMechanicFeatures).GetField(nameof(UnitMechanicFeatures.ShieldBash))),
                        new CodeInstruction(OpCodes.Call),
                        new CodeInstruction(OpCodes.Brtrue_S)
                    };

                int index = IndexFinder(instructions, toSearch);
                if (index == -1) { return instructions; };

                _instructions.InsertRange(index -1,
                    new CodeInstruction[]
                    {
                        //new CodeInstruction (OpCodes.Ldsfld, typeof(OffHandParryUnitPart).GetField(nameof(OffHandParryUnitPart.flag))),
                        new CodeInstruction( OpCodes.Ldarg_0),
                        CodeInstruction.Call(typeof(OffHandParry), nameof(BucklerOrBash4)),
                        //new CodeInstruction(_instructions[index -1]),
                    });

                return _instructions;

            }


            [HarmonyPatch(typeof(CharSheetWeapons), nameof(CharSheetWeapons.Initialize))]
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler3(IEnumerable<CodeInstruction> instructions)
            {
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log("CharSheetWeapons Initialize transpiler"); 
#endif
                List<CodeInstruction> _instructions = instructions.ToList();

                CodeInstruction[] toSearch =
                    new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Ldfld, typeof(UnitDescriptor).GetField(nameof(UnitDescriptor.State))),
                        new CodeInstruction(OpCodes.Ldfld, typeof(UnitState).GetField(nameof(UnitState.Features))),
                        new CodeInstruction(OpCodes.Ldfld, typeof(UnitMechanicFeatures).GetField(nameof(UnitMechanicFeatures.ShieldBash))),
                        new CodeInstruction(OpCodes.Call),
                        new CodeInstruction(OpCodes.Brtrue_S)
                    };

                int index = IndexFinder(instructions, toSearch);
                if (index == -1) { return instructions; };

                _instructions.InsertRange(index - 1,
                    new CodeInstruction[]
                    {
                        new CodeInstruction (OpCodes.Ldarg_1),
                        CodeInstruction.Call(typeof(OffHandParry), nameof(OffHandParry.BucklerOrBash3))
                    });

                return _instructions;

            }


        }

        [HarmonyPatch(typeof(TwoWeaponFightingAttackPenalty), nameof(TwoWeaponFightingAttackPenalty.OnEventAboutToTrigger))]
        public static class TwoWeaponFightingAttackPenalty_patch_OffHandParry
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log("Entered TwoWeaponFightingAttackPenalty OnEventAboutToTrigger transpiler for OffHand parry"); 
#endif

                List<CodeInstruction> _instructions = instructions.ToList();

                CodeInstruction[] toSearch1 = new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Stloc_1)
                };
                CodeInstruction[] toSearch2 = new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Stloc_2)
                };

                int index1 = IndexFinder(_instructions, toSearch1);
                int index2 = IndexFinder(_instructions, toSearch2);
                int index = index1 > index2 ? index1 : index2;
                
                CodeInstruction[] toInsert = new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldloca, 1),
                    new CodeInstruction(OpCodes.Ldloca, 2),
                    CodeInstruction.Call(typeof(TwoWeaponFightingAttackPenalty_patch_OffHandParry), nameof(OffHandParryCheck))
                };
                _instructions.InsertRange(index, toInsert);
                return _instructions;
            }


            static ItemEntityWeapon light;
            static ItemEntityWeapon heavy;


            [HarmonyPatch(typeof(BlueprintsCache), nameof (BlueprintsCache.Init))]
            [HarmonyPostfix]
            public static void BlueprintsCache_Init_Patch_CreateFakeWeaponsForDualWielding()
            {
                light = new(ResourcesLibrary.TryGetBlueprint<BlueprintItemWeapon>("62c90581f9892e9468f0d8229c7321c4"));
                heavy = new(ResourcesLibrary.TryGetBlueprint<BlueprintItemWeapon>("ff8047f887565284e93773b4a698c393"));
            }

            public static void OffHandParryCheck(RuleCalculateAttackBonusWithoutTarget evt, ref ItemEntityWeapon weapon2, ref bool flag)
            {
                var featuresPart = evt.Initiator.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>();
                if (featuresPart == null || featuresPart.ForceDualWieldingPenalties == false) return;
                OffHandParryUnitPart part = evt.Initiator.Get<OffHandParryUnitPart>();
                if (part is not null && part.activated)
                {
                    weapon2 = part.weapon;
                }
                else if (weapon2 is null)
                {
                    ItemEntityShield shield = evt.Initiator.Body.SecondaryHand.MaybeShield;
                    weapon2 = shield is null ? null : shield.WeaponComponent ?? shield.ArmorComponent.Blueprint.ProficiencyGroup switch
                    {
                       ArmorProficiencyGroup.Buckler => light,
                       ArmorProficiencyGroup.Light => light,
                       ArmorProficiencyGroup.Heavy => heavy,
                       ArmorProficiencyGroup.TowerShield => heavy,
                       _ => null,
                    };                    
                }
                
                flag = weapon2 is null && flag;
            } 
        }


        public static bool BucklerOrBash(bool HasShieldBash, RuleCalculateAttacksCount evt)
        {
            bool result = OffHandParryUnitPart.flag
                || (HasShieldBash && evt.Initiator.Body.SecondaryHand.HasShield && (evt.Initiator.Body.SecondaryHand.Shield.Blueprint.Type.ProficiencyGroup != ArmorProficiencyGroup.Buckler
                                        || (evt.Initiator.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.BucklerBash
                                                && (!(evt.Initiator.Body.PrimaryHand.Weapon?.HoldInTwoHands ?? false) || (evt.Initiator.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.UnhinderingShield || AllowBucklerBashWhenTwoHandedWithUnhindering.GetValue()))
                                           )
                                      )
                    );

#if DEBUG
            var sb = new StringBuilder().Append($"BucklerOrBash - Parry flag  is {OffHandParryUnitPart.flag}, HasShieldBash is {HasShieldBash}. ");
            if (evt.Initiator.Body.SecondaryHand.MaybeShield is not ItemEntityShield shield)
            {
                sb.Append("No shield in secondary hand.");
            }
            else
            {
                sb.Append($"Uses shield {shield.Name} (proficiency group {shield.Blueprint.Type.ProficiencyGroup}). "); 
                if (shield.Blueprint.Type.ProficiencyGroup != ArmorProficiencyGroup.Buckler)
                    goto Print;
                sb.AppendLine();
                sb.Append($"Has BucklerBash? {evt.Initiator.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.BucklerBash}.");
                var twoHanded = evt.Initiator.Body.PrimaryHand.Weapon?.HoldInTwoHands ?? false;
                sb.Append($"Primary hand is hold with both hands? {twoHanded}");
                if (twoHanded)
                {
                    sb.AppendLine();
                    sb.Append($"Has Unhindering Shield? {evt.Initiator.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.UnhinderingShield}. ");
                    sb.Append($"Stupid setting is enabled? {AllowBucklerBashWhenTwoHandedWithUnhindering.GetValue()}");
                }
            }
            sb.AppendLine();

            Print:
            Comment.Log(sb.Append($"Result is {result}").ToString());
#endif
            return result;
        }
        public static bool BucklerOrBash4(bool flag, HandSlot slot)
        {
            if (flag is true) return flag;
            OffHandParryUnitPart part = slot.Owner.Get<OffHandParryUnitPart>();
            if (part is null || !OffHandParryUnitPart.flag) return false;
            //Comment.Log("Went through checks for BucklerOrBash4");
            return true;
        }
        public static bool BucklerOrBash3(bool flag, UnitEntityData unit)
        {
            if (flag is false) return flag;
            return flag && (unit.Body.SecondaryHand.Shield.Blueprint.Type.ProficiencyGroup != ArmorProficiencyGroup.Buckler 
                                || (unit.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.BucklerBash
                                    && (!(unit.Body.PrimaryHand.Weapon?.HoldInTwoHands ?? false) || unit.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.UnhinderingShield)
                                )
                            );
        }


        [HarmonyPatch(typeof(ItemEntityShield))]
        public static class ItemEntityShield_patch_WeaponComponentForBucklers
        {
            [HarmonyPatch(MethodType.Constructor, new Type[] { typeof(BlueprintItemShield) })]
            [HarmonyPostfix]
            public static void Postfix_Constructor(ItemEntityShield __instance, BlueprintItemShield bpItem)
            {
                //Comment.Log("Entered Shield Constructor postfix");
                if (__instance.WeaponComponent == null && bpItem.Type.ProficiencyGroup == ArmorProficiencyGroup.Buckler)
                
                __instance.WeaponComponent = new ItemEntityWeapon(LightShieldWeapon, __instance);
                
                //Comment.Log("Weapon component for the shield " + __instance.Name + " is " + __instance.WeaponComponent);
            }

            [HarmonyPatch(nameof(ItemEntityShield.OnTurnOn))]
            [HarmonyPrefix]
            public static void Prefix_OnTurnOn(ItemEntityShield __instance)
            {
                //Comment.Log("Entered Shield OnTurnOn Prefix");
                if (__instance.WeaponComponent == null && __instance.Blueprint.Type.ProficiencyGroup == ArmorProficiencyGroup.Buckler)

                    __instance.WeaponComponent = new ItemEntityWeapon(LightShieldWeapon, __instance);

                //Comment.Log("Weapon component for the shield " + __instance.Name + " is " + __instance.WeaponComponent.Name);
            }

            [HarmonyPatch(nameof(ItemEntityShield.OnPostLoad))]
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> DontDestroyBucklerWeapon(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
            {
                var __inst = instructions.ToList();



                var toSearch = new CodeInstruction[]
                {
                    new (OpCodes.Call, typeof(ItemEntity<>).MakeGenericType(typeof(BlueprintItemWeapon)).GetProperty(nameof(ItemEntity<BlueprintItemWeapon>.Blueprint), typeof(BlueprintItemWeapon)).GetMethod), //BlueprintItemWeapon>::get_Blueprint()
                    new (OpCodes.Ldarg_0),
                    new (OpCodes.Call, typeof(ItemEntity<>).MakeGenericType(typeof(BlueprintItemShield)).GetProperty(nameof(ItemEntity<BlueprintItemShield>.Blueprint), typeof(BlueprintItemShield)).GetMethod),
                    new (OpCodes.Callvirt, typeof(BlueprintItemShield).GetProperty(nameof(BlueprintItemShield.WeaponComponent)).GetMethod),
                    new (OpCodes.Beq)
                };
                int index = IndexFinder(__inst, toSearch);
                if (index == -1) return instructions;

                var toInsert = new CodeInstruction[]
                {
                    new (OpCodes.Ldarg_0),
                    CodeInstruction.Call(typeof(ItemEntityShield_patch_WeaponComponentForBucklers), nameof(BucklerCheckForWeaponComponent)),
                    new (OpCodes.Brtrue_S, __inst[index - 1].operand)
                };

                __inst.InsertRange(index, toInsert);
                return __inst;
            }

            public static bool BucklerCheckForWeaponComponent(ItemEntityShield shield)
            {
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log($"BucklerCheckForWeaponComponent - item is {shield?.Name}, wielder is {shield?.Wielder}. Buckler? {shield?.ArmorComponent.Blueprint.ProficiencyGroup == ArmorProficiencyGroup.Buckler}." +
                        $"Check result is {(shield?.ArmorComponent.Blueprint.ProficiencyGroup == ArmorProficiencyGroup.Buckler && shield.WeaponComponent?.Blueprint == LightShieldWeapon)}." +
                        $"Weapon part has wielder? {(shield.WeaponComponent?.Wielder is null ? "No." : "Yes," + shield.WeaponComponent.Wielder)}"); 
#endif
                return shield?.ArmorComponent.Blueprint.ProficiencyGroup == ArmorProficiencyGroup.Buckler && shield.WeaponComponent?.Blueprint == LightShieldWeapon;
            }
        }
    }
}
