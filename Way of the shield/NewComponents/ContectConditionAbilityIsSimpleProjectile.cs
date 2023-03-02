using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;

namespace Way_of_the_shield.NewComponents
{
    public class ContextConditionProjectileType : ContextCondition
    {
        public override string GetConditionCaption()
        {
            return string.Format("Check if ability casted by {0} is a projectile of types {1}", Context.MaybeCaster?.CharacterName ?? "UndefinedName", projTypes.ToString());
        }
        public override bool CheckCondition()
        {
            //Comment.Log("I'm inside the ContectConditionAbilityIsSimpleProjectile");
            //Comment.Log("AbilityContext is null?" + (AbilityContext is null));
            AbilityDeliverProjectile proj = AbilityContext?.Ability?.AbilityDeliverProjectile;
            //Comment.Log("Projectile is null?" + (proj is null));
            if (proj is null) return false;

            if (projTypes.Any(t => t == proj.Type)) return true;
            else return false;
        }
        public AbilityProjectileType[] projTypes;
    }
}
