using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.UnitLogic;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using UnityEngine;
using Kingmaker.Designers;
using Owlcat.Runtime.Core.Utils;
using Kingmaker.EntitySystem.Stats;

namespace Way_of_the_shield.NewComponents
{

    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [AllowMultipleComponents]
    [TypeId("3ACCFF25E47344EB9A41426207E4E442")]
    public class ShieldWallNew : UnitFactComponentDelegate, ITargetRulebookHandler<RuleCalculateAC>
    {
        public BlueprintUnitFact ShieldWallFact
        {
            get
            {
                return m_ShieldWallFact?.Get();
            }
        }



        public void OnEventAboutToTrigger(RuleCalculateAC evt)
        {
            if (!Owner.Body.SecondaryHand.HasShield)
            {
                return;
            }
            int num = 0;
            IEnumerable<UnitEntityData> shielders = GameHelper.GetTargetsAround(Owner.Position, Radius + 3, true, false)
                                                                .Where(unit => (unit != Owner
                                                                                && !unit.IsEnemy(Owner)
                                                                                && (Owner.Position - unit.Position).magnitude <= Radius + Owner.Corpulence + unit.Corpulence
                                                                                && unit.Descriptor.HasFact(ShieldWallFact) || Owner.State.Features.SoloTactics)
                                                                                && unit.Body.SecondaryHand.HasShield
                                                                                //&& Vector3.Angle(Owner.OrientationDirection, unit.Position - Owner.Position) is <= 45 and >= 135
                                                                                );
#if DEBUG
            Comment.Log($"ShieldWallNew - RuleCalculateAC OnEventAboutToTrigger there're total {shielders.Count()} shielders. " + "(" + string.Join(", ", shielders.Select(s => s.CharacterName + " - position angle " + Vector2.SignedAngle(Owner.OrientationDirection.To2D(), (s.Position - Owner.Position).To2D()) + ", orientation angle " + Vector2.Angle(Owner.OrientationDirection.To2D(), s.OrientationDirection.To2D()))) + ")"); 
#endif
            if (shielders.Count() == 0) return;

            Func<IEnumerable<UnitEntityData>, IEnumerable<UnitEntityData>> left = new(shielders => shielders.Where(shielder => ( (Vector2.SignedAngle(Owner.OrientationDirection.To2D(), (shielder.Position - Owner.Position).To2D()) is >= 50 and <= 130 ) && Vector2.Angle(Owner.OrientationDirection.To2D(), shielder.OrientationDirection.To2D()) is <= 50)));
            Func<IEnumerable<UnitEntityData>, IEnumerable<UnitEntityData>> right = new(shielders => shielders.Where(shielder => ((Vector2.SignedAngle(Owner.OrientationDirection.To2D(), (shielder.Position - Owner.Position).To2D()) is <= -50 and >= -130) && Vector2.Angle(Owner.OrientationDirection.To2D(), shielder.OrientationDirection.To2D()) is <= 50)));

            IEnumerable<ArmorProficiencyGroup> leftSide = left(shielders).Select(leftShielder => leftShielder.Body.SecondaryHand.Shield.Blueprint.Type.ProficiencyGroup);
            if (leftSide.Count() > 0)
            {
                if (leftSide.Any(shieldProf => shieldProf == ArmorProficiencyGroup.TowerShield)) num +=3;
                else if (leftSide.Any(shieldProf => shieldProf == ArmorProficiencyGroup.HeavyShield)) num +=2;
                else num++;
            }
#if DEBUG
            Comment.Log($"ShieldWallNew - RuleCalculateAC OnEventAboutToTrigger bonus after left side is {num}"); 
#endif
            IEnumerable<ArmorProficiencyGroup> rightSide = right(shielders).Select(rightShielder => rightShielder.Body.SecondaryHand.Shield.Blueprint.Type.ProficiencyGroup);
            if (rightSide.Count() > 0)
            {
                if (rightSide.Any(shieldProf => shieldProf == ArmorProficiencyGroup.TowerShield)) num += 3;
                else if (rightSide.Any(shieldProf => shieldProf == ArmorProficiencyGroup.HeavyShield)) num += 2;
                else num++;
            }
#if DEBUG
            Comment.Log($"ShieldWallNew - RuleCalculateAC OnEventAboutToTrigger bonus after right side is {num}"); 
#endif

#if DEBUG
            Comment.Log($"ShieldWallNew - RuleCalculateAC OnEventAboutToTrigger {(leftSide.Count() > 0 ? ("Shielders on the left side are " + string.Join(", ", left(shielders).Select(leftShielder => leftShielder.CharacterName)) + ".") : "There're no shielders on the left side.")}" +
                                                                              $"{(rightSide.Count() > 0 ? ("Shielders on the right side are " + string.Join(", ", right(shielders).Select(rightShielder => rightShielder.CharacterName)) + ".") : "There're no shielders on the left side.")}"); 
#endif

            ModifiableValue.Modifier mod = new()
            {
                ModValue = num,
                ModDescriptor = ModifierDescriptor.Shield,
                StackMode = ModifiableValue.StackMode.ForceStack,
                Source = Fact,
                SourceComponent = Runtime.SourceBlueprintComponentName
            };
            Owner.Stats.AC.AddModifier(mod);
        }
        public void OnEventDidTrigger(RuleCalculateAC evt)
        {
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log($"ShieldWallNew: I'm inside OnEventDidTrigger (component {Fact.Blueprint?.name} on unit {Owner?.CharacterName})"); 
#endif

            ModifiableValueArmorClass value = Owner.Stats.AC;
            if (!value.ModifierList.TryGetValue(ModifierDescriptor.Shield, out List<ModifiableValue.Modifier> list))
            {
                Comment.Warning("ShieldWallNew: Could not find the list of modifier Shield descriptors {2} (component {0} on unit {1})", Fact.Blueprint?.name, Owner?.CharacterName);
                return;
            }
            List<ModifiableValue.Modifier> tmp = new();
            foreach (var mod in list.Where(mod => mod.Source == Fact && mod.SourceComponent == Runtime.SourceBlueprintComponentName))
            {
                tmp.Add(mod);
            }
            foreach (var mod in tmp)
            {
                if (list.Remove(mod))
                {
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log($"ShieldWallNew: Removing a modifier (component {Fact.Blueprint?.name} on unit {Owner?.CharacterName})"); 
#endif

                    value.PrepareForRemoval(mod);
                    value.UpdateValue();
                };
            }
            tmp.Clear();
        }


        public BlueprintUnitFactReference m_ShieldWallFact;
        public int Radius;
    }
}
