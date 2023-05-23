using Kingmaker.TextTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Way_of_the_shield.NewComponents
{
    public class BuffDynamicDescriptionComponent_Caster : BuffDynamicDescriptionComponent_Base
    {
        static LocalizedString SourceIs = new() { m_Key = "BuffDynamicDescriptionComponent_Caster" };

        public override string GenerateDescription()
        {
            var unit = Buff.MaybeContext?.MaybeCaster;
            if (unit is not null) return SourceIs.ToString().Replace("{Caster}" , LogHelper.GetUnitName(unit));
            Comment.Warning($"BuffDynamicDescriptionComponent_Caster - null caster.");
            return "";
        }
    }
}
