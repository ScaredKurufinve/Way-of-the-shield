using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Blueprints.Root;

namespace Way_of_the_shield.NewComponents
{
    public class AbilityTargetEquippedWithShield : BlueprintComponent, IAbilityTargetRestriction
    {
        public string GetAbilityTargetRestrictionUIText(UnitEntityData caster, TargetWrapper target)
        {
            return new LocalizedString() { Key = "ContextConditionEquippedWithShield_UIText" };
        }
        public bool IsTargetRestrictionPassed(UnitEntityData _, TargetWrapper Target)
        {
            UnitEntityData target = Target.Unit;
            if (target is null) 
            {
                PFLog.Mods.Error("Target unit is missing");
                return false;
            };
            UnitBody body = target.Body;
            if (body is null)
            {
                PFLog.Mods.Error("Target has no body");
                return false;
            };
            ItemEntityShield shield = body.SecondaryHand?.MaybeShield;
            if (shield is not null) return true;
            ItemEntityWeapon weapon = body.SecondaryHand?.MaybeWeapon;
            if (weapon is not null
                && (weapon.Blueprint.Category is WeaponCategory.SpikedHeavyShield
                                              or WeaponCategory.SpikedLightShield)) return true;
            weapon = body.PrimaryHand?.MaybeWeapon;
            if (weapon is not null
                && (weapon.Blueprint.Category is WeaponCategory.SpikedHeavyShield
                                              or WeaponCategory.SpikedLightShield)) return true;
            return false;
            

        }
    }
}
