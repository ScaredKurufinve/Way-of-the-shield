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
        public bool Require;
        public override bool IsAvailable()
        {
            bool RestrictionIsFound = false;
            if (m_ActivatableAbilities != null)
                foreach (var blueprintActivatableAbilityReference in m_ActivatableAbilities)
                    if (Owner.ActivatableAbilities.Enumerable.Any(activatable => activatable.Blueprint == blueprintActivatableAbilityReference.Get() && activatable.IsOn))
                    { RestrictionIsFound = true; break; }
                
            return Require ? RestrictionIsFound : !RestrictionIsFound;
        }
    }
}
