using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using UnityEngine;
using Way_of_the_shield.NewComponents;
using static Way_of_the_shield.Main;
using static Way_of_the_shield.Utilities;

namespace Way_of_the_shield.NewFeatsAndAbilities.SacredShieldFeatures
{
    
    public class HolyShield
    {
        public static BlueprintFeature SHAbility4;
        public static BlueprintFeature SHAbility11;
        public static BlueprintFeature SHAbility20;
        public static void CreateHolyShieldAbilities()
        {
            Sprite Icon = LoadIcon("HolyShield");
            LocalizedString HolyShield4Feature_DisplayName = new() { m_Key = "HolyShield4Feature_DisplayName" };
            LocalizedString Empty = new() { Key = "" };
            #region Create TotalShieldBonus property blueprint
            BlueprintUnitProperty TotalShieldBonusUnitProperty = new()
            {
                AssetGuid = new BlueprintGuid( new Guid("f4978acab2cf4344930318ae423d26b3")),
                name = modName + "TotalShieldBonusUnitProperty"
            };
            TotalShieldBonusUnitProperty.AddComponent(new NewComponents.TotalShieldBonusGetter());
            TotalShieldBonusUnitProperty.AddToCache();
            #endregion
            #region Create HolyShieldEffectBuff blueprint
            AddStatBonusAbilityValue acBonus = new()
            {
                Descriptor = ModifierDescriptor.Shield,
                Stat = StatType.AC,
                Value = new()
                {
                    ValueType = ContextValueType.CasterCustomProperty,
                    m_CustomProperty = TotalShieldBonusUnitProperty.ToReference<BlueprintUnitPropertyReference>(),
                }
            };
            BlueprintBuff HolyShieldEffectBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("2d06dfdd653e417f8decb62f00e68d4c")),
                name = modName + "HolyShieldEffectBuff",
                m_DisplayName = HolyShield4Feature_DisplayName,
                m_Description = new LocalizedString() { Key = "HolyShieldEffectBuff_Description" },
                m_DescriptionShort = new LocalizedString() { Key = "HolyShieldEffectBuff_ShortDescription" },
                FxOnRemove = new(),
                FxOnStart = new(),
                m_Icon = Icon,
                IsClassFeature = true,
            };
            HolyShieldEffectBuff.AddComponent(acBonus);
            HolyShieldEffectBuff.AddToCache();
            #endregion
            #region create HolyShieldAreaEffect blueprints
            BlueprintAbilityAreaEffect HolyShield5AreaEffect = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("1c76cf0c5061449b90079515390a1afb")),
                name = modName + "HolyShield5AreaEffect",
                Fx = new(),
                m_TargetType = BlueprintAbilityAreaEffect.TargetType.Ally,
                Shape = AreaEffectShape.Cylinder,
                Size = new(5)
            };
            HolyShield5AreaEffect.AddComponent(new AbilityAreaEffectBuff()
            {
                Condition = new(),
                m_Buff = HolyShieldEffectBuff.ToReference<BlueprintBuffReference>()
            });
            HolyShield5AreaEffect.AddToCache();
            BlueprintAbilityAreaEffect HolyShield10AreaEffect = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("2819b139d1734ea8bccaf95cdd06b712")),
                name = modName + "HolyShield10AreaEffect",
                Fx = new(),
                m_TargetType = BlueprintAbilityAreaEffect.TargetType.Ally,
                Shape = AreaEffectShape.Cylinder,
                Size = new(10)
            };
            HolyShield10AreaEffect.AddComponent(new AbilityAreaEffectBuff()
            {
                Condition = new(),
                m_Buff = HolyShieldEffectBuff.ToReference<BlueprintBuffReference>()
            });
            HolyShield10AreaEffect.AddToCache();
            BlueprintAbilityAreaEffect HolyShield20AreaEffect = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("479f3c2f4e064672a4668f0056196c17")),
                name = modName + "HolyShield20AreaEffect",
                Fx = new(),
                m_TargetType = BlueprintAbilityAreaEffect.TargetType.Ally,
                Shape = AreaEffectShape.Cylinder,
                Size = new(20)
            };
            HolyShield20AreaEffect.AddComponent(new AbilityAreaEffectBuff()
            {
                Condition = new(),
                m_Buff = HolyShieldEffectBuff.ToReference<BlueprintBuffReference>()
            });
            HolyShield20AreaEffect.AddToCache();
            #endregion
            #region Create HolyShieldAura buff blueprints
            BlueprintBuff HolyShield5Buff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("45e81afeee5d472ab5f2d84da805638b")),
                name = modName + "HolyShieldBuff",
                m_DisplayName = HolyShield4Feature_DisplayName,
                m_Description = new LocalizedString() { Key = "HolyShieldBuff5_Description", m_ShouldProcess = true },
                m_DescriptionShort = new LocalizedString() { Key = "HolyShieldBuff5_ShortDescription" },
                FxOnRemove = new(),
                m_Icon = Icon,
                IsClassFeature = true,
            };
            if (RetrieveBlueprint("571baa4cf65bbcb4996fe429ca77d1a5", out BlueprintBuff Light, "MageLightBuff",
                                "when creating the HolyShield5Buff blueprint."))
                HolyShield5Buff.FxOnStart = Light.FxOnStart;
            HolyShield5Buff.AddComponent(new AddAreaEffect() { m_AreaEffect = HolyShield5AreaEffect.ToReference<BlueprintAbilityAreaEffectReference>()});
            HolyShield5Buff.AddToCache();

            RetrieveBlueprint("5da5d4e1e4ac5db428999a88df4f6bfe", out BlueprintBuff DayLight, "DayLightBuff",
                                "when cerating the HolyShield5Buff blueprint.");
            BlueprintBuff HolyShield10Buff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("fff3176f4f6a4e4fac84fe56b8b5b230")),
                name = modName + "HolyShield10Buff",
                m_DisplayName = HolyShield4Feature_DisplayName,
                m_Description = new LocalizedString() { Key = "HolyShieldBuff10_Description", m_ShouldProcess = true },
                m_DescriptionShort = new LocalizedString() { Key = "HolyShieldBuff10_ShortDescription" },
                FxOnRemove = new(),
                FxOnStart = DayLight?.FxOnStart ?? new(),
                m_Icon = Icon,
                IsClassFeature = true,
            };
            HolyShield10Buff.AddComponent(new AddAreaEffect() { m_AreaEffect = HolyShield10AreaEffect.ToReference<BlueprintAbilityAreaEffectReference>() });
            HolyShield10Buff.AddToCache();

            BlueprintBuff HolyShield20Buff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("1d1e81761cdf4c07b8730607c69817d0")),
                name = modName + "HolyShield20Buff",
                m_DisplayName = HolyShield4Feature_DisplayName,
                m_Description = new LocalizedString() { Key = "HolyShieldBuff20_Description", m_ShouldProcess = true },
                m_DescriptionShort = new LocalizedString() { Key = "HolyShieldBuff20_ShortDescription" },
                FxOnRemove = new(),
                m_Icon = Icon,
                IsClassFeature = true,
                FxOnStart = DayLight?.FxOnStart ?? new()
            };
            HolyShield20Buff.AddComponent(new AddAreaEffect() { m_AreaEffect = HolyShield20AreaEffect.ToReference<BlueprintAbilityAreaEffectReference>() });
            HolyShield20Buff.AddToCache();
            #endregion
            #region Create HolyShieldCountingFeature blueprint
            BlueprintFeature HolyShieldCountingFeature = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("ae26d801a0ab4b628bf1c50e52f69bb9")),
                name = modName + "HolyShieldUnitFact",
                Ranks = 3,
                HideInCharacterSheetAndLevelUp = true,
                HideInUI = true
            };
            HolyShieldCountingFeature.AddToCache();
            #endregion
            #region Create HolyShieldUnitFact
            BlueprintUnitFact HolyShieldUnitFact = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("08535fa73dca4ebfa3bfe9a174c09fe7")),
                name = modName + "_HolyShieldUnitFact",

            };
            AuraFeatureComponentLadder ladder = new()
            {
                m_Buffs = new BlueprintBuffReference[]
                {
                    HolyShield5Buff.ToReference<BlueprintBuffReference>(),
                    HolyShield10Buff.ToReference<BlueprintBuffReference>(),
                    HolyShield20Buff.ToReference<BlueprintBuffReference>(),
                },
                m_featureToCheck = HolyShieldCountingFeature.ToReference<BlueprintFeatureReference>()
            };
            HolyShieldUnitFact.AddComponent(ladder);
            HolyShieldUnitFact.AddToCache();
            #endregion
            #region Create HolyShieldEnchantment
            BlueprintEquipmentEnchantment HolyShieldEnchantment = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("060d81a154064113b7671b68e954e047")),
                name = modName + "_HolyShieldEnchantment",
                m_EnchantName = HolyShield4Feature_DisplayName,
                m_Description = new LocalizedString() { Key = "HolyShieldEnchantment_Description" },
            };
            HolyShieldEnchantment.AddComponent(new AddUnitFactEquipment() { m_Blueprint = HolyShieldUnitFact.ToReference<BlueprintUnitFactReference>() });
            HolyShieldEnchantment.AddToCache();
            #endregion
            #region Create HolyShieldAbility blueprint
            BlueprintAbility HolyShieldAbility = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("c3914f1cf66448c48d526089b108de2a")),
                name = modName + "HolyShieldAbility",
                m_DisplayName = HolyShield4Feature_DisplayName,
                m_Description = new LocalizedString() { Key = "HolyShield4Feature_Description" },
                m_DescriptionShort = new LocalizedString() { Key = "HolyShield4Feature_ShortDescription" },
                m_Icon = Icon,
                ActionType = UnitCommand.CommandType.Standard,
                CanTargetSelf = true,
                Range = AbilityRange.Personal,
                Animation = UnitAnimationActionCastSpell.CastAnimationStyle.SelfTouch,
                LocalizedSavingThrow = Empty,
                LocalizedDuration = Empty,
            };
            AbilityEffectRunAction aefr = new()
            {
                Actions = new()
                {
                    Actions = new GameAction[1]
                    {
                        new ContextActionEnchantShield()
                        {
                            ToCaster = true,
                            RemoveOnUnequip = true,
                            m_Enchantment = HolyShieldEnchantment.ToReference<BlueprintItemEnchantmentReference>(),
                            DurationValue = new()
                            {
                                Rate = DurationRate.Rounds,
                                m_IsExtendable = false,
                                DiceType = DiceType.One,
                                DiceCountValue = 3,
                                BonusValue = new()
                                {
                                    ValueType = ContextValueType.Rank,
                                }
                            }
                        },
                    }
                }
            };
            HolyShieldAbility.AddComponent(aefr);
            ContextRankConfig contextRankDuration = new()
            {
                m_BaseValueType = ContextRankBaseValueType.StatBonus,
                m_Stat = StatType.Charisma
            };
            HolyShieldAbility.AddComponent(contextRankDuration);
            HolyShieldAbility.AddComponent(new AbilityTargetEquippedWithShield());
            if (RetrieveBlueprint("9dedf41d995ff4446a181f143c3db98c", out BlueprintAbilityResource LayOnHandsResource, "LayOnHandsResource", "when creating the HolyShieldAbility blueprint"))
                HolyShieldAbility.AddComponent(new AbilityResourceLogic()
                {
                    m_RequiredResource = LayOnHandsResource.ToReference<BlueprintAbilityResourceReference>(),
                    m_IsSpendResource = true,
                    Amount = 2
                });
            HolyShieldAbility.AddToCache();
            #endregion
            #region Create HolyShieldFeature blueprints
            BlueprintFeature HolyShield4Feature = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("c6a9e7cb970d4985852e7496a7f1f348")),
                name = modName + "_HolyShield4Feature",
                m_DisplayName = HolyShield4Feature_DisplayName,
                m_Description = new LocalizedString() { Key = "HolyShield4Feature_Description", m_ShouldProcess = true },
                m_DescriptionShort = new LocalizedString() { Key = "HolyShield4Feature_ShortDescription" },
                m_Icon = Icon,
                IsClassFeature = true,
            };
            HolyShield4Feature.AddComponent(new AddFacts() 
                { m_Facts = new BlueprintUnitFactReference[] 
                    { HolyShieldCountingFeature.ToReference<BlueprintUnitFactReference>(),
                      HolyShieldAbility.ToReference<BlueprintUnitFactReference>()} });
            HolyShield4Feature.AddToCache();
            SHAbility4 = HolyShield4Feature;
            BlueprintFeature HolyShield11Feature = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("a9ca599a90f8437eab5ebde8ee46b5b8")),
                name = modName + "_HolyShield11Feature",
                m_DisplayName = HolyShield4Feature_DisplayName,
                m_Description = new LocalizedString() { Key = "HolyShield11Feature_Description" },
                m_DescriptionShort = new LocalizedString() { Key = "HolyShield11Feature_ShortDescription" },
                m_Icon = Icon,
                IsClassFeature = true,
            };
            HolyShield11Feature.AddComponent(new AddFacts()
            {
                m_Facts = new BlueprintUnitFactReference[]
                    { HolyShieldCountingFeature.ToReference<BlueprintUnitFactReference>()  }
            });
            HolyShield11Feature.AddToCache();
            SHAbility11 = HolyShield11Feature;
            BlueprintFeature HolyShield20Feature = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("a57404de7a3e462ea12f07cf28b13cea")),
                name = modName + "_HolyShield20Feature",
                m_DisplayName = HolyShield4Feature_DisplayName,
                m_Description = new LocalizedString() { Key = "HolyShield20Feature_Description" },
                m_DescriptionShort = new LocalizedString() { Key = "HolyShield20Feature_ShortDescription" },
                m_Icon = Icon,
                IsClassFeature = true,
            };
            HolyShield20Feature.AddComponent(new AddFacts()
            {
                m_Facts = new BlueprintUnitFactReference[]
                    { HolyShieldCountingFeature.ToReference<BlueprintUnitFactReference>()  }
            });
            HolyShield20Feature.AddToCache();
            SHAbility20 = HolyShield20Feature;
            #endregion
        }
    }
}
