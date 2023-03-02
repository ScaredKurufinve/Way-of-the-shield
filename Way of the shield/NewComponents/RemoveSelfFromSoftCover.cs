using System;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using static Way_of_the_shield.Main;
using static Way_of_the_shield.Utilities;

namespace Way_of_the_shield.NewComponents
{
    public class RemoveSelfFromSoftCover : UnitFactComponentDelegate, IRulebookHandler<SoftCover.RuleSoftCover>
    {
        public void OnEventAboutToTrigger(SoftCover.RuleSoftCover evt)
        {

        }
        public void OnEventDidTrigger(SoftCover.RuleSoftCover evt)
        {

            UnitEntityData owner = Owner;
            //if (Settings.IsEnabled("Debug"))
            Comment.Log("There is an exception, but we are inside the RemoveSelfFromSoftCover!");


            if (evt.Initiator == owner || !evt.Result.Any(entry => entry.obstacle == owner)) return;

            if (CheckWeaponType == 0) goto checkAllies;
            else if (CheckWeaponType == WeaponTypesForSoftCoverDenial.Reach && !(UIUtilityItem.GetRange(evt.Weapon) == UIStrings.Instance.Tooltips.ReachWeapon)) return;
            else if (CheckWeaponType == WeaponTypesForSoftCoverDenial.Ranged && !evt.Weapon.Blueprint.IsRanged) return;

            checkAllies:
            if (OnlyAlly && !evt.Initiator.IsAlly(Owner)) return;

            if (CheckFacts && FactsToCheck.Length > 0)
            {
                foreach (BlueprintUnitFactReference reference in FactsToCheck)
                {
                    if (!owner.HasFact(reference.Get())) return;
                }
            }
            evt.Result.Remove(entry => entry.obstacle == owner);
        }

        public WeaponTypesForSoftCoverDenial CheckWeaponType = 0;
        public bool OnlyAlly;
        public bool CheckFacts;
        public BlueprintUnitFactReference[] FactsToCheck = Array.Empty<BlueprintUnitFactReference>();

    }

}
