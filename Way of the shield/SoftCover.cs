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
using System.Text;
using static Kingmaker.Visual.CharacterSystem.Dismemberment.UI.DismembermentUIController;

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
                AttackerSize = Initiator.State.Size;
                targetPosition = target.Position;
                Result = new();
                var cached = cachedIsSwarm.FirstOrDefault(tuple => tuple.unit == target);
                if (cached.Equals(default))
                {
                    cached = new(target, target.Descriptor.Buffs.Enumerable.Any(buff => buff.Blueprint.AssetGuid == GenericSwarmBuffRef));
                    cachedIsSwarm.Enqueue(cached);
                }
                TargetIsSwarm = cached.IsSwarm;
                if (!TargetIsSwarm)
                {
                    TargetIsProne = target.State.Prone.Active;
                    TargetSize = TargetIsProne ? target.State.Size - 3 : target.State.Size;
                }
                else
                {
                    var cachedAir = cachedIsAirborne.FirstOrDefault(tuple => tuple.unit == target);
                    if (cachedAir.Equals(default))
                    {
                        cachedAir = new(target, target.Progression.Features.Enumerable.Any(Feature => Feature.Blueprint.AssetGuid == AirborneRef));
                        cachedIsAirborne.Enqueue(cachedAir);
                    }
                    TargetIsProne = cachedAir.IsAirborneSwarm;
                    TargetSize = target.State.Size;
                    SwarmCorpulence = target.View.Corpulence;
                    SwarmTargetSize = SwarmCorpulence switch
                    {
                        <= 0.5f => Size.Small,
                        <= 1.0f => Size.Medium,
                        <= 1.5f => Size.Large,
                        <= 2.0f => Size.Huge,
                        _ => Size.Gargantuan
                    };
                }
                Fake = fake;
            }
            public RuleSoftCover(UnitEntityData initiator, UnitEntityData target, ItemEntityWeapon weapon, bool fake = false) : this(initiator, target, fake)
            {
                //attackerPosition = initiator.Position;
                //targetPosition = target.Position;
                Result = new();
                Weapon = weapon;
            }

            Vector3 attackerPosition;
            Vector3 targetPosition;
            public Size TargetSize;
            public Size AttackerSize;
            public ItemEntityWeapon Weapon;
            public List<(UnitEntityData obstacle, int sizeDifference, int bonus)> Result = new();
            public bool SoftCoverDenied = false;
            readonly public bool TargetIsProne;
            internal bool Fake;
            public bool TargetIsSwarm;
            public float SwarmCorpulence;
            public Size SwarmTargetSize;

            static readonly BlueprintGuid GenericSwarmBuffRef = BlueprintGuid.Parse("aaee201820f34400a2702d46a4260fbf");
            static readonly BlueprintGuid AirborneRef = BlueprintGuid.Parse("70cffb448c132fa409e49156d013b175");
            static readonly Queue<(UnitEntityData unit, bool IsSwarm)> cachedIsSwarm = new(20);
            static readonly Queue<(UnitEntityData unit, bool IsAirborneSwarm)> cachedIsAirborne = new(10);

            public override void OnTrigger(RulebookEventContext context)
            {
                if (TacticalCombatHelper.IsActive) return;
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log($"Soft Cover SoftCoverRule - entered OnTrigger. Target is a swarm? {TargetIsSwarm}. Target is prone? {TargetIsProne}.");
#endif
                if (SoftCoverDenied)
                {
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log("Soft Cover SoftCoverRule - Soft Cover has been denied."); 
#endif
                    return;
                }
                Vector3 vectorofAttack = targetPosition - attackerPosition;
                float vectorofAttackSqrMagnitude = vectorofAttack.sqrMagnitude;
                float swarmCase_vectorofAttackSqrMagnitude = vectorofAttack.magnitude - SwarmCorpulence;
                UnitEntityData unitWithCorpulence = Initiator.Get<UnitPartRider>()?.SaddledUnit ?? Initiator;
                float attackersCorpulenceLess = unitWithCorpulence.Corpulence * unitWithCorpulence.Corpulence * 0.75f;
                List<UnitEntityData> PeopleAround = GameHelper
                    .GetTargetsAround(attackerPosition, TargetIsSwarm ? vectorofAttack.magnitude +3f : vectorofAttack.magnitude, false) //if the target is swarm, look for 3 meters further
                    .Where(unit => unit != Initiator && unit != Target)
                    .ToList();
                PeopleAround.Remove(Initiator.Get<UnitPartRider>()?.SaddledUnit ?? Initiator.Get<UnitPartSaddled>()?.Rider);
                StringBuilder sb = new();
                foreach (UnitEntityData unit in PeopleAround)
                {
#if DEBUG
                    if (Settings.Debug.GetValue())
                        sb.Clear(); 
#endif
                    Vector3 unitPosition = unit.Position;
                    Vector3 towardsUnitVector = unitPosition - attackerPosition;
                    float sqrDistanceFromUnitToAttacker = towardsUnitVector.sqrMagnitude;
                    float angle = Vector3.Angle(towardsUnitVector, vectorofAttack);
                    float sqrCorpulenceLess = unit.Corpulence * unit.Corpulence * 0.75f;
                    bool CheckResult = false;

                    if (TargetIsSwarm && (towardsUnitVector.magnitude - unit.Corpulence) > vectorofAttack.magnitude) //if target is a swarm, but the unit is too far away, skip it
                        continue;

                    else if (!TargetIsSwarm || towardsUnitVector.magnitude < swarmCase_vectorofAttackSqrMagnitude - unit.Corpulence) //if the target is not swarm or the unit is not inside the swarm
                    {
                            CheckResult = 
                                sqrDistanceFromUnitToAttacker < vectorofAttackSqrMagnitude //the unit is not behind the target
                             && VectorMath.SqrDistancePointSegment(attackerPosition, targetPosition, unitPosition) <= sqrCorpulenceLess //line of attack more or less passes through the unit
                             && sqrDistanceFromUnitToAttacker > attackersCorpulenceLess; //the unit is more or less in front of the attacker


#if DEBUG
                        if (Settings.Debug.GetValue())
                            Comment.Log($"Soft Cover - Unit is {unit.CharacterName}, " +
                                $"vectorofAttackSqrMagnitude is {vectorofAttackSqrMagnitude},  " +
                                $"sqrDistanceFromUnitToAttacker.sqrMagnitude is {sqrDistanceFromUnitToAttacker}, " +
                                $"angle is {angle}, " +
                                $"reduced squared corpulence is {sqrCorpulenceLess}. " +
                                $"vectorofAttack.sqrMagnitude is {vectorofAttack.sqrMagnitude}. " +
                                $"Distance of unit from trajectory is {VectorMath.SqrDistancePointSegment(attackerPosition, targetPosition, unitPosition)}" +
                                $"{(sqrDistanceFromUnitToAttacker > attackersCorpulenceLess ? "" : "UNIT IS INSIDE THE ATTACKER")}. " +
                                $"CheckResult is {CheckResult}.");
#endif

                        if (!CheckResult)
                            continue;
                        
                        UnitState unitState = unit.State;
                        Size effectiveTargetSize;
                        if (!TargetIsSwarm || TargetIsProne)
                            effectiveTargetSize = TargetSize;
                        else 
                            effectiveTargetSize = SwarmTargetSize;
                        Size effectiveUnitSize = (unitState.Prone.Active ? (unitState.Size > Size.Tiny ? unitState.Size - 3 : Size.Fine) : unitState.Size);
                        int sizeDifference = effectiveUnitSize - effectiveUnitSize;
                        int penalty = sizeDifference switch
                        {
                            -2 => -1,
                            -1 => -2,
                            > -1 => -4 * (sizeDifference + 1),
                            _ => 0
                        };
                        Result.Add(new(unit, sizeDifference, penalty));
#if DEBUG
                        if (Settings.Debug.GetValue())
                            Comment.Log(
                                $"Soft Cover - Size Difference between {unit.CharacterName} " +
                                $"(effective size is {effectiveUnitSize}. Prone? {unitState.Prone.Active}) " +
                                $"and {Target.CharacterName} (effective size {effectiveTargetSize}) is {sizeDifference}, " +
                                $"penalty is {penalty}"); 
#endif
                    }
                    else
                    {
                        bool lineOfAttackUnitIntersection = VectorMath.SqrDistancePointSegment(attackerPosition, targetPosition, unitPosition) <= sqrCorpulenceLess; //line of attack more or less passes through the unit
#if  DEBUG
                        if (Settings.Debug.GetValue())
                        sb.Append($"Soft Cover -  Unit is {unit.CharacterName}. " +
                                    $"Attacker position is ({attackerPosition}), " +
                                    $"target position is ({targetPosition}), " +
                                    $"unit position is ({unitPosition}). " +
                                    $"Distance of unit from trajectory is {lineOfAttackUnitIntersection}"); 
#endif
                        if (!lineOfAttackUnitIntersection)
                        {
#if DEBUG
                            if (Settings.Debug.GetValue())
                                Comment.Log(sb.ToString()); 
#endif
                            continue;
                        }

                        Utilities.LineCircleIntersect(attackerPosition, targetPosition, unitPosition, unit.Corpulence);
                        var OffsetFromAttackerCenter = (Utilities.results[0] - (Vector2)attackerPosition).magnitude; //
                        bool further = OffsetFromAttackerCenter > vectorofAttack.magnitude; //unit is behind the swarm's centers and does not block the attack line
                        bool closer = OffsetFromAttackerCenter < swarmCase_vectorofAttackSqrMagnitude; //unit is not partially inside the swarm
                        float FreeSwarmDistance = SwarmCorpulence - (vectorofAttack.magnitude - OffsetFromAttackerCenter);
                                   
#if DEBUG
                        if (Settings.Debug.GetValue())
                        {
                            sb.AppendLine($"Intersection points are {Utilities.results[0]} and {Utilities.results[1]}. " +
                                    $"OffsetFromAttackerCenter is {OffsetFromAttackerCenter}, " +
                                    $"swarmCase_vectorofAttackSqrMagnitude is {swarmCase_vectorofAttackSqrMagnitude}, " +
                                    $"vectorofAttack.magnitude is {vectorofAttack.magnitude}. " +
                                    $"Further is {further}, Closer is {closer}.");
                            if (!further && !closer)
                                sb.Append($"FreeSwarmDistance is {FreeSwarmDistance}.");
                            Comment.Log(sb.ToString());
                        }
#endif
                        if (further)
                        {
                            continue;
                        }
                        else if (closer) //copy-paste from line 163 and on, because extracting local methods is difficult for my brain
                        {
                            UnitState unitState = unit.State;
                            Size effectiveTargetSize;
                            if (!TargetIsSwarm || TargetIsProne)
                                effectiveTargetSize = TargetSize;
                            else
                                effectiveTargetSize = SwarmTargetSize;
                            Size effectiveUnitSize = (unitState.Prone.Active ? (unitState.Size > Size.Tiny ? unitState.Size - 3 : Size.Fine) : unitState.Size);
                            int sizeDifference = effectiveUnitSize - effectiveUnitSize;
                            int penalty = sizeDifference switch
                            {
                                -2 => -1,
                                -1 => -2,
                                > -1 => -4 * (sizeDifference + 1),
                                _ => 0
                            };
                            Result.Add(new(unit, sizeDifference, penalty));
#if DEBUG
                            if (Settings.Debug.GetValue())
                                Comment.Log(
                                    $"Soft Cover - Size Difference between {unit.CharacterName} " +
                                    $"(effective size is {effectiveUnitSize}. Prone? {unitState.Prone.Active}) " +
                                    $"and {Target.CharacterName} (effective size {effectiveTargetSize}) is {sizeDifference}, " +
                                    $"penalty is {penalty}");
#endif
                        }
                        else
                        {
                            var RequiredDistance = 0.4f * (Mathf.Pow(1.5f, AttackerSize - Size.Medium));
                            float IntendedPenalty = ;
                            //The resulting penalty is proportional to the shortage of swarm's visible corpulence available to attack
                            //compared to the distance required for a non-penalized attack
                            int resultingPenalty = Convert.ToInt32(Math.Ceiling(IntendedPenalty * (1f - (FreeSwarmDistance / RequiredDistance))));
                            Result.Add(new(unit, sizeDifference, resultingPenalty));
#if DEBUG
                            if (Settings.Debug.GetValue())
                                Comment.Log(
                                $"Soft Cover - " +
                                    //$"Size Difference between {unit.CharacterName} " +
                                    //$"(effective size is {effectiveUnitSize}. Prone? {unitState.Prone.Active}) " +
                                    //$"and {Target.CharacterName} (effective size {effectiveTargetSize}) is {sizeDifference}, " +
                                    $"RequiredDistance is {RequiredDistance}, FreeSwarmDistance is {FreeSwarmDistance}, factor is {IntendedPenalty * (1f - (FreeSwarmDistance / RequiredDistance))}. " +
                                    $"IntendedPenalty is {IntendedPenalty}, resultingPenalty is {resultingPenalty}");
#endif
                        }
                    }
                }
            }
        }

        [HarmonyPrepare]
        public static bool Prepare() 
        { 
            if (EnableSoftCover.GetValue()) return true;
            else { Comment.Log("Soft Cover - EnableSoftCover setting is disabled, patches from SoftCover won't be applied."); return false; };
        }

        [HarmonyPatch(typeof(RuleAttackRoll), nameof(RuleAttackRoll.OnTrigger))]
        [HarmonyPrefix]
        public static void RuleAttackRoll_OnTrigger_Prefix(RuleAttackRoll __instance)
        {
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log($"Soft Cover - entered RuleAttackRoll_OnTrigger_Prefix for {__instance.Target.CharacterName}"); ;
#endif
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
                    Comment.Log($"Soft Cover - {obstacle.CharacterName} is a soft cover provider for {__instance.Target?.CharacterName}, penalty is {penalty}"); 
#endif
                if (penalty < THEobstacle.bonus) THEobstacle = (obstacle, penalty);
            };
#if DEBUG

#endif
            if (THEobstacle.obstacle is null) return;
            __instance.AddModifier(THEobstacle.bonus, THEobstacle.obstacle.MainFact, ModifierDescriptorExtension.SoftCover);
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log($"Soft Cover - Assigned Soft Cover attack penalty {THEobstacle.bonus} from unit {THEobstacle.obstacle.CharacterName} to unit {__instance.Target.CharacterName}"); 
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
                Comment.Log("Soft Cover - Begin transpiling UnitCombatState.AttackOfOpportunity to insert SoftCoverCheckForAoO."); 
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
