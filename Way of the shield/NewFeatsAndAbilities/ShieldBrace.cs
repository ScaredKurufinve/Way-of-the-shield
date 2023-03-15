using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Weapons;
using static Kingmaker.Blueprints.Items.Weapons.WeaponFighterGroupHelper;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;


namespace Way_of_the_shield.NewFeatsAndAbilities
{
    public class ShieldBrace
    {
        [TypeId("a9d4a3aad229457698fe4d93ddcc01b3")]
        public class ShieldBrace_Component : CanUse2hWeaponAs1hBase, IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>
        {
            public override bool CanBeUsedAs2h(ItemEntityWeapon weapon)
            {
                return false;
            }

            public override bool CanBeUsedOn(ItemEntityWeapon weapon)
            {
                if (weapon is null) 
                    return false; 

                if (Fact.Owner.Unit.GetSaddledUnit() is not null) 
                    return false; 

                if (!weapon.Blueprint.IsTwoHanded) 
                    return false;

                ItemEntityShield shield = (weapon.HoldingSlot as HandSlot)?.PairSlot?.MaybeShield;
                if (shield is null) 
                    return false;

                var shield_proficiency = shield.ArmorComponent.Blueprint.ProficiencyGroup;

                if (shield_proficiency == ArmorProficiencyGroup.Buckler || !ProficiencyRework.ProficiencyPatches.IsProficient_Short(shield))
                    return false;

                return weapon.Blueprint.FighterGroup.Contains(WeaponFighterGroup.Spears) ||  weapon.Blueprint.FighterGroup.Contains(WeaponFighterGroup.Polearms) ;
            }

            public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
            {
                if (evt.Weapon == null) { return; }
                if (!CanBeUsedOn(evt.Weapon)) { return; }
                var shield = (evt.Weapon?.HoldingSlot as HandSlot)?.PairSlot.MaybeShield;
                if (shield is null) { return; };
                int penalty = Rulebook.Trigger(new RuleCalculateArmorCheckPenalty(evt.Initiator, shield.ArmorComponent)).Result;
                if (penalty < 0) { evt.AddModifier(penalty, Fact, ModifierDescriptor.Shield); }

            }

            public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
            {

            }
        }

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        public static class CachePatch
        {
            public static HashSet<(string GUID, string name)> selections = new()
            {
                new ("247a4068296e8be42890143f451b4b45", "BasicFeatSelection"),
                new ("41c8486641f7d6d4283ca9dae4147a9f", "FighterFeatSelection"),
                new ("c5357c05cf4f8414ebd0a33e534aec50", "CrusaderFeat1"),
                new ("50dc57d2662ccbd479b6bc8ab44edc44", "CrusaderFeat10"),
                new ("2049abc955bf6fe41a76f2cb6ba8214a", "CrusaderFeat20"),
                new ("303fd456ddb14437946e344bad9a893b", "WarpriestFeatSelection"),
                new ("dd17090d14958ef48ba601688b611970", "CavalierBonusFeatSelection"),
            };

            [HarmonyAfter("TabletopTweaks-Base")]
            [HarmonyPostfix]
            public static void Postfix()
            {
#if DEBUG
                Comment.Log("Begin creting the Shield Brace blueprint."); 
#endif
                #region Create feature blueprint
                BlueprintFeature feature = new()
                {
                    AssetGuid = new BlueprintGuid(new Guid("bacc4f29695d4aec868d31be1c04f29b")),
                    name = "Way of the Shield - Shield Brace feature blueprint",
                    HideInUI = false,
                    HideInCharacterSheetAndLevelUp = false,
                    HideNotAvailibleInUI = false,
                    Ranks = 1,
                    Groups = new FeatureGroup[]
                    {
                        FeatureGroup.Feat,
                        FeatureGroup.CombatFeat,
                    },
                    IsClassFeature = false,
                    m_DisplayName = new LocalizedString() { Key = "ShieldBraceFeature_DisplayName" },
                    m_Description = new LocalizedString() { Key = "ShieldBraceFeature_Description" },
                    m_DescriptionShort = new LocalizedString() { Key = "hieldBraceFeature_ShortDescription" },
                    m_Icon = LoadIcon("ShieldBrace", 200, 64)
                };
                feature.AddComponent(new ShieldBrace_Component());
                feature.AddComponent(new PrerequisiteProficiency()
                {
                    ArmorProficiencies = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.LightShield, ArmorProficiencyGroup.HeavyShield, ArmorProficiencyGroup.TowerShield },
                    WeaponProficiencies = Array.Empty<WeaponCategory>(),
                    HideInUI = false,
                    Group = Prerequisite.GroupType.All
                });
                BlueprintFeatureReference ShieldFocusReference = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("ac57069b6bf8c904086171683992a92a")?.ToReference<BlueprintFeatureReference>();
                if (ShieldFocusReference.Get() is null) { Comment.Warning("WARNING. Failed to find the Shield Focus feature blueprint when creating prerequisites for Shield Brace"); }
                BlueprintFeatureReference ArmorTrainingReference = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("3c380607706f209499d951b29d3c44f3")?.ToReference<BlueprintFeatureReference>();
                if (ShieldFocusReference.Get() is null) { Comment.Warning("WARNING. Failed to find the Armor Training feature blueprint when creating prerequisites for Shield Brace"); }
                feature.AddComponent(new PrerequisiteFeaturesFromList()
                {
                    m_Features = new BlueprintFeatureReference[]
                    {
                        ShieldFocusReference,
                        ArmorTrainingReference
                    },
                    HideInUI = false,
                    Group = Prerequisite.GroupType.All
                });

                BlueprintCharacterClassReference FighterClassReference = new();
                if (RetrieveBlueprint("48ac8db94d5de7645906c7d0ad3bcfbd", out BlueprintCharacterClass FighterClass, "FighterClass", "when creating prerequisites for Shield Brace"))
                    FighterClassReference = FighterClass.ToReference<BlueprintCharacterClassReference>();
                feature.AddComponent(new PrerequisiteClassLevel()
                {
                    Level = 1,
                    m_CharacterClass = FighterClassReference,
                    HideInUI = false,
                    Group = Prerequisite.GroupType.Any
                });
                feature.AddComponent(new PrerequisiteStatValue()
                {
                    Stat = StatType.BaseAttackBonus,
                    Value = 3,
                    HideInUI = false,
                    Group = Prerequisite.GroupType.Any
                });
                feature.AddComponent(new FeatureTagsComponent() { FeatureTags = FeatureTag.Defense });
                feature.AddToCache();
                #endregion
                if (!RetrieveBlueprint("ef38e0fe68f14c88a9deacc421455d14", out BlueprintFeatureSelection ShieldMastery, "ShieldMasterySelection", "to add Shield Brace")) goto skipShieldMastery;
                selections.Add((ShieldMastery.AssetGuid.ToString(), "TTT-ShieldMasterySelection"));
                skipShieldMastery:;
                feature.AddFeatureToSelections(selections);
            }
        }
    }
}
