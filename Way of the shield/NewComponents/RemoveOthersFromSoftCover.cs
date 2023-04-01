using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;

namespace Way_of_the_shield.NewComponents
{
    public class RemoveOthersFromSoftCover : UnitFactComponentDelegate, IInitiatorRulebookHandler<SoftCover.RuleSoftCover>
    {
        public void OnEventAboutToTrigger(SoftCover.RuleSoftCover evt) { }
        public void OnEventDidTrigger(SoftCover.RuleSoftCover evt)
        {
            UnitEntityData owner = Owner;
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log($"RemoveOthersFromSoftCover SoftCover OnEventDidTrigger. Owner is {owner.CharacterName}, event weapon is {evt.Weapon.Blueprint.m_DisplayNameText}. " +
                    $"Checking for facts? {CheckFacts}{(!CheckFacts ? "" : ". Facts are: " + String.Join(", ", FactsToCheck.Select(f => (f?.NameSafe() ?? "Nameless fact") + " of guid " + f.deserializedGuid)))}"); 
#endif

            if (CheckWeaponType == 0) goto checkFacts;
            else if (CheckWeaponType == WeaponTypesForSoftCoverDenial.Reach && !(UIUtilityItem.GetRange(evt.Weapon) == UIStrings.Instance.Tooltips.ReachWeapon)) return;
            else if (CheckWeaponType == WeaponTypesForSoftCoverDenial.Ranged && !evt.Weapon.Blueprint.IsRanged) return;

        checkFacts:
            evt.Result = evt.Result.Where(r => 
            {
                UnitEntityData unit = r.obstacle;
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log($"RemoveOthersFromSoftCover SoftCover - Checking unit {unit.CharacterName}. {(!OnlyAlly ? "" : "Is ally? " + unit.IsAlly(Owner) + ".")}");
#endif

                if (OnlyAlly && !unit.IsAlly(Owner)) return true;
                if (CheckFacts is false || FactsToCheck.Length < 1)
                {
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log($"RemoveOthersFromSoftCover SoftCover - {unit.CharacterName} is prepared to be removed from the obstacles list.");
#endif
                    return false; 
                };
                foreach (BlueprintUnitFactReference reference in FactsToCheck)
                {
                    if (!unit.HasFact(reference.Get()))
                    {
                        return true;
                    }
                }
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log($"RemoveOthersFromSoftCover SoftCover - {unit.CharacterName} is prepared to be removed from the obstacles list.");
#endif
                return false;

            }).ToList();
        }

        public WeaponTypesForSoftCoverDenial CheckWeaponType = 0;
        public bool OnlyAlly;
        public bool CheckFacts;
        public BlueprintUnitFactReference[] FactsToCheck = Array.Empty<BlueprintUnitFactReference>();
    }
}
