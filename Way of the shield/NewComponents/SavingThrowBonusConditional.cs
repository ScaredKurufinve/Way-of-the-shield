using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
namespace Way_of_the_shield.NewComponents
{
    public class SavingThrowBonusConditional : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleSavingThrow>
    {
        public void OnEventAboutToTrigger(RuleSavingThrow evt)
        {
#if DEBUG
            if (Debug.GetValue())
                Comment.Log($"SavingThrowBonusConditional - RuleSavingThrow OnEventAboutToTrigger. Initiator is {evt.Initiator?.CharacterName}, Saving Throw type of the event is {evt.Type}, type of the bonus is {savingThrowType}"); 
#endif
            if (savingThrowType != evt.Type) return;
            using (ContextData<SavingThrowData>.Request().Setup(evt))
            {
                using (evt.Reason.Ability.CreateExecutionContext(evt.Initiator).GetDataScope(evt.Initiator))
                {
#if DEBUG
                    if (Debug.GetValue())
                        Comment.Log("SavingThrowBonusConditional - RuleSavingThrow OnEventAboutToTrigger. Set up the context");
#endif
                    if (!Conditions.Check()) return;
                }
            }
#if DEBUG
            if (Debug.GetValue())
                Comment.Log("SavingThrowBonusConditional - RuleSavingThrow OnEventAboutToTrigger. Conditions are checked"); 
#endif
            int value = Bonus.Calculate(Context) + Value * Fact.GetRank();
#if DEBUG
            if (Debug.GetValue())
                Comment.Log("SavingThrowBonusConditional - RuleSavingThrow OnEventAboutToTrigger. value = " + value); 
#endif
            evt.AddModifier(value, Fact, ModifierDescriptor);
        }
        public void OnEventDidTrigger(RuleSavingThrow evt)
        {
        }

        public SavingThrowType savingThrowType;
        public ModifierDescriptor ModifierDescriptor;
        public int Value;
        public ContextValue Bonus;
        public ConditionsChecker Conditions;
    }
}
