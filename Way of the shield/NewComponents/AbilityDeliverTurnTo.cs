using System.Collections.Generic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using static Way_of_the_shield.Main;

namespace Way_of_the_shield.NewComponents
{
    public class AbilityDeliverTurnTo : AbilityCustomLogic
    {
        public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
        {
#if DEBUG
            if (Debug.GetValue())
                Comment.Log($"Delivering AbilityDeliverTurnTo. Caster is {context.Caster?.CharacterName}. Current orientation is {context.Caster?.OrientationDirection}, target position is {target.Point}."); 
#endif
            context.Caster?.ForceLookAt(target.Point);
            yield return null;

        }
        public override void Cleanup(AbilityExecutionContext context)
        {
        }
    }
}
