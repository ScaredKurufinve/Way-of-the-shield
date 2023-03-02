using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Way_of_the_shield.NewComponents;
using static Way_of_the_shield.Main;

namespace Way_of_the_shield.NewFeatsAndAbilities.SacredShieldFeatures
{
    public class SacredShieldBond
    {
        public static BlueprintFeature SSFeature;
        public static BlueprintFeature BondPlus2;
        public static BlueprintFeature BondPlus3;
        public static BlueprintFeature BondPlus4;
        public static BlueprintFeature BondPlus5;
        public static BlueprintFeature BondPlus6;
        public static BlueprintFeature AdditionalUsesFeature; 

        public static void CreateSacredShieldBond()
        {
            string circ = "when creating Sacred Shield bond";
            Sprite Icon = LoadIcon("SacredShieldBond");
            LocalizedString featureName = new() { Key = "ShieldSacredBondFeature_DisplayName" };
            LocalizedString featureDesc = new() { Key = "ShieldSacredBondFeature_DisplayName" };
            LocalizedString featureDescMod = new() { Key = "ShieldSacredBondFeature_DescriptionModified" };
            LocalizedString featureDescShort = new() { Key = "ShieldSacredBondFeature_ShortDescription" };

            #region Create Shield Sacred Bond Switch Ability blueprint
            BlueprintAbility ShieldSacredBondSwitchAbillity = new()
            {
                AssetGuid = new(new Guid("61fd6fcec7664d67800cd8b2da12c31e")),
                name = modName + "_ShieldSacredBondSwitchAbillity",
                m_DisplayName = featureName,
                m_Description = featureDesc,
                m_DescriptionShort = featureDescShort,
                LocalizedDuration = new LocalizedString() { Key = "Empty"},
                LocalizedSavingThrow = new LocalizedString() { m_Key = "Empty"},
                m_Icon = Icon,
                ActionType = UnitCommand.CommandType.Standard,
                Range = AbilityRange.Personal,
                EffectOnAlly = AbilityEffectOnUnit.Helpful,
                Type = AbilityType.Supernatural
            };
            #region create temprorary enhancement enchants
            BlueprintWeaponEnchantment TemporaryWeaponEnhancement6 = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("b35dc9414232480fae269110209bd736")),
                name = modName + "_TemporaryEnhancement6",
                m_EnchantName = new LocalizedString() { Key = "TemporaryEnhancement6_EnchantName", },
                m_Description = new LocalizedString() { Key = "TemporaryEnhancement6_Description", m_ShouldProcess = true },
                m_EnchantmentCost = 6,
            };
            TemporaryWeaponEnhancement6.AddComponent(new WeaponEnhancementBonus() { EnhancementBonus = 6, Stack = true });
            TemporaryWeaponEnhancement6.AddToCache();
            BlueprintWeaponEnchantment TemporaryArmorEnhancement6 = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("1d1d494f523f4c4ebcdb74793b336b3e")),
                name = modName + "_TemporaryArmorEnhancementBonus6",
                m_EnchantName = new LocalizedString() { Key = "TemporaryArmorEnhancementBonus6_EnchantName" },
                m_Description = new LocalizedString() { Key = "TemporaryArmorEnhancementBonus6_Description", m_ShouldProcess = true },
                m_EnchantmentCost = 6,
            };
            TemporaryArmorEnhancement6.AddComponent(new ArmorEnhancementBonus() { EnhancementValue = 6});
            TemporaryArmorEnhancement6.AddComponent(new AdvanceArmorStats() { ArmorCheckPenaltyShift = 1 });
            TemporaryArmorEnhancement6.AddToCache();
            #endregion
            BlueprintArmorEnchantmentReference[] temporaryArmorEnchantsArray = new BlueprintArmorEnchantmentReference[5]
            {
                ResourcesLibrary.TryGetBlueprint<BlueprintArmorEnchantment>("1d9b60d57afb45c4f9bb0a3c21bb3b98")?.ToReference<BlueprintArmorEnchantmentReference>(),
                ResourcesLibrary.TryGetBlueprint<BlueprintArmorEnchantment>("d45bfd838c541bb40bde7b0bf0e1b684")?.ToReference<BlueprintArmorEnchantmentReference>(),
                ResourcesLibrary.TryGetBlueprint<BlueprintArmorEnchantment>("51c51d841e9f16046a169729c13c4d4f")?.ToReference<BlueprintArmorEnchantmentReference>(),
                ResourcesLibrary.TryGetBlueprint<BlueprintArmorEnchantment>("a23bcee56c9fcf64d863dafedb369387")?.ToReference<BlueprintArmorEnchantmentReference>(),
                ResourcesLibrary.TryGetBlueprint<BlueprintArmorEnchantment>("15d7d6cbbf56bd744b37bbf9225ea83b")?.ToReference<BlueprintArmorEnchantmentReference>(),
            };
            BlueprintWeaponEnchantmentReference[] temporaryWeaponEnchantsArray = new BlueprintWeaponEnchantmentReference[5]
            {
                ResourcesLibrary.TryGetBlueprint<BlueprintWeaponEnchantment>("d704f90f54f813043a525f304f6c0050")?.ToReference<BlueprintWeaponEnchantmentReference>(),
                ResourcesLibrary.TryGetBlueprint<BlueprintWeaponEnchantment>("9e9bab3020ec5f64499e007880b37e52")?.ToReference<BlueprintWeaponEnchantmentReference>(),
                ResourcesLibrary.TryGetBlueprint<BlueprintWeaponEnchantment>("d072b841ba0668846adeb007f623bd6c")?.ToReference<BlueprintWeaponEnchantmentReference>(),
                ResourcesLibrary.TryGetBlueprint<BlueprintWeaponEnchantment>("6a6a0901d799ceb49b33d4851ff72132")?.ToReference<BlueprintWeaponEnchantmentReference>(),
                ResourcesLibrary.TryGetBlueprint<BlueprintWeaponEnchantment>("746ee366e50611146821d61e391edf16")?.ToReference<BlueprintWeaponEnchantmentReference>(),
            };
            if (BuffSacredShieldEnhacementArray.GetValue())
            {
                temporaryArmorEnchantsArray = temporaryArmorEnchantsArray.Append(TemporaryArmorEnhancement6.ToReference<BlueprintArmorEnchantmentReference>()).ToArray();
                temporaryWeaponEnchantsArray = temporaryWeaponEnchantsArray.Append(TemporaryWeaponEnhancement6.ToReference<BlueprintWeaponEnchantmentReference>()).ToArray();
                
            }
            ShieldSacredBondSwitchAbillity.AddComponent(
                new AbilityEffectRunAction()
                {
                    Actions = new()
                    {
                        Actions = new GameAction[1]
                        {
                            new Conditional()
                            {
                                ConditionsChecker = new()
                                {
                                    Conditions = new Condition[1]
                                    {
                                        new ContextConditionIsShieldEquipped()
                                    }
                                },
                                IfFalse = new ActionList()
                                {
                                    Actions = new GameAction[]{ }
                                },
                                IfTrue = new ActionList()
                                {
                                    Actions = new GameAction[]
                                    {
                                        new ContextActionShieldGeneralEnchantPool()
                                        {
                                            m_DefaultEnchantmentsArmor = temporaryArmorEnchantsArray,
                                            m_DefaultEnchantmentsWeapon = temporaryWeaponEnchantsArray,
                                            DurationValue = new ContextDurationValue()
                                            {
                                                Rate = DurationRate.Minutes,
                                                DiceType = DiceType.Zero,
                                                DiceCountValue = new(),
                                                BonusValue = new()
                                                {
                                                    ValueType = ContextValueType.Rank,
                                                    ValueRank = AbilityRankType.DamageBonus,
                                                }
                                            },
                                            EnchantPool = EnchantPoolExtensions.DivineShield
                                        }
                                    }
                                },
                            }
                        }
                    }
                });
            ShieldSacredBondSwitchAbillity.AddComponent(
                new ContextRankConfig()
                {
                    m_Progression = ContextRankProgression.AsIs,
                    m_BaseValueType = ContextRankBaseValueType.SummClassLevelWithArchetype,
                    m_Class = new BlueprintCharacterClassReference[1] { new BlueprintCharacterClassReference() { deserializedGuid = new(new Guid("bfa11238e7ae3544bbeb4d0b92e897ec")) } },
                    Archetype = new BlueprintArchetypeReference() { deserializedGuid = new(new Guid("56F19F65E28B4C3FB03E425FB047E08A")) }
                });
            if (RetrieveBlueprint("7ff088ab58c69854b82ea95c2b0e35b4", out BlueprintAbility WeaponBondSwitchAbility, "WeaponBondSwitchAbility", circ))
            {
                if (WeaponBondSwitchAbility.Components.TryFind(c => c is AbilityResourceLogic, out BlueprintComponent ARL))
                    ShieldSacredBondSwitchAbillity.AddComponent(ARL);
                if (WeaponBondSwitchAbility.Components.TryFind(c => c is AbilityCasterAlignment, out BlueprintComponent ACA))
                    ShieldSacredBondSwitchAbillity.AddComponent(ACA);
                IEnumerable<BlueprintComponent> ASF = WeaponBondSwitchAbility.Components.Where(c => c is AbilitySpawnFx);
                foreach (BlueprintComponent component in ASF) ShieldSacredBondSwitchAbillity.AddComponent(component);
            };
            ShieldSacredBondSwitchAbillity.AddToCache();
            #endregion
            #region create Bashing enchantment
            BlueprintWeaponEnchantment BashingEnchantment = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("c9534ca2e5274888ba61761a78945c52")),
                name = modName + "_BashingEnchantment",
                m_EnchantmentCost = 1,
                m_EnchantName = new() { Key = "BashingEnchantment_EnchantName" },
                m_Description = new() { Key = "BashingEnchantment_Description" }
            };
            BashingEnchantment.AddComponent(new MeleeWeaponSizeChange() { SizeCategoryChange = 3 });
            BashingEnchantment.AddToCache();
            if (RetrieveBlueprint("d7fb623f94b42304db03645c6fdef245", out BlueprintItemWeapon BashingShieldWeapon, "BashingShieldWeapon", circ))
            {
                BashingShieldWeapon.m_DamageDice = new(1, DiceType.D4);
                BashingShieldWeapon.m_Enchantments = BashingShieldWeapon.m_Enchantments.Append(BashingEnchantment.ToReference<BlueprintWeaponEnchantmentReference>()).ToArray();
            }

            #endregion
            #region Create Bashing bond
            BlueprintBuff BashingBondBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("9e24dd8ac237426aa30c00e7ec68e90f")),
                name = modName + "_BashingBondBuff",
                m_Flags = BlueprintBuff.Flags.HiddenInUi | BlueprintBuff.Flags.StayOnDeath,
                IsClassFeature = true,
            };
            BashingBondBuff.AddComponent(
                new AddBondProperty() 
                { 
                    EnchantPool = EnchantPoolExtensions.DivineShield,
                    m_Enchant = BashingEnchantment.ToReference<BlueprintItemEnchantmentReference>() 
                });
            BashingBondBuff.AddToCache();
            BlueprintActivatableAbility BashingBondActivatableAbility = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("3a1706ab6d984ca1b5ef4e3a41fe38e5")),
                name = modName + "_BashingBondActivatableAbility",
                m_DisplayName = new() { Key = "BashingBondActivatableAbility_DisplayName" },
                m_Description = new() { Key = "BashingBondActivatableAbility_Description" },
                m_Buff = BashingBondBuff.ToReference<BlueprintBuffReference>(),
                Group = ActivatableAbilityGroup.DivineWeaponProperty,
                WeightInGroup = 1,
                DeactivateImmediately = true,
                m_Icon = LoadIcon("HolyShield_Bashing"),
                ActivationType = AbilityActivationType.Immediately
            };
            BashingBondActivatableAbility.AddToCache();
            #endregion
            #region Create Arrow Catching bond
            Sprite ArrowCatchingIcon = LoadIcon("HolyShield_ArrowCatching");
            ArrowCatching.CreateArrowCatchingEnchantment();
            BlueprintBuff ArrowCatchingBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("51e0793643d34fdaa8a569887b990c8c")),
                name = modName + "_ArrowCatchingBuff",
                m_Flags = BlueprintBuff.Flags.HiddenInUi | BlueprintBuff.Flags.StayOnDeath,
                IsClassFeature = true,
            };
            ArrowCatchingBuff.AddComponent(
                new AddBondProperty()
                {
                    EnchantPool = EnchantPoolExtensions.DivineShield,
                    m_Enchant = ArrowCatching.Enchantment.ToReference<BlueprintItemEnchantmentReference>()
                });
            ArrowCatchingBuff.AddToCache();
            BlueprintActivatableAbility ArrowCatchingActivatableAbility = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("74195ff512e94fed93d07e5660130f37")),
                name = modName + "_ArrowCatchingActivatableAbility",
                m_DisplayName = new() { Key = "ArrowCatchingActivatableAbility_DisplayName" },
                m_Description = new() { Key = "ArrowCatchingActivatableAbility_Description" },
                m_DescriptionShort = new() { Key = "ArrowCatchingActivatableAbility_DescriptionShort" },
                m_Buff = ArrowCatchingBuff.ToReference<BlueprintBuffReference>(),
                Group = ActivatableAbilityGroup.DivineWeaponProperty,
                WeightInGroup = 1,
                DeactivateImmediately = true,
                ActivationType = AbilityActivationType.Immediately,
                m_Icon = ArrowCatchingIcon,
            };
            ArrowCatchingActivatableAbility.AddToCache();
            #endregion
            #region Create Fortification bonds
            RetrieveBlueprint("1e69e9029c627914eb06608dad707b36", out BlueprintArmorEnchantment Fortification25Enchant, "Fortification25Enchant", circ);
            Sprite Fortification25Icon = LoadIcon("SacredArmorEnchantFortification25Buff");
            BlueprintBuff Fortification25BondBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("45629f3d210341ddbbc2a1acc5a94219")),
                name = modName + "_Fortification25BondBuff",
                m_Flags = BlueprintBuff.Flags.HiddenInUi | BlueprintBuff.Flags.StayOnDeath,
                IsClassFeature = true,
            };
            Fortification25BondBuff.AddComponent(
                new AddBondProperty()
                {
                    EnchantPool = EnchantPoolExtensions.DivineShield,
                    m_Enchant = Fortification25Enchant?.ToReference<BlueprintItemEnchantmentReference>()
                });
            Fortification25BondBuff.AddToCache();
            BlueprintActivatableAbility Fortification25BondActivatableAbility = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("d884f15665f947dd98602d67b46ea389")),
                name = modName + "_Fortification25BondActivatableAbility",
                m_DisplayName = new() { Key = "Fortification25BondActivatableAbility_DisplayName" },
                m_Description = new() { Key = "Fortification25BondActivatableAbility_Description", m_ShouldProcess = true },
                m_Buff = Fortification25BondBuff.ToReference<BlueprintBuffReference>(),
                Group = ActivatableAbilityGroup.DivineWeaponProperty,
                DeactivateImmediately = true,
                WeightInGroup = 1,
                ActivationType = AbilityActivationType.Immediately,
                m_Icon = Fortification25Icon,
            };
            Fortification25BondActivatableAbility.AddToCache();
            RetrieveBlueprint("62ec0b22425fb424c82fd52d7f4c02a5", out BlueprintArmorEnchantment Fortification50Enchant, "Fortification50Enchant", circ);
            Sprite Fortification50Icon = LoadIcon("SacredArmorEnchantFortification50Buff");
            BlueprintBuff Fortification50BondBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("78d016818ba34066a02c14e3b5d76ae1")),
                name = modName + "_Fortification50BondBuff",
                m_Flags = BlueprintBuff.Flags.HiddenInUi | BlueprintBuff.Flags.StayOnDeath,
                IsClassFeature = true,
                
            };
            Fortification50BondBuff.AddComponent(
                new AddBondProperty()
                {
                    EnchantPool = EnchantPoolExtensions.DivineShield,
                    m_Enchant = Fortification50Enchant?.ToReference<BlueprintItemEnchantmentReference>()
                });
            Fortification50BondBuff.AddToCache();
            BlueprintActivatableAbility Fortification50BondActivatableAbility = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("020d22f531d54b3699e93774cb5336a9")),
                name = modName + "_Fortification50BondActivatableAbility",
                m_DisplayName = new() { Key = "Fortification50BondActivatableAbility_DisplayName" },
                m_Description = new() { Key = "Fortification50BondActivatableAbility_Description", m_ShouldProcess = true },
                m_Buff = Fortification50BondBuff.ToReference<BlueprintBuffReference>(),
                Group = ActivatableAbilityGroup.DivineWeaponProperty,
                WeightInGroup = 3,
                DeactivateImmediately = true,
                ActivationType = AbilityActivationType.Immediately,
                m_Icon = Fortification50Icon,
            };
            Fortification50BondActivatableAbility.AddToCache();
            RetrieveBlueprint("9b1538c732e06544bbd955fee570a2be", out BlueprintArmorEnchantment Fortification75Enchant, "Fortification75Enchant", circ);
            Sprite Fortification75Icon = LoadIcon("SacredArmorEnchantFortification75Choice");
            BlueprintBuff Fortification75BondBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("b04ef8e30d284a8c8c64b739d35d3276")),
                name = modName + "_Fortification75BondBuff",
                m_Flags = BlueprintBuff.Flags.HiddenInUi | BlueprintBuff.Flags.StayOnDeath,
                IsClassFeature = true,
            };
            Fortification75BondBuff.AddComponent(
                new AddBondProperty()
                {
                    EnchantPool = EnchantPoolExtensions.DivineShield,
                    m_Enchant = Fortification75Enchant?.ToReference<BlueprintItemEnchantmentReference>()
                });
            Fortification75BondBuff.AddToCache();
            BlueprintActivatableAbility Fortification75BondActivatableAbility = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("37332ef2cc8147f8ae4650eae4f4c2ad")),
                name = modName + "_Fortification75BondActivatableAbility",
                m_DisplayName = new() { Key = "Fortification75BondActivatableAbility_DisplayName" },
                m_Description = new() { Key = "Fortification75BondActivatableAbility_Description", m_ShouldProcess = true },
                m_Buff = Fortification50BondBuff.ToReference<BlueprintBuffReference>(),
                Group = ActivatableAbilityGroup.DivineWeaponProperty,
                DeactivateImmediately = true,
                WeightInGroup = 5,
                ActivationType = AbilityActivationType.Immediately,
                m_Icon = Fortification75Icon,
            };
            Fortification75BondActivatableAbility.AddToCache();
            #endregion
            #region Create Spell Resistance 
            RetrieveBlueprint("4bc20fd0e137e1645a18f030b961ef3d", out BlueprintArmorEnchantment SpellResistance13Enchant, "SpellResistance13Enchant", circ);
            Sprite SpellResistance13Icon = LoadIcon("SacredArmorEnchantSpellResistance13Buff");
            BlueprintBuff SpellResistance13BondBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("44f61997371845c9a6899d8ca46ff167")),
                name = modName + "_EnergyResistance13BondBuff",
                m_Flags = BlueprintBuff.Flags.HiddenInUi | BlueprintBuff.Flags.StayOnDeath,
                IsClassFeature = true,
            };
            SpellResistance13BondBuff.AddComponent(
                new AddBondProperty()
                {
                    EnchantPool = EnchantPoolExtensions.DivineShield,
                    m_Enchant = SpellResistance13Enchant.ToReference<BlueprintItemEnchantmentReference>()
                });
            SpellResistance13BondBuff.AddToCache();
            BlueprintActivatableAbility SpellResistance13BondActivatableAbility = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("712509092c3d4121a61d5d5b041dcf42")),
                name = modName + "_EnergyResistance13BondActivatableAbility",
                m_DisplayName = new() { Key = "SpellResistance13BondActivatableAbility_DisplayName" },
                m_Description = new() { Key = "SpellResistance13BondActivatableAbility_Description" },
                m_Buff = SpellResistance13BondBuff?.ToReference<BlueprintBuffReference>(),
                Group = ActivatableAbilityGroup.DivineWeaponProperty,
                WeightInGroup = 2,
                DeactivateImmediately = true,
                ActivationType = AbilityActivationType.Immediately,
                m_Icon = SpellResistance13Icon,
            };
            SpellResistance13BondActivatableAbility.AddToCache();

            RetrieveBlueprint("ad0f81f6377180d4292a2316efb950f2", out BlueprintArmorEnchantment SpellResistance15Enchant, "SpellResistance15Enchant", circ);
            Sprite SpellResistance15Icon = LoadIcon("SacredArmorEnchantSpellResistance15Buff");
            BlueprintBuff SpellResistance15BondBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("179b9a1b07ff4695b6c0961019551efe")),
                name = modName + "_SpellResistance15BondBuff",
                m_Flags = BlueprintBuff.Flags.HiddenInUi | BlueprintBuff.Flags.StayOnDeath,
                IsClassFeature = true,
            };
            SpellResistance15BondBuff.AddComponent(
                new AddBondProperty()
                {
                    EnchantPool = EnchantPoolExtensions.DivineShield,
                    m_Enchant = SpellResistance15Enchant?.ToReference<BlueprintItemEnchantmentReference>()
                });
            SpellResistance15BondBuff.AddToCache();
            BlueprintActivatableAbility SpellResistance15BondActivatableAbility = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("37d988c3331c443983b9bcfe5dcd2892")),
                name = modName + "_SpellResistance15BondActivatableAbility",
                m_DisplayName = new() { Key = "SpellResistance15BondActivatableAbility_DisplayName" },
                m_Description = new() { Key = "EnergyResistance15BondActivatableAbility_Description" },
                m_Buff = SpellResistance15BondBuff.ToReference<BlueprintBuffReference>(),
                Group = ActivatableAbilityGroup.DivineWeaponProperty,
                WeightInGroup = 3,
                DeactivateImmediately = true,
                ActivationType = AbilityActivationType.Immediately,
                m_Icon = SpellResistance15Icon,
            };
            SpellResistance15BondActivatableAbility.AddToCache();

            RetrieveBlueprint("49fe9e1969afd874181ed7613120c250", out BlueprintArmorEnchantment SpellResistance17Enchant, "SpellResistance17Enchant", circ);
            Sprite SpellResistance17Icon = LoadIcon("SacredArmorEnchantSpellResistance17Buff");
            BlueprintBuff SpellResistance17BondBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("0373f4dc8b274eee815a666dfe463670")),
                name = modName + "_SpellResistance17BondBuff",
                m_Flags = BlueprintBuff.Flags.HiddenInUi | BlueprintBuff.Flags.StayOnDeath,
                IsClassFeature = true,
            };
            SpellResistance17BondBuff.AddComponent(
                new AddBondProperty()
                {
                    EnchantPool = EnchantPoolExtensions.DivineShield,
                    m_Enchant = SpellResistance17Enchant?.ToReference<BlueprintItemEnchantmentReference>()
                });
            SpellResistance17BondBuff.AddToCache();
            BlueprintActivatableAbility SpellResistance17BondActivatableAbility = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("625ace3fcc924ee48cd2ed4c37f2eeab")),
                name = modName + "_SpellResistance17BondActivatableAbility",
                m_DisplayName = new() { Key = "SpellResistance17BondActivatableAbility_DisplayName" },
                m_Description = new() { Key = "SpellResistance17BondActivatableAbility_Description" },
                m_Buff = SpellResistance17BondBuff.ToReference<BlueprintBuffReference>(),
                Group = ActivatableAbilityGroup.DivineWeaponProperty,
                WeightInGroup = 4,
                DeactivateImmediately = true,
                ActivationType = AbilityActivationType.Immediately,
                m_Icon = SpellResistance17Icon,
            };
            SpellResistance17BondActivatableAbility.AddToCache();

            RetrieveBlueprint("583938eaafc820f49ad94eca1e5a98ca", out BlueprintArmorEnchantment SpellResistance19Enchant, "SpellResistance19Enchant", circ);
            Sprite SpellResistance19Icon = LoadIcon("SacredArmorEnchantSpellResistance19Buff");
            BlueprintBuff SpellResistance19BondBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("46431fa695cb47a48c316a6d1eac91b7")),
                name = modName + "_SpellResistance19BondBuff",
                m_Flags = BlueprintBuff.Flags.HiddenInUi | BlueprintBuff.Flags.StayOnDeath,
                IsClassFeature = true,
            };
            SpellResistance19BondBuff.AddComponent(
                new AddBondProperty()
                {
                    EnchantPool = EnchantPoolExtensions.DivineShield,
                    m_Enchant = SpellResistance19Enchant?.ToReference<BlueprintItemEnchantmentReference>()
                });
            SpellResistance19BondBuff.AddToCache();
            BlueprintActivatableAbility SpellResistance19BondActivatableAbility = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("566cbefbe87548b3859cd79292cd26d6")),
                name = modName + "_SpellResistance19BondActivatableAbility",
                m_DisplayName = new() { Key = "SpellResistance19BondActivatableAbility_DisplayName" },
                m_Description = new() { Key = "SpellResistance19BondActivatableAbility_Description" },
                m_Buff = SpellResistance19BondBuff.ToReference<BlueprintBuffReference>(),
                Group = ActivatableAbilityGroup.DivineWeaponProperty,
                WeightInGroup = 5,
                DeactivateImmediately = true,
                ActivationType = AbilityActivationType.Immediately,
                m_Icon = SpellResistance19Icon,
            };
            SpellResistance19BondActivatableAbility.AddToCache();
            #endregion
            #region Create Determination enchantment
            Sprite DeterminationIcon = LoadIcon("HolyShield_Determination");
            BlueprintBuff DeterminationBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("98d37c69ac834ea198ed6a7bc415706e")),
                name = modName + "_DeterminationBuff",
                FxOnRemove = new(),
                FxOnStart = new(),
                m_DisplayName = new LocalizedString() { m_Key = "DeterminationBuff_DisplayName" },
                m_Description = new LocalizedString() { m_Key = "DeterminationBuff_Description" },
                //m_DescriptionShort = new LocalizedString() { m_Key = "DeterminationBuff_ShortDescription" },
                Stacking = StackingType.Replace,
                m_Icon = DeterminationIcon,
            };
            DeterminationBuff.AddComponent(
                new DeathActions()
                { Actions = new ActionList(){ 
                    Actions = new GameAction[] 
                    { 
                        new ContextActionBreathOfLife()
                        {
                            Value = new()
                            {
                                DiceType = DiceType.Zero,
                                DiceCountValue = 0,
                                BonusValue = 50
                            }
                        },
                        new ContextActionRemoveSelf()
                        {

                        }
                    }}
                });
            DeterminationBuff.AddToCache();
            BlueprintArmorEnchantment DeterminationEnchantment = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("1a9efbfb0a4840e4aa45bc2b5ea4cbaa")),
                name = modName + "_DeterminationEnchantment",
                m_EnchantName = new LocalizedString() { m_Key = "DeterminationEnchantment_EnchantName" },
                m_Description = new LocalizedString() { m_Key = "DeterminationEnchantment_Description" },
                m_EnchantmentCost = 5
            };
            DeterminationEnchantment.AddComponent(new AddFactToEquipmentWielder() { m_Fact = DeterminationBuff.ToReference<BlueprintUnitFactReference>() });
            DeterminationEnchantment.AddToCache();
            #endregion
            #region Create Determination Bond
            BlueprintBuff DeterminationBondBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("53054a1e378b42ca8e733b992d26bb9b")),
                name = modName + "_DeterminationBondBuff",
                m_Flags = BlueprintBuff.Flags.HiddenInUi | BlueprintBuff.Flags.StayOnDeath,
                IsClassFeature = true,
            };
            DeterminationBondBuff.AddComponent(
                new AddBondProperty()
                {
                    EnchantPool = EnchantPoolExtensions.DivineShield,
                    m_Enchant = DeterminationEnchantment.ToReference<BlueprintItemEnchantmentReference>()
                });
            DeterminationBondBuff.AddToCache();
            BlueprintActivatableAbility DeterminationBondActivatableAbility = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("9083f9b2e8794150bf6cc427d43c9917")),
                name = modName + "_DeterminationBondActivatableAbility",
                m_DisplayName = new() { Key = "DeterminationBondActivatableAbility_DisplayName" },
                m_Description = new() { Key = "DeterminationBondActivatableAbility_Description" },
                m_Buff = DeterminationBondBuff.ToReference<BlueprintBuffReference>(),
                Group = ActivatableAbilityGroup.DivineWeaponProperty,
                WeightInGroup = 5,
                DeactivateImmediately = true,
                ActivationType = AbilityActivationType.Immediately,
                m_Icon = DeterminationIcon,
            };
            DeterminationBondActivatableAbility.AddToCache();
            #endregion
            #region Create Rallying enchantment
            Sprite RallyingIcon = LoadIcon("HolyShield_Rallying");
            BlueprintBuff RallyingBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("6694bf0ea33d4b5c89912f0a59dddb06")),
                name = modName + "_RallyingBuff",
                m_DisplayName = new() { m_Key = "RallyingBuff_DisplayName" },
                m_Description = new() { m_Key = "RallyingBuff_Description" },
                Stacking = StackingType.Replace,
                FxOnRemove = new(),
                m_Icon = RallyingIcon,
            };
            RallyingBuff.AddComponent(
                new SavingThrowBonusAgainstDescriptor()
                {
                    SpellDescriptor = new SpellDescriptorWrapper(SpellDescriptor.Fear),
                    ModifierDescriptor = ModifierDescriptor.Morale,
                    Value = 6,
                    Bonus = new()
                });
            RallyingBuff.AddToCache();

            BlueprintAbilityAreaEffect RallyingAreaEffect = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("ff38f0dd17d144a4a323e49e096febb6")),
                name = modName + "_RallyingAreaEffect",
                Size = new(30),
                Shape = AreaEffectShape.Cylinder,
                Fx = new(),
                m_TargetType = BlueprintAbilityAreaEffect.TargetType.Ally,
            };
            RallyingAreaEffect.AddComponent(
                new AbilityAreaEffectBuff()
                {
                    m_Buff = RallyingBuff.ToReference<BlueprintBuffReference>(),
                    Condition = new()
                });
            RallyingAreaEffect.AddToCache();

            BlueprintBuff RallyingAuraBuff = new()
            {

                AssetGuid = new BlueprintGuid(new Guid("f260e0516f8443a6b90efbc1b7ccfd2f")),
                name = modName + "_RallyingBuff",
                Stacking = StackingType.Replace,
                m_Flags = BlueprintBuff.Flags.StayOnDeath | BlueprintBuff.Flags.HiddenInUi,
                FxOnRemove = new(),
            };
            RallyingAuraBuff.AddComponent(
                new AddAreaEffect()
                {
                    m_AreaEffect = RallyingAreaEffect.ToReference<BlueprintAbilityAreaEffectReference>()
                });
            RallyingAuraBuff.AddToCache();

            

            BlueprintArmorEnchantment RallyingEnchantment = new()
            {
                AssetGuid = new(new Guid("858a26f34cf14c2eb691bfeae6794965")),
                name = modName + "_RallyingEnchantment",
                m_EnchantName = new() { m_Key = "RallyingEnchantment_EnchantName" },
                m_Description = new() { m_Key = "RallyingEnchantment_Description", m_ShouldProcess = true },
                m_EnchantmentCost = 2,
            };
            RallyingEnchantment.AddComponent(
                new AddFactToEquipmentWielder()
                {
                    m_Fact = RallyingAreaEffect.ToReference<BlueprintUnitFactReference>()
                });
            RallyingEnchantment.AddToCache();
            #endregion
            #region Create Rallying Bond
            BlueprintBuff RallyingBondBuff = new()
            {
                AssetGuid = new(new Guid("5c7943bfa60545a389dd6f0f21886070")),
                name = modName + "_RallyingBondBuff",
                m_Flags = BlueprintBuff.Flags.HiddenInUi | BlueprintBuff.Flags.StayOnDeath,
                IsClassFeature = true,
            };
            RallyingBondBuff.AddComponent(
                new AddBondProperty()
                {
                    EnchantPool = EnchantPoolExtensions.DivineShield,
                    m_Enchant = RallyingEnchantment.ToReference<BlueprintItemEnchantmentReference>()
                });
            RallyingBondBuff.AddToCache();
            BlueprintActivatableAbility RallyingBondActivatableAbility = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("da0e13b11c9649fc93fe4d35c02d6b67")),
                name = modName + "_RallyingBondActivatableAbility",
                m_DisplayName = new() { Key = "RallyingBondActivatableAbility_DisplayName" },
                m_Description = new() { Key = "RallyingBondActivatableAbility_Description" },
                m_Buff = RallyingBondBuff.ToReference<BlueprintBuffReference>(),
                Group = ActivatableAbilityGroup.DivineWeaponProperty,
                WeightInGroup = 2,
                DeactivateImmediately = true,
                ActivationType = AbilityActivationType.Immediately,
                m_Icon = RallyingIcon,
            };
            RallyingBondActivatableAbility.AddToCache();
            #endregion
            #region Create Shield Sacred Bond feature blueprint
            RetrieveBlueprint("3683d1af071c1744185ff93cba9db10b", out BlueprintAbilityResource WeaponBondResourse, "WeaponBondResourse", circ);
            BlueprintFeature ShieldSacredBondFeature = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("e02318fbf171403b9956b6dee9f5e6e5")),
                name = modName + "_ShieldSacredBondFeature",
                m_DisplayName = featureName,
                m_Description = featureDesc,
                m_DescriptionShort = featureDescShort,
                m_Icon = Icon,
                Ranks = 1,
                IsClassFeature = true,
            };
            if (BuffSacredShieldEnhacementArray.GetValue()) ShieldSacredBondFeature.m_Description = featureDescMod;
            if (WeaponBondResourse is not null) ShieldSacredBondFeature.AddComponent(new AddAbilityResources()
            {
                m_Resource = WeaponBondResourse.ToReference<BlueprintAbilityResourceReference>(),
                Amount = 0,
                RestoreAmount = true
            });
            ShieldSacredBondFeature.AddComponent(new AddFacts()
            {
                m_Facts = new BlueprintUnitFactReference[4] 
                {
                    ShieldSacredBondSwitchAbillity.ToReference<BlueprintUnitFactReference>(),
                    BashingBondActivatableAbility.ToReference<BlueprintUnitFactReference>(),
                    ArrowCatchingActivatableAbility.ToReference<BlueprintUnitFactReference>(),
                    Fortification25BondActivatableAbility.ToReference<BlueprintUnitFactReference>(),
                }
            });

            ShieldSacredBondFeature.AddToCache();
            SSFeature = ShieldSacredBondFeature;
            #endregion
            #region Create Bond Features
            BlueprintFeature Plus2 = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("8f806e0a50f1448aafb08f76819a6e5a")),
                name = modName + "_SacredBondPlus2Feature",
                m_DisplayName = new LocalizedString() { Key = "SacredBondPlus2Feature_DisplayName" },
                m_Description = new LocalizedString() { Key = "SacredBondPlus2Feature_Description" },
                //m_DescriptionShort = new LocalizedString() { Key = "SacredBondPlus2Feature_ShortDescription" },
                m_Icon = Icon,
                Ranks = 1,
                IsClassFeature = true,
            };
            Plus2.AddComponent(new IncreaseActivatableAbilityGroupSize() { Group = ActivatableAbilityGroup.DivineWeaponProperty});
            Plus2.AddComponent(new AddFacts()
            {
                m_Facts = new BlueprintUnitFactReference[]
                {
                    SpellResistance13BondActivatableAbility.ToReference<BlueprintUnitFactReference>(),
                }
            });
            Plus2.AddToCache();
            BondPlus2 = Plus2;
            BlueprintFeature Plus3 = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("15bb026605fc49e4bc6812ddfc3448b5")),
                name = modName + "_SacredBondPlus3Feature",
                m_DisplayName = new LocalizedString() { Key = "SacredBondPlus3Feature_DisplayName" },
                m_Description = new LocalizedString() { Key = "SacredBondPlus3Feature_Description" },
                //m_DescriptionShort = new LocalizedString() { Key = "SacredBondPlus3Feature_ShortDescription" },
                m_Icon = Icon,
                Ranks = 1,
                IsClassFeature = true,
            };
            Plus3.AddComponent(new IncreaseActivatableAbilityGroupSize() { Group = ActivatableAbilityGroup.DivineWeaponProperty });
            Plus3.AddComponent(new AddFacts() { 
                m_Facts = new BlueprintUnitFactReference[] 
                { 
                    Fortification50BondActivatableAbility.ToReference<BlueprintUnitFactReference>(),
                    SpellResistance15BondActivatableAbility.ToReference<BlueprintUnitFactReference>(),
                    RallyingBondActivatableAbility.ToReference<BlueprintUnitFactReference>(),
                } });
            Plus3.AddToCache();
            BondPlus3 = Plus3;
            BlueprintFeature Plus4 = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("e8485da5493f4ce1add3a319b15d4a89")),
                name = modName + "_SacredBondPlus4Feature",
                m_DisplayName = new LocalizedString() { Key = "SacredBondPlus4Feature_DisplayName" },
                m_Description = new LocalizedString() { Key = "SacredBondPlus4Feature_Description" },
                m_DescriptionShort = new LocalizedString() { Key = "SacredBondPlus4Feature_ShortDescription" },
                m_Icon = Icon,
                Ranks = 1,
                IsClassFeature = true,
            };
            Plus4.AddComponent(new IncreaseActivatableAbilityGroupSize() { Group = ActivatableAbilityGroup.DivineWeaponProperty });
            Plus4.AddComponent(new AddFacts()
            {
                m_Facts = new BlueprintUnitFactReference[]
                {
                    SpellResistance17BondActivatableAbility.ToReference<BlueprintUnitFactReference>(),
                }
            });
            Plus4.AddToCache();
            BondPlus4 = Plus4;
            BlueprintFeature Plus5 = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("9d3afe4f61a54048a7d85d2c973ee588")),
                name = modName + "_SacredBondPlus4Feature",
                m_DisplayName = new LocalizedString() { Key = "SacredBondPlus5Feature_DisplayName" },
                m_Description = new LocalizedString() { Key = "SacredBondPlus5Feature_Description" },
                m_DescriptionShort = new LocalizedString() { Key = "SacredBondPlus5Feature_ShortDescription" },
                m_Icon = Icon,
                Ranks = 1,
                IsClassFeature = true,
            };
            Plus5.AddComponent(new IncreaseActivatableAbilityGroupSize() { Group = ActivatableAbilityGroup.DivineWeaponProperty });
            Plus5.AddComponent(new AddFacts() 
            { 
                m_Facts = new BlueprintUnitFactReference[] 
                { 
                    Fortification50BondActivatableAbility.ToReference<BlueprintUnitFactReference>(),
                    DeterminationBondActivatableAbility.ToReference<BlueprintUnitFactReference>(),
                    SpellResistance19BondActivatableAbility.ToReference<BlueprintUnitFactReference>(),
                } 
            });
            Plus5.AddToCache();
            BondPlus5 = Plus5;
            BlueprintFeature Plus6 = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("b320616d77ec43aca955253e937be5df")),
                name = modName + "_SacredBondPlus4Feature",
                m_DisplayName = new LocalizedString() { Key = "SacredBondPlus6Feature_DisplayName" },
                m_Description = new LocalizedString() { Key = "SacredBondPlus6Feature_Description" },
                m_DescriptionShort = new LocalizedString() { Key = "SacredBondPlus6Feature_ShortDescription" },
                m_Icon = Icon,
                Ranks = 1,
                IsClassFeature = true,
            };
            Plus6.AddComponent(new IncreaseActivatableAbilityGroupSize() { Group = ActivatableAbilityGroup.DivineWeaponProperty });
            Plus6.AddToCache();
            BondPlus6 = Plus6;
            #endregion
            #region Create AdditionalUses
            LocalizedString AdditionalUsesDisplayName = new() { m_Key = "SacredShieldBondAdditionalUsesFeature_DisplayName" };
            LocalizedString AdditionalUsesDescription = new() { m_Key = "SacredShieldBondAdditionalUsesFeature_Description" };
            BlueprintFeature AdditionalUses = new()
            {
                m_DisplayName = AdditionalUsesDisplayName,
                m_Description = AdditionalUsesDescription,
                m_Icon = Icon,
                Ranks = 20,
                IsClassFeature = true,
            };
            AdditionalUses.AddComponent(
                new IncreaseResourceAmount() 
                {
                    Value = 1,
                    m_Resource = new() { deserializedGuid = BlueprintGuid.Parse("3683d1af071c1744185ff93cba9db10b") },
                });
            AdditionalUses.AddToCache("c807862eeacf40afb083e288ca539e1b", "SacredShieldBondAdditionalUsesFeature");
            AdditionalUsesFeature = AdditionalUses;
            #endregion
        }
    }
}
