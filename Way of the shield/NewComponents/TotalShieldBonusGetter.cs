using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Designers;

namespace Way_of_the_shield.NewComponents
{
    public class TotalShieldBonusGetter : PropertyValueGetter
    {
        public override int GetBaseValue(UnitEntityData unit)
        {
#if DEBUG
            if (Debug.GetValue())
                Comment.Log($"Inside the TotalShieldBonusGetter for {unit.CharacterName}"); 
#endif
            HandSlot offhand = unit?.Descriptor?.Body?.SecondaryHand;
            if (offhand is null || !offhand.HasShield)
            {
                return 0;
            }
            ItemEntityShield shield = offhand.MaybeShield;
            int num = shield.Blueprint.ArmorComponent.Type.ArmorBonus + GameHelper.GetItemEnhancementBonus(shield.ArmorComponent);
#if DEBUG
            if (Debug.GetValue())
                Comment.Log($"Total shield bonus is {num}"); 
#endif
            return num;
        }
    }
}
