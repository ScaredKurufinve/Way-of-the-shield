using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Way_of_the_shield.NewComponents
{
    [ComponentName("AA Restriction condition - requires melee weapon")]
    [TypeId("00750ebb07d94e66a7345001276c417c")]
    public class RestrictionNonRangedWeapon : ActivatableAbilityRestriction
    {
        public override bool IsAvailable()
            => (Owner.Body.PrimaryHand.MaybeWeapon?.Blueprint.Category.HasSubCategory(WeaponSubCategory.Melee) ?? true);
    }
}
