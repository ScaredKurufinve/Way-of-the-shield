using HarmonyLib;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._PCView.ServiceWindows.CharacterInfo.Sections.Martial.Attack;
using Kingmaker.UI.MVVM._PCView.ServiceWindows.CharacterInfo.Sections.Martial.Attack.Attack;
using Kingmaker.UI.MVVM._VM.ServiceWindows.CharacterInfo.Sections.Martial.Attack;
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
using System.Reflection;
using System.Reflection.Emit;
using Way_of_the_shield.NewComponents;

namespace Way_of_the_shield
{
    [HarmonyPatch]
    class ItemEntityWeapon__HoldInTwoHands__Patch
    {
        //Check if the hand slot in question is not off-hand.
        //Otherwise it will return the overall hands equipment grip
        //meaning that off-hand weapon may end-up two-handed and break attack count calculation
        [HarmonyPatch(typeof(ItemEntityWeapon), nameof(ItemEntityWeapon.HoldInTwoHands), MethodType.Getter)]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> FixTheTwoHandedCheckForWeapon(IEnumerable<CodeInstruction> __instructions, ILGenerator generator)
        {
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log("Entered FixTheTwoHandedCheckForWeapon transpiler"); 
#endif
            var method = AccessTools.DeclaredPropertyGetter(typeof(HandSlot), nameof(HandSlot.HandsEquipmentSet));
            var label = generator.DefineLabel();
            foreach (var i in __instructions)
                if (i.Calls(method))
                {
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.DeclaredPropertyGetter(typeof(HandSlot), nameof(HandSlot.IsPrimaryHand)));
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return new CodeInstruction(OpCodes.Beq, label);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Ret);
                    yield return new CodeInstruction(OpCodes.Ldloc_0).WithLabels(label);
                    yield return i;
                }
                else
                    yield return i;
        }

        //we don't need it and re-implement
        [HarmonyPatch(typeof(HandSlot), nameof(HandSlot.IsItemTypeSupported))]
        [HarmonyPrefix]
        static bool RemoveIsItemTypeSupported()
        {
            return false;
        }
        //we run all required weapon slot availability checks inside the HandSlot.IsItemSupported
        [HarmonyPatch(typeof(ItemSlot), nameof(ItemSlot.CanInsertItem))]
        [HarmonyPrefix]
        static bool FixOneHandedChecksInCanInsertItem(ItemEntity item, ref bool skipSlotAvailabilityCheck)
        {
            skipSlotAvailabilityCheck = skipSlotAvailabilityCheck || item is ItemEntityWeapon;
            return true;
        }

        [HarmonyPatch(typeof(HandSlot), nameof(HandSlot.IsItemSupported))]
        [HarmonyPrefix]
        static bool FixOneHandedChecksInIsItemSupported(HandSlot __instance, ItemEntity item, ref bool __result)
        {
            (bool, bool) a = new(false, false);
            (bool, bool) b = new(false, false);
            __result = ItemIsSupported(__instance, item, ref a, ref b);
            return false;
        }

        static bool ItemIsSupported(HandSlot __instance, ItemEntity item, ref (bool RunCheck, bool Check) checkThis, ref (bool RunCheck, bool Check) checkOther)
        {
            //Comment.Log($"ItemIsSupported {item?.Name ?? "NULL item"} {__instance?.MaybeItem?.Name ?? "no other item in the slot"}");
            if (__instance.HasItem && !__instance.CanRemoveItem())
            {
                return false;
            }

            bool IsItemTypeSupported;

            if (item is ItemEntityWeapon weapon)
            {
                if (__instance.IsPrimaryHand)
                    IsItemTypeSupported = true;
                else
                {
                    IsItemTypeSupported = checkThis.Check = CanInsertSingleHandSlotForReal(weapon, __instance, item);
                    checkThis.RunCheck = true;
                }
            }
            else if (item is ItemEntityShield)
                IsItemTypeSupported = !__instance.IsPrimaryHand;
            else
                IsItemTypeSupported = item.Blueprint is BlueprintItemEquipmentHandSimple;

            if (!IsItemTypeSupported)
            {
                return false;
            }

            var PairSlot = __instance.PairSlot;

            if (!__instance.PairSlot.HasItem)
            {
                return true;                
            }

            if (PairSlot.CanRemoveItem())
            {
                return true;
            }
            if (PairSlot.HasWeapon)
            {
                checkOther.Check = CanInsertSingleHandSlotForReal(PairSlot.MaybeWeapon, __instance, item);
                checkOther.RunCheck = true;
                var res1 = checkOther.Check
                    && (checkThis.RunCheck ? checkThis.Check : CanInsertSingleHandSlotForReal(item as ItemEntityWeapon, __instance, PairSlot.MaybeItem));
                return res1;
            }
            else
            {
                var res2 = (checkThis.RunCheck ? checkThis.Check : CanInsertSingleHandSlotForReal(item as ItemEntityWeapon, __instance, PairSlot.MaybeItem));
                return res2;
            }
        }


        [HarmonyPatch(typeof(HandsEquipmentSet), nameof(HandsEquipmentSet.GetSuitableSlot))]
        [HarmonyPrefix]
        static bool FixOneHandedChecksInGetSuitableSlot(HandsEquipmentSet __instance, ItemEntity item, ref HandSlot __result)
        {
            Comment.Log($"Entered ItemEntityWeapon__HoldInTwoHands__Patch FixGetSuitableSlot {item?.Name}");
            bool PrimaryHandIsFine;
            (bool RunCheck, bool Check) SecondaryHandIsFine = new(false, false);

            (bool RunCheck, bool Check) checkPrimaryCurrent = new(false, false);
            (bool RunCheck, bool Check) checkSecondaryCurrent = new(false, false);
            (bool RunCheck, bool Check) checkPrimaryPotential = new(false, false);
            (bool RunCheck, bool Check) checkSecondaryPotential = new(false, false);

            if (__instance.PrimaryHand.HasItem)
            {
                Comment.Log($"Entered ItemEntityWeapon__HoldInTwoHands__Patch FixGetSuitableSlot 1");
                SecondaryHandIsFine.Check = ItemIsSupported(__instance.SecondaryHand, item, ref checkSecondaryPotential, ref checkPrimaryCurrent);
                SecondaryHandIsFine.RunCheck = true;
                if (SecondaryHandIsFine.Check && !__instance.SecondaryHand.HasItem)
                {
                    Comment.Log($"Entered ItemEntityWeapon__HoldInTwoHands__Patch FixGetSuitableSlot 2");
                    if (!checkSecondaryPotential.RunCheck)
                    {
                        checkSecondaryPotential.Check = CanInsertSingleHandSlotForReal(item as ItemEntityWeapon, __instance.SecondaryHand, item);
                        checkSecondaryPotential.RunCheck = true;
                    }
                    if (!checkPrimaryCurrent.RunCheck)
                    {
                        checkPrimaryCurrent.Check = CanInsertSingleHandSlotForReal(item as ItemEntityWeapon, __instance.SecondaryHand, item);
                        checkPrimaryCurrent.RunCheck = true;
                    }
                    Comment.Log($"Entered ItemEntityWeapon__HoldInTwoHands__Patch FixGetSuitableSlot {checkSecondaryPotential.Check == true} && {checkPrimaryCurrent.Check == true}");
                    if (checkSecondaryPotential.Check == true && checkPrimaryCurrent.Check == true)
                    {
                        __result = __instance.SecondaryHand;
                        return false;
                    }
                }

            }

            Comment.Log($"Entered ItemEntityWeapon__HoldInTwoHands__Patch FixGetSuitableSlot 3");
            PrimaryHandIsFine = ItemIsSupported(__instance.SecondaryHand, item, ref checkPrimaryPotential, ref checkSecondaryCurrent);
            if (PrimaryHandIsFine && !__instance.PrimaryHand.HasItem)
            {
                Comment.Log($"Entered ItemEntityWeapon__HoldInTwoHands__Patch FixGetSuitableSlot 4");
                if (!checkPrimaryPotential.RunCheck)
                {
                    checkPrimaryPotential.Check = CanInsertSingleHandSlotForReal(item as ItemEntityWeapon, __instance.PrimaryHand, item);
                    checkPrimaryPotential.RunCheck = true;
                }
                if (!checkSecondaryCurrent.RunCheck)
                {
                    checkSecondaryCurrent.Check = CanInsertSingleHandSlotForReal(item as ItemEntityWeapon, __instance.PrimaryHand, item);
                    checkSecondaryCurrent.RunCheck = true;
                }
                Comment.Log($"Entered ItemEntityWeapon__HoldInTwoHands__Patch FixGetSuitableSlot {checkPrimaryPotential.Check == true} && {checkSecondaryCurrent.Check == true}");
                if (checkPrimaryPotential.Check == true && checkSecondaryCurrent.Check == true)
                {
                    __result = __instance.PrimaryHand;
                    return false;
                }
            }

            if (PrimaryHandIsFine)
                __result = __instance.PrimaryHand;
            else if ((SecondaryHandIsFine.RunCheck && SecondaryHandIsFine.Check) || ItemIsSupported(__instance.SecondaryHand, item, ref checkSecondaryPotential, ref checkPrimaryCurrent))
                __result = __instance.SecondaryHand;
            else
                __result = null;
            Comment.Log($"Entered ItemEntityWeapon__HoldInTwoHands__Patch FixGetSuitableSlot result Primary? {__result == __instance.PrimaryHand}. Secondary? {__result = __instance.SecondaryHand}");
            return false;
        }

        [HarmonyPatch(typeof(HandSlot), nameof(HandSlot.RemoveItem))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> FixOneHandedRemoveItem(IEnumerable<CodeInstruction> instructions)
        {
#if DEBUG
            Comment.Log("Entered FixOneHandedRemoveItem transpiler"); 
#endif
            foreach (var i in instructions)
                if (i.Calls(AccessTools.Method(typeof(ItemEntityWeapon), nameof(ItemEntityWeapon.CanTakeOneHand))))
                {
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return CodeInstruction.Call((ItemEntityWeapon weapon, HandSlot slotToInsert, ItemEntity itemBeingInserted) => CanInsertSingleHandSlotForReal);
                }
                else
                    yield return i;
        }

        [HarmonyPatch(typeof(HandSlot), nameof(HandSlot.OnItemInserted))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> FixOneHandedChecksInOnItemInserted (IEnumerable<CodeInstruction> instructions)
        {
#if DEBUG
            Comment.Log("Entered FixOneHandedChecksInOnItemInserted transpiler"); 
#endif
            foreach (var i in instructions)
                if (i.Calls(AccessTools.Method(typeof(ItemEntityWeapon), nameof(ItemEntityWeapon.CanTakeOneHand))))
                {
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldnull);
                    yield return CodeInstruction.Call((ItemEntityWeapon weapon, HandSlot slotToInsert, ItemEntity itemBeingInserted) => CanInsertSingleHandSlotForReal);
                }
                else 
                    yield return i;
        }

        [HarmonyPatch(typeof(HandsEquipmentSet), nameof(HandsEquipmentSet.IsGripAllowed))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> FixOneHandedChecksInIsGripAllowed(IEnumerable<CodeInstruction> instructions)
        {
#if DEBUG
            Comment.Log("Entered FixOneHandedChecksInIsGripAllowed transpiler"); 
#endif
            foreach (var i in instructions)
                if (i.Calls(AccessTools.Method(typeof(ItemEntityWeapon), nameof(ItemEntityWeapon.CanTakeOneHand))))
                {
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldnull);
                    yield return new CodeInstruction(OpCodes.Ldnull);
                    yield return CodeInstruction.Call((ItemEntityWeapon weapon, HandSlot slotToInsert, ItemEntity itemBeingInserted) => CanInsertSingleHandSlotForReal);
                }
                else
                    yield return i;
        }

        [HarmonyPatch(typeof(HandsEquipmentSet), nameof(HandsEquipmentSet.IsGripAllowed))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> FixTwoHandedChecksInIsGripAllowed(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
#if DEBUG
            Comment.Log("Entered FixTwoHandedChecksInIsGripAllowed transpiler"); 
#endif

            var instr = instructions.ToList();
            CodeInstruction[] toSearch = new[]
            {
                new CodeInstruction(OpCodes.Ldfld, AccessTools.DeclaredField(typeof(HandsEquipmentSet), nameof(HandsEquipmentSet.SecondaryHand))),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(ItemSlot), nameof(ItemSlot.HasItem))),
                new CodeInstruction(OpCodes.Brtrue_S)
            };
            int index = IndexFinder(instructions, toSearch);

            if (index == -1)
            {
                Comment.Error("Failed to find index when tranpiling HandsEquipmentSet.IsGripAllowed for FixTwoHandedChecksInIsGripAllowed");
                return instructions;
            }

            var labelContinue = instr[index].labels.FirstOrDefault();
            if (labelContinue == default)
            {
                labelContinue = generator.DefineLabel();
                instr[index].labels.Add(labelContinue);
            }

            var labelDoCall = generator.DefineLabel();

            var toInsert = new CodeInstruction[]
            {
                new (OpCodes.Brfalse_S, labelContinue),
                new (OpCodes.Ldarg_0),
                new (OpCodes.Ldfld, AccessTools.DeclaredField(typeof(HandsEquipmentSet), nameof(HandsEquipmentSet.PrimaryHand))),
                new (OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(HandSlot), nameof(HandSlot.MaybeWeapon))),
                new (OpCodes.Dup),
                new (OpCodes.Ldnull),
                new (OpCodes.Ceq),
                new (OpCodes.Brfalse_S, labelDoCall),
                new (OpCodes.Pop),
                new (OpCodes.Br, labelContinue),
                new CodeInstruction(OpCodes.Ldarg_1).WithLabels(labelDoCall),
                CodeInstruction.Call(() => CanUseGripIfSecondaryHandOccupied),
                new (OpCodes.Ret)
            };
            instr.RemoveAt(index - 1);
            instr.InsertRange(index-1, toInsert);
            return instr;
        }
        public static bool CanInsertSingleHandSlotForReal(ItemEntityWeapon weapon, HandSlot slotToInsert = null, ItemEntity itemBeingInserted = null)
        {
            if (weapon is null)
                return false;
            var unit = slotToInsert?.Owner.Unit ?? weapon?.Wielder;
            bool result = weapon.CanTakeOneHand(unit) ||
                (unit?.Get<UnitPartCanHold2hWeaponIn1h>()?.CanBeUsedOn(weapon, slotToInsert, itemBeingInserted) ?? false);
            return result;
        }

        public static bool CanUseGripIfSecondaryHandOccupied(ItemEntityWeapon weapon, GripType gripType)
        {
            bool? result = weapon?.Owner?.Get<UnitPartCanHold2hWeaponIn1h>()?.CanHoldWeaponWithGrip(weapon, gripType);
            return result ?? false;
        }

        [HarmonyPatch(typeof(RuleCalculateAttacksCount), nameof(RuleCalculateAttacksCount.CalculateSecondaryHand))]
        [HarmonyPrefix]
        static void check(ref bool isPrimaryTwoHanded, HandSlot offHand, RuleCalculateAttacksCount __instance)
        {
            isPrimaryTwoHanded = isPrimaryTwoHanded && !OffHandParry.BucklerOrBash(offHand.Owner.State.Features.ShieldBash, __instance);
        }

    }

    [HarmonyPatch(typeof (CharInfoAttacksBlockView<CharInfoAttackEntityView<CharInfoAttackAttacksView>, CharInfoAttackAttacksView>), "SetOffHandAttack")]
    class MakeShieldBashAppearInUI
    {
        [UsedImplicitly]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> __instructions)
        {
            var instruction = __instructions.ToList();

            var toSearch1 = new CodeInstruction[]
            {
                new (OpCodes.Ldfld, AccessTools.Field(typeof(CharInfoAttacksBlockVM), nameof(CharInfoAttacksBlockVM.MainHandAttack))),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(CharInfoAttackEntityVM), nameof(CharInfoAttackEntityVM.IsTwoHanded)))
            };


            var toSearch2 = new CodeInstruction[]
            {
                new (OpCodes.Ldfld, AccessTools.Field(typeof(CharInfoAttacksBlockVM), nameof(CharInfoAttacksBlockVM.OffHandAttack))),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(CharInfoAttackEntityVM), nameof(CharInfoAttackEntityVM.IsShield))),
            };

            int index1 = IndexFinder(instruction, toSearch1, true) -2;
            int index2 = IndexFinder(instruction, toSearch2);

            if (index1 == -1 || index2 == -1)
            {
                Comment.Error("Failed to find index when transpiling to fix UI for shield bash with two-handed weapon");
                return __instructions;
            }
                        
            if (instruction[index2].operand is not Label labelShow)
            {
                Comment.Error("Failed to find labels when transpiling to fix UI for shield bash with two-handed weapon");
                return __instructions;
            }

            var toInsert = new CodeInstruction[]
            {
                new (OpCodes.Ldarg_0),
                new (instruction[index1+1].opcode, instruction[index1+1].operand),
                new (OpCodes.Ldfld, AccessTools.Field(typeof(CharInfoAttacksBlockVM), nameof(CharInfoAttacksBlockVM.OffHandAttack))),
                new (OpCodes.Ldfld, AccessTools.Field(typeof(CharInfoAttackEntityVM), nameof(CharInfoAttackEntityVM.AttackData))),
                new (OpCodes.Ldfld, AccessTools.Field(typeof(UIUtilityItem.AttackData), nameof(UIUtilityItem.AttackData.hasWeapon))),
                new (OpCodes.Brtrue_S, labelShow)
            };

            instruction.InsertRange(index1, toInsert);
            return instruction;
        }
    }

    public abstract class CanUse2hWeaponAs1hBase : UnitBuffComponentDelegate
    {
        abstract public bool CanBeUsedOn(ItemEntityWeapon weapon, HandSlot slotToInsert = null, ItemEntity itemBeingInserted = null);

        abstract public bool CanHoldWeaponWithGrip(ItemEntityWeapon weapon,GripType grip);

        abstract public bool IsApplicableToOffHand { get;}

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

        public bool CanBeUsedOn(ItemEntityWeapon weapon, HandSlot slotToInsert = null, ItemEntity itemBeingInserted = null)
        {
            if (!buffs.Any())
            {
                return false;
            }

            bool SlotIsPrimary;
            if (weapon == itemBeingInserted && slotToInsert is not null)
                SlotIsPrimary = slotToInsert.IsPrimaryHand;
            else if (weapon.HoldingSlot is HandSlot handSlot)
                SlotIsPrimary = handSlot.IsPrimaryHand;
            else
                SlotIsPrimary = true;

            foreach (var b in buffs)
            {
                bool result = false;
                b.CallComponents<CanUse2hWeaponAs1hBase>
                    (c => result = (SlotIsPrimary || c.IsApplicableToOffHand)
                    && c.CanBeUsedOn(weapon, slotToInsert, itemBeingInserted));
                if (result)
                {
                    return true;
                }
            }

            return false;
        }


        public bool CanHoldWeaponWithGrip(ItemEntityWeapon weapon, GripType gripType)
        {
            bool result = false;
            foreach (var b in buffs)
            {
                b.CallComponents<CanUse2hWeaponAs1hBase>
                    (component => 
                    {
                        bool r = component.CanBeUsedOn(weapon) && component.CanHoldWeaponWithGrip(weapon, gripType);
                        result = r;
                    });
                if (result)
                {
                    return true;
                }
            }
            return result;
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
        //[HarmonyPrefix]
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
            if (Way_of_the_shield.Settings.Debug.GetValue())
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