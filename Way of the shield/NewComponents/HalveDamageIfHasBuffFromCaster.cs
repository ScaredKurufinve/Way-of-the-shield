using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;

namespace Way_of_the_shield.NewComponents
{
    public class HalveDamageIfHasBuffFromCaster : UnitBuffComponentDelegate, IInitiatorRulebookHandler<RuleCalculateDamage>
    {
        public BlueprintBuffReference m_Buff;
        public BlueprintBuff TheBuff
        {
            get
            {
                return m_Buff?.Get();
            }
        }
        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            if (TheBuff is null) goto ReduceDamge;
            UnitEntityData caster = Buff.Context.MaybeCaster;
            foreach (Buff buff in evt.Target.Buffs)
            {
                if (buff.Blueprint == TheBuff && buff.Context.MaybeCaster == caster) goto ReduceDamge;
            }
            return;
            ReduceDamge:
            foreach (BaseDamage baseDamage in evt.DamageBundle)
            {
                baseDamage.Durability *= 0.5f;
            }
        }
        public void OnEventDidTrigger(RuleCalculateDamage evt)
        {
        }
    }
}
