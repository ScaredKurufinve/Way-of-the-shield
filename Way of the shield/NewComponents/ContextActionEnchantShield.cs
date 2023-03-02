using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.ElementsSystem;
using Kingmaker.Items.Slots;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker;

namespace Way_of_the_shield.NewComponents
{
    [TypeId("d3eb7d90b0d6427d85b099c33ef77784")]
    public class ContextActionEnchantShield : ContextActionEnchantWornItem
    {
        public override void RunAction()
        {
            MechanicsContext.Data data = ContextData<MechanicsContext.Data>.Current;
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log("Inside ContextActionEnchantShield"); 
#endif
            MechanicsContext mechanicsContext = data?.Context;
            if (mechanicsContext == null)
            {
                PFLog.Mods.Error(this, "Unable to apply Buff: no context found");
                return;
            }
            Rounds value = DurationValue.Calculate(mechanicsContext);
            UnitEntityData unitEntityData = ToCaster ? mechanicsContext.MaybeCaster : Target.Unit;
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log("unitEntityData = " + unitEntityData?.CharacterName); 
#endif
            if (unitEntityData == null)
            {
                PFLog.Mods.Error(this, "Can't apply Buff: target is null");
                return;
            };            
            ItemEntity shield;
            HandSlot slot = unitEntityData.Body.SecondaryHand;
            if (Enchantment is BlueprintWeaponEnchantment) goto weapon;
            shield = slot?.MaybeShield?.ArmorComponent;
            if (shield is not null) goto doEnchant;
            weapon:
            shield = slot?.MaybeShield?.WeaponComponent;
            if (shield is not null) goto doEnchant;
            shield = slot?.MaybeWeapon;
            if (shield is not null
                && shield is ItemEntityWeapon weapon
                && weapon.Blueprint.Category is WeaponCategory.SpikedHeavyShield or WeaponCategory.SpikedLightShield)
                goto doEnchant;
            slot = unitEntityData.Body.PrimaryHand;
            weapon = slot.MaybeWeapon;
            if (weapon is null || !(weapon.Blueprint.Category is WeaponCategory.SpikedHeavyShield or WeaponCategory.SpikedLightShield)) return;

            doEnchant:
            ItemEnchantment fact = shield.Enchantments.GetFact(Enchantment);
            if (fact != null)
            {
                if (!fact.IsTemporary)
                {
                    return;
                }
                shield.RemoveEnchantment(fact);
            }
            shield.AddEnchantment(Enchantment, mechanicsContext, new Rounds?(value)).RemoveOnUnequipItem = RemoveOnUnequip;
        }

    }
}
