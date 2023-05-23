using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using Way_of_the_shield.NewComponents;
using UnityEngine;

namespace Way_of_the_shield.NewFeatsAndAbilities
{
    [HarmonyPatch]
    public class UpsettingShieldStyle
    {

        public static HashSet<(string, string)> selections = new()
        {
            new ("247a4068296e8be42890143f451b4b45", "BasicFeatSelection"),
            new ("41c8486641f7d6d4283ca9dae4147a9f", "FighterFeatSelection"),
            new ("c5357c05cf4f8414ebd0a33e534aec50", "CrusaderFeat1"),
            new ("50dc57d2662ccbd479b6bc8ab44edc44", "CrusaderFeat10"),
            new ("2049abc955bf6fe41a76f2cb6ba8214a", "CrusaderFeat20"),
            new ("303fd456ddb14437946e344bad9a893b", "WarpriestFeatSelection"),
        };

        public static BlueprintActivatableAbility Ability { get { if (!Created) BlueprintsCache_Init_Patch(); return m_Ability; } }
        private static BlueprintActivatableAbility m_Ability;
        public static BlueprintFeature Strike {get { if (!Created) BlueprintsCache_Init_Patch(); return m_Strike; } }
        private static BlueprintFeature m_Strike;
        public static BlueprintFeature Vengeance { get { if (!Created) BlueprintsCache_Init_Patch(); return m_Vengeance; } }
        private static BlueprintFeature m_Vengeance;
        public static BlueprintBuff VengeanceBuff { get { if (!Created) BlueprintsCache_Init_Patch(); return m_VengeanceBuff; } }
        private static BlueprintBuff m_VengeanceBuff;
        public static BlueprintBuff StrikeBuff { get { if (!Created) BlueprintsCache_Init_Patch(); return m_StrikeBuff; } }
        private static BlueprintBuff m_StrikeBuff;
        public static BlueprintBuff StyleBuff { get { if (!Created) BlueprintsCache_Init_Patch(); return m_StyleBuff; } }
        private static BlueprintBuff m_StyleBuff;
        public static BlueprintBuff checker { get { if (!Created) BlueprintsCache_Init_Patch(); return m_checker; } }
        private static BlueprintBuff m_checker;
        public static BlueprintBuff MainBuff { get { if (!Created) BlueprintsCache_Init_Patch(); return m_MainBuff; } }
        private static BlueprintBuff m_MainBuff;

        internal static bool Created;


        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPostfix]
        public static void BlueprintsCache_Init_Patch()
        {
#if DEBUG
            Comment.Log("Entered Blueprint cache patch for Upsetting style"); 
#endif
            if (!RetrieveBlueprint("121811173a614534e8720d7550aae253", out BlueprintFeature ShieldBash, "ShieldBashFeature")) return;
            BlueprintFeatureReference ShieldBashReference = ShieldBash.ToReference<BlueprintFeatureReference>();
            if (!RetrieveBlueprint("0f8939ae6f220984e8fb568abbdfba95", out BlueprintFeature CombatReflexes, "CombatReflexes")) return;
            BlueprintFeatureReference CombatReflexesReference = CombatReflexes.ToReference<BlueprintFeatureReference>();
            BlueprintFeatureSelection selection;
            Sprite UpsettingStyleIcon = LoadIcon("UpsettingBucklerStyle");
            Sprite UpsettingStrikeIcon = LoadIcon("UpsettingStrike");
            Sprite UpsettingVengeanceIcon = LoadIcon("UpsettingVengeance");
            #region Create UpsettingShield buff for the activatable ability
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log("Begin creating UpsettingShield Buff for the activatable ability."); 
#endif
            BlueprintBuff UpsettingShieldBuffMain = new()
            {
                m_DisplayName = new LocalizedString() { m_Key = "UpsettingShieldStyle_DisplayName" },
                m_Description = new LocalizedString() { m_Key = "UpsettingShieldStyle_Description" },
                m_DescriptionShort = new LocalizedString() { m_Key = "UpsettingShieldStyle_ShortDescription" },
                FxOnRemove = new(),
                FxOnStart = new(),
                m_Flags = BlueprintBuff.Flags.StayOnDeath | BlueprintBuff.Flags.HiddenInUi,
                Stacking = StackingType.Replace,
                m_Icon = UpsettingStyleIcon,
            };
            UpsettingShieldBuffMain.AddComponent(new UpsettingShieldComponent());
            UpsettingShieldBuffMain.AddToCache("dfcbc91d3c2d4f8685acf158dcb58815", "UpsettingShieldStyleBuff");
            m_MainBuff = UpsettingShieldBuffMain;
            #endregion
            #region Create Upsetting Shield activatable ability
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log("Begin creating UpsettingShield activatable ability blueprint"); 
#endif
            BlueprintActivatableAbility UpsettingShieldStyleAbility = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("fc452aa2ff72401f943a133706b35acc")),
                name = Main.modName + "UpsettingShieldStyleAbility",
                m_DisplayName = new LocalizedString() { m_Key = "UpsettingShieldStyle_DisplayName" },
                m_Description = new LocalizedString() { m_Key = "UpsettingShieldStyle_Description" },
                m_DescriptionShort = new LocalizedString() { m_Key = "UpsettingShieldStyle_ShortDescription" },
                m_Buff = UpsettingShieldBuffMain.ToReference<BlueprintBuffReference>(),
                Group = ActivatableAbilityGroup.CombatStyle,
                WeightInGroup = 1,
                ActivationType = AbilityActivationType.Immediately,
                m_Icon = UpsettingStyleIcon,

            };
            UpsettingShieldStyleAbility.AddComponent(new DeactivateImmediatelyIfNoAttacksThisRound());
            UpsettingShieldStyleAbility.AddToCache();
            m_Ability = UpsettingShieldStyleAbility;
            #endregion
            #region Create UpsettingShield feature
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log("Begin creating UpsettingShield feature blueprint"); 
#endif
            BlueprintFeature UpsettingShieldStyleFeature = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("7da0bf0d789d4528b4c3b707eb98fbd2")),
                name = "Upsetting Shield Style feature - Way of the Shield",
                m_DisplayName = new LocalizedString() { m_Key = "UpsettingShieldStyle_DisplayName" },
                m_Description = new LocalizedString() { m_Key = "UpsettingShieldStyle_Description" },
                m_DescriptionShort = new LocalizedString() { m_Key = "UpsettingShieldStyle_ShortDescription" },
                Groups = new FeatureGroup[]
                {
                    FeatureGroup.CombatFeat,
                    FeatureGroup.Feat,
                    FeatureGroup.StyleFeat
                },
                HideInUI = false,
                HideInCharacterSheetAndLevelUp = false,
                HideNotAvailibleInUI = false,
                IsClassFeature = true,
                Ranks = 1,
                m_Icon = UpsettingStrikeIcon,
            };
            UpsettingShieldStyleFeature.AddComponent(new PrerequisiteStatValue()
            {
                Stat = StatType.Dexterity,
                Value = 13,
                Group = Prerequisite.GroupType.All
            });
            UpsettingShieldStyleFeature.AddComponent(new PrerequisiteProficiency()
            {
                ArmorProficiencies = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.Buckler },
                WeaponProficiencies = Array.Empty<WeaponCategory>(),
                Group = Prerequisite.GroupType.All
            });
            UpsettingShieldStyleFeature.AddComponent(new PrerequisiteIsPet() { Not = true, HideInUI = true, Group = Prerequisite.GroupType.All });
            UpsettingShieldStyleFeature.AddComponent(new AddFacts() { m_Facts = new [] 
                { UpsettingShieldStyleAbility.ToReference<BlueprintUnitFactReference>(),
                  new BlueprintUnitFactReference(){deserializedGuid = BlueprintGuid.Parse("f42adaab0f24462c87a7875c259ffccb")} //Shield Bash
                }});
            UpsettingShieldStyleFeature.AddComponent(new FeatureTagsComponent()
            {
                FeatureTags =
                FeatureTag.Attack |
                FeatureTag.Melee |
                FeatureTag.Style
            });
            UpsettingShieldStyleFeature.AddComponent(new AddMechanicsFeature() { m_Feature = MechanicsFeatureExtension.BucklerBash });
            UpsettingShieldStyleFeature.AddToCache();
            BlueprintFeatureReference UpsettingShieldStyleReference = UpsettingShieldStyleFeature.ToReference<BlueprintFeatureReference>();
            foreach ((string guid, string name) in selections)
            {
                if (!RetrieveBlueprint(guid, out selection, name)) continue;
                else
                {
                    selection.m_AllFeatures = selection.m_AllFeatures.Append(UpsettingShieldStyleReference).ToArray();
#if DEBUG
                    Comment.Log("Added UpsettingShieldStyleFeature to the " + name + " blueprint."); 
#endif
                }
            };
            #endregion
            #region Create UpsettingStrike feature
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log("Begin creating Upsetting Strike feature blueprint"); 
#endif
            BlueprintFeature UpsettingStrikeFeature = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("836dafe9db094691bce5dc0c6b0305ac")),
                name = Main.modName + "_UpsettingStrikeFeature",
                m_DisplayName = new LocalizedString() { m_Key = "UpsettingStrike_DisplayName" },
                m_Description = new LocalizedString() { m_Key = "UpsettingStrike_Description" },
                m_DescriptionShort = new LocalizedString() { m_Key = "UpsettingStrike_ShortDescription" },
                Groups = new FeatureGroup[]
                {
                    FeatureGroup.CombatFeat,
                    FeatureGroup.Feat,
                    FeatureGroup.StyleFeat
                },
                HideInUI = false,
                HideInCharacterSheetAndLevelUp = false,
                HideNotAvailibleInUI = false,
                IsClassFeature = false,
                Ranks = 1,
                m_Icon = UpsettingStrikeIcon,
            };
            UpsettingStrikeFeature.AddComponent(new PrerequisiteStatValue()
            {
                Stat = StatType.Dexterity,
                Value = 15,
                Group = Prerequisite.GroupType.All
            });
            UpsettingStrikeFeature.AddComponent(new PrerequisiteProficiency()
            {
                ArmorProficiencies = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.Buckler },
                WeaponProficiencies = Array.Empty<WeaponCategory>(),
                Group = Prerequisite.GroupType.All
            });
            UpsettingStrikeFeature.AddComponent(new PrerequisiteFeature() { name = "Upsetting Shield Style Prerequisite", m_Feature = UpsettingShieldStyleFeature.ToReference<BlueprintFeatureReference>(), Group = Prerequisite.GroupType.All });
            UpsettingStrikeFeature.AddComponent(new PrerequisiteFeature() { name = "Shield Bash Prerequisite", m_Feature = ShieldBashReference, Group = Prerequisite.GroupType.All });
            UpsettingStrikeFeature.AddComponent(new PrerequisiteFeature() { name = "Combat Reflexes Prerequisite", m_Feature = CombatReflexesReference, Group = Prerequisite.GroupType.All });
            UpsettingStrikeFeature.AddComponent(new PrerequisiteIsPet() { Not = true, HideInUI = true });
            UpsettingStrikeFeature.AddComponent(new FeatureTagsComponent()
            {
                FeatureTags =
                FeatureTag.Attack |
                FeatureTag.Melee |
                FeatureTag.Style
            });
            UpsettingStrikeFeature.AddToCache();
            BlueprintFeatureReference UpsettingStrikeFeatureReference = UpsettingStrikeFeature.ToReference<BlueprintFeatureReference>();
            UpsettingShieldStyleFeature.IsPrerequisiteFor ??= new();
            UpsettingShieldStyleFeature.IsPrerequisiteFor.Add(UpsettingStrikeFeatureReference);
            ShieldBash.IsPrerequisiteFor ??= new();
            ShieldBash.IsPrerequisiteFor.Add(UpsettingStrikeFeatureReference);
            CombatReflexes.IsPrerequisiteFor ??= new();
            CombatReflexes.IsPrerequisiteFor.Add(UpsettingStrikeFeatureReference);
            m_Strike = UpsettingStrikeFeature;
            UpsettingStrikeFeature.AddFeatureToSelections(selections);

            #endregion
            #region Create UpsettingVengeance feature
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log("Begin creating UpsettingVengeance feature blueprint"); 
#endif
            BlueprintFeature UpsettingVengeanceFeature = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("c748ba6f94fb4dd68e5aebbe8562a80b")),
                name = "Upsetting Vengeance feature - Way of the Shield",
                m_DisplayName = new LocalizedString() { m_Key = "UpsettingVengeance_DisplayName" },
                m_Description = new LocalizedString() { m_Key = "UpsettingVengeance_Description" },
                m_DescriptionShort = new LocalizedString() { m_Key = "UpsettingVengeance_ShortDescription" },
                Groups = new FeatureGroup[]
                {
                    FeatureGroup.CombatFeat,
                    FeatureGroup.Feat,
                    FeatureGroup.StyleFeat
                },
                HideInUI = false,
                HideInCharacterSheetAndLevelUp = false,
                HideNotAvailibleInUI = false,
                IsClassFeature = false,
                Ranks = 1,
                m_Icon = UpsettingVengeanceIcon,
            };
            UpsettingVengeanceFeature.AddComponent(new PrerequisiteStatValue()
            {
                Stat = StatType.Dexterity,
                Value = 15,
                Group = Prerequisite.GroupType.All
            });
            UpsettingVengeanceFeature.AddComponent(new PrerequisiteProficiency()
            {
                ArmorProficiencies = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.Buckler },
                WeaponProficiencies = Array.Empty<WeaponCategory>(),
                Group = Prerequisite.GroupType.All
            });
            UpsettingVengeanceFeature.AddComponent(new PrerequisiteFeature() { name = "Upsetting Shield Style Prerequisite", m_Feature = UpsettingShieldStyleFeature.ToReference<BlueprintFeatureReference>(), Group = Prerequisite.GroupType.All });
            UpsettingVengeanceFeature.AddComponent(new PrerequisiteFeature() { name = "Upsetting Strike Prerequisite", m_Feature = UpsettingStrikeFeature.ToReference<BlueprintFeatureReference>(), Group = Prerequisite.GroupType.All });
            UpsettingVengeanceFeature.AddComponent(new PrerequisiteFeature() { name = "Shield Bash Prerequisite", m_Feature = ShieldBashReference, Group = Prerequisite.GroupType.All });
            UpsettingVengeanceFeature.AddComponent(new PrerequisiteFeature() { name = "Combat Reflexes Prerequisite", m_Feature = CombatReflexesReference, Group = Prerequisite.GroupType.All });
            UpsettingVengeanceFeature.AddComponent(new PrerequisiteIsPet() { Not = true, HideInUI = true });
            UpsettingVengeanceFeature.AddComponent(new FeatureTagsComponent()
            {
                FeatureTags =
                FeatureTag.Attack |
                FeatureTag.Melee |
                FeatureTag.Style
            });
            UpsettingVengeanceFeature.AddToCache();
            BlueprintFeatureReference UpsettingVengeanceFeatureReference = UpsettingVengeanceFeature.ToReference<BlueprintFeatureReference>();
            UpsettingShieldStyleFeature.IsPrerequisiteFor ??= new();
            UpsettingShieldStyleFeature.IsPrerequisiteFor.Add(UpsettingVengeanceFeatureReference);
            UpsettingStrikeFeature.IsPrerequisiteFor ??= new();
            UpsettingStrikeFeature.IsPrerequisiteFor.Add(UpsettingVengeanceFeatureReference);
            ShieldBash.IsPrerequisiteFor ??= new();
            ShieldBash.IsPrerequisiteFor.Add(UpsettingVengeanceFeatureReference);
            CombatReflexes.IsPrerequisiteFor ??= new();
            CombatReflexes.IsPrerequisiteFor.Add(UpsettingVengeanceFeatureReference);
            m_Vengeance = UpsettingVengeanceFeature;
            foreach ((string guid, string name) in selections)
            {
                if (!RetrieveBlueprint(guid, out selection, name)) continue;
                else
                {
                    selection.m_AllFeatures = selection.m_AllFeatures.Append(UpsettingVengeanceFeatureReference).ToArray();
#if DEBUG
                    Comment.Log("Added UpsettingVengeanceFeature to the " + name + " blueprint."); 
#endif
                }
            };
            #endregion
            #region Create checker buff
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log("Begin creating Checker Buff for the Upsetting Shield style.");
#endif
            BlueprintBuff checker = new()
            {
                m_Flags = BlueprintBuff.Flags.HiddenInUi,
                FxOnRemove = new(),
            };
            checker.AddToCache("725ff3821a1242a8bf9e15429319152d", "Checker Buff for the Upsetting Shield style");
            m_checker = checker;
            BlueprintBuffReference checkerReference = checker.ToReference<BlueprintBuffReference>();
            #endregion
            #region Create Style buff
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log("Begin creating the style Buff for the Upsetting Shield component");
#endif
            BlueprintBuff UpsettingShieldBuff = new()
            {
                m_DisplayName = new() { Key = "UpsettingShieldEffect_DisplayName" },
                m_Description = new() { Key = "UpsettingShieldEffect_Description" },
                m_DescriptionShort = new() { Key = "UpsettingShieldEffect_ShortDescription" },
                FxOnRemove = new(),
                FxOnStart = new(),
                Stacking = StackingType.Stack,
                m_Icon = UpsettingStyleIcon,
            };
            UpsettingShieldBuff.AddComponent(new AttackBonusAgainstCaster()
            {
                Value = -2,
                Descriptor = ModifierDescriptor.UntypedStackable
            });
            UpsettingShieldBuff.AddComponent(new BuffDynamicDescriptionComponent_Caster());
            UpsettingShieldBuff.AddToCache("5a698c8b9ebe4ce49f21d965a8723786", "UpsettingShieldEffectBuff");
            m_StyleBuff = UpsettingShieldBuff;
            #endregion
            #region Create Strike buff
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log("Begin creating the Strike Buff for the Upsetting Shield component"); 
#endif
            BlueprintBuff UpsettingStrikeBuff = new()
            {
                m_DisplayName = new() { Key = "UpsettingStrikeEffect_DisplayName" },
                m_Description = new() { Key = "UpsettingStrikeEffect_Description" },
                m_DescriptionShort = new() { Key = "UpsettingStrikeEffectt_ShortDescription" },
                FxOnRemove = new(),
                FxOnStart = new(),
                Stacking = StackingType.Stack,
                m_Icon = UpsettingStrikeIcon,
            };
            UpsettingStrikeBuff.AddComponent(new AttackBonusAgainstCaster()
            {
                Value = -2,
                Descriptor = ModifierDescriptor.UntypedStackable
            });
            UpsettingStrikeBuff.AddComponent(new AoOOnFarMiss() { CheckBuff = true, CheckOnCaster = true, m_FactToCheck = checkerReference, CasterOnly = true });
            UpsettingShieldBuff.AddComponent(new BuffDynamicDescriptionComponent_Caster());
            UpsettingShieldBuff.AddToCache("8256f265fae94c7a891a23e796d23956", "UpsettingStrikeBuff");
            m_StrikeBuff = UpsettingStrikeBuff;
            #endregion
            #region Create Vengeance Strike buff
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log("Begin creating the Strike Buff for the Upsetting Shield component"); 
#endif
            BlueprintBuff UpsettingVengeanceBuff = new()
            {
                m_DisplayName = new() { Key = "UpsettingVengeanceEffect_DisplayName" },
                m_Description = new() { Key = "UpsettingVengeaceEffect_Description" },
                m_DescriptionShort = new() { Key = "UpsettingVengeanceEffect_ShortDescription" },
                FxOnRemove = new(),
                FxOnStart = new(),
                Stacking = StackingType.Stack,
                Frequency = Kingmaker.UnitLogic.Mechanics.DurationRate.Rounds
            };
            UpsettingVengeanceBuff.AddComponent(new AddContextStatBonus()
            {
                Value = -2,
                Descriptor = ModifierDescriptor.UntypedStackable,
                Stat = StatType.AdditionalAttackBonus
            });
            UpsettingVengeanceBuff.AddComponent(new BuffDynamicDescriptionComponent_Caster());
            UpsettingVengeanceBuff.AddComponent(new AoOOnFarMiss() { CheckBuff = true, CheckOnCaster = true, m_FactToCheck = checkerReference, CasterOnly = false });
            UpsettingVengeanceBuff.AddToCache("f66c775241f54f79a520f25ef3c0887e", "UpsettingVengeanceBuff");
            m_VengeanceBuff = UpsettingVengeanceBuff;
            #endregion
            Created = true;
        }

    }
}
