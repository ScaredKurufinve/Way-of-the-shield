using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Dungeon.Actions;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Way_of_the_shield.NewComponents
{
    public class BackToBackNew : UnitFactComponentDelegate, ITargetRulebookHandler<SoftCover.RuleSoftCover>, ITargetRulebookHandler<RuleCalculateAC>
    {
        public static readonly RulebookEvent.CustomDataKey BackToBackUnitsKey = new("BackToBackUnitsKey");
        public static BlueprintFeatureReference m_BackToBackFact = new() { deserializedGuid = BlueprintGuid.Parse("c920f2cd2244d284aa69a146aeefcb2c") };
        BlueprintFeature BackToBackFact { get { return m_BackToBackFact; } }


        public void OnEventAboutToTrigger(SoftCover.RuleSoftCover evt)
        {
        }
        public void OnEventDidTrigger(SoftCover.RuleSoftCover evt)
        {
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log($"I'm inside BackToBackNew.OnEventDidTrigger<RuleSoftCover>. Attacker is {evt.Initiator.CharacterName} and defender is {evt.Target.CharacterName}."); 
#endif
            if (evt.Fake) return;
            bool solo = Owner?.State.Features.SoloTactics;
            Vector2 targetPos = Owner.Position;
            List<UnitEntityData> Backers = new();
            foreach ((UnitEntityData unit, int _, int _) in evt.Result)
            {
                #region Debug
#if DEBUG
                bool ally = unit.IsAlly(Owner);
                bool hasFact = unit.HasFact(BackToBackFact);
                float distance = ((Vector2)unit.Position - targetPos).magnitude - Owner.Corpulence - unit.Corpulence;
                bool allowed = AllowedForBacking(Owner, unit);
                if (Settings.Debug.GetValue())
                {
                    Comment.Log($"Checking for unit {unit.CharacterName}. Unit is Ally of {Owner.CharacterName}? {ally}. Solo? {solo}. Has BackToBack? {hasFact}. Distance is {distance}. Allowed for backing? {allowed}");
                }

                if (ally && (solo || hasFact) && (distance < 2) && allowed) 
                {
                    Backers.Add(unit);
                    if (Settings.Debug.GetValue())
                        Comment.Log($"Added {unit?.CharacterName} to the list of backers.");
                } 
#endif
                #endregion


                #region non-Debug
#if !DEBUG
                if (unit.IsAlly(Owner)
            && (solo || unit.HasFact(BackToBackFact))
            && (((Vector2)unit.Position - targetPos).magnitude - Owner.Corpulence - unit.Corpulence > 2)
            && AllowedForBacking(Owner, unit)
            )
                {
                    Backers.Add(unit);
                } 
#endif
                #endregion
            }

            RuleAttackRoll attack = Rulebook.CurrentContext.EventStack?.OfType<RuleAttackRoll>().Where(r => r.Initiator == evt.Initiator && r.Target == evt.Target)?.LastOrDefault();
            if (attack != null) 
            {
                attack.SetCustomData(BackToBackUnitsKey, Backers);                
            }
#if DEBUG
            else if (Settings.Debug.GetValue())
            {
                Comment.Warning($"Failed to find an attack rule on even stack to save the backer list. (There's currently {Rulebook.CurrentContext.EventStack?.OfType<RuleAttackRoll>().Count()} attacks on the stack)");
            } 
#endif
            evt.Result = evt.Result.Select(x=> { if (!Backers.Contains(x.obstacle)) return (x.obstacle, x.sizeDifference, x.bonus); else return (x.obstacle, x.sizeDifference, (x.bonus * 3) >> 1); }).ToList();
        }
        public void OnEventAboutToTrigger(RuleCalculateAC evt)
        {
            
        }
        public void OnEventDidTrigger(RuleCalculateAC evt)
        {
#if DEBUG

            if (Settings.Debug.GetValue())
                Comment.Log($"Inside BackToBackNew.OnEventDidTrigger<RuleCalculateAC>. Attacker is {evt.Initiator.CharacterName} and defender is {evt.Target.CharacterName}.");
#endif
            RuleAttackRoll attack = Rulebook.CurrentContext.EventStack?.OfType<RuleAttackRoll>().Where(r => r.Initiator == evt.Initiator && r.Target == evt.Target)?.LastOrDefault();
#if DEBUG

            if (Settings.Debug.GetValue())
                Comment.Log($"Attack is null? {attack is null}. " + (attack is null ? $"There are totally {Rulebook.CurrentContext.EventStack?.OfType<RuleAttackRoll>().Count()} attacks on the stack" : "")); 
#endif
            if (attack is null) return;
            List <UnitEntityData> Backers;
            if (EnableSoftCover.GetValue())
            {
                if (!attack.TryGetCustomData(BackToBackUnitsKey, out Backers))
                {
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log("Soft cover is enabled, but no backers found. Returning"); 
#endif
                    return;
                }
            }
            else
            {
                Rulebook.Trigger<SoftCover.RuleSoftCover>(new(evt.Initiator, evt.Target)).TryGetCustomData(BackToBackUnitsKey, out Backers);
            }
#if DEBUG

            if (Settings.Debug.GetValue())
                Comment.Log((Backers is null ? "Backers list is null." : $"There are {Backers.Count} backers.")); 
#endif
            if (Backers?.Count < 1) return;
            if (!DenyShieldBonusOnBackstab.GetValue())
            {
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log($"Shield denying is disabled. Target is flanked? {attack.TargetIsFlanked}"); 
#endif
                if (attack.TargetIsFlanked) evt.AddModifier(2, Fact, ModifierDescriptor.Circumstance);
                return;
            }
            if (!(evt.TryGetCustomData(Backstab.Backstabkey, out bool backstab) && backstab)) return;
#if DEBUG

            if (Settings.Debug.GetValue())
                Comment.Log($"Shield denying is enabled, backstab is {backstab}. Begin searching for a True Backer.");
#endif
            if (evt.AttackType.IsTouch() || evt.BrilliantEnergy is not null)
            {
#if DEBUG

                if (Settings.Debug.GetValue())
                    Comment.Log("Attack is touch or Brilliant energy, leaving the BackToBack shield bonus adder.");
#endif
                return;
            }
            int ShieldBonus = 0;
            int EnhancementBonus = 0;
            UnitEntityData TrueBacker = null;
            foreach (var backer in Backers)
            {
#if DEBUG

                if (Settings.Debug.GetValue())
                    Comment.Log($"Potential backer is {backer.CharacterName}. Shield denied? {backer.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.ShieldDenied.Value is not null and true}. "); 
#endif
                if (backer.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.ShieldDenied || !backer.IsAbleToAct()) continue;
                ItemEntityShield shield = backer.Body.SecondaryHand.MaybeShield;
#if DEBUG

                if (Settings.Debug.GetValue())
                    Comment.Log($"Shield is null? {shield is null}. s and e are {shield.ArmorComponent.Blueprint.ArmorBonus} and {GameHelper.GetItemEnhancementBonus(shield)}"); 
#endif
                if (shield is null) continue;
                int s = shield.ArmorComponent.Blueprint.ArmorBonus;
                int e = GameHelper.GetItemEnhancementBonus(shield);
#if DEBUG

                if (Settings.Debug.GetValue())
                    Comment.Log($"Shield is  {shield.Blueprint.m_DisplayNameText}. s and e are {shield.ArmorComponent.Blueprint.ArmorBonus} and {GameHelper.GetItemEnhancementBonus(shield)}. Sum is {s + e}. This is " + (s + e > ShieldBonus + EnhancementBonus ? "greater" : "not greater" + $" than the current sum {ShieldBonus + EnhancementBonus}.")); 
#endif
                if (s + e > ShieldBonus + EnhancementBonus)
                {
                    ShieldBonus = s;
                    EnhancementBonus = e;
                    TrueBacker = backer;
                }
            }
#if DEBUG

            if (Settings.Debug.GetValue())
                Comment.Log($"True Backer is {TrueBacker?.CharacterName}."); 
#endif
            if (TrueBacker is null) return;

            evt.AddModifier(ShieldBonus, Fact, ModifierDescriptor.Shield);
            evt.AddModifier(EnhancementBonus, Fact, ModifierDescriptor.Shield);
            evt.Result += ShieldBonus + EnhancementBonus;
        }


        static bool AllowedForBacking(UnitEntityData target, UnitEntityData backer)
        {
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log($"Backing checking for allowed backers.");
#endif
            Vector2 directionToBacker = (backer.Position - target.Position).To2D();
            float angle = Vector2.SignedAngle(directionToBacker, - target.OrientationDirection.To2D());
            if (Math.Abs(angle) > 75) return false;
            float angle2 = Vector2.SignedAngle(directionToBacker, backer.OrientationDirection.To2D());
            float result = angle + angle2;
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log($"Angle 1 is {angle}, angle2 is {angle2}, result is {result}."); 
#endif
            return Math.Abs(result) < 75;
        }
        
    }
}
