using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;

namespace Way_of_the_shield.ProficiencyRework
{


    public static class ProficiencyPatches

    {
        [HarmonyPatch]
        public static class FixTheArmorBug_ThisPatchDoesNotWantToLoad
        {
            [HarmonyPatch(typeof(UnitEntityData), nameof(UnitEntityData.OnTurnOn))]
            [HarmonyPostfix]
            public static void Postfix(UnitEntityData __instance)
            {
                ArmorSlot armorSlot = __instance.Body?.Armor;
                if (armorSlot is not null && armorSlot.HasArmor) armorSlot.Armor.RecalculateStats();
                ItemEntityShield shield = __instance.Body?.SecondaryHand?.MaybeShield;
                if (shield is not null && shield.ArmorComponent is not null) shield.ArmorComponent?.RecalculateStats();
            }
        }


        [HarmonyPatch]
        public static class ItemEntity_EquipNonProfficient_Patch
        {
            [HarmonyPrepare]
            public static bool Prepare() 
            { 
                if (AllowEquipNonProfficientItems.GetValue()) return true;
                else { Comment.Log("AllowEquipNonProfficientItems setting is disabled, patch ItemEntity_EquipNonProfficient_Patch won't be applied."); return false; };
            }

            [HarmonyTargetMethods]
            public static IEnumerable<MethodBase> GetMethods()
            {
                yield return AccessTools.Method(typeof(ItemEntityShield), nameof(ItemEntityShield.CanBeEquippedInternal));
                yield return AccessTools.Method(typeof(ItemEntityArmor), nameof(ItemEntityArmor.CanBeEquippedInternal));
                yield return AccessTools.Method(typeof(ItemEntityWeapon), nameof(ItemEntityWeapon.CanBeEquippedInternal));
            }

            //Trasnpiler changes (base.CanBeEquippedInternal && Proficientains.Contains) into just base.CanBeEquippedInternal

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
            {
                List<CodeInstruction> _instructions = instructions.ToList();
                MethodInfo methodInfo = AccessTools.Method(typeof(ItemEntity), nameof(ItemEntity.CanBeEquippedInternal));
                int index = _instructions.FindIndex(code => code.Calls(methodInfo)) + 1;
                int count = _instructions.Count() - 1 - index;
                _instructions.RemoveRange(index, count);
                return _instructions;
            }

            
        }


        [HarmonyPatch(typeof(UIUtilityItem), nameof(UIUtilityItem.GetEquipPossibility), new Type[] { typeof(ItemEntity) })]
        public class UIUtilityItemPatch
        {
            [HarmonyPrepare]
            public static bool Init() { return AllowEquipNonProfficientItems.GetValue(); }


            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
#if DEBUG
                Comment.Log("transpile UIUtilityItem.GetEquipPosibility"); 
#endif

                List<CodeInstruction> _instructions = instructions.ToList();
                MethodInfo _CanInsertItem = AccessTools.Method(typeof(ItemSlot), nameof(ItemSlot.CanInsertItem));
                int index = _instructions.FindIndex(x => x.Calls(_CanInsertItem)) + 1;

                _instructions.InsertRange(index, new CodeInstruction[]
                                                            {
                                                                new CodeInstruction(OpCodes.Ldloc_0),
                                                                new CodeInstruction(OpCodes.Ldarg_0),
                                                                CodeInstruction.Call(typeof(ProficiencyPatches), nameof(ProficiencyPatches.IsProficient))
                                                            }

                );

                return _instructions;
            }
        }

        [HarmonyPatch(typeof(ItemEntityArmor))]
        public static class ItemEntityArmorPatch
        {
            [HarmonyPrepare]
            public static bool Init() { return AllowEquipNonProfficientItems.GetValue(); }

            [HarmonyPatch(nameof(ItemEntityArmor.RecalculateStats))]
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
#if DEBUG
                Comment.Log("transpile ItemEntityArmor.RecalculateStats"); 
#endif
                List<CodeInstruction> _instructions = instructions.ToList();

                MethodInfo GetResult = AccessTools.Method(typeof(RuleCalculateArmorCheckPenalty), typeof(RuleCalculateArmorCheckPenalty).GetProperty(nameof(RuleCalculateArmorCheckPenalty.Result)).GetMethod.Name);

                int index = -1;
                int[] indices = _instructions.Where(x => x.Calls(GetResult)).Select(x => _instructions.IndexOf(x)).ToArray();
                if (indices is null || indices.Length == 0)
                {
                    Comment.Error(string.Format("Failed to transpile ItemEntityArmor.RecalculateStats. No entries for {0}", nameof(GetResult), Array.Empty<object>()));
                    return instructions;
                }

                // search for "result = RuleCalculateArmorCheckPenalty.Result; if (result < 0);"
                foreach (int i in indices)
                {

                    if (
                        _instructions[i + 1].IsStloc() &&
                        _instructions[i + 2].IsLdloc() &&
                        _instructions[i + 3].LoadsConstant(0L) &&
                        _instructions[i + 4].opcode == OpCodes.Bge_S
                        )
                    { index = i + 5; break; }

                }
                if (index == -1)
                {
                    Comment.Error("Failed to transpile ItemEntityArmor.RecalculateStats. Failed to find the code that compares the result");
                    return instructions;
                }

                Comment.Log("index = " + index.ToString());
                Label skip = generator.DefineLabel();
                _instructions[index].labels.Add(skip);

                CodeInstruction[] instArray =
                {
                     new CodeInstruction(OpCodes.Ldarg_0),
                     new CodeInstruction(OpCodes.Ldloc_2),
                     CodeInstruction.Call(typeof(ProficiencyPatches), nameof(ProficiencyPatches.AddAttackBonusPenaltyFromArmor))
                };

                _instructions.InsertRange(index, instArray);

                return _instructions;


            }
        }

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        public static class BlueprintsCache_Init_Patch_For_Shields_Proficiency
        {
            const string _name = "WayOfTheShield_AddProficiencies_ShieldsWeaponProficiency";
            static void Postfix()
            {
#if DEBUG
                if (Debug.GetValue())
                    Comment.Log("Adding shields' weapon proficiency to the ShieldsProficiency feat");
#endif
                string circ = "when adding weapon proficiency to the Shields Proficiency blueprint";
                if (!RetrieveBlueprint("cb8686e7357a68c42bdd9d4e65334633", out BlueprintFeature ShieldsProficiency, "ShieldProficiency", circ) )
                    return;
                AddProficiencies AP = ShieldsProficiency.ComponentsArray.OfType<AddProficiencies>().FirstOrDefault(c => c.name.Contains(_name));
                if (AP is not null) 
                    return;
                AP = new()
                {
                    name = _name,
                    ArmorProficiencies = new ArmorProficiencyGroup[] { },
                    WeaponProficiencies = new WeaponCategory[] { 
                        WeaponCategory.WeaponLightShield, 
                        WeaponCategory.WeaponHeavyShield, 
                        WeaponCategory.SpikedHeavyShield,
                        WeaponCategory.SpikedLightShield
                    },
                };
                ShieldsProficiency.AddComponent(AP);
            }
        }


        public static void AddAttackBonusPenaltyFromArmor(ItemEntityArmor armor, int penalty)
        {
            if (IsProficient_Short(armor)) { return; };
            StatType stat = StatType.AdditionalAttackBonus;
            armor.AddModifier(armor.Wielder.Stats.GetStat(stat), penalty, ModifierDescriptor.Armor);
        }


        public static bool IsProficient_Short(ItemEntity item)
        {
            if (item is null)
            {
#if DEBUG
                if (Debug.GetValue())
                    Comment.Log("item is null inside Prof_Short"); 
#endif
                return true;
            };
            UnitDescriptor unit = item.Wielder ?? item.Owner;
            if (unit is null)
            {
#if DEBUG
                if (Debug.GetValue())
                    Comment.Log($"unit is null inside Prof_Short for item {item}"); 
#endif
                return true;
            };
//#if DEBUG
//            if (Debug.GetValue())
//                Comment.Log($"Will be checking proficiency of {item.Name} for {unit.CharacterName}"); 
//#endif
            if (!unit.Unit.IsDirectlyControllable) return true;
            bool result = IsProficient(true, unit, item);
//#if DEBUG
//            if (Debug.GetValue())
//                Comment.Log($"{unit.CharacterName} proficiency with {item.Name} is {result}"); 
//#endif
            return result;
        }



        public static bool IsProficient(bool flag, UnitEntityData owner, ItemEntity item)
        {

            if (item is ItemEntityArmor) return flag && owner.Descriptor.Proficiencies.Contains((item as ItemEntity<BlueprintItemArmor>).Blueprint.ProficiencyGroup);
            if (item is ItemEntityShield) return flag && owner.Descriptor.Proficiencies.Contains((item as ItemEntity<BlueprintItemShield>).Blueprint.ArmorComponent.ProficiencyGroup);
            if (item is ItemEntityWeapon) return flag && owner.Descriptor.Proficiencies.Contains((item as ItemEntity<BlueprintItemWeapon>).Blueprint.Category);

            return flag && true;
        }
    }

    [HarmonyPatch(typeof(RuleCalculateAttackBonusWithoutTarget), nameof(RuleCalculateAttackBonusWithoutTarget.OnTrigger))]
    public static class WeaponNoProficiency
    {
        public static BonusType penalty = Utilities.BonusTypeExtenstions.GetBonusType(160);

        //static WeaponNoProficiency() => penalty = Utilities.BonusTypeExtenstions.GetBonusType(160);


        [HarmonyPrepare]
        public static bool Prepare() 
        {
            if (AllowEquipNonProfficientItems.GetValue()) return true;
            else { Comment.Log("AllowEquipNonProfficientItems setting is disabled, patch WeaponNoProficiency won't be applied."); return false; };
        }


        [HarmonyPrefix]
        public static void Prefix(RuleCalculateAttackBonusWithoutTarget __instance)
        {
            ItemEntityWeapon weapon = __instance.Weapon;
            UnitEntityData unit = __instance.Initiator;
            if (!unit.IsDirectlyControllable || weapon.Blueprint.Category.HasSubCategory(WeaponSubCategory.Natural)) 
                return;
            if (weapon.Blueprint.Category == WeaponCategory.WeaponLightShield
                && weapon.IsShield
                && weapon.Shield.ArmorComponent.Blueprint.ProficiencyGroup == ArmorProficiencyGroup.Buckler
               )
                return;
            if (unit.Get<NewComponents.OffHandParry.OffHandParryUnitPart>()?.weapon == weapon) 
                return;
            if ( !unit.Proficiencies.Contains(weapon.Blueprint.Category)) 
                __instance.AddModifier(-4, penalty, ModifierDescriptor.Penalty);
        }


    }



}
