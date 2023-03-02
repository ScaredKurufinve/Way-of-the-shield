using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Way_of_the_shield.NewComponents
{
    public class ShieldedDefenseAcBonus : UnitFactComponentDelegate, ITargetRulebookHandler<RuleCalculateAC>
    {
        public ContextValue Value;
        public ModifiableValue.StackMode StackMode;
        public ModifierDescriptor Descriptor = ModifierDescriptor.None;
        public void OnEventAboutToTrigger(RuleCalculateAC evt)
        {
#if DEBUG
            if (Debug.GetValue())
                Comment.Log($"ShieldedDefenseAcBonus: I'm inside OnEventAboutToTrigger (component {Fact.Blueprint?.name} on unit {Owner?.CharacterName})"); 
#endif
            if (evt.IsTargetFlatFooted) return;
            if (Value is null)
            {
                Comment.Error("ShieldedDefenseAcBonus: Value is null (component {0} on unit {1})", Fact.Blueprint?.name, Owner?.CharacterName);
                return;
            }
            if (!Value.IsValueSimple && Fact.MaybeContext is null)
            {
                Comment.Error("ShieldedDefenseAcBonus: Context is null when the value is not simple (component {0} on unit {1})", Fact.Blueprint?.name, Owner?.CharacterName);
                return;
            }
            int Bonus = Value.Calculate(Fact.MaybeContext);
#if DEBUG
            if (Debug.GetValue())
                Comment.Log($"ShieldedDefenseAcBonus: Calculated value is {Bonus} (component {Fact.Blueprint?.name} on unit {Owner?.CharacterName}). Source is {StatModifiersBreakdown.GetBonusSourceText(Fact, false)}"); 
#endif

            ModifiableValue.Modifier mod = new()
            {
                ModValue = Bonus,
                ModDescriptor = Descriptor,
                StackMode = StackMode,
                Source = Fact,
                SourceComponent = Runtime.SourceBlueprintComponentName
            };
            Owner.Stats.AC.AddModifier(mod);


        }

        public void OnEventDidTrigger(RuleCalculateAC evt)
        {
#if DEBUG
            if (Debug.GetValue())
                Comment.Log($"ShieldedDefenseAcBonus: I'm inside OnEventDidTrigger (component {Fact.Blueprint?.name} on unit {Owner?.CharacterName})"); 
#endif
            if (evt.IsTargetFlatFooted) return;
            ModifiableValueArmorClass value = Owner.Stats.AC;
            if (!value.ModifierList.TryGetValue(Descriptor, out List<ModifiableValue.Modifier> list))
            {
                Comment.Warning("ShieldedDefenseAcBonus: Could not find the list of modifier descriptors {2} (component {0} on unit {1})", Fact.Blueprint?.name, Owner?.CharacterName, Descriptor);
                return;
            }
            List<ModifiableValue.Modifier> tmp = new();
            foreach (var mod in list.Where(mod => mod.Source == Fact && mod.SourceComponent == Runtime.SourceBlueprintComponentName))
            {
                tmp.Add(mod);
            }
            foreach (var mod in tmp)
            {
                if (list.Remove(mod))
                {
#if DEBUG
                    if (Debug.GetValue())
                        Comment.Log($"ShieldedDefenseAcBonus: Removing a modifier (component {Fact.Blueprint?.name} on unit {Owner?.CharacterName})"); 
#endif
                    value.PrepareForRemoval(mod);
                    value.UpdateValue();
                };
            }
            tmp.Clear();
        }
    }
}
