using Kingmaker;
using Kingmaker.Armies.TacticalCombat;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Way_of_the_shield.NewComponents
{
    public class UpsettingShieldComponent : UnitBuffComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>
    {
        public static BlueprintFeature Strike = new();
        public static BlueprintFeature Vengeance = new();
        public static BlueprintBuff VengeanceBuff = new();
        public static BlueprintBuff StrikeBuff = new();
        public static BlueprintBuff StyleBuff = new();
        public static BlueprintBuff checker = new();

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
            if (Owner.Progression.Features.HasFact(Vengeance))
            {
                evt.Target.Descriptor.AddBuff(VengeanceBuff, Owner, new TimeSpan?(new Rounds(1).Seconds), null);
                Owner.Descriptor.AddBuff(checker, Owner, new TimeSpan?(new Rounds(1).Seconds), null).SetRank(checkerRanks);
                return;
            }
            else if (Owner.Progression.Features.HasFact(Strike))
            {
                evt.Target.Descriptor.AddBuff(StrikeBuff, Owner, new TimeSpan?(new Rounds(1).Seconds), null);
                Owner.Descriptor.AddBuff(checker, Owner, new TimeSpan?(new Rounds(1).Seconds), null).SetRank(checkerRanks);
                return;
            };
            evt.Target.Descriptor.AddBuff(StyleBuff, Owner, new TimeSpan?(new Rounds(1).Seconds), null);
        }
    }
}
