using Kingmaker.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.UnitLogic.Mechanics.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Way_of_the_shield.NewComponents
{
    public class ShieldedDefensePropertyGetter : PropertyValueGetter
    {
        public override int GetBaseValue(UnitEntityData unit)
        {
#if DEBUG
            if (Debug.GetValue())
                Comment.Log($"Inside the TotalShieldBonusGetter for {unit.CharacterName}"); 
#endif
            ItemEntityShield shield = unit.Body.SecondaryHand?.MaybeShield;
            if (shield == null) return -2;
            int Base = unit.Stats.BaseAttackBonus;
            ArmorProficiencyGroup prof = shield.ArmorComponent.Blueprint.ProficiencyGroup;
            if (prof is ArmorProficiencyGroup.LightShield) return Base / 3 ;
            else if (prof is ArmorProficiencyGroup.HeavyShield or ArmorProficiencyGroup.TowerShield) return Base / 2 ;
            else return -2;
        }
    }
}
