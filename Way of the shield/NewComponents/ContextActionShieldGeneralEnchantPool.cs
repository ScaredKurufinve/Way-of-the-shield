using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using UnityEngine.Serialization;
using UnityEngine;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.UnitLogic;
using Kingmaker.EntitySystem;
using static Kingmaker.UnitLogic.UnitHelper;
using System.Runtime.Remoting.Contexts;

namespace Way_of_the_shield.NewComponents
{
    [TypeId("1fe5b88460f04fa2873b0c421b77e31b")]
    public class ContextActionShieldGeneralEnchantPool : ContextAction
    {
        public EnchantPoolType EnchantPool = EnchantPoolType.DivineWeaponBond;
        public ActivatableAbilityGroup Group;
        [SerializeField]
        [FormerlySerializedAs("DefaultEnchantmentsWeapon")]
        public BlueprintWeaponEnchantmentReference[] m_DefaultEnchantmentsWeapon = new BlueprintWeaponEnchantmentReference[6];
        public ReferenceArrayProxy<BlueprintWeaponEnchantment, BlueprintWeaponEnchantmentReference> DefaultEnchantmentsWeapon
        {
            get
            {
                return m_DefaultEnchantmentsWeapon;
            }
        }
        [SerializeField]
        [FormerlySerializedAs("DefaultEnchantmentsArmor")]
        public BlueprintArmorEnchantmentReference[] m_DefaultEnchantmentsArmor = new BlueprintArmorEnchantmentReference[6];
        public ReferenceArrayProxy<BlueprintArmorEnchantment, BlueprintArmorEnchantmentReference> DefaultEnchantmentsArmor
        {
            get
            {
                return m_DefaultEnchantmentsArmor;
            }
        }

        public ContextDurationValue DurationValue;
        public override string GetCaption()
        {
            return string.Format("Add enchants from pool to caster's shield (for {0})", DurationValue);
        }
        public override void RunAction()
        {
            UnitEntityData maybeCaster = Context.MaybeCaster;
            if (maybeCaster == null)
            {
                Comment.Error(this, "ContextActionShieldGeneralEnchantPool: target is null");
                return;
            }
            UnitPartEnchantPoolData unitPartEnchantPoolData = maybeCaster.Ensure<UnitPartEnchantPoolData>();
            unitPartEnchantPoolData.ClearEnchantPool(EnchantPool);
            ItemEntityShield maybeShield = maybeCaster.Body.SecondaryHand.MaybeShield;
            if (maybeShield is null)
            {
                Comment.Error(this, "ContextActionShieldGeneralEnchantPool: caster is not equipped with a shield");
                return;
            }
            ItemEntityWeapon itemEntityWeapon = maybeShield.WeaponComponent;
            ItemEntityArmor itemEntityArmor = maybeShield.ArmorComponent;
            int num = maybeCaster.Ensure<UnitPartActivatableAbility>().GetGroupSize(Group);
            int numW = num;
            int numA = num;
            List<ItemEnchantment> weaponEnchants = itemEntityWeapon?.Enchantments;
            if (weaponEnchants is not null && weaponEnchants.Any())
            {
                numW += GameHelper.GetItemEnhancementBonus(itemEntityWeapon);
            };
            List<ItemEnchantment> armorEnchants = itemEntityArmor?.Enchantments;
            if (armorEnchants is not null && armorEnchants.Any())
            {
                numA += GameHelper.GetItemEnhancementBonus(itemEntityArmor);
            };
            Rounds duration = DurationValue.Calculate(Context);
            BlueprintItemEnchantment enchant;
            foreach (AddBondProperty addBondProperty in maybeCaster.Buffs.SelectFactComponents<AddBondProperty>())
            {
                try
                {
                    if (addBondProperty.Enchant == null)
                    {
                        Comment.Error(this, "ContextActionShieldGeneralEnchantPool: no Enchantment in the AddBondedProperty component of {0}", addBondProperty.m_Enchant.guid);
                        continue;
                    }
                }
                catch
                {
                    Comment.Error(this, "ContextActionShieldGeneralEnchantPool: no Enchantment in the AddBondedProperty component of {0}", addBondProperty.m_Enchant.guid);
                    continue;
                }

                if (addBondProperty.EnchantPool != EnchantPool) continue;
                enchant = addBondProperty.Enchant;
                if (enchant.EnchantmentCost > num) continue;
                if (enchant is BlueprintWeaponEnchantment && weaponEnchants is not null && !itemEntityWeapon.HasEnchantment(enchant))
                {
                    unitPartEnchantPoolData.AddEnchant(itemEntityWeapon, EnchantPool, enchant, Context, duration);
                    numW -= addBondProperty.Enchant.EnchantmentCost;
                }
                else if (enchant is BlueprintArmorEnchantment && armorEnchants is not null && !itemEntityArmor.HasEnchantment(enchant))
                {
                    unitPartEnchantPoolData.AddEnchant(itemEntityArmor, EnchantPool, enchant, Context, duration);
                    numA -= addBondProperty.Enchant.EnchantmentCost;
                }
                else continue;
                num -= addBondProperty.Enchant.EnchantmentCost;
                if (num <= 0) break;
            }
            int num2W = Math.Min(6, numW);
            num2W--;
            int num2A = Math.Min(6, numW);
            num2A--;
            if (weaponEnchants is not null)
                for (int i = num2W; i < 0; i--)
                {
                    if (DefaultEnchantmentsWeapon[i] is not null)
                    {
                        unitPartEnchantPoolData.AddEnchant(itemEntityWeapon, EnchantPool, DefaultEnchantmentsWeapon[num2W], Context, duration);
                        break;
                    }
                }
            if (armorEnchants is not null)
                for (int i = num2A; i < 0; i--)
                {
                    if (DefaultEnchantmentsArmor[num2A] is not null)
                    {
                        unitPartEnchantPoolData.AddEnchant(itemEntityArmor, EnchantPool, DefaultEnchantmentsArmor[num2A], Context, duration);
                        break;
                    }
                }
        }
    }

    [HarmonyPatch(typeof(ItemEntity))]
    public static class ExternalEnchantmentsAddFactsFix
    {
        [HarmonyPatch(nameof(ItemEntity.AddEnchantment))]
        [HarmonyPostfix]
        public static void OnAdded(ItemEntity __instance, BlueprintItemEnchantment blueprint, MechanicsContext parentContext = null)
        {
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log($"ExternalEnchantmentsAddFactsFix OnAdded - parentContext is not null? {parentContext is not null}. Array has any entries? {blueprint.Components?.Select(c => c as AddFactToEquipmentWielder)?.Where(c => c is not null)?.Any().ToString() ?? "False"}"); 
#endif
            IEnumerable<AddFactToEquipmentWielder> arr = blueprint.Components?.Select(c => c as AddFactToEquipmentWielder).Where(c => c is not null);
            if (!arr.Any()) return;
            __instance.m_FactsAppliedToWielder.EmptyIfNull();
            EntityFact f;
            foreach (AddFactToEquipmentWielder fact in arr)
            {
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log("fact is " + fact?.name); 
#endif
                try
                {
                    if (fact.Fact is null)
                    {
                        Comment.Warning("AddFactToEquipmentWielder component on {0} enchantment blueprint with guid {1} has null Fact.", fact.OwnerBlueprint.name, fact.OwnerBlueprint.AssetGuid);
                        continue;
                    }
                }
                catch
                {
                    Comment.Error("Failed to resolve the Fact reference on the AddFactToEquipmentWielder component of {0} enchantment blueprint with guid {1}", fact.OwnerBlueprint.name, fact.OwnerBlueprint.AssetGuid);
                }
                
                f = __instance.Wielder?.Unit.AddFact(fact.Fact, parentContext);
                if (f is not null)
                {
                    f.SetSourceItem(__instance);
                    __instance.m_FactsAppliedToWielder = __instance.m_FactsAppliedToWielder.AddToArray(f);

#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log("added the fact " + fact.Fact.name); 
#endif
                }
            }
        }

        [HarmonyPatch(nameof(ItemEntity.OnEnchantmentRemoved))]
        [HarmonyPrefix]
        public static void OnRemoved(ItemEntity __instance, ItemEnchantment enchantment)
        {
            IEnumerable<AddFactToEquipmentWielder> arr = enchantment.BlueprintComponents.Select(c => c as AddFactToEquipmentWielder).Where(c => c is not null);
            if (!arr.Any() || __instance.Wielder is null) return;
            ItemEntity item = null;
            if (__instance is not ItemEntityShield shield) item = __instance;
            else if (enchantment.Blueprint is BlueprintWeaponEnchantment) item = shield.WeaponComponent;
            else if (enchantment.Blueprint is BlueprintEquipmentEnchantment) item = shield.ArmorComponent;
            if (item is null) Comment.Warning("When removing enchantment {0} from an item {1} on unit {2}, " +
                    "it was impossible to resolve the item.",
                    enchantment.Blueprint.m_EnchantName, item.Blueprint.name, __instance.Wielder.CharacterName);
            if (item.m_FactsAppliedToWielder is null || item.m_FactsAppliedToWielder.Length == 0)
            {
                Comment.Warning("When removing enchantment {0} from item {1} on unit {2}, " +
                    "an AddFactToEquipmentWielder component was found on the enchantment blueprint," +
                    "but no m_FactsAppliedToWielder was found on the item.",
                    enchantment.Blueprint.m_EnchantName, item.Blueprint.name, __instance.Wielder.CharacterName);
#if DEBUG
                if (Settings.Debug.GetValue())
                {
                    if (item.m_FactsAppliedToWielder is null)
                        Comment.Log("m_FactsAppliedToWielder is null");
                    else if (item.m_FactsAppliedToWielder.Length == 0)
                        Comment.Log("m_FactsAppliedToWielder.Length == 0");
                } 
#endif
                return;
            };
            EntityFact f;
            foreach(var component in arr)
            {
                try
                {
                    if (component.Fact is null)
                    {
                        Comment.Warning("When removing enchantment {0} from item {1} on unit {2}, " +
                        "an AddFactToEquipmentWielder component with name {3} has null cached fact.",
                        enchantment.Blueprint.m_EnchantName, __instance.Blueprint.m_DisplayNameText, __instance.Wielder.CharacterName, component.name);
                        continue;
                    }
                }
                catch
                {
                    Comment.Warning("When removing enchantment {0} from item {1} on unit {2}, " +
                        "an AddFactToEquipmentWielder component with name {3} has null cached fact.",
                        enchantment.Blueprint.m_EnchantName, __instance.Blueprint.m_DisplayNameText, __instance.Wielder.CharacterName, component.name);
                    continue;
                }
                f = item.m_FactsAppliedToWielder.FindOrDefault(x => x.Blueprint == component.Fact);
                if (f is null)
                {
                    Comment.Warning("When removing enchantment {0} from item {1} on unit {2}, " +
                        "an AddFactToEquipmentWielder component with name {3} has  fact.",
                        enchantment.Blueprint.m_EnchantName, __instance.Blueprint.m_DisplayNameText, __instance.Wielder.CharacterName, component.name);
                    continue;
                }
                __instance.Wielder.RemoveFact(f);
                List<EntityFact> l = item.m_FactsAppliedToWielder.ToList();
                l.Remove(f);
                item.m_FactsAppliedToWielder = l.ToArray();
            }
        }
    }
    
}
