using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Way_of_the_shield.NewComponents.OffHandParry;

namespace Way_of_the_shield.NewComponents
{
    [TypeId("ebb8579958c449a0b0bfb7025dc91bab")]
    public class AdditionalParriesComponent : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAttacksCount>
    {
        public ContextValue Bonus = 1;
        public ConditionsChecker Conditions;

        public bool Haste;
        public bool Penalized;

        public void OnEventAboutToTrigger(RuleCalculateAttacksCount evt)
        {
            if (!OffHandParryUnitPart.flag) return;
            OffHandParryUnitPart p = evt.Initiator?.Parts.Get<OffHandParryUnitPart>();
            bool weaponCheck = p is not null ? p.weapon == evt.Initiator.Body?.SecondaryHand.MaybeWeapon : false;
            bool conditionsNotEmpty = Conditions?.Conditions is Condition[] ar && !ar.Empty();
#if DEBUG
            if (Debug.GetValue())
                Comment.Log($"AdditionalParriesComponent - RuleCalculateAttacksCount EventAboutToTrigger. " +
                    $"Owner is {Owner?.CharacterName}, owner blueprint is {OwnerBlueprint?.name}. " +
                    $"Weapon check is {weaponCheck}. " +
                    $"{(conditionsNotEmpty ? "There are " + Conditions.Conditions.Count() + " conditions" : "There are no conditions")}. " +
                    $"Bonus is not null? {Bonus is not null}");

#endif
            if (!conditionsNotEmpty)
                goto CalculateBonus;
            using (new MechanicsContext(evt.Initiator, evt.Initiator, OwnerBlueprint, evt.Reason?.Context).GetDataScope(evt.Initiator))
            {
#if DEBUG
                if (Debug.GetValue())
                    Comment.Log("AdditionalParriesComponent - RuleCalculateAttacksCount EventAboutToTrigger. Set up the context.");
#endif
                if (!Conditions.Check()) 
                {
#if DEBUG
                    Comment.Log("AdditionalParriesComponent - RuleCalculateAttacksCount EventAboutToTrigger. Conditions were not met");  
#endif
                    return;
                }
#if DEBUG
                else if (Debug.GetValue()) { Comment.Log("AdditionalParriesComponent - RuleCalculateAttacksCount EventAboutToTrigger. Conditions are met"); } 
#endif
            }

            CalculateBonus:
            int Number = Bonus.Calculate(Context);
#if DEBUG
            if (Debug.GetValue())
                Comment.Log($"AdditionalParriesComponent - RuleCalculateAttacksCount EventAboutToTrigger. Adding attacks. Calculated bonus is {Number}. Haste is {Haste},  Penalized is {Penalized}."); 
#endif
            evt.AddExtraAttacks(Number, Haste, Penalized, p.weapon);
        }
        public void OnEventDidTrigger(RuleCalculateAttacksCount evt)
        {
        }
    }
}
