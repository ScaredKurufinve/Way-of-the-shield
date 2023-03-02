using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.Mechanics.Facts; 
using System;
using System.Collections.Generic;
using static Way_of_the_shield.Main;

namespace Way_of_the_shield.NewFeatsAndAbilities
{
    [HarmonyPatch]
    public static class PhalanxFormation
    {
        public static (string GUID, string name)[] selections = new (string GUID, string name)[]
        {
                new ("247a4068296e8be42890143f451b4b45", "BasicFeatSelection"),
                new ("41c8486641f7d6d4283ca9dae4147a9f", "FighterFeatSelection"),
                new ("dd17090d14958ef48ba601688b611970", "CavalierBonusFeatSelection")
        };

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPostfix]
        public static void AddPhalaxFormationBlueprint_BlueprintCache_Init_Postifx()
        {
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log("Begin creating the Phalanx Formation blueprint."); 
#endif
            var Icon = LoadIcon("PhalanxFormation");
            BlueprintFeature PhalanxFormationFeature = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("17a57b7e99a04112a3810b4590c3f49f")),
                name = "WayOfTheShield_PhalanxFormationFeature",
                m_DisplayName = new() { Key = "PhalanxFormationFeature_DisplayName" },
                m_Description = new() { Key = "PhalanxFormationFeature_Description" },
                m_DescriptionShort = new() { Key = "PhalanxFormationFeature_ShortDescription" },
                Ranks = 1,
                m_Icon = Icon,
            };
            PhalanxFormationFeature.AddComponent(new NewComponents.RemoveOthersFromSoftCover() { OnlyAlly = true, CheckWeaponType = WeaponTypesForSoftCoverDenial.Reach });
            PhalanxFormationFeature.AddComponent(new FeatureTagsComponent() { FeatureTags = FeatureTag.Attack });
            PhalanxFormationFeature.AddToCache();
            PhalanxFormationFeature.AddFeatureToSelections(selections);
        }

    }
}
