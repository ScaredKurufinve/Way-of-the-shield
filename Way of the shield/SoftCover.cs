//#undef DEBUG
using Kingmaker;
using Kingmaker.Armies.TacticalCombat;
using Kingmaker.Controllers.Combat;
using Kingmaker.Designers;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic;
using Kingmaker.UI.Models.Log;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Math;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using Way_of_the_shield.NewComponents;
using Owlcat.Runtime.Core.Utils;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using System.Diagnostics;

namespace Way_of_the_shield
{
    [HarmonyPatch]
    public static class SoftCover
    {
        public class RuleSoftCover : RulebookTargetEvent
        {
            static RuleSoftCover()
            {
                Type gameLogEventType = typeof(GameLogRuleEvent<>).MakeGenericType(typeof(RuleSoftCover));
                GameLogEventsFactory.RegisterCreator((RuleSoftCover rule) => (GameLogEvent)Activator.CreateInstance(gameLogEventType,rule));
            }
            public RuleSoftCover(UnitEntityData initiator, UnitEntityData target, bool fake = false) : base(initiator: initiator, target: target)
            {
                attackerPosition = initiator.Position;
                targetPosition = target.Position;
                Result = new();
                targetSize = target.OriginalSize;
                Fake = fake;
            }
            public RuleSoftCover(UnitEntityData initiator, UnitEntityData target, ItemEntityWeapon weapon, bool fake = false) : this(initiator: initiator, target: target, fake: fake)
            {
                attackerPosition = initiator.Position;
                targetPosition = target.Position;
                Result = new();
                Weapon = weapon;
            }

            Vector3 attackerPosition;
            Vector3 targetPosition;
            public Size targetSize;
            public ItemEntityWeapon Weapon;
            public List<(UnitEntityData obstacle, int sizeDifference, int bonus)> Result = new();
            public bool SoftCoverDenied = false;
            internal bool Fake;

            public override void OnTrigger(RulebookEventContext context)
            {
                if (TacticalCombatHelper.IsActive) return;
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log("SoftCoverRule - entered OnTrigger");
#endif
                if (SoftCoverDenied)
                {
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log("Soft Cover has been denied."); 
#endif
                    return;
                }
                Vector3 vectorofAttack = targetPosition - attackerPosition;
                float vectorofAttackSqrMagnitude = vectorofAttack.sqrMagnitude;
                UnitEntityData unitWithCorpulence = Initiator.Get<UnitPartRider>()?.SaddledUnit ?? Initiator;
                float sqrcorpulencecorpulencecorpulence = unitWithCorpulence.Corpulence * unitWithCorpulence.Corpulence * Convert.ToSingle(0.75);
                List<UnitEntityData> PeopleAround = GameHelper.GetTargetsAround(attackerPosition, vectorofAttack.magnitude, false).Where(unit => unit != Initiator
                                                                                     && unit != Target).ToList();
                PeopleAround.Remove(Initiator.Get<UnitPartRider>()?.SaddledUnit ?? Initiator.Get<UnitPartSaddled>()?.Rider);
                foreach (UnitEntityData unit in PeopleAround)
                {
                    Vector3 unitPosition = unit.Position;
                    Vector3 towardsUnitVector = unitPosition - attackerPosition;
                    float sqrDistanceFromUnitToAttacker = towardsUnitVector.sqrMagnitude;
                    float angle = Vector3.Angle(towardsUnitVector, vectorofAttack);
                    float sqrCorpulence = unit.Corpulence * unit.Corpulence;
                    float sqrCorpulenceLess = sqrCorpulence * Convert.ToSingle(0.75);
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log($"Soft Cover - Unit is {unit.CharacterName}, " +
                            $"vectorofAttackSqrMagnitude is {vectorofAttackSqrMagnitude},  " +
                            $"sqrDistanceFromUnitToAttacker.sqrMagnitude is {sqrDistanceFromUnitToAttacker}, " +
                            $"angle is {angle}, " +
                            $"reduced squared corpulence is {sqrCorpulenceLess}. " +
                            $"vectorofAttack.sqrMagnitude is {vectorofAttack.sqrMagnitude}." +
                            $"{(sqrDistanceFromUnitToAttacker > sqrcorpulencecorpulencecorpulence ? "" : "UNIT IS INSIDE THE ATTACKER")}"); 
#endif

                    if (   sqrDistanceFromUnitToAttacker < vectorofAttackSqrMagnitude
                        && VectorMath.SqrDistancePointSegment(attackerPosition, targetPosition, unitPosition) <= sqrCorpulenceLess
                        && !(sqrDistanceFromUnitToAttacker <= sqrcorpulencecorpulencecorpulence))
                    {
                        int sizeDifference = unit.OriginalSize - targetSize;
                        int penalty = sizeDifference switch
                        {
                            -2 => -1,
                            -1 => -2,
                            > -1 => -4 * (sizeDifference + 1),
                            _ => 0
                        };
#if DEBUG
                        if (Settings.Debug.GetValue())
                            Comment.Log($"Soft Cover - Size Difference between {unit.CharacterName} (size {unit.OriginalSize}) and {Target.CharacterName} (size {targetSize}) is {sizeDifference}, penalty is {penalty}"); 
#endif
                        Result.Add(new(unit, sizeDifference, penalty));
                    }
                }
            }
        }

        [HarmonyPrepare]
        public static bool Prepare() 
        { 
            if (EnableSoftCover.GetValue()) return true;
            else { Comment.Log("EnableSoftCover setting is disabled, patches from SoftCover won't be applied."); return false; };
        }

        [HarmonyPatch(typeof(RuleAttackRoll), nameof(RuleAttackRoll.OnTrigger))]
        [HarmonyPrefix]
        public static void RuleAttackRoll_OnTrigger_Prefix(RuleAttackRoll __instance)
        {
            RuleSoftCover SoftCover = Rulebook.Trigger<RuleSoftCover>(new(__instance.Initiator, __instance.Target, __instance.Weapon));
            List<(UnitEntityData obstacle, int sizeDifference, int penalty)> obstacles = SoftCover.Result;
            if (SoftCover.TryGetCustomData(BackToBackNew.BackToBackUnitsKey, out List<UnitEntityData> Backers)) __instance.SetCustomData(BackToBackNew.BackToBackUnitsKey, Backers);
            if (SoftCover.TryGetCustomData(Backstab.CachedConcealment, out Concealment Concealment)) __instance.SetCustomData(Backstab.CachedConcealment, Concealment);
            if (obstacles.Count == 0) return;
            (UnitEntityData obstacle, int bonus) THEobstacle = new(null, 0);
            foreach ((UnitEntityData obstacle, int _, int penalty) in obstacles)
            {
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log($"{obstacle.CharacterName} is a soft cover provider for {__instance.Target?.CharacterName}, penalty is {penalty}"); 
#endif
                if (penalty < THEobstacle.bonus) THEobstacle = (obstacle, penalty);
            };
#if DEBUG

#endif
            if (THEobstacle.obstacle is null) return;
            __instance.AddModifier(THEobstacle.bonus, THEobstacle.obstacle.MainFact, ModifierDescriptorExtension.SoftCover);
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log($"Assigned Soft Cover attack penalty {THEobstacle.bonus} from unit {THEobstacle.obstacle.CharacterName} to unit {__instance.Target.CharacterName}"); 
#endif
        }

        [HarmonyPatch]
        public static class AoOPatches
        {
            [HarmonyTargetMethods]
            public static IEnumerable<MethodBase> GetMethods()
            {

//#if DEBUG
//                Comment.Log("Nested types of UnitCombatState:"); 
//                foreach (Type t in typeof(UnitCombatState).GetNestedTypes()) Comment.Log(t.Name); 
//#endif

                yield return typeof(UnitCombatState).GetMethod(nameof(UnitCombatState.AttackOfOpportunity));
                //yield return typeof(UnitCombatState).GetProperty(nameof(UnitCombatState.CanAttackOfOpportunity)).GetMethod;
                //yield return typeof(UnitCombatState).GetNestedType("'<>c'").GetMethod("'<get_CanAttackOfOpportunity>b__92_0'");
            }

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> InsertSoftCoverCheckIntoAoO(IEnumerable<CodeInstruction> instructions)
            {
#if DEBUG
                Comment.Log("Begin transpiling UnitCombatState.AttackOfOpportunity to insert SoftCoverCheckForAoO."); 
#endif
                List<CodeInstruction> _instructions = instructions.ToList();

                int index = IndexFinder(_instructions, new CodeInstruction[] { new CodeInstruction(OpCodes.Call, typeof(UnitHelper).GetMethod(nameof(UnitHelper.GetThreatHand))) });

                if (index == -1)
                {
                    return instructions;
                }

                CodeInstruction[] toInsert = new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    CodeInstruction.Call(typeof(AoOPatches), nameof(SoftCoverCheckForAoO))
                };

                _instructions.InsertRange(index, toInsert);


                return _instructions;
            }


            public static bool SoftCoverCheckForAoO(WeaponSlot weaponSlot, UnitCombatState __instance, UnitEntityData target)
            {
                if (weaponSlot is null) return false;
                ItemEntityWeapon weapon = weaponSlot.Weapon;
                if (weapon is not null &&
                     Rulebook.Trigger(new RuleSoftCover(__instance.Unit, target, weapon, true)).Result.Count > 0)
                    return false;
                else return true;
            }
        }

        

        public class SoftCoverDenialComponent : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleSoftCover>
        {
            public void OnEventAboutToTrigger(RuleSoftCover evt)
            {
                if (CheckWeaponCategory && !Categories.Contains(evt.Weapon.Blueprint.Category)) return;
                if (CheckWeaponSubCategory && !Subcategories.Any(subcategory => evt.Weapon.Blueprint.Category.HasSubCategory(subcategory))) return;
                if (CheckWeaponRange && !(Ranged ? evt.Weapon.Blueprint.IsRanged : evt.Weapon.Blueprint.IsMelee)) return;
                evt.SoftCoverDenied = true;
            }
            public void OnEventDidTrigger(RuleSoftCover evt)
            {

            }

            public bool CheckWeaponCategory;
            public WeaponCategory[] Categories = Array.Empty<WeaponCategory>();
            public bool CheckWeaponSubCategory;
            public WeaponSubCategory[] Subcategories = Array.Empty<WeaponSubCategory>();
            public bool CheckWeaponRange;
            public bool Ranged;
        }
    }
}
