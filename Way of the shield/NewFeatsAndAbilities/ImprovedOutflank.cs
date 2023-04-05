using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.Mechanics.Facts;
using System;

namespace Way_of_the_shield.NewFeatsAndAbilities
{
    [HarmonyPatch]
    public class ImprovedOutflank
    {

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        public static void Postfix()
        {
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log("Begin creating the ImprovedOutflankFeature blueprint."); 
#endif
            BlueprintFeature ImprovedOutflankFeature = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("3e376df15e00413f96238d123798b5d6")),
                name = Main.modName + "_ImprovedOutflankFeature",
                Groups = new FeatureGroup[] {   FeatureGroup.TeamworkFeat,
                                                FeatureGroup.Feat,
                                                FeatureGroup.CombatFeat},
                Ranks = 1,
                m_DisplayName = new LocalizedString() { m_Key = "ImprovedOutflankFeature_DisplayName" },
                m_Description = new LocalizedString() { m_Key = "ImprovedOutflankFeature_Description" },
                m_DescriptionShort = new LocalizedString() { m_Key = "ImprovedOutflankFeature_ShortDescription" },
            };
            ImprovedOutflankFeature.AddToCache();
            ImprovedOutflankFeature.AddComponent(new PrerequisiteStatValue() { Stat = Kingmaker.EntitySystem.Stats.StatType.BaseAttackBonus, Value = 6 });
            ImprovedOutflankFeature.AddComponent(new FeatureTagsComponent() { FeatureTags = FeatureTag.Attack | FeatureTag.Melee | FeatureTag.Ranged | FeatureTag.Teamwork });
            if (!RetrieveBlueprint("422dab7309e1ad343935f33a4d6e9f11", out BlueprintFeature Outflank, "Outflank", "when creating the Improved Outflank feature blueprint")) return;
            ImprovedOutflankFeature.AddComponent(new PrerequisiteFeature() { m_Feature = Outflank.ToReference<BlueprintFeatureReference>() });
            ImprovedOutflankFeature.m_Icon = Outflank.Icon;
            Flanking.ImprovedOutflank = ImprovedOutflankFeature;
            ImprovedOutflankFeature.AddFeatureAsTeamwork(PackRagerGuids: ("829eaa032d9d4a0facf3071b0700e05b", "9b1100488cdb4afd8e80221164ede23c", "120fec891ada400da3f87fc84bd8b3dd", "a25936fd46444104b7a6f989a7da9e58", "369f6bf8b22242eab0aed6bc8c83a6ea"),
                                                            CavalierGuid: "c7e4894380d8415bb7f4b782ba8d25ef",
                                                            VanguardGuids: ("f81a97adb2ce47428e3fd8e7b06cf6f0", "b6ad5d0f1a214add9824ef4a483c551e"),
                                                            DoNotAdd: Main.CheckForModEnabled("TabletopTweaks-Flanking"));
        }
    }
}
