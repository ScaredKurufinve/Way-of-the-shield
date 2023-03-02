using Kingmaker.UI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Way_of_the_shield.NewComponents
{
    public class BuffDynamicDescriptionComponent_Parries : BuffDynamicDescriptionComponent_Base
    {
        public BuffDynamicDescriptionComponent_Parries(LocalizedString basic)
        {
            m_basic = basic;
        }

        private readonly LocalizedString m_basic;

        public override string GenerateDescription()
        {
            var part = Owner?.Parts.Get<OffHandParry.OffHandParryUnitPart>();
            if (part is null || !part.activated) return "";
            string result = string.Format(UIUtilityTexts.TooltipString.SimpleParameter, m_basic, part.n);
            return result;
        }
    }
}
