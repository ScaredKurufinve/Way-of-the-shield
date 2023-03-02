using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.ContextData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Way_of_the_shield.NewComponents
{
    public class AddInitiatorShieldBashTrigger : EntityFactComponentDelegate<AddInitiatorShieldBashTrigger.ComponentData>, IInitiatorRulebookHandler<RuleAttackWithWeapon>
    {
        public ActionList Actions;
        public bool IgnoreAutoHit;
        public bool NotExtraAttack;
        public bool OnlyOnFirstBashHit;
        public bool OnlyOnFirstBashAttack;
        public bool OnMiss;
        public bool OnlyHit;
        public bool CriticalHit;
        public bool NotCriticalHit;
        public bool WaitForAttackResolve;
        public bool ActionsOnInitiator;

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {
        }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            if (!IsSuitable(evt)) return;
            MechanicsContext context = Context;
            EntityFact fact = Fact;

            foreach (RuleAttackWithWeaponResolve ruleAttackWithWeaponResolve in evt.ResolveRules)
            {
                if (ruleAttackWithWeaponResolve.IsTriggered)
                {
                    RunActions(this, evt, context, fact);
                }
                else
                {
                    RuleAttackWithWeaponResolve ruleAttackWithWeaponResolve2 = ruleAttackWithWeaponResolve;
                    Delegate onResolve = ruleAttackWithWeaponResolve2.OnResolve;
                    Action<RuleAttackWithWeaponResolve> actionOnReolsve = new(rule =>
                    {
                        if (!fact.Active) return;
                        RunActions(this, evt, context, fact);
                    });
                    ruleAttackWithWeaponResolve2.OnResolve += actionOnReolsve;
                }

            }
        }


        private bool IsSuitable(RuleAttackWithWeapon evt)
        {
            ItemEntityWeapon weapon = evt.Weapon;
            if (evt.IsFirstAttack) Data.FirstBash = true;
            if (weapon is null || !weapon.IsShield) return false;

            if (OnlyOnFirstBashAttack && !Data.FirstBash) return false;
            if (OnlyOnFirstBashHit && (Data.HadHit || !evt.AttackRoll.IsHit)) return false;

            if (Data.FirstBash) Data.FirstBash = false;
            if (evt.AttackRoll.IsHit) Data.HadHit = true;

            if (OnlyHit && !evt.AttackRoll.IsHit) return false;
            if (OnMiss && evt.AttackRoll.IsHit)return false;
            if (CriticalHit && (!evt.AttackRoll.IsCriticalConfirmed || evt.AttackRoll.FortificationNegatesCriticalHit))return false;
            if (NotCriticalHit && evt.AttackRoll.IsCriticalConfirmed && !evt.AttackRoll.FortificationNegatesCriticalHit)return false;
            if (NotExtraAttack && !evt.ExtraAttack) return false;

            return true;
        }

        private static void RunActions(AddInitiatorShieldBashTrigger c, RuleAttackWithWeapon rule, MechanicsContext context, EntityFact fact)
        {
            UnitEntityData unit = c.ActionsOnInitiator ? rule.Initiator : rule.Target;
            using (ContextData<ContextAttackData>.Request().Setup(rule.AttackRoll, null, 0, 0))
            {
                if (!fact.IsDisposed)
                {
                    fact.RunActionInContext(c.Actions, unit);
                }
                else
                {
                    using (context.GetDataScope(unit))
                    {
                        c.Actions.Run();
                    }
                }
            }
        }


        public class ComponentData
        {
            public bool FirstBash;
            public bool HadHit;
        }
    }
}
