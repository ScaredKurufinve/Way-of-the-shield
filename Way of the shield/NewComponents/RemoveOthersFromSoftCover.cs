using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using static Way_of_the_shield.Main;
using static Way_of_the_shield.Utilities;

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
                Comment.Log($"Entered RemoveOthersFromSoftCover component OnEventDidTrigger. Owner is {owner.CharacterName}, event weapon is {evt.Weapon.Blueprint.m_DisplayNameText}."); 
#endif

            if (CheckWeaponType == 0) goto checkFacts;
            else if (CheckWeaponType == WeaponTypesForSoftCoverDenial.Reach && !(UIUtilityItem.GetRange(evt.Weapon) == UIStrings.Instance.Tooltips.ReachWeapon)) return;
            else if (CheckWeaponType == WeaponTypesForSoftCoverDenial.Ranged && !evt.Weapon.Blueprint.IsRanged) return;
            bool flag1;

        checkFacts:
            List<(UnitEntityData unit, int, int)> s = new();
            foreach ((UnitEntityData unit, int, int) obstacle in evt.Result)
            {
                UnitEntityData unit = obstacle.unit;
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log($"Checking unit {unit.CharacterName}. Is ally? {unit.IsAlly(Owner)}."); 
#endif
                if (!(OnlyAlly && unit.IsAlly(Owner))) continue;
                flag1 = true;
                if (CheckFacts)
                {
                    foreach (BlueprintUnitFactReference reference in FactsToCheck)
                    {
                        if (!unit.HasFact(reference.Get()))
                        {
                            flag1 = false;
                            break;
                        }
                    }
                }
                if (flag1)
                {
                    s.Add(obstacle);
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log($"{obstacle.unit.CharacterName} is prepared to be removed from the obstacles list."); 
#endif
                }
            }
            for (int i = 1; i < s.Count; i++) evt.Result.Remove(s[i]);
        }

        public WeaponTypesForSoftCoverDenial CheckWeaponType = 0;
        public bool OnlyAlly;
        public bool CheckFacts;
        public BlueprintUnitFactReference[] FactsToCheck = Array.Empty<BlueprintUnitFactReference>();
    }
}
