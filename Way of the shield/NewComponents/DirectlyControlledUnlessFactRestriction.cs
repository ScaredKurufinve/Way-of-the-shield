using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Way_of_the_shield.NewComponents
{
    [AllowedOn(typeof(BlueprintActivatableAbility), false)]
    [TypeId("06a65710d247460d80597eebf64154f3")]
    public class DirectlyControlledUnlessFactRestriction : ActivatableAbilityRestriction
    {
        public override bool IsAvailable()
        {
            return Owner.IsDirectlyControllable || Owner.HasFact(m_Fact);
        }
        public BlueprintUnitFactReference m_Fact;
    }
}
