using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Controllers.Units;
using Kingmaker.Controllers;
using Kingmaker.Designers;
using Kingmaker;

namespace Way_of_the_shield.NewComponents
{
    public class AddRegerationFromTarget : AddEffectRegeneration, ITickEachRound, ITargetRulebookHandler<RuleDealDamage>
    {
        public bool m_checkCaster;
        public BlueprintBuffReference m_checkedFact;
        public BlueprintBuff CheckedFact
        {
            get
            {
                return m_checkedFact?.Get();
            }
        }
        
        public new void OnEventDidTrigger(RuleDealDamage evt)
        {
            if (evt.Target != base.Owner || evt.DamageBundle == null || this.Unremovable)
            {
                return;
            }
            BlueprintBuff b = CheckedFact;
            if (b is null ) Suppress();
            UnitEntityData caster = Fact.MaybeContext.MaybeCaster;
            bool flag = m_checkCaster && caster is not null;
            foreach (Buff buff in evt.Initiator.Buffs)
            {
                if (buff.Blueprint == b && (flag && buff.Context.MaybeCaster == caster))
                {
                    return;
                }
            }
            Suppress();
        }
        void ITickEachRound.OnNewRound()
        {
           if (Data.SuppressTimestamp != null && Game.Instance.TimeController.GameTime - Data.SuppressTimestamp >= 6f.Seconds())
            {
                Data.SuppressTimestamp = null;
                SetRegenerationActive(true);
            }
            if (Data.RegenerationActive && Owner.Damage > 0)
            {
                GameHelper.HealDamage(Owner, Owner, IsHalved ? (Heal / 2) : Heal, Fact);
            }
        }

    }

}
