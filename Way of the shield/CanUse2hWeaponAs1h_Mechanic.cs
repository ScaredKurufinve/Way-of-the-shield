using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Kingmaker.View.Animation;
using Kingmaker.View.Equipment;
using Kingmaker.Visual.Animation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;

namespace Way_of_the_shield
{

    [HarmonyPatch(typeof(ItemEntityWeapon))]
    [HarmonyPatch(nameof(ItemEntityWeapon.ShouldHoldInTwoHands))]
    class ItemEntityWeapon__HoldInTwoHands__Patch
    {
        [HarmonyPriority(800)]
        [HarmonyPatch(typeof(BlueprintWeaponType), nameof(BlueprintWeaponType.IsOneHandedWhichCanBeUsedWithTwoHands), MethodType.Getter)]
        [HarmonyPostfix]
        public static void MakeRapierOneHanded_Postfix(ref bool __result, BlueprintWeaponType __instance)
        {
            if (FixRapiers.GetValue() is false) return;
            if (__instance.Category == WeaponCategory.Rapier) __result = false;
        }


        [HarmonyPostfix]
        public static void Postfix(ItemEntityWeapon __instance, ref bool __result)
        {
            bool spell_combat = false;
            UnitPartMagus unit_part_magus = __instance.Wielder?.Get<UnitPartMagus>();
            if ((bool)(unit_part_magus) && unit_part_magus.SpellCombat.Active)
            {
                spell_combat = true;
            }

            var unit_part = __instance.Wielder?.Get<UnitPartCanHold2hWeaponIn1h>();
            if (!unit_part) return;

            if (__instance.Blueprint.IsTwoHanded || (__instance.Blueprint.IsOneHandedWhichCanBeUsedWithTwoHands && __result == false))
            {

                if (!spell_combat)   //check if we can hold the 2h weapon in 1h
                {
                    __result = unit_part.CanBeUsedAs2h(__instance);
                }
                else
                {
                    if (unit_part.CanBeUsedOn(__instance))
                    {//weapon is being held as one-handed
                        __result = false;
                        return;
                    }
                    //normally we can not 2h with spell combat, so we check only magus specific feature that would allow us
                    var use_spell_combat_part = __instance.Wielder?.Get<UnitPartCanUseSpellCombat>();
                    if (use_spell_combat_part == null)
                    {
                        return;
                    }

                    var pair_slot = (__instance.HoldingSlot as HandSlot)?.PairSlot;
                    __result = use_spell_combat_part.CanBeUsedOn(__instance.HoldingSlot as HandSlot, pair_slot, true);
                }
            }
        }
    }

    [HarmonyPatch(typeof(HandSlot))]
    [HarmonyPatch(nameof(HandSlot.OnItemInserted))]
    class HandSlot__OnItemInserted__Patch
    {

        static bool Prefix(HandSlot __instance)
        {
            var unit_part = __instance.Owner?.Get<UnitPartCanHold2hWeaponIn1h>();

            if (unit_part == null)
            {
                return true;
            }

            if (!__instance.IsPrimaryHand)
            {
                HandSlot primaryHand = __instance.HandsEquipmentSet.PrimaryHand;
                if (primaryHand.MaybeItem is ItemEntityWeapon maybeItem && maybeItem != null && ((maybeItem.Blueprint.IsTwoHanded && !unit_part.CanBeUsedOn(maybeItem)) || maybeItem.Blueprint.Double))
                {
                    primaryHand.RemoveItem();
                }
            }
            if (__instance.MaybeItem is ItemEntityWeapon maybeItem1 && ((maybeItem1.Blueprint.IsTwoHanded && !unit_part.CanBeUsedOn(maybeItem1)) || maybeItem1.Blueprint.Double))
            {
                if (__instance.IsPrimaryHand)
                {
                    __instance.PairSlot.RemoveItem();
                }
                else
                {
                    __instance.RemoveItem();
                    __instance.PairSlot.InsertItem(maybeItem1);
                }
            };
            __instance.IsDirty = true;

            return false;
        }
    }

    public abstract class CanUse2hWeaponAs1hBase : UnitBuffComponentDelegate
    {
        abstract public bool CanBeUsedOn(ItemEntityWeapon weapon);

        abstract public bool CanBeUsedAs2h(ItemEntityWeapon weapon);

        public override void OnTurnOn()
        {
            Owner.Ensure<UnitPartCanHold2hWeaponIn1h>().buffs.Add(Fact);
        }

        public override void OnTurnOff()
        {
            Owner.Ensure<UnitPartCanHold2hWeaponIn1h>().buffs.Remove(Fact);
        }
    }


    public class UnitPartCanHold2hWeaponIn1h : OldStyleUnitPart
    {

        public List<EntityFact> buffs = new();

        public bool CanBeUsedOn(ItemEntityWeapon weapon)
        {
            if (!buffs.Any())
            {
                return false;
            }

            foreach (var b in buffs)
            {
                bool result = false;
                b.CallComponents<CanUse2hWeaponAs1hBase>(c => result = c.CanBeUsedOn(weapon));
                if (result)
                {
                    return true;
                }
            }

            return false;
        }


        public bool CanBeUsedAs2h(ItemEntityWeapon weapon)
        {
            bool can_use_at_all = false;
            foreach (var b in buffs)
            {
                bool can_use = false;
                bool can_2h = false;
                b.CallComponents<CanUse2hWeaponAs1hBase>(c => { can_2h = c.CanBeUsedAs2h(weapon); can_use = c.CanBeUsedOn(weapon); });
                if (can_use && can_2h)
                {
                    return true;
                }
                can_use_at_all = can_use_at_all || can_use;
            }

            if (!can_use_at_all)
            {
                HandSlot pair_slot = (weapon?.HoldingSlot as HandSlot)?.PairSlot;
                if (pair_slot != null)
                    return !pair_slot.HasItem;
                return false;
            }
            else
            {
                return false;
            }
        }

    }

    public class UnitPartCanUseSpellCombat : OldStyleUnitPart
    {
        public List<EntityFact> buffs = new();

        public bool CanBeUsedOn(HandSlot primary_hand, HandSlot secondary_hand, bool use_two_handed)
        {

            if (!buffs.Any())
            {
                return false;
            }

            foreach (var b in buffs)
            {
                bool result = false;
                b.CallComponents<CanUseSpellCombatBase>(c => result = c.CanBeUsedOn(primary_hand, secondary_hand, use_two_handed));
                if (result)
                {
                    return true;
                }
            }

            return false;
        }
    }


    public abstract class CanUseSpellCombatBase : UnitBuffComponentDelegate
    {
        abstract public bool CanBeUsedOn(HandSlot primary_hand_slot, HandSlot secondary_hand_slot, bool use_two_handed);

        public override void OnActivate()
        {
            Owner.Ensure<UnitPartCanUseSpellCombat>().buffs.Add(Fact);
        }


        public override void OnDeactivate()
        {
            Owner.Ensure<UnitPartCanUseSpellCombat>().buffs.Remove(Fact);
        }
    }


    [HarmonyPatch]
    public static class TwoHandedAsOneHandedAnimationPatches
    {
        [HarmonyPatch(typeof(ItemEntityWeapon), nameof(ItemEntityWeapon.GetAnimationStyle))]
        [HarmonyPrefix]
        public static bool ItemEntityWeapon__GetAnimationStyle__Patch(ItemEntityWeapon __instance, ref WeaponAnimationStyle __result, bool forDollRoom)
        {
            if (__instance.HoldInTwoHands || !__instance.Blueprint.IsTwoHanded) return true;
            
            if (!__instance.Wielder?.Unit?.View?.CharacterAvatar) return true;

            WeaponAnimationStyle animStyle = __instance.Blueprint.VisualParameters.AnimStyle;
            if (forDollRoom && animStyle == WeaponAnimationStyle.ShieldLight && __instance.Shield?.Blueprint.Type.ProficiencyGroup == ArmorProficiencyGroup.Buckler)
            {
                __result = WeaponAnimationStyle.None;
                return false;
            }
            //if (wielder.Unit.AreHandsBusyWithAnimation) return true;
            
            if (!forDollRoom)
                __result = animStyle switch
                {
                    WeaponAnimationStyle.SlashingTwoHanded  => WeaponAnimationStyle.SlashingOneHanded,
                    WeaponAnimationStyle.PiercingTwoHanded  => WeaponAnimationStyle.PiercingOneHanded,
                    WeaponAnimationStyle.AxeTwoHanded       => WeaponAnimationStyle.SlashingOneHanded,
                    _ => __result
                };

            else 
                __result = animStyle switch
                {
                    WeaponAnimationStyle.AxeTwoHanded => WeaponAnimationStyle.PiercingTwoHanded,
                    _ => __result
                };

            return __result is WeaponAnimationStyle.None;
        }

        [HarmonyPatch(typeof(IKController), nameof(IKController.CheckStylesForIk))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> IKController__IsShield__Transpiler(IEnumerable<CodeInstruction> instructions)
        {
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log("Begin reanspiling IKController.CheckStylesForIk"); 
#endif

            var _inst = instructions.ToList();

            var toSearch = new CodeInstruction[] 
            {
                new(OpCodes.Call, typeof(IKController).GetMethod(nameof(IKController.IsShield), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
            };

            int index = IndexFinder(_inst, toSearch, true);

            if (index == -1) return instructions;

            var toInsert = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(TwoHandedAsOneHandedAnimationPatches).GetMethod(nameof(GetBucklerStyle)))
            };

            _inst.InsertRange(index, toInsert);
            return _inst;
        }

        public static WeaponAnimationStyle GetBucklerStyle(WeaponAnimationStyle original, UnitEntityView unitEntityView)
        {
            WeaponAnimationStyle result;
            if (original == WeaponAnimationStyle.None) { result = original; goto ret; }
            ItemEntityShield maybeShield = unitEntityView.EntityData.Body.CurrentHandsEquipmentSet.SecondaryHand?.MaybeShield;
            if (maybeShield?.Blueprint is BlueprintItemShield bpShield && bpShield.Type.ProficiencyGroup == ArmorProficiencyGroup.Buckler)
                result = bpShield.VisualParameters.AnimStyle;
            else result = original;

            ret:
            return result;

        }


        //[HarmonyPatch(typeof(UnitViewHandsEquipment), nameof(UnitViewHandsEquipment.ActiveOffHandWeaponStyle), MethodType.Getter)]
        //[HarmonyPrefix]
        //public static bool UnitViewHandsEquipment__ActiveOffHandWeaponStyle__Patch(UnitViewHandsEquipment __instance, ref WeaponAnimationStyle __result)
        //{
        //    if (__instance.IsDollRoom
        //        && __instance.m_ActiveSet.OffHand?.Slot.MaybeShield is ItemEntityShield shield
        //        && shield.Blueprint.Type.ProficiencyGroup == ArmorProficiencyGroup.Buckler)
        //    {
        //        __result = WeaponAnimationStyle.None;
        //        return false;
        //    }
        //    return true;
        //}

        //[HarmonyPatch(typeof(UnitViewHandSlotData), nameof(UnitViewHandSlotData.GetPossibleVisualSlots))]
        //[HarmonyTranspiler]
        //public static IEnumerable<CodeInstruction> MoveBucklerAttachPointToHip(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
        //{
        //    var _inst = instructions.ToList();

        //    var toSearch = new CodeInstruction[]
        //    {
        //        new(OpCodes.Ldloc_0),
        //        new(OpCodes.Callvirt, typeof(WeaponVisualParameters).GetProperty(nameof(WeaponVisualParameters.AttachSlots)).GetMethod)
        //    };

        //    int index = IndexFinder(_inst, toSearch);
        //    if (index == -1) return instructions;

        //    Label jump = gen.DefineLabel();
        //    _inst[index].WithLabels(jump);

        //    var toInsert = new CodeInstruction[]
        //    {
        //        new(OpCodes.Ldarg_0),
        //        new(OpCodes.Call, typeof(TwoHandedAsOneHandedAnimationPatches).GetMethod(nameof(GetBucklerSlots))),
        //        new(OpCodes.Dup),
        //        new(OpCodes.Brtrue_S, jump),
        //        new(OpCodes.Pop)
        //    };

        //    _inst.InsertRange(index - toSearch.Length, toInsert);
        //    return _inst;

        //}

        [HarmonyPatch(typeof(UnitViewHandSlotData), nameof(UnitViewHandSlotData.GetPossibleVisualSlots))]
        [HarmonyPrefix]
        public static bool MoveBucklerAttachPointToHip(UnitViewHandSlotData __instance, List<UnitEquipmentVisualSlotType> possibleSlots)
        {
            var slots = GetBucklerSlots(__instance);
            if (slots == null) return true;
            possibleSlots.AddRange(slots);
            return false;
        }

        public static UnitEquipmentVisualSlotType[] GetBucklerSlots(UnitViewHandSlotData slotData)
        {
            if (slotData == null) return null;
            if ( slotData?.VisibleItemVisualParameters?.AnimStyle == WeaponAnimationStyle.ShieldBuckler ||
                (slotData?.VisibleItem?.Blueprint as BlueprintItemShield)?.Type.ProficiencyGroup == ArmorProficiencyGroup.Buckler)
                return BucklerSlots;
            else return null;
        }

        public static UnitEquipmentVisualSlotType[] BucklerSlots = new[]
        {
            UnitEquipmentVisualSlotType.Shield,
            UnitEquipmentVisualSlotType.LeftFront01,
            UnitEquipmentVisualSlotType.Quiver
        };
    }
}
