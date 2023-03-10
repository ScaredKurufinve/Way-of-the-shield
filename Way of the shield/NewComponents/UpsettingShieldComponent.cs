using Kingmaker.Armies.TacticalCombat;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Way_of_the_shield.NewFeatsAndAbilities;

namespace Way_of_the_shield.NewComponents
{
    public class UpsettingShieldComponent : UnitBuffComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>
    {
        public int checkerRanks = 1;

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {

        }
        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            if (TacticalCombatHelper.IsActive) return;
            ItemEntityWeapon weapon = evt.DamageBundle.Weapon;
            if (!weapon.IsShield || weapon.Shield?.ArmorComponent.Blueprint.ProficiencyGroup != ArmorProficiencyGroup.Buckler) return;
            if (evt.Result < 1) return;
            if (Owner.Progression.Features.HasFact(UpsettingShieldStyle.Vengeance))
            {
                evt.Target.Descriptor.AddBuff(UpsettingShieldStyle.VengeanceBuff, Owner, new TimeSpan?(new Rounds(1).Seconds), null);
                Owner.Descriptor.AddBuff(UpsettingShieldStyle.checker, Owner, new TimeSpan?(new Rounds(1).Seconds), null).SetRank(checkerRanks);
                return;
            }
            else if (Owner.Progression.Features.HasFact(UpsettingShieldStyle.Strike))
            {
                evt.Target.Descriptor.AddBuff(UpsettingShieldStyle.StrikeBuff, Owner, new TimeSpan?(new Rounds(1).Seconds), null);
                Owner.Descriptor.AddBuff(UpsettingShieldStyle.checker, Owner, new TimeSpan?(new Rounds(1).Seconds), null).SetRank(checkerRanks);
                return;
            };
            evt.Target.Descriptor.AddBuff(UpsettingShieldStyle.StyleBuff, Owner, new TimeSpan?(new Rounds(1).Seconds), null);
        }
    }
}
