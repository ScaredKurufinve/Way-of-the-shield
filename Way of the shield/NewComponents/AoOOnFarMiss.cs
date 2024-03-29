﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.ElementsSystem;

namespace Way_of_the_shield.NewComponents
{
    public class AoOOnFarMiss : UnitBuffComponentDelegate, IInitiatorRulebookHandler<RuleAttackWithWeapon>
    {
        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {
        }
        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            BlueprintBuff buff = m_FactToCheck?.Get();
#if DEBUG
            if (Debug.GetValue())
            {
                Comment.Log($"AoOOnFarMiss RuleAttackWithWeapon OnEventAboutToTrigger - " +
                    $"Weapon is not melee? {!evt.Weapon.Blueprint.IsMelee}. " +
                    $"Check the buff? {CheckBuff}." +
                    $"Buff is  {(buff is null ? "null" : buff.name)}." +
                    $"Has buff? {(CheckOnCaster ? !Buff.Context.MaybeCaster?.Descriptor.Buffs.HasFact(buff) : evt.Target.Descriptor.Buffs.HasFact(buff))}" +
                    $"Final result is {(!evt.Weapon.Blueprint.IsMelee || CheckBuff && (buff is null || (CheckOnCaster ? !(Buff.Context.MaybeCaster?.Descriptor.Buffs.HasFact(buff) ?? true) : !evt.Target.Descriptor.Buffs.HasFact(buff))))}");
            } 
#endif
            if (!evt.Weapon.Blueprint.IsMelee ||
                CheckBuff && (buff is null || (CheckOnCaster ? (!Buff.Context.MaybeCaster?.Descriptor.Buffs.HasFact(buff) ?? false) : !evt.Target.Descriptor.Buffs.HasFact(buff)))) return;
#if DEBUG
            if (Debug.GetValue())
                Comment.Log("Did not return1"); 
#endif
            UnitEntityData StylishDude = Buff.Context.MaybeCaster;
            if (CasterOnly && evt.Target != StylishDude) return;
#if DEBUG
            if (Debug.GetValue())
            {
                Comment.Log("Did not return2");
                Comment.Log("(evt.AttackRoll.D20 + evt.AttackRoll.AttackBonus - evt.AttackRoll.TargetAC) = " + (evt.AttackRoll.D20 + evt.AttackRoll.AttackBonus - evt.AttackRoll.TargetAC));
            }
#endif
            if (evt.AttackRoll.D20 + evt.AttackRoll.AttackBonus - evt.AttackRoll.TargetAC <= -5)
            {
                Game.Instance.CombatEngagementController.ForceAttackOfOpportunity(StylishDude, evt.Initiator);
                if (CheckBuff && ReduceBuffRanksAfterAOO)
                {
                    UnitEntityData BuffOwner = CheckOnCaster ? Owner : StylishDude;
                    using (ContextData<BuffCollection.RemoveByRank>.Request())
                    {
                        var b = BuffOwner.Buffs.GetBuff(buff);
                        b.Remove();
                    }
                }
            };
        }

        public bool CheckBuff;
        public bool CheckOnCaster;
        public BlueprintBuffReference m_FactToCheck;
        public bool CasterOnly;
        public bool ReduceBuffRanksAfterAOO = true;
    }
}
