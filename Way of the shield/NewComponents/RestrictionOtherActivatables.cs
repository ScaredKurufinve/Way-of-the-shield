using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Way_of_the_shield.NewComponents
{
    public class RestrictionOtherActivatables : ActivatableAbilityRestriction
    {
        public BlueprintActivatableAbilityReference[] m_ActivatableAbilities;
        public override bool IsAvailable()
        {
            if (m_ActivatableAbilities == null) return true;

            foreach (var blueprintActivatableAbilityReference in m_ActivatableAbilities)
            {
                if (Owner.ActivatableAbilities.Enumerable.Any(activatable => activatable.Blueprint == blueprintActivatableAbilityReference.Get() && activatable.IsOn)) return false;
            }
            return true;
        }
    }
}
