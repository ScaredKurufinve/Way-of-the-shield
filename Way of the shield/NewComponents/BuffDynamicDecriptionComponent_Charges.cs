using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Way_of_the_shield.NewComponents
{
    public class BuffDynamicDecriptionComponent_Charges : BuffDynamicDescriptionComponent_Base
    {
        public BuffDynamicDecriptionComponent_Charges(LocalizedString basic, ContextRankConfig contextCalculation = null)
        {
            m_basic = basic;
            m_calculation = contextCalculation;
        }

        private readonly LocalizedString m_basic;
        private readonly ContextRankConfig m_calculation;

        public override string GenerateDescription()
        {
            if (Buff is null)
            {
                Comment.Error(this, $"BuffDynamicDecriptionComponent_Charges - Owner blueprint is {OwnerBlueprint}, Owner is {Owner?.CharacterName}. Could not find a buff to retrieve ranks.");
                return "";
            }
#if DEBUG
            if (Debug.GetValue())
                Comment.Log($"Progression is null? {m_calculation is null}. {(m_calculation is not null ? "calculation is " + m_calculation.ToString() + ", value is " + m_calculation.ApplyProgression(Buff.Rank) : "")}"); 
#endif
            int num = m_calculation is null ? Buff.Rank : m_calculation.ApplyProgression(Buff.Rank);
            return string.Format(UIUtilityTexts.TooltipString.SimpleParameter, m_basic, num);
        }
    }
}
