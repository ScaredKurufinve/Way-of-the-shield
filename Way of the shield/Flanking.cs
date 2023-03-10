using Kingmaker;
using Kingmaker.Armies.TacticalCombat;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Controllers.Combat;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using TurnBased.Controllers;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Kingmaker.TextTools;
using Kingmaker.UnitLogic.Parts;

namespace Way_of_the_shield

{
    public static class Flanking
    {

        const float MinimalFlankingAngle = 110;
        const float AngleOffsetByImprovedOutflank = 50;
        static readonly RulebookEvent.CustomDataKey FlankedKey = new("Flanking");
        static readonly RulebookEvent.CustomDataKey FlankingUnitsKey = new("FlankingUnits");
        static readonly RulebookEvent.CustomDataKey Outflank = new("Outflank");
        public static BlueprintFeature ImprovedOutflank = null;
        static float GetMinimalFlankingAngle(bool AmazingOutflankers)
        {
            if (AmazingOutflankers)
                return (MinimalFlankingAngle - AngleOffsetByImprovedOutflank);
            else return MinimalFlankingAngle;
        }
        static bool AmazingOuflankers(UnitEntityData attacker, UnitEntityData flanker)
        {
            return (attacker.Progression.Features.HasFact(ImprovedOutflank) && flanker.Progression.Features.HasFact(ImprovedOutflank));
        }

        [HarmonyPatch]
        public static class UnitCombatState_IsFlanked_Patch
        {


            [HarmonyTargetMethod]
            public static MethodBase TargetMethod()
            {
                return typeof(UnitCombatState).GetProperty(nameof(UnitCombatState.IsFlanked)).GetMethod;
            }


            [HarmonyPrepare]
            public static bool Prepare()
            {
                if (ForbidCloseFlanking.GetValue()) return true;
                else { Comment.Log("ForbidCloseFlanking setting is disabled, patch UnitCombatState_IsFlanked_Patch won't be applied."); return false; };
            }

            [HarmonyPostfix]
            public static bool Prefix(UnitCombatState __instance, ref bool __result)
            {
#if Debug
                if (Settings.Debug.GetValue()) Comment.Log("Entered the flanking prefix"); 
#endif
                if (AllowCloseFlankingToEnemies.GetValue() && __instance.Unit.IsPlayerFaction)
                {
#if Debug
                    if (Settings.Debug.GetValue())
                        Comment.Log("Unit {0} is not player faction, prefix is skipped", new object[] { __instance.Unit.CharacterName }); 
#endif
                    return true;
                };
                if (__instance.Unit.Descriptor.State.Features.CannotBeFlanked) { __result = false; return false; }
                if (TacticalCombatHelper.IsActive) { __result = __instance.Unit.IsInCombat && __instance.EngagedBy.Count > 1; return false; }

                Vector3 position = __instance.Unit.Position;
                UnitEntityData[] engaged_by = CombatController.IsInTurnBasedCombat() ? __instance.EngagedBy.ToArray() : __instance.EngagedBy.Where(x => x.Commands.AnyCommandTargets(__instance.Unit)).ToArray();
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log($"{__instance.Unit.CharacterName} is engaged by {engaged_by.Length.ToString()} people."); 
#endif
                int i = 0;
                while (i < engaged_by.Count() - 1)
                {

                    for (int further = i + 1; further < engaged_by.Count(); further++)
                    {
                        float angle = Vector3.Angle(engaged_by[i].Position - position,
                                          engaged_by[further].Position - position);

#if DEBUG
                        if (Settings.Debug.GetValue())
                            Comment.Log("When attacking {0}, the angle between {1} and {2} is {3} degrees. Required angle is {4}.",
                            __instance.Unit.CharacterName, engaged_by[i].CharacterName, engaged_by[further].CharacterName, angle.ToString(), GetMinimalFlankingAngle(AmazingOuflankers(engaged_by[i], engaged_by[further]))); 
#endif
                        if (angle > GetMinimalFlankingAngle(AmazingOuflankers(engaged_by[i], engaged_by[further])))
                        {
#if DEBUG
                            if (Settings.Debug.GetValue())
                                Comment.Log("Is Flanked"); 
#endif
                            __result = true; return false;
                        }

                    }
                    i++;
                };
#if DEBUG

                if (Settings.Debug.GetValue()) Comment.Log("Not Flanked"); 
#endif
                __result = false;
                return false;
            }
        }

        [HarmonyPatch]
        public static class RuleAttackRoll_TargetIsFlanked_Patch
        {

            [HarmonyTargetMethods]
            public static IEnumerable<MethodBase> TargetMethods()
            {
                return typeof(RuleAttackRoll).GetConstructors();
            }

            [HarmonyPostfix]
            public static void Postfix(RuleAttackRoll __instance)
            { 
#if DEBUG
        if(Settings.Debug.GetValue())
                    Comment.Log("Entered the RuleAttackRoll constructor postfix"); 
#endif
                if (__instance.IsFake) return;
                UnitEntityData target = __instance.Target;
                if (!ForbidCloseFlanking.GetValue()
                    || (AllowCloseFlankingToEnemies.GetValue() && target.IsPlayerFaction))
                { __instance.TargetIsFlanked = target.CombatState.IsFlanked;
#if DEBUG
                    if (Settings.Debug.GetValue()) Comment.Log("return from RuleAttackRoll constructor at 1"); 
#endif
                    return; };
                if (target.State.Features.CannotBeFlanked) { __instance.TargetIsFlanked = false; return; };
                if (TacticalCombatHelper.IsActive) { __instance.TargetIsFlanked = target.IsInCombat && target.CombatState.EngagedBy.Count > 1;
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log("return from RuleAttackRoll constructor at 2"); 
#endif
                    return; };
                List<UnitEntityData> EngagedUnits = CombatController.IsInTurnBasedCombat() ? target.CombatState.EngagedBy.ToList() : target.CombatState.EngagedBy.Where(x => x.Commands.AnyCommandTargets(target)).ToList();
                UnitEntityData attacker = __instance.Initiator;
                UnitEntityData MountRider = attacker.Get<UnitPartRider>()?.SaddledUnit ?? attacker.Get<UnitPartSaddled>()?.Rider;
                if (MountRider is not null) EngagedUnits.Remove(MountRider);
                if (!EngagedUnits.Contains(attacker) || EngagedUnits.Count < 2) { __instance.TargetIsFlanked = false;
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log("return from RuleAttackRoll constructor at 3"); 
#endif
                    return; };
                EngagedUnits.Remove(attacker);
                UnitEntityData flanker = attacker; //dumb initialization to make compiler happy
                List<(UnitEntityData, float, bool)> flankers = new();
                float biggestAngle = 0;
                Vector3 targetPosition = target.Position;
                Vector3 AttackVector = attacker.Position;
                bool AREamazingOutflankers = false;
                float angle;
                bool amazingOutflankers;
                foreach (UnitEntityData possibleFlanker in EngagedUnits)
                {
                    angle = Vector3.Angle(targetPosition - AttackVector, targetPosition - possibleFlanker.Position);
                    amazingOutflankers = AmazingOuflankers(attacker, possibleFlanker);
                    flankers.Add((possibleFlanker, angle, amazingOutflankers));
                    if ((amazingOutflankers ? angle + AngleOffsetByImprovedOutflank : angle) > (AREamazingOutflankers ? biggestAngle + AngleOffsetByImprovedOutflank : biggestAngle))
                    { biggestAngle = angle; flanker = possibleFlanker; AREamazingOutflankers = amazingOutflankers; }
                }
                if (biggestAngle >= GetMinimalFlankingAngle(AREamazingOutflankers)) __instance.TargetIsFlanked = true;
                else __instance.TargetIsFlanked = false;
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log("When attacking {0} the angle between {1} and {2} is {3}. Improved Outflank is {5}. Flanking is {4}", target.CharacterName, attacker.CharacterName, flanker.CharacterName, biggestAngle.ToString(), __instance.TargetIsFlanked, AREamazingOutflankers); 
#endif

                __instance.SetCustomData(FlankingUnitsKey, flankers);
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log("CustomData is " + __instance.TryGetCustomData(FlankingUnitsKey, out List<(UnitEntityData, float, bool)> team)); 
#endif
                __instance.SetCustomData(FlankedKey, (flanker, biggestAngle, AREamazingOutflankers));

            }
        }

        [HarmonyPatch(typeof(RuleAttackRoll), nameof(RuleAttackRoll.OnTrigger))]
        public static class RuleAttackRoll_OnTrigger_Patch
        {

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log("Entered Flanking - RuleAttackRoll_OnTrigger_Patch transpiler"); 
#endif
                List<CodeInstruction> _instructions = instructions.ToList();

                CodeInstruction[] toSearch = new CodeInstruction[]
                {
                    new CodeInstruction (OpCodes.Ldfld, typeof(RulebookTargetEvent).GetField(nameof(RulebookTargetEvent.Target))),
                    new CodeInstruction (OpCodes.Callvirt, typeof(UnitEntityData).GetProperty(nameof(UnitEntityData.CombatState)).GetMethod),
                    new CodeInstruction (OpCodes.Callvirt, typeof(UnitCombatState).GetProperty(nameof(UnitCombatState.IsFlanked)).GetMethod)
                };

                int index = IndexFinder(_instructions, toSearch, true);
                if (index == -1) { return instructions; };

                _instructions.RemoveRange(index, toSearch.Count());
                _instructions.Insert(index, CodeInstruction.Call(typeof(RuleAttackRoll), typeof(RuleAttackRoll).GetProperty(nameof(RuleAttackRoll.TargetIsFlanked)).GetMethod.Name));


                return _instructions;

            }
        }

        [HarmonyPatch(typeof(RuleCalculateAttackBonus), nameof(RuleCalculateAttackBonus.OnTrigger))]
        public static class RuleCalculateAttackBonus_OnTrigger_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Trasnpiler(IEnumerable<CodeInstruction> instructions)
            {
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log("Entered RuleCalculateAttackBonus_OnTrigger_Patch trasnpiler"); 
#endif
                List<CodeInstruction> _instructions = instructions.ToList();

                CodeInstruction[] toSearch = new CodeInstruction[]
                {
                    new CodeInstruction (OpCodes.Ldfld, typeof(RulebookTargetEvent).GetField(nameof(RulebookTargetEvent.Target))),
                    new CodeInstruction (OpCodes.Callvirt, typeof(UnitEntityData).GetProperty(nameof(UnitEntityData.CombatState)).GetMethod),
                    new CodeInstruction (OpCodes.Callvirt, typeof(UnitCombatState).GetProperty(nameof(UnitCombatState.IsFlanked)).GetMethod),
                    new CodeInstruction (OpCodes.Brtrue_S),
                    new CodeInstruction (OpCodes.Ldarg_0)
                };

                int index = IndexFinder(_instructions, toSearch, true);
                if (index == -1) { return instructions; };

                _instructions.RemoveRange(index, toSearch.Count());

                return _instructions;
            }
        }

        [HarmonyPatch]
        public static class Outflank_patches
        {

            [HarmonyPatch(typeof(OutflankAttackBonus), nameof(OutflankAttackBonus.OnEventAboutToTrigger))]
            public static class OutflankAttackBonus_OnEventAboutToTrigger_patch
            {
                [HarmonyTranspiler]
                public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log("Entered Flanking -OutflankAttackBonus_OnEventAboutToTrigger_patch transpiler"); 
#endif
                    List<CodeInstruction> _instructions = instructions.ToList();

                    CodeInstruction[] toSearch = new CodeInstruction[]
                    {
                    new CodeInstruction (OpCodes.Ldfld, typeof(RulebookTargetEvent).GetField(nameof(RulebookTargetEvent.Target))),
                    new CodeInstruction (OpCodes.Callvirt, typeof(UnitEntityData).GetProperty(nameof(UnitEntityData.CombatState)).GetMethod),
                    new CodeInstruction (OpCodes.Callvirt, typeof(UnitCombatState).GetProperty(nameof(UnitCombatState.IsFlanked)).GetMethod)
                    };

                    int index = IndexFinder(_instructions, toSearch, true);
                    if (index == -1) { return instructions; };

                    _instructions.RemoveRange(index, toSearch.Count());
                    _instructions.Insert(index, CodeInstruction.Call(typeof(RuleCalculateAttackBonus), typeof(RuleCalculateAttackBonus).GetProperty(nameof(RuleCalculateAttackBonus.TargetIsFlanked)).GetMethod.Name));


                    CodeInstruction[] toSearch2 = new CodeInstruction[]
                    {
                    new CodeInstruction (OpCodes.Callvirt, typeof(UnitEntityData).GetProperty(nameof(UnitEntityData.State)).GetMethod),
                    new CodeInstruction (OpCodes.Ldfld, typeof(UnitCombatState).GetField(nameof(UnitState.Features)))
                    };

                    CodeInstruction[] toSearch3 = new CodeInstruction[]
                    {
                    new CodeInstruction (OpCodes.Ldloc_0),
                    new CodeInstruction (OpCodes.Brfalse_S),
                    new CodeInstruction (OpCodes.Ldarg_1),
                    new CodeInstruction (OpCodes.Ldarg_0),
                    new CodeInstruction (OpCodes.Ldfld, typeof(OutflankAttackBonus).GetField(nameof(OutflankAttackBonus.AttackBonus)))
                    };

                    int index2 = IndexFinder(_instructions, toSearch2, true) - 1;
                    int index3 = IndexFinder(_instructions, toSearch3, true);
                    if (index2 == -1) { return instructions; };
                    if (index3 == -1) { return instructions; };
                    _instructions.RemoveRange(index2, index3 - index2);

                    CodeInstruction[] ToInsert2 = new CodeInstruction[]
                    {
                        new CodeInstruction (OpCodes.Ldarg_1),
                        CodeInstruction.Call(typeof(Outflank_patches), nameof(IsSuitableForOutflank)),
                        new CodeInstruction(OpCodes.Stloc_0)
                     };

                    _instructions.InsertRange(index2, ToInsert2);

                    return _instructions;


                }

                //[HarmonyPrefix]
                //public static void Prefix(RuleCalculateAttackBonus evt)
                //{
                //    Comment.Log("Entered the OutflankAttackBonus.OnEventAboutToTrigger prefix. Target is flanked? {0}. Weapon is melee? {1}", new object[] { evt.TargetIsFlanked, evt.Weapon.Blueprint.IsMelee });
                //}

                //[HarmonyPostfix]
                //public static void Postfix(OutflankAttackBonus __instance, RuleCalculateAttackBonus evt)
                //{
                //    Comment.Log("Entered the OutflankAttackBonus.OnEventAboutToTrigger postfix.");
                //    if (!evt.TargetIsFlanked || !evt.Weapon.Blueprint.IsMelee)
                //    Comment.Log("(!evt.TargetIsFlanked || !evt.Weapon.Blueprint.IsMelee) is true, exit the method.");
                //    Comment.Log("Attempt to call IsSuitable. Result is {0}", new object[] { IsSuitableForOutflank(__instance, evt)});

                //                }
            }


            [HarmonyPatch(typeof(OutflankDamageBonus), nameof(OutflankDamageBonus.OnEventAboutToTrigger))]
            public static class OutflankDamageBonus_OnEventAboutToTrigger_patch
            {
                [HarmonyTranspiler]
                public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log("OutflankDamageBonus transpiler"); 
#endif
                    List<CodeInstruction> _instructions = instructions.ToList();
                    CodeInstruction[] toSearch = new CodeInstruction[]
                    {
                    new CodeInstruction (OpCodes.Ldfld, typeof(RulebookTargetEvent).GetField(nameof(RulebookTargetEvent.Target))),
                    new CodeInstruction (OpCodes.Callvirt, typeof(UnitEntityData).GetProperty(nameof(UnitEntityData.CombatState)).GetMethod),
                    new CodeInstruction (OpCodes.Callvirt, typeof(UnitCombatState).GetProperty(nameof(UnitCombatState.IsFlanked)).GetMethod)
                    };

                    int index = IndexFinder(_instructions, toSearch, true);
                    if (index == -1) { return instructions; };

                    _instructions.RemoveRange(index, toSearch.Count());
                    _instructions.Insert(index, CodeInstruction.Call(typeof(RuleAttackRoll), typeof(RuleAttackRoll).GetProperty(nameof(RuleAttackRoll.TargetIsFlanked)).GetMethod.Name));

                    CodeInstruction[] toSearch2 = new CodeInstruction[]
                    {
                    new CodeInstruction (OpCodes.Callvirt, typeof(UnitEntityData).GetProperty(nameof(UnitEntityData.State)).GetMethod),
                    new CodeInstruction (OpCodes.Ldfld, typeof(UnitCombatState).GetField(nameof(UnitState.Features))),
                    new CodeInstruction (OpCodes.Ldfld, typeof(UnitMechanicFeatures).GetField(nameof(UnitMechanicFeatures.SoloTactics)))
                    };

                    CodeInstruction[] toSearch3 = new CodeInstruction[]
                    {
                    new CodeInstruction (OpCodes.Ldloc_0),
                    new CodeInstruction (OpCodes.Ldarg_0),
                    new CodeInstruction (OpCodes.Ldfld, typeof(OutflankDamageBonus).GetField(nameof(OutflankDamageBonus.IncreasedDamageBonus)))
                    };

                    int index2 = IndexFinder(_instructions, toSearch2, true) - 1;
                    int index3 = IndexFinder(_instructions, toSearch3, true) - 1;
                    if (index2 == -1) { return instructions; };
                    if (index3 == -1) { return instructions; };
                    _instructions.RemoveRange(index2, index3 - index2);

                    _instructions.InsertRange(index2, new CodeInstruction[]
                                                          {   new CodeInstruction (OpCodes.Ldarg_1),
                                                          CodeInstruction.Call(typeof(Outflank_patches), nameof(IsSuitableForOutflank)),
                                                          }
                    );

                    return _instructions;

                }
            }

            [HarmonyPatch(typeof(OutflankProvokeAttack), nameof(OutflankProvokeAttack.OnEventDidTrigger))]
            public static class OutflankProvokeAttack_OnEventDidTrigger_patch
            {
                [HarmonyPrefix]
                public static bool Prefix(RuleAttackRoll evt, OutflankProvokeAttack __instance)
                {
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log("Entered OutflankProvokeAttack"); 
#endif

                    BlueprintUnitFact outflank = __instance.OutflankFact;
                    if (evt.IsFake || !evt.IsCriticalConfirmed || (!evt.TargetIsFlanked && !evt.Weapon.Blueprint.IsMelee))
                    {
#if DEBUG
                        if (Settings.Debug.GetValue())
                            Comment.Log($"evt.IsFake is {evt.IsFake}, !evt.IsCriticalConfirmed is {!evt.IsCriticalConfirmed}, !evt.TargetIsFlanked is {!evt.TargetIsFlanked}, !evt.Weapon.Blueprint.IsMelee is {!evt.Weapon.Blueprint.IsMelee}, total result is {(evt.IsFake || !evt.IsCriticalConfirmed || (!evt.TargetIsFlanked && !evt.Weapon.Blueprint.IsMelee))}"); 
#endif
                        return false;
                    }
                    if (!evt.TryGetCustomData(FlankingUnitsKey, out List<(UnitEntityData, float, bool)> flankers)) return false;

                    foreach ((UnitEntityData unit, float angle, bool amazingOutflankers) in flankers)
                    {
#if DEBUG
                        if (Settings.Debug.GetValue())
                            Comment.Log("Check for " + unit.CharacterName + ". Angle is " + angle + ". Improved Outlflank is " + amazingOutflankers); 
#endif
                        if (angle > GetMinimalFlankingAngle(amazingOutflankers) && (__instance.Owner.State.Features.SoloTactics || unit.Descriptor.HasFact(outflank)))
                        {
#if DEBUG
                            if (Settings.Debug.GetValue())
                                Comment.Log("Provoked AoO"); 
#endif
                            Game.Instance.CombatEngagementController.ForceAttackOfOpportunity(unit, evt.Target, false);
                        }
                    };
                    return false;
                }
            }

#pragma warning disable IDE0060 // Удалите неиспользуемый параметр
            public static bool IsSuitableForOutflank(UnitFactComponentDelegate outflank, RulebookEvent evt)
#pragma warning restore IDE0060 // Удалите неиспользуемый параметр
            {
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log("Entered the IsSuitableForOutflank method. Delegate is {0}, owner is {1}", new object[] { outflank.GetType(), outflank.Owner.CharacterName }); 
#endif
                if (outflank is not OutflankAttackBonus or OutflankDamageBonus)
                {
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log("Delegate is not OutflankAttackBonus or OutflankDamageBonus. Return false."); 
#endif
                    return false;
                }

                RuleAttackRoll ruleAttackRoll = Rulebook.CurrentContext.LastEvent<RuleAttackRoll>();
                if (!ruleAttackRoll.TryGetCustomData(FlankingUnitsKey, out List<(UnitEntityData, float, bool)> flankers))
                {
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log("No flankers!"); 
#endif
                    return false;
                };

                if (ruleAttackRoll.TryGetCustomData(Outflank, out UnitEntityData previousOutflanker))
                {
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log("Outflanker has already been set by another call to the method. Outflanker is {0}", new object[] { previousOutflanker?.CharacterName }); 
#endif
                };

                BlueprintUnitFact fact = null;
                if (outflank is OutflankAttackBonus outflankAttackBonus) fact = outflankAttackBonus.OutflankFact;
                else if (outflank is OutflankDamageBonus outflankDamageBonus) fact = outflankDamageBonus.OutflankFact;
                if (fact is null)
                {
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Warning("Fact is null!!!"); 
#endif
                    return false;
                }
                foreach ((UnitEntityData unit, float angle, bool amazingOutflankers) in flankers)
                {
                    if (angle > GetMinimalFlankingAngle(amazingOutflankers) && unit.HasFact(fact))
                    {
                        if (outflank is OutflankAttackBonus)
                        {
                            ruleAttackRoll.SetCustomData(Outflank, unit);
#if DEBUG
                            if (Settings.Debug.GetValue())
                                Comment.Log("Set {0} into the Outflank custom data", new object[] { unit.CharacterName }); 
#endif
                        };
                        return true;
                    }
                };
                if (outflank.Owner.State.Features.SoloTactics)
                {
                    if (outflank is OutflankAttackBonus)
                        ruleAttackRoll.SetCustomData(Outflank, outflank.Owner);
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log("Outflank owner has Solo Tactics. Set the owner into Outflank custom data."); 
#endif
                    return true;
                }
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log("Return false"); 
#endif
                return false;
            }


            [HarmonyPatch(typeof(AttackLogMessage), nameof(AttackLogMessage.GetData))]
            public static class AttackLogMessage_GetData_patch
            {
                public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log("Entered AttackLogMessage_GetData_patch trasnpiler"); 
#endif
                    List<CodeInstruction> _instructions = instructions.ToList();
                    int index = IndexFinder(_instructions, new CodeInstruction[] { new CodeInstruction(OpCodes.Call, typeof(AttackLogMessage).GetMethod(nameof(AttackLogMessage.AppendAttackBonusBreakdown))) }, true);
                    if (index == -1) { return instructions; };

                    CodeInstruction[] toInsert = new CodeInstruction[]
                    {
                        new CodeInstruction (OpCodes.Ldloc_2),
                        new CodeInstruction (OpCodes.Ldarg_1),
                        CodeInstruction.Call(typeof(AttackLogMessage_GetData_patch), nameof(AttackLogMessage_GetData_patch.AppendFlankingData))
                    };

                    _instructions.InsertRange(index, toInsert);
                    return _instructions;

                }


                public static void AppendFlankingData(StringBuilder sb, RuleAttackRoll rule)
                {
                    UnitEntityData target = rule.Target;
                    if (!rule.TargetIsFlanked && target.CombatState.EngagedBy.Count > 1 && target.Descriptor.State.Features.CannotBeFlanked)
                    {
                        sb.Append(target.CharacterName);
                        sb.Append(" " + m_canNotBeFkaned + "."); //Can not be flanked
                        sb.AppendLine();
                        sb.AppendLine();
                        return;
                    }
                    bool flag2 = rule.TryGetCustomData(FlankingUnitsKey, out List<(UnitEntityData, float, bool)> team);
                    bool flag3 = rule.TryGetCustomData(FlankedKey, out (UnitEntityData unit, float angle, bool amazingOutflankers) flanker);
                    if (!flag2 || !flag3)
                    {
#if DEBUG
                        if (Settings.Debug.GetValue())
                            Comment.Log("AppendFlankingData - Outflank flag2 is {0}, Outflank flag3 is {1}", flag2, flag3); 
#endif
                        return;
                    }
                    bool presentOutflank = rule.TryGetCustomData(Outflank, out UnitEntityData outflanker);
                    bool soloTactics = presentOutflank && outflanker == rule.Initiator;
                    UnitEntityData TrueFlanker = (presentOutflank && !soloTactics) ? outflanker : flanker.unit;
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log("Outflank flag is {0}, True Flanker is {1}", presentOutflank, TrueFlanker.CharacterName); 
#endif
                    (UnitEntityData, float, bool) FlankerInfo = team.Find(f => f.Item1 == TrueFlanker);
                    if (rule.TargetIsFlanked)
                    {
                        sb.Append(m_distracts.ToString().Replace("{trueFlanker}", LogHelper.GetUnitName(TrueFlanker)) + " "); //{trueFlanker} distracts {target}

                        if (!soloTactics)
                        {
                            if (FlankerInfo.Item3) sb.Append(" " + m_improvedOutflank + " "); // with an unbelievable feat of teamwork
                            else if (presentOutflank) sb.Append(" <b>" + m_masterfulFeat + "</b>" + " "); // with a remarkable feat of teamwork                        
                        }
                        sb.Append(m_fromAnotherFlank.ToString().Replace("{angle}", FlankerInfo.Item2.ToString("0.00"))); //while {source} attacks from another flank. The angle between them is {angle}."
                        if (soloTactics)
                        {
                            if (rule.Initiator.State.Features.SoloTactics)
                            {
                                sb.Append(" " + m_soloTactics.ToString().Replace("{outflanker}", LogHelper.GetUnitName(outflanker) )); // {outflanker} demonstrates the mastery of <b>Solo Tactics</b> and outflanks the target <b>alone.</b>
                                sb.AppendLine();
                                sb.AppendLine();
                                return;
                            }
                            else Comment.Error("When {0} was attacking {1}, True FLanker was {2}, while {2} has no Solo Tactics feature!", new object[] { rule.Initiator.CharacterName, rule.Target.CharacterName, outflanker.CharacterName });
                        }
                        sb.AppendLine();
                        sb.AppendLine();
                        return;
                    }
                    else
                    {
                        sb.Append(m_notEnoughToDistract.ToString()
                            .Replace("{flanker}", LogHelper.GetUnitName(flanker.unit))
                            .Replace("{angle}", "<b>" + flanker.angle.ToString("0.00") + "</b>")
                            .Replace("{requirement}", GetMinimalFlankingAngle(AmazingOuflankers(rule.Initiator, flanker.unit)).ToString("0.00"))
                            ); //The angle of attack between {source} and {flanker} is only {angle}. That is less than {requirement} and not enough to distract {target}

                        if (FlankerInfo.Item3)
                            sb.Append(m_outsandingTeamwork); // even with outstandng teamwork
                        sb.Append(". " + m_notFlanking); // This is not a flanking attack
                        sb.AppendLine();
                        sb.AppendLine();

                        return;
                    }

                }

                public static LocalizedString m_canNotBeFkaned = new() { Key = "Flanking_can_not_be_flanked" };
                public static LocalizedString m_distracts = new() { Key = "Flanking_distracts", m_ShouldProcess = true };
                public static LocalizedString m_masterfulFeat = new() { Key = "Flanking_With_a_masterful_feat_of_teamwork" };
                public static LocalizedString m_fromAnotherFlank = new() { Key = "Flanking_from_another_flank", m_ShouldProcess = true };
                //public static LocalizedString m_angleofAttacks = new() { Key = "Flanking_angle_of_attacks" };
                //public static LocalizedString m_and = new() { Key = "Flanking_and" };
                //public static LocalizedString m_isOnly = new() { Key = "Flanking_is_only" };
                //public static LocalizedString m_lessThan = new() { Key = "Flanking_less_than" };
                public static LocalizedString m_notEnoughToDistract = new() { Key = "Flanking_not_enough_to_distract", m_ShouldProcess = true };
                public static LocalizedString m_notFlanking = new() { Key = "Flanking_not_flanking_attack" };
                public static LocalizedString m_soloTactics = new() { Key = "Flanking_soloTactics" };
                public static LocalizedString m_improvedOutflank = new() { Key = "Flanking_improvedOutflank" };
                //public static LocalizedString m_but = new() { Key = "Flanking_but" };
                public static LocalizedString m_outsandingTeamwork = new() { Key = "Flanking_outsandingTeamwork" };
            }


        }
    }

}
