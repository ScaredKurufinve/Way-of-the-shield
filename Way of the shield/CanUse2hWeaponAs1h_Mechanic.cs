using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Animation;
using System.Collections.Generic;
using System.Linq;




namespace Way_of_the_shield
{

    [HarmonyPatch(typeof(ItemEntityWeapon))]
    [HarmonyPatch(nameof(ItemEntityWeapon.HoldInTwoHands), MethodType.Getter)]
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



    [HarmonyPatch(typeof(ItemEntityWeapon), nameof(ItemEntityWeapon.GetAnimationStyle))]
    public class ItemEntityWeapon__GetAnimationStyle__Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ItemEntityWeapon __instance, ref WeaponAnimationStyle __result, bool forDollRoom)
        {
            if (!__instance.HoldInTwoHands && __instance.Blueprint.IsTwoHanded)
            {
                UnitDescriptor wielder = __instance.Wielder;
                // UnityEngine.Object x;
                // if (wielder == null)
                // {
                //     x = null;
                // }
                // else
                // {
                //     UnitEntityData unit = wielder.Unit;
                //     x = ((unit != null) ? unit.View : null);
                // }
                if (__instance.Wielder?.Unit?.View?.CharacterAvatar && !wielder.Unit.AreHandsBusyWithAnimation)
                {
                    WeaponAnimationStyle animStyle = __instance.Blueprint.VisualParameters.AnimStyle;

                    if (!forDollRoom)
                    {


                        if (animStyle == WeaponAnimationStyle.SlashingTwoHanded) __result = WeaponAnimationStyle.SlashingOneHanded;
                        else if (animStyle == WeaponAnimationStyle.PiercingTwoHanded) __result = WeaponAnimationStyle.PiercingOneHanded;
                        else if (animStyle == WeaponAnimationStyle.AxeTwoHanded) __result = WeaponAnimationStyle.SlashingOneHanded;
                        return false;
                    }
                    else if (animStyle == WeaponAnimationStyle.AxeTwoHanded)
                    {
                        __result = WeaponAnimationStyle.SlashingOneHanded;
                        return false;
                    }
                }

            }
            return true;
        }

    }

}
