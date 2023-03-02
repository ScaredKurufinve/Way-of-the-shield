using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.RuleSystem.Rules;

namespace Way_of_the_shield.NewComponents
{
    public class SavingBonusAgainstAlliesIfAllyHasFactAndSimpleProjectile : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleSavingThrow>
    {
        public void OnEventAboutToTrigger(RuleSavingThrow evt)
        {
            UnitEntityData caster = evt.Reason.Caster;

            if (caster is null
                || evt.Initiator == caster
                || !evt.Initiator.IsAlly(caster)
                || savingThrowType != evt.Type
                || EnablingFeature is not null && !caster.HasFact(EnablingFeature.Get())
                || !(evt.Reason?.Ability?.AbilityDeliverProjectile?.Type != AbilityProjectileType.Cone)) return;
            int value = Bonus.Calculate(Context) + Value * Fact.GetRank();
            evt.AddModifier(value, Fact, ModifierDescriptor);
        }
        public void OnEventDidTrigger(RuleSavingThrow evt)
        {
        }

        public SavingThrowType savingThrowType;
        public ModifierDescriptor ModifierDescriptor;
        public int Value;
        public ContextValue Bonus;
        public BlueprintUnitFactReference EnablingFeature;
    }
}
