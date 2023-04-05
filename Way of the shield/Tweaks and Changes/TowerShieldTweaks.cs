#undef DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Kingmaker.AI;
using Kingmaker.AI.Blueprints;
using Kingmaker.Armies.TacticalCombat;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.View;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.FogOfWar;
using UnityEngine;
using static Way_of_the_shield.Main;
using static Way_of_the_shield.Utilities;

namespace Way_of_the_shield
{
    public class TowerShieldTweaks
    {
        //[HarmonyPatch]
        //public static class Check
        //{
        //[HarmonyPatch(typeof(ActivatableAbility), nameof(ActivatableAbility.Stop))]
        //[HarmonyPrefix]
        //public static void Prefix(ActivatableAbility __instance)
        //{                
        //Comment.Log("Entered the check for activatable ability. Ability is " + __instance.Blueprint.m_DisplayName);
        //Comment.Log("this.m_WasInCombat is " + __instance.m_WasInCombat);
        //Comment.Log("base.Owner.Unit.IsInCombat " + __instance.Owner.Unit.IsInCombat);
        //Comment.Log("this.m_ShouldBeDeactivatedInNextRound is " + __instance.m_ShouldBeDeactivatedInNextRound);
        //Comment.Log("!this.IsOn is " + !__instance.IsOn);
        //Comment.Log("!this.IsAvailableByRestrictions is " + !__instance.IsAvailableByRestrictions);
        //Comment.Log("base.Blueprint.DeactivateIfCombatEnded is " + __instance.Blueprint.DeactivateIfCombatEnded);
        //Comment.Log("!base.Owner.Unit.IsInCombat is " + !__instance.Owner.Unit.IsInCombat);
        //Comment.Log("base.Blueprint.ActivateOnCombatStarts is " + __instance.Blueprint.ActivateOnCombatStarts);
        //Comment.Log("this.m_WasInCombat is " + __instance.m_WasInCombat);
        //bool ShouldDeactivate = (
        //    __instance.m_ShouldBeDeactivatedInNextRound
        //        || !__instance.IsOn
        //        || !__instance.IsAvailableByRestrictions
        //        || (__instance.Blueprint.DeactivateIfCombatEnded
        //            && !__instance.Owner.Unit.IsInCombat
        //            && (__instance.Blueprint.ActivateOnCombatStarts
        //            || (__instance.m_WasInCombat || __instance.Owner.Unit.IsInCombat))));
        //Comment.Log("Should Deactivate? " + ShouldDeactivate + (ShouldDeactivate ? ". CAUGHT YOU!" : ""));

        //StackTrace trace = new();
        //StackFrame[] frames = trace.GetFrames();
        //for (int i = 1; i < 10; i++)
        //{
        //    StackFrame frame = frames[i];
        //    Comment.Log("Method is {0}, line is {1}", new object[] { frame.GetMethod().Name, frame.GetFileLineNumber() });
        //}
        //}
        //[HarmonyPatch(typeof(ActivatableAbility), nameof(ActivatableAbility.SetIsOn))]
        //[HarmonyPrefix]
        //public static void Prefix2(ActivatableAbility __instance, bool value)
        //{
        //    Comment.Log("Entered checker2 for Activatable ability SetIsOn. Name is {0}, value is {1}", new object[] { __instance.Blueprint.m_DisplayName, value });
        //    if (__instance.Blueprint.name == "TowerShieldDefenseAbility")
        //   {
        //        StackTrace trace = new();
        //        StackFrame[] frames = trace.GetFrames();
        //        for (int i = 1; i < 10; i++)
        //        {
        //            StackFrame frame = frames[i];
        //            Comment.Log("Method is {0}, line is {1}", new object[] { frame.GetMethod().Name, frame.GetFileLineNumber() });
        //       }
        //    }
        // }
        //}


        [HarmonyPatch]
        public static class MovementPatches
        {
            public static Vector3? TargetOrientation = null;

            [HarmonyPatch]
            public static class ScoresPatches
            {

                [HarmonyTargetMethods]
                public static IEnumerable<MethodBase> ScoreMethods()
                {
                    yield return typeof(AiAction).GetMethod(nameof(AiAction.ScoreTarget));
                    yield return typeof(AiAction).GetMethod(nameof(AiAction.ScorePath));
                }

                [HarmonyPrefix]
                public static void ScorePrefixes(DecisionContext context, ref bool __state)
                {
                    if (TacticalCombatHelper.IsActive) return;
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log("Entered Scoring prefix for Tower Shield movement patches"); 
#endif
                    if (TargetOrientation != null)
                    {
#if DEBUG
                        if (Settings.Debug.GetValue())
                            Comment.Log("TargetOrientation has already been set by upper-level method."); 
#endif
                        return;
                    }
                    UnitEntityData target = context.Target?.Unit;
                    if (!(target?.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.TowerShieldBrace))
                    {
#if DEBUG

                        if (Settings.Debug.GetValue())
                            Comment.Log("No fancy Tower Shield feature on the target, we don't care about this action. Move on"); 
#endif
                        return;
                    };
                    UnitEntityData actor = context.Unit;
                    AiAction action = context.Action;
                    BlueprintAiAction bp = action.Blueprint as BlueprintAiAction;
                    //Comment.Log("{0} acts on {1} with {2}", new object[] { actor.CharacterName, target.CharacterName, action.Blueprint.name });
                    if (actor == target) return;
                    if (bp is BlueprintAiAttack)
                    {
                        AttackType attackType = actor.GetFirstWeapon().Blueprint.AttackType;
                        if (attackType == AttackType.Touch || attackType == AttackType.RangedTouch)
                        {
                            //Comment.Log("Main weapon is a touch weapon and will ignore the tower shield");
                            return;
                        }
                    }
                    else if (bp is BlueprintAiCastSpell)
                    {
                        BlueprintAbility ability = context.Ability.Blueprint;
                        AbilityDeliverProjectile deliverProjectile = ability.GetComponent<AbilityDeliverProjectile>();
                        if (ability != null
                            && !ability.GetComponent<AbilityTargetsAround>()
                            && !ability.GetComponent<AbilityDeliverChain>()
                            && !ability.GetComponent<AbilityDeliverTouch>()
                            && (deliverProjectile == null || (deliverProjectile.Type == AbilityProjectileType.Simple && !deliverProjectile.NeedAttackRoll))
                            && !ability.GetComponent<AbilityDeliverAttackWithWeapon>() && ability.AoERadius == 0.Feet())
                        {
                            //Comment.Log("Ability is a single-target ability and will target the tower shield itself.");
                        }
                    }
                    else
                    {
                        //Comment.Log("Action is neither an attack or ability, so it is unlikely to be subject to Tower Shield rules? Question mark?..");
                        return;
                    };
                    TargetOrientation = target.OrientationDirection;
                    __state = true;
                    //Comment.Log("Set up target orientation.");
                }
                [HarmonyPostfix]
                public static void ScorePostfixes(ref bool __state)
                {
                    if (__state)
                    {
                        TargetOrientation = null;
                        __state = false;
                        //Comment.Log("Set off TargetOrientation");
                    };
                }
            }

            [HarmonyPatch]
            public static class CommandPatches
            {

                [HarmonyTargetMethods]
                public static IEnumerable<MethodBase> ScoreMethods()
                {
                    yield return typeof(UnitCommand).GetMethod(nameof(UnitCommand.TickApproaching));
                    //yield return typeof(UnitCommand).GetProperty(nameof(UnitCommand.IsUnitEnoughClose)).GetMethod;
                    yield return typeof(UnitCommand).GetMethod(nameof(UnitCommand.IsUnitCloseEnough), BindingFlags.Instance | BindingFlags.Public);
                }

                [HarmonyPrefix]
                public static void UnitCommand_TickApproaching_Prefix(UnitCommand __instance, ref bool __state)
                {
                    if (TacticalCombatHelper.IsActive) return;
                    // Comment.Log("Entered Command prefix for Tower Shield movement patches");
                    if (TargetOrientation != null)
                    {
                        //Comment.Log("TargetOrientation has already been set by upper-level method.");
                        return;
                    }
                    UnitEntityData target = __instance.TargetUnit;
                    if (!(target?.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.TowerShieldBrace))
                    {
                        //Comment.Log("No fancy Tower Shield feature on the target, we don't care about this action. Move on");
                        return;
                    }
                    UnitEntityData executor = __instance.Executor;


                    //Comment.Log("{0} acts on {1} with {2}", new object[] { executor.CharacterName, target.CharacterName, __instance.GetType().Name });
                    if (executor == target) return;
                    if (__instance is UnitAttack)
                    {
                        AttackType attackType = executor.GetFirstWeapon().Blueprint.AttackType;
                        if (attackType == AttackType.Touch || attackType == AttackType.RangedTouch)
                        {
                            //Comment.Log("Main weapon is a touch weapon and will ignore the tower shield");
                            return;
                        }
                    }
                    else if (__instance is UnitUseAbility)
                    {
                        BlueprintAbility ability = (__instance as UnitUseAbility).Ability.Blueprint;
                        AbilityDeliverProjectile deliverProjectile = ability.GetComponent<AbilityDeliverProjectile>();
                        if (ability != null
                            && !ability.GetComponent<AbilityTargetsAround>()
                            && !ability.GetComponent<AbilityDeliverChain>()
                            && !ability.GetComponent<AbilityDeliverTouch>()
                            && (deliverProjectile == null || (deliverProjectile.Type == AbilityProjectileType.Simple && !deliverProjectile.NeedAttackRoll))
                            && !ability.GetComponent<AbilityDeliverAttackWithWeapon>() && ability.AoERadius == 0.Feet())
                        {
                            //Comment.Log("Ability is a single-target ability and will target the tower shield itself.");
                        }
                    }
                    else
                    {
                        //Comment.Log("Action is neither an attack or ability, so it is unlikely to be subject to Tower Shield rules? Question mark?..");
                        return;
                    };
                    TargetOrientation = target.OrientationDirection;
                    __state = true;
                    //Comment.Log("Set up target orientation.");
                }

                [HarmonyPostfix]
                public static void UnitCommand_TickApproaching_Postfix(ref bool __state)
                {
                    if (__state)
                    {
                        TargetOrientation = null;
                        __state = false;
                        //Comment.Log("Set off target orientation");
                    }
                }

            }

            [HarmonyPatch(typeof(LineOfSightGeometry), nameof(LineOfSightGeometry.HasObstacle), new Type[] { typeof(Vector3), typeof(Vector3), typeof(float) })]
            [HarmonyPostfix]
            public static void LineOfSightGeometry_HasObstacle_Postfix(ref bool __result, Vector3 from, Vector3 to)
            {
                //Comment.Log("Entered TargetInfo_InLos_Postfix.");
                if (__result) return;
                if (TargetOrientation is null)
                {
                    //Comment.Log("Target Orientation is null");
                    return;
                };

                float angle = Vector3.Angle(from - to, TargetOrientation.Value);
                if (angle < 65) __result = true;
                //Comment.Log("Angle is " + angle + ", has obstacle is " + __result);
            }

            [HarmonyPatch(typeof(UnitMovementAgent), nameof(UnitMovementAgent.UpdateUnitAvoidance))]
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> UnitMovementAgent_UpdateUnitAvoidance_Transpiler(IEnumerable<CodeInstruction> instructions)
            {
#if DEBUG
                Comment.Log("Entered UnitMovementAgent_UpdateUnitAvoidance transpiler"); 
#endif
                List<CodeInstruction> _instructions = instructions.ToList();

                CodeInstruction[] toSearch = new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, typeof(UnitMovementAgentBase).GetProperty(nameof(UnitMovementAgentBase.Corpulence)).GetMethod),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Callvirt, typeof(UnitMovementAgentBase).GetProperty(nameof(UnitMovementAgentBase.Corpulence)).GetMethod),
                };

                int index = IndexFinder(_instructions, toSearch);
                if (index == -1)
                {
                    return instructions;
                };

                CodeInstruction[] toInsert = new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_3),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, typeof(UnitMovementAgentBase).GetProperty(nameof(UnitMovementAgentBase.Unit)).GetMethod),
                    CodeInstruction.Call(typeof(MovementPatches), nameof(MovementPatches.CorpulenceChange))
                };

                _instructions.InsertRange(index, toInsert);
                return _instructions;
            }

            public static float CorpulenceChange(float originalCorpulence, UnitEntityData mover, UnitEntityView shielderView)
            {
                UnitEntityData shielder = shielderView?.Data;
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log($"Entered CorpulenceChange patch. Shielder is {shielder?.CharacterName}, mover is {mover?.CharacterName}, original corpulence is {originalCorpulence}");
#endif
                if (shielder is null) return originalCorpulence;
                if (!shielder.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.TowerShieldBrace) return originalCorpulence;
                float angle = Vector3.Angle(shielder.OrientationDirection, mover.Position - shielder.Position);
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log($"Angle is {angle}"); 
#endif
                if (angle > 65) return originalCorpulence;
                float sine = Convert.ToSingle(Math.Sin(angle));
                float offset = Convert.ToSingle(1.1);
                float changedCorpulence = originalCorpulence * offset / sine;
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log($"Original corpulence is {originalCorpulence}, angle is {angle}, sine is {sine}, changed corpuence is {changedCorpulence}"); 
#endif
                return changedCorpulence;
            }
        }

        [HarmonyPatch]
        public static class TowerShieldBlueprintFix
        {
            [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
            [HarmonyPostfix]
            public static void Postfix()
            {
#if DEBUG
                Comment.Log("Entered TowerShieldBlueprintFix cache postfix"); 
#endif
                if (!RetrieveBlueprint("3dccdf27a8209af478ac71cded18a271", out BlueprintBuff stalwart, "DefensiveStanceBuff")) return;
                if (!RetrieveBlueprint("f6b1f4378dd64044db145a1c2afa589f", out BlueprintArmorEnchantment TowerShieldEnchantment, "TowerShieldEnchantment")) return;
                TowerShieldEnchantment.m_Description = new LocalizedString() { Key = "TowerShieldEnchantment_Description" };
                Sprite TSDefenseIcon = LoadIcon("Tower");
                #region Create tower shield block buff
#if DEBUG
                Comment.Log("Begin creating the Tower Shield Defense Buff blueprint."); 
#endif
                BlueprintBuff TowerShieldDefenseBuff = new()
                {
                    AssetGuid = new BlueprintGuid(new Guid("a55e02b905324b00a86a40e776515646")),
                    name = "TowerShieldDefenseBuff",
                    m_DisplayName = new LocalizedString() { Key = "TowerShieldDefense_DisplayName" },
                    m_Description = new LocalizedString() { Key = "TowerShieldDefense_Description" },
                    m_DescriptionShort = new LocalizedString() { Key = "TowerShieldDefense_Description" },
                    m_Icon = TSDefenseIcon,
                    FxOnStart = stalwart ? stalwart.FxOnStart : new Kingmaker.ResourceLinks.PrefabLink(),
                    FxOnRemove = new()
                };
                TowerShieldDefenseBuff.AddComponent(new AddMechanicsFeature() { m_Feature = MechanicsFeatureExtension.TowerShieldDefense });
                TowerShieldDefenseBuff.AddComponent(new AddMechanicsFeature() { m_Feature = AddMechanicsFeature.MechanicsFeatureType.RotationForbidden });
                TowerShieldDefenseBuff.AddComponent(new AddCondition() { Condition = UnitCondition.CantMove });
                TowerShieldDefenseBuff.AddComponent(new AddMechanicsFeature() { m_Feature = MechanicsFeatureExtension.ShieldDenied });
                TowerShieldDefenseBuff.AddToCache();
                #endregion
                #region Create Tower shield block ability
#if DEBUG
                Comment.Log("Begin creating the Tower Shield Defense Ability blueprint."); 
#endif
                BlueprintActivatableAbility TowerShieldDefenseAbility = new()
                {
                    AssetGuid = new BlueprintGuid(new Guid("e5c223f4bdda466189972bb889f4c3af")),
                    name = "TowerShieldDefenseAbility",
                    m_Icon = TSDefenseIcon,
                    m_DisplayName = new LocalizedString() { Key = "TowerShieldDefense_DisplayName" },
                    m_Description = new LocalizedString() { Key = "TowerShieldDefense_Description" },
                    m_DescriptionShort = new LocalizedString() { Key = "TowerShieldDefense_Description" },
                    Group = ActivatableAbilityGroup.None,
                    IsOnByDefault = false,
                    DeactivateIfCombatEnded = true,
                    DeactivateIfOwnerDisabled = true,
                    DeactivateIfOwnerUnconscious = true,
                    ActivationType = AbilityActivationType.WithUnitCommand,
                    m_ActivateWithUnitCommand = UnitCommand.CommandType.Standard,
                    m_Buff = TowerShieldDefenseBuff.ToReference<BlueprintBuffReference>()
                };

                TowerShieldDefenseAbility.AddComponent(new ActivatableAbilityUnitCommand()
                {
                    Type = UnitCommand.CommandType.Standard
                });

                TowerShieldDefenseAbility.AddToCache();
                #endregion
                #region Create Tower Shield block feature
#if DEBUG
                Comment.Log("Begin creating the Tower Shield Defense feature blueprint."); 
#endif
                BlueprintFeature TowerShieldDefenseFeature = new()
                {
                    AssetGuid = new BlueprintGuid(new Guid("a83b1f033ddf49e5b083c25bfd6e245b")),
                    name = "WayOfTheShield_TowerShieldDefenseFeature",
                    HideInCharacterSheetAndLevelUp = true,
                    HideInUI = true,
                    Ranks = 1
                };
                TowerShieldDefenseFeature.AddComponent(new AddFacts() { m_Facts = new BlueprintUnitFactReference[] { TowerShieldDefenseAbility.ToReference<BlueprintUnitFactReference>() } });
                TowerShieldDefenseFeature.AddToCache();
                #endregion
                TowerShieldEnchantment.AddComponent(new AddUnitFeatureEquipment()
                {
                    m_Feature = TowerShieldDefenseFeature.ToReference<BlueprintFeatureReference>()
                });
            }
        }


    }
}
