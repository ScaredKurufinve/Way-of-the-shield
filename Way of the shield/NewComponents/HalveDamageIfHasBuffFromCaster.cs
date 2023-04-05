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
#if DEBUG
            if (Debug.GetValue())
                Comment.Log($"HalveDamageIfHasBuffFromCaster - target is {evt.Target.CharacterName}, initiator is {evt.Initiator.CharacterName}. " +
                    $"Checking for buff {TheBuff?.name} of guid {m_Buff?.deserializedGuid} from caster {Buff?.Context.MaybeCaster?.CharacterName}. Checked? {!(TheBuff is not null && !evt.Target.Buffs.Enumerable.Any(buff => buff.Blueprint == TheBuff && buff.Context.MaybeCaster == Buff.Context.MaybeCaster))}"); 
#endif
            UnitEntityData caster = Buff.Context.MaybeCaster;
            if (TheBuff is not null && !evt.Target.Buffs.Enumerable.Any(buff => buff.Blueprint == TheBuff && buff.Context.MaybeCaster == caster))
                return;
            foreach (BaseDamage baseDamage in evt.DamageBundle)
            {
                baseDamage.AddDecline(new(DamageDeclineType.ByHalf, Buff));
            }
        }
        public void OnEventDidTrigger(RuleCalculateDamage evt)
        {
        }
    }
}
