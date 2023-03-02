using Kingmaker;
using Kingmaker.Armies.TacticalCombat;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Way_of_the_shield.NewComponents;
using Way_of_the_shield.NewFeatsAndAbilities;

namespace Way_of_the_shield
{
    public static class Backstab
    {
        public enum ArmorDenialType
        {
            IsTouch = 0,
            IsBrilliantEnergy = 1,
            IsBackstab = 2,
            IsSnipe = 5,
        }

        const float BackstabAngle = 55;
        public static readonly RulebookEvent.CustomDataKey Backstabkey = new("Backstab");
        public static readonly RulebookEvent.CustomDataKey ShieldBonusACDenied = new("ShieldBonusACDenied");
        public static readonly RulebookEvent.CustomDataKey CachedConcealment = new("CachedConcealment");


        //  [HarmonyPatch]
        //  public class RuleAttackRoll_Constructor_patch
        //  {
        //      
        //

        //      [HarmonyTargetMethods]
        //      public static IEnumerable<MethodBase> TargetMethods()
        //      {
        //         IEnumerable<MethodBase> result = typeof(RuleAttackRoll).GetConstructors();
        //#if DEBUG
        //                Comment.Log("RuleAttackRoll constructor patcher obtained" + result.Count().ToString() + "following methods: \n" + result.ToString());
        //#endif
        //                return result;
        //            }

        //      [HarmonyPostfix]
        //     public static void Postfix(UnitEntityData initiator, UnitEntityData target, RuleAttackRoll __instance)
        //           {
        //              Comment.Log("Entered RuleAttackRoll constructor Postifx");
        //              float angle1 = initiator.Orientation;
        //              float angle2 = target.Orientation;
        //              if (angle1 - angle2 > 180) angle2 += 360;
        //              else if (angle2 - angle1 > 180) angle1 += 360;
        //              float angle_difference = Math.Abs(angle2 - angle1);

        //             bool sighted = false;
        //             List<(Feet, UnitConditionExceptions)> blindsight = target.Get<UnitPartConcealment>()?.m_BlindsightRanges;
        //             if (blindsight is not null)
        //             {
        //                 Feet f = 0.Feet();
        //                 foreach (ValueTuple<Feet, UnitConditionExceptions> valueTuple in blindsight)
        //                 {
        //                     if ((valueTuple.Item2 == null || !valueTuple.Item2.IsExceptional(target)) && f < valueTuple.Item1)
        //                    {
        //                        f = valueTuple.Item1;
        //                     }
        //                 }
        //                float num = initiator.View.Corpulence + target.View.Corpulence;
        //                if (initiator.DistanceTo(target) - num <= f.Meters)
        //                {
        //                     sighted = true;
        //                 }
        //             }
        //             bool backstab = angle_difference < BackstabAngle && !sighted;

        //             Comment.Log(initiator.CharacterName + " attacks " + target.CharacterName + ". Angles are " + initiator.Orientation + " and " + target.Orientation + ", difference is " + angle_difference + ". Sighted is " + sighted + "Backstab is " + backstab + ".");

        //            if (backstab) __instance.SetCustomData(key, true);
        //        }


        //  }

        //  [HarmonyPatch(typeof(RuleAttackRoll), nameof(RuleAttackRoll.OnTrigger))]
        //  public static class RuleAttackRoll_OnTrigger_patch
        //  {
        //      [HarmonyTranspiler]
        //      public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        //      {
        //
        //          Comment.Log("Transpiling RuleAttackRoll.OnTrigger");
        //          ConstructorInfo info1 = typeof(RuleCalculateAC).GetConstructor(new Type[] { typeof(UnitEntityData),
        //                                                                                      typeof(UnitEntityData),
        //                                                                                      typeof(AttackType)
        //         });

        //         if (info1 is null) { Comment.Log("could not find the constructor for RuleCalculateAC. Abort Transpiling"); return instructions; }
        //#if DEBUG
        //                else Comment.Log("found the constructor for RuleCalculateAC");
        //#endif

        //                CodeInstruction[] toSearch1 =
        //                {
        //                   new CodeInstruction(OpCodes.Newobj, info1)
        //               };


        //              ConstructorInfo info2 = typeof(RuleCalculateAttackBonus).GetConstructor(new Type[] {typeof(UnitEntityData),
        //                                                                                                  typeof(UnitEntityData),
        //                                                                                                  typeof(RuleCalculateWeaponStats),
        //                                                                                                  typeof(int)
        //              });
        //
        //               if (info2 is null) { Comment.Log("could not find the constructor for RuleCalculateAttackBonus. Abort Transpiling"); ; return instructions; }
        //#if DEBUG
        //                else Comment.Log("found the constructor for RuleCalculateAttackBonus");
        //#endif


        //               CodeInstruction[] toSearch2 =
        //              {
        //                  new CodeInstruction(OpCodes.Newobj,info2)
        //              };


        //             CodeInstruction[] toInsert = { new CodeInstruction(OpCodes.Dup),
        //                                           new CodeInstruction(OpCodes.Ldarg_0),
        //                                            new CodeInstruction(OpCodes.Call, nameof(RuleAttackRoll_OnTrigger_patch.PutBackstabIntoDependantRules))
        //                                          };




        //            int index = -1;
        //            index = Utilities.IndexFinder(instructions, toSearch1);


        //           if (index == -1)
        //           {
        //               Comment.Log("did not find the index for constructing RuleCalculateAC"); Comment.Log("");
        //              return instructions;
        //          };
        //          List<CodeInstruction> result = instructions.ToList();
        //          result.InsertRange(index, toInsert);

        //          index = -1;
        //          index = Utilities.IndexFinder(result, toSearch2);



        //          if (!(index == -1)) result.InsertRange(index, toInsert);
        //           else { Comment.Log("did not find the index for constructing RuleCalculateAttackBonus"); Comment.Log(""); }
        //#if DEBUG
        //             Comment.Log("");
        //             foreach (CodeInstruction i in result) Comment.Log(i.ToString());
        //             Comment.Log("");
        //#endif

        //         if (index == -1) return instructions;
        //         return result;

        //    }

        //      private static void PutBackstabIntoDependantRules(RulebookEvent evt, RuleAttackRoll attack)
        //     {
        //         Comment.Log("Entered PutBackstabIntoDependantRules");
        //         if (attack.TryGetCustomData(key, out bool backstab)) evt.SetCustomData(key, backstab);
        //     }


        //   }

        [HarmonyPatch(typeof(RuleCalculateAttackBonus), nameof(RuleCalculateAttackBonus.OnTrigger))]
        public static class RuleCalculateAttackBonus_OnTrigger_patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
#if DEBUG
                if (Debug.GetValue())
                    Comment.Log("Entered the transpiler RuleCalculateAttackBonus_OnTrigger_patch"); 
#endif
                List<CodeInstruction> _instructions = instructions.ToList();

                int i2 = IndexFinder(
                    instructions,
                    new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Ldc_I4_0),
                        new CodeInstruction(OpCodes.Call, typeof(UnitPartConcealment).GetMethod(nameof(UnitPartConcealment.Calculate))),
                        new CodeInstruction(OpCodes.Ldc_I4_2)
                    },
                    true);
                if (i2 == -1) return instructions;

                _instructions[i2 + 1].operand = typeof(Backstab).GetMethod(nameof(Backstab.CalculateConcealment));
                List<CodeInstruction> toInsert = new() { new CodeInstruction(OpCodes.Ldarg_0), };
                _instructions.InsertRange(i2, toInsert);
                return _instructions;
            }
        }

        [HarmonyPatch(typeof(RuleCalculateAC), nameof(RuleCalculateAC.OnTrigger))]
        public static class RuleCalculateAC_OnTrigger_patch
        {

            

            [HarmonyPrefix]
            public static void Prefix(RuleCalculateAC __instance)
            {
                RuleCheckTargetFlatFooted_OnTrigger_patch.rule = __instance;
            }

            [HarmonyPatch(typeof(RuleCheckTargetFlatFooted), nameof(RuleCheckTargetFlatFooted.OnTrigger))]
            public static class RuleCheckTargetFlatFooted_OnTrigger_patch
            {
                public static RulebookEvent rule;

                [HarmonyPrefix]
                public static bool Prefix(RuleCheckTargetFlatFooted __instance)
                {
                    if (!TacticalCombatHelper.IsActive)
                    {
                        __instance.IsFlatFooted = ((CalculateConcealment(__instance.Target, __instance.Initiator, rule, false) == Concealment.Total && !__instance.IgnoreConcealment)
                            || __instance.ForceFlatFooted
                            || (!__instance.Target.CombatState.CanActInCombat && !__instance.IgnoreVisibility)
                            || __instance.Target.Descriptor.State.IsHelpless
                            || __instance.Target.Descriptor.State.HasCondition(UnitCondition.Stunned)
                            || (!__instance.Target.Memory.Contains(__instance.Initiator) && !__instance.IgnoreVisibility)
                            || __instance.Target.Descriptor.State.HasCondition(UnitCondition.LoseDexterityToAC)
                            || ((__instance.Target.Descriptor.State.HasCondition(UnitCondition.Shaken)
                            || __instance.Target.Descriptor.State.HasCondition(UnitCondition.Frightened)) && __instance.Initiator.Descriptor.State.Features.ShatterDefenses));
                    }
                    else __instance.IsFlatFooted = false;
#if DEBUG
                    if (Debug.GetValue())
                        Comment.Log($"RuleCheckTargetFlatFooted_OnTrigger_Prefix - IsFlatFooted is {__instance.IsFlatFooted}.");
#endif
                    rule = null;
                    return false;
                }

                //[HarmonyTranspiler]
                //public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                //{
                //    Comment.Log("Entered RuleCheckTargetFlatFooted.OnTrigger transpiler");
                //    List<CodeInstruction> _instructions = instructions.ToList();
                //    int index = _instructions.FindIndex(instruction => instruction.Calls(typeof(UnitPartConcealment).GetMethod(nameof(UnitPartConcealment.Calculate))));
                //   if (index == -1) { Comment.Log("Failed to find the index of UnitPartConcealment.Calculate in RuleCheckTargetFlatFooted.OnTrigger"); return instructions; };
                //   _instructions[index].operand = typeof(Backstab).GetMethod(nameof(CalculateConcealment));
                //   _instructions.Insert(index - 1, new CodeInstruction(OpCodes.Ldsfld, typeof(RuleCalculateAC_OnTrigger_patch).GetField(nameof(rule))));


                //   return _instructions;
                //}



                //[HarmonyPostfix]
                //public static void Postfix()
                //{
                //    RuleCheckTargetFlatFooted_OnTrigger_patch.rule = null;
                //}
            }


            public static int CalculateACResult(RuleCalculateAC rule)
            {
                ModifiableValueArmorClass ac = rule.Target.Stats.AC;
                using (ac.GetTemporaryModifiersScope(rule.AllBonuses))
#if DEBUG
                    if (Debug.GetValue())
                    Comment.Log($"RuleCalculateAC_OnTrigger_CalculateACResult - Entered. Target is {rule.Target.CharacterName}. IsTargetFlatFooted? {rule.IsTargetFlatFooted}. Touch? {rule.AttackType.IsTouch()}. Normal AC is {(int)rule.Target.Stats.AC}, Flatfooted AC is {rule.Target.Stats.AC.FlatFooted}, Touch AC is {rule.Target.Stats.AC.Touch}, FlatfootedTouch AC is {rule.Target.Stats.AC.FlatFootedTouch}."); 
#endif
                if (rule.IsTargetFlatFooted)
                {
                    if (rule.AttackType.IsTouch()) return ac.FlatFootedTouch;
                    else if (rule.BrilliantEnergy != null) return ac.FlatFooted - RuleCalculateAC.CalculateArmorAndShieldBonuses(rule.Target);
                    else if (((rule.TryGetCustomData(ShieldBonusACDenied, out bool shieldDenied) && shieldDenied)) || (rule.TryGetCustomData(Backstabkey, out bool backstab) && DenyShieldBonusOnBackstab.GetValue() && backstab)) return ac.FlatFooted - CalculateShieldBonuses(rule.Target);
                    else return ac.FlatFooted;
                }
                else
                {
                    if (rule.AttackType.IsTouch()) return ac.Touch;
                    else if (rule.BrilliantEnergy != null) return ac - RuleCalculateAC.CalculateArmorAndShieldBonuses(rule.Target);
                    else if (rule.Target.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.ShieldDenied || (rule.TryGetCustomData(ShieldBonusACDenied, out bool shieldDenied) && shieldDenied) || (DenyShieldBonusOnBackstab.GetValue() && rule.TryGetCustomData(Backstabkey, out bool backstab) && backstab)) return ac - CalculateShieldBonuses(rule.Target);
                    else return ac;
                }
            }

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
#if DEBUG
                if (Debug.GetValue())
                    Comment.Log("Entered RuleCalculateAC.OnTrigger transpiler"); 
#endif
                List<CodeInstruction> _instructions = instructions.ToList();

                CodeInstruction[] toSearch_1 = new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    CodeInstruction.Call(typeof(RuleCalculateAC), typeof(RuleCalculateAC).GetProperty(nameof(RuleCalculateAC.IsTargetFlatFooted)).GetMethod.Name),
                    new CodeInstruction(OpCodes.Brtrue_S)
                };

                CodeInstruction[] toSearch_2 = new CodeInstruction[]
                {
                    CodeInstruction.Call(typeof(RuleCalculateAC), typeof(RuleCalculateAC).GetProperty(nameof(RuleCalculateAC.Result)).SetMethod.Name),
                    new CodeInstruction(OpCodes.Ldarg_0),
                   // new CodeInstruction(OpCodes.Ldfld, typeof(RuleCalculateAC).GetField(nameof(RuleCalculateAC.Target)))
                };

                int index1 = IndexFinder(instructions, toSearch_1, true);
                int index2 = IndexFinder(instructions, toSearch_2, true);
                if (index1 == -1) { return instructions; };
                if (index2 == -1) { return instructions; };

                _instructions.RemoveRange(index1, index2 - index1);

                _instructions.InsertRange(index1, new CodeInstruction[]
                                                  {
                                                      new CodeInstruction(OpCodes.Ldarg_0),
                                                      CodeInstruction.Call(typeof(RuleCalculateAC_OnTrigger_patch), typeof(RuleCalculateAC_OnTrigger_patch).GetMethod(nameof(RuleCalculateAC_OnTrigger_patch.CalculateACResult)).Name)
                                                  }
                );


                CodeInstruction[] toSearchBrilliant = new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    CodeInstruction.Call(typeof(RuleCalculateAC), typeof(RuleCalculateAC).GetProperty(nameof(RuleCalculateAC.BrilliantEnergy)).GetMethod.Name),
                    new CodeInstruction(OpCodes.Brfalse_S),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld),
                    CodeInstruction.Call(typeof(RuleCalculateAC), nameof(RuleCalculateAC.CalculateArmorAndShieldBonuses)),
                    new CodeInstruction(OpCodes.Neg),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    CodeInstruction.Call(typeof(RuleCalculateAC), typeof(RuleCalculateAC).GetProperty(nameof(RuleCalculateAC.BrilliantEnergy)).GetMethod.Name),
                    new CodeInstruction(OpCodes.Ldc_I4_S, 25),
                };

                int indexBrilliant = IndexFinder(_instructions, toSearchBrilliant, true);
                if (index1 == -1) { return instructions; };

                _instructions[indexBrilliant].MoveLabelsTo(_instructions[indexBrilliant + toSearchBrilliant.Length + 1]);
                _instructions.RemoveRange(indexBrilliant, toSearchBrilliant.Length + 1);

                return _instructions;

            }


        }

        [HarmonyPatch(typeof(AttackLogMessage), nameof(AttackLogMessage.AppendArmorClassBreakdown))]
        public static class AppendArmorClassBreakdown_Patch
        {

            public static string BrilliantEnergy_Name = ResourcesLibrary.TryGetBlueprint<BlueprintWeaponEnchantment>("66e9e299c9002ea4bb65b6f300e43770").m_EnchantName;


            [HarmonyPrefix]
            public static bool Prefix(StringBuilder sb, RuleCalculateAC rule, AttackLogMessage __instance)
            {
#if DEBUG
                if (Debug.GetValue())
                    Comment.Log("AppendArmorClassBreakdown_Patch - Entered "); 
#endif

                if (rule == null)
                {
                    return false;
                }
                bool isTargetFlatFooted = rule.IsTargetFlatFooted;
                bool touch = rule.AttackType.IsTouch();
                bool stab = rule.TryGetCustomData(Backstabkey, out bool backstab) && backstab;
#if DEBUG
                if (Debug.GetValue())
                    Comment.Log("AppendArmorClassBreakdown_Patch - Stab is " + stab); 
#endif
                bool Brilliant = rule.BrilliantEnergy is not null;
                sb.Append("<b>").Append(__instance.ArmorClassBreakdown).Append(": ").Append(rule.Result);
                if (isTargetFlatFooted || touch || stab || Brilliant)
                {
                    sb.Append(" (");
                    if (isTargetFlatFooted)
                    {
                        sb.Append(__instance.Flatfooted);
                        if (touch || stab || Brilliant)
                        {
                            sb.Append(", ");
                        }
                    }
                    if (touch)
                    {
                        sb.Append(__instance.Touch);
                        if (stab)sb.Append(", ");
                        
                    }
                    else if (Brilliant)
                    {
                        sb.Append(BrilliantEnergy_Name);
                        if (stab) sb.Append(", ");
                        
                    }
                    if (stab)
                    {
                        sb.Append(LocalizedTexts.Instance.BonusSources.GetText(BonusTypeExtenstions.GetBonusType(161)));
                    }

                    sb.Append(")");
                }
                sb.Append("</b>\n");
                sb.Append(LocalizedTexts.Instance.BonusSources.ArmorClassBase + ": " + rule.Target.Stats.AC.BaseValue + '\n');
                //StatModifiersBreakdown.AddBonusSources(rule.AllBonuses);
                bool ShieldDenial = (stab && DenyShieldBonusOnBackstab.GetValue()) || rule.Target.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.ShieldDenied || ((rule.TryGetCustomData(ShieldBonusACDenied, out bool shieldDenied) && shieldDenied));
                AddArmorClassModifiers(rule.StatModifiersAtTheMoment, isTargetFlatFooted, touch, Brilliant, ShieldDenial, false);
                sb.AppendModifiersBreakdown("");
                //rule.TryGetCustomData(ShieldBonusACDenied, out bool check);
#if DEBUG
                if (Debug.GetValue())
                    Comment.Log(rule.Initiator.CharacterName + " attacks " + rule.Target.CharacterName + ", ShieldDenial is " + ShieldDenial + "."); 
#endif
                return false;
            }


            public static void AddArmorClassModifiers(IEnumerable<ModifiableValue.Modifier> modifiers, bool flatfooted, bool touch, bool BrilliantEnergy, bool shieldDenied, bool ignoreDexterityBonus = false)
            {
                foreach (ModifiableValue.Modifier modifier in modifiers)
                {
                    if (modifier is not null
                        && modifier.ModValue != 0
                        && (!flatfooted || ModifiableValueArmorClass.FilterAllowedForFlatFooted(modifier))
                        && (!touch || ModifiableValueArmorClass.FilterAllowedForTouch(modifier))
                        && (!BrilliantEnergy || FilterAllowedForBrilliantEnergy(modifier))
                        && (!shieldDenied || !ModifiableValueArmorClass.FilterIsShield(modifier) || (modifier.SourceComponent is not null && modifier.SourceComponent.Contains("BackToBack")))
                        && (!ignoreDexterityBonus || modifier.ModDescriptor != ModifierDescriptor.DexterityBonus))
                    {
                        ModifierDescriptor descriptor = (modifier.ModDescriptor != ModifierDescriptor.None) ? modifier.ModDescriptor : ModifierDescriptor.Other;
                        IUIDataProvider source = modifier.Source;
                        IUIDataProvider bonusSource = source ?? modifier.ItemSource;
                        StatModifiersBreakdown.AddBonus(modifier.ModValue, bonusSource, descriptor);
                    }
                }
            }


        }


        public static readonly Func<ModifiableValue.Modifier, bool> FilterAllowedForBrilliantEnergy = (m =>
        {
            ModifierDescriptor modDescriptor = m.ModDescriptor;
            return !(ModifiableValueArmorClass.FilterIsShield(m) || modDescriptor == ModifierDescriptor.Armor || modDescriptor == ModifierDescriptor.ArmorEnhancement || modDescriptor == ModifierDescriptor.Focus);
        });
        public static int CalculateShieldBonuses(UnitEntityData unit)
        {
            int num = 0;
            foreach (ModifiableValue.Modifier modifier in unit.Stats.AC.Modifiers)
            {
                if (ModifiableValueArmorClass.FilterIsShield(modifier) && !(modifier.SourceComponent is not null && modifier.SourceComponent.Contains("BackToBack"))) 
                        num += modifier.ModValue;
            }
#if DEBUG
            if (Debug.GetValue())
                Comment.Log($"CalculateShieldBonuses - Shield bonus to AC is {num}"); 
#endif
            return num;
        }
        public static Concealment CalculateConcealment(UnitEntityData initiator, UnitEntityData target, RulebookEvent rule = null, bool attack = false)
        {
            if (rule is not null && rule.TryGetCustomData(CachedConcealment, out Concealment cachedConcealment))
            {
#if DEBUG
                if (Debug.GetValue())
                    Comment.Log($"CalculateConcealment - retrieved a cached concealment {cachedConcealment} for the rule {rule.GetType()}"); 
#endif
                return cachedConcealment;
            }
            float angle1 = initiator.Orientation;
            float angle2 = target.Orientation;
            if (angle1 - angle2 > 180) angle2 += 360;
            else if (angle2 - angle1 > 180) angle1 += 360;
            float angle_difference = Math.Abs(angle2 - angle1);
            bool backstab = angle_difference < BackstabAngle;
#if DEBUG
            if (Debug.GetValue())
                Comment.Log($"CalculateConcealment - {target.CharacterName} approaches {initiator.CharacterName}, Orientation angles are {initiator.Orientation} and {target.Orientation}, difference is {angle_difference}. Backstab is {backstab}, { (rule is null ? "rule is null" : "type of rule is " +(rule?.GetType().Name))}.");
#endif
            
            if (DenyShieldBonusOnBackstab.GetValue() || FlatFootedOnBackstab.GetValue()) { rule?.SetCustomData(Backstabkey, backstab); };


            UnitPartConcealment unitPartConcealment = initiator.Get<UnitPartConcealment>();
            UnitPartConcealment unitPartConcealment2 = target.Get<UnitPartConcealment>();
            if (unitPartConcealment != null && unitPartConcealment.IgnoreAll)
            {
                return Concealment.None;
            }
            if ((unitPartConcealment?.m_BlindsightRanges) != null)
            {
                Feet f = 0.Feet();
                foreach (ValueTuple<Feet, UnitConditionExceptions> valueTuple in unitPartConcealment.m_BlindsightRanges)
                {
                    if ((valueTuple.Item2 == null || !valueTuple.Item2.IsExceptional(target)) && f < valueTuple.Item1)
                    {
                        f = valueTuple.Item1;
                    }
                }
                float num = initiator.View.Corpulence + target.View.Corpulence;
                if (initiator.DistanceTo(target) - num <= f.Meters)
                {
                    return Concealment.None;
                }
            }
            bool debugFlag1 =  backstab
                            && ((ConcealmentAttackBonusOnBackstab.GetValue() && rule is RuleCalculateAttackBonus)
                                || FlatFootedOnBackstab.GetValue() && rule is RuleCalculateAC)
                            && !(Rulebook.CurrentContext?.EventStack.OfType<RuleAttackRoll>()?.LastOrDefault() is RuleAttackRoll ruleAttack
                                && ruleAttack.TryGetCustomData(BackToBackImprovedComponent.BTBImprovedKey, out bool BTB_Improved) && BTB_Improved
                                && ImprovedBackToBackDeniedConceament(ruleAttack));

#if DEBUG
            if (Debug.GetValue() && backstab)
            {
                Comment.Log("CalculateConcealment - ConcealmentAttackBonusOnBackstab is " + (!ConcealmentAttackBonusOnBackstab.GetValue() ? "deactivated" : ("activated" + ((rule is RuleCalculateAttackBonus) ? " and the rule is RuleCalculateAttackBonus" : ", but the rule is not RuleCalculateAttackBonus"))
                           + "\n; FlatFootedOnBackstab is " + (!FlatFootedOnBackstab.GetValue() ? "deactivated." : ("activated" + ((rule is RuleCalculateAC) ? " and the rule is RuleCalculateAC." : ", but the rule is not RuleCalculateAC."))))
                           );
                Comment.Log("CalculateConcealment - There is " + (Rulebook.CurrentContext.EventStack.OfType<RuleAttackRoll>()?.LastOrDefault() is RuleAttackRoll rAttack ? ("a RuleAttackRoll on stack" + (rAttack.TryGetCustomData(BackToBackImprovedComponent.BTBImprovedKey, out bool debugBTB_Improved) && debugBTB_Improved
                                && ImprovedBackToBackDeniedConceament(rAttack) ? " with a custom BTBImproved data." : ", but it doesn't have a custom BTBImproved data.")) : "no RuleAttackRoll on stack."
                           )+ $" End result is {debugFlag1}."
                           ); 

            }
#endif
            if (debugFlag1)
            {
                if (rule is not null) rule.SetCustomData(CachedConcealment, Concealment.Total);
                return Concealment.Total;
            }

            Concealment concealment = (unitPartConcealment2 != null && unitPartConcealment2.IsConcealedFor(initiator)) ? Concealment.Total : Concealment.None;
            if (target.Descriptor.State.HasCondition(UnitCondition.Invisible) && (!initiator.Descriptor.State.HasCondition(UnitCondition.SeeInvisibility) || !initiator.Descriptor.State.GetConditionExceptions(UnitCondition.SeeInvisibility).Any((UnitConditionExceptions _exception) => _exception == null || !_exception.IsExceptional(target))))
            {
                if (rule is not null) rule.SetCustomData(CachedConcealment, Concealment.Total);
                concealment = Concealment.Total;
            }
            if (concealment < Concealment.Total && (unitPartConcealment2?.m_Concealments) != null)
            {
                foreach (UnitPartConcealment.ConcealmentEntry concealmentEntry in unitPartConcealment2.m_Concealments)
                {
                    if (!concealmentEntry.OnlyForAttacks || attack)
                    {
                        if (concealmentEntry.DistanceGreater > 0.Feet())
                        {
                            float num2 = initiator.DistanceTo(target);
                            float num3 = initiator.View.Corpulence + target.View.Corpulence;
                            if (num2 <= concealmentEntry.DistanceGreater.Meters + num3)
                            {
                                continue;
                            }
                        }
                        if (concealmentEntry.RangeType != null)
                        {
                            RuleAttackRoll ruleAttackRoll =  Rulebook.CurrentContext.LastEvent<RuleAttackRoll>();
                            ItemEntityWeapon itemEntityWeapon = (ruleAttackRoll != null) ? ruleAttackRoll.Weapon : initiator.GetFirstWeapon();
                            if (itemEntityWeapon == null || !concealmentEntry.RangeType.Value.IsSuitableWeapon(itemEntityWeapon))
                            {
                                continue;
                            }
                        }
                        if ((concealmentEntry.Descriptor == ConcealmentDescriptor.Blur || concealmentEntry.Descriptor == ConcealmentDescriptor.Displacement) && initiator.Descriptor.State.HasCondition(UnitCondition.TrueSeeing))
                        {
                            IEnumerable<UnitConditionExceptions> source = initiator.Descriptor.State.GetConditionExceptions(UnitCondition.TrueSeeing).EmptyIfNull<UnitConditionExceptions>();
                            if (source.Any(_exception =>
                            {
                                UnitConditionExceptionsTargetHasFacts unitConditionExceptionsTargetHasFacts = _exception as UnitConditionExceptionsTargetHasFacts;
                                return _exception as UnitConditionExceptionsTargetHasFacts == null || !unitConditionExceptionsTargetHasFacts.IsExceptional(target);
                            }))
                            {
                                continue;
                            }
                        }
                        concealment = UnitPartConcealment.Max(concealment, concealmentEntry.Concealment);
                    }
                }
            }
            if (unitPartConcealment2 != null && unitPartConcealment2.Disable)
            {
                concealment = Concealment.None;
            }
            if (initiator.Descriptor.State.HasCondition(UnitCondition.PartialConcealmentOnAttacks))
            {
                concealment = Concealment.Partial;
            }
            if (initiator.Descriptor.State.HasCondition(UnitCondition.Blindness))
            {
                concealment = Concealment.Total;
            }
            if (concealment == Concealment.None && Game.Instance.Player.Weather.ActualWeather >= BlueprintRoot.Instance.WeatherSettings.ConcealmentBeginsOn)
            {
                RuleAttackRoll ruleAttackRoll2 = Rulebook.CurrentContext.LastEvent<RuleAttackRoll>();
                ItemEntityWeapon itemEntityWeapon2 = (ruleAttackRoll2 != null) ? ruleAttackRoll2.Weapon : initiator.GetFirstWeapon();
                if (itemEntityWeapon2 != null && WeaponRangeType.Ranged.IsSuitableWeapon(itemEntityWeapon2))
                {
                    concealment = Concealment.Partial;
                }
            }
            if (unitPartConcealment != null && unitPartConcealment.IgnorePartial && concealment == Concealment.Partial)
            {
                concealment = Concealment.None;
            }
            if (unitPartConcealment != null && unitPartConcealment.TreatTotalAsPartial && concealment == Concealment.Total)
            {
                concealment = Concealment.Partial;
            }

            if (rule is not null) rule.SetCustomData(CachedConcealment, concealment);
            return concealment;
        }

        static bool ImprovedBackToBackDeniedConceament(RuleAttackRoll evt)
        {
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log("BackToBackImproved - checking for backers."); 
#endif
            List<UnitEntityData> Backers;
            if (EnableSoftCover)evt.TryGetCustomData(BackToBackNew.BackToBackUnitsKey, out Backers);
            else Rulebook.Trigger<SoftCover.RuleSoftCover>(new(evt.Initiator, evt.Target)).TryGetCustomData(BackToBackNew.BackToBackUnitsKey, out Backers);
            bool flag2 = Backers.Count > 0 && Backers.Any(backer => backer.HasFact(BackToBackImproved.Feature));
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log($"BackToBackImproved - There are {Backers.Count} backers. {Backers.Where(backer => backer.HasFact(BackToBackImproved.Feature)).Count()} among them have {BackToBackImproved.Feature.m_DisplayName} feature. Return is {flag2}"); 
#endif
            return flag2;
        }

    }
}
