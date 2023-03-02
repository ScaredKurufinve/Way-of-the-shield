using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Way_of_the_shield.NewComponents
{
    public  class BackToBackImprovedComponent : UnitFactComponentDelegate, ITargetRulebookHandler<RuleAttackRoll>
    {
        public static RulebookEvent.CustomDataKey BTBImprovedKey = new("BackToBackImproved");
        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            evt.SetCustomData(BTBImprovedKey, true);
        }
        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
        }
    }
}
