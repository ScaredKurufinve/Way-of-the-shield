using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using static Way_of_the_shield.Main;

namespace Way_of_the_shield
{
    [HarmonyPatch]
    public static class TSSpecialistTweaks
    {
        public static BlueprintFeatureReference ImmediateRepositioning;

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPostfix]
        public static void BlueprintsCache_Init_Postfix()
        {
            LocalizedString RepositionName = new() { m_Key = "ImmediateRepositioningFeature_DisplayName" };
            LocalizedString RepositionDescription = new() { m_Key = "ImmediateRepositioningFeature_Description" };
            LocalizedString RepositionDescriptionShort = new () { m_Key = "ImmediateRepositioningFeature_ShortDescription" };

#if DEBUG
            Comment.Log("Entered the Blueprints Cache postfix for TSSpecialistTweaks."); 
#endif
            #region Create Immediate Repositioning ability
#if DEBUG
            Comment.Log("Begin creating the Immediate Repositioning ability blueprint"); 
#endif
            BlueprintAbility ImmediateRepositioningAbility = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("d9f2b950745141deabefb24dbfe6adea")),
                name = "ImmediateRepositioningAbility_WayOfTheShield",
                m_DisplayName = RepositionName,
                m_Description = RepositionDescriptionShort,
                ActionType = Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift,
                CanTargetEnemies = true,
                CanTargetFriends = true,
                CanTargetPoint = true,
                CanTargetSelf = false,
                EffectOnAlly = AbilityEffectOnUnit.None,
                EffectOnEnemy = AbilityEffectOnUnit.None,
                Type = AbilityType.Extraordinary
            };
            ImmediateRepositioningAbility.AddComponent(new NewComponents.AbilityDeliverTurnTo());
            ImmediateRepositioningAbility.AddToCache();

            #endregion
            #region Create Immediate Repositioning feature

            BlueprintFeature ImmediateRepositioningFeature = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("12cf206dc2174df9ac28d2204f461d94")),
                name = "WayOfTheShield_ImmediateRepositioningFeature",
                m_DisplayName = RepositionName,
                m_Description = RepositionDescription,
                m_DescriptionShort = RepositionDescriptionShort,
                Ranks = 1,
                IsClassFeature = true,
                HideInCharacterSheetAndLevelUp = false,
                HideInUI = false,
                HideNotAvailibleInUI = false
            };
            ImmediateRepositioningFeature.AddComponent(new AddFacts() { m_Facts = new BlueprintUnitFactReference[] { ImmediateRepositioningAbility.ToReference<BlueprintUnitFactReference>() } });
            ImmediateRepositioningFeature.AddToCache();

            #endregion
            if (!GiveImmediateRepositioningToTSS.GetValue() && !RemoveTotalCoverFeatureFromTSS.GetValue()) return;

            if (!RetrieveBlueprint("a599da9a8a6b9e54083b0a4d2a25db59", out BlueprintArchetype TSS_progression, "TowerShieldSpecialistArchetype")) return;
            List<BlueprintFeatureBaseReference> lvl13features = TSS_progression.AddFeatures.First(x => x.Level == 13).m_Features;
            if (!RetrieveBlueprint("4068bdf2373538e4fbd1c70438102f2e", out BlueprintFeature TSS_TotalCover, "TowerShieldSpecialistArchetype")) return;
            BlueprintFeatureBaseReference TSS_TotalCoverReference = TSS_TotalCover.ToReference<BlueprintFeatureBaseReference>();
            if (!RetrieveBlueprint("b50e94b57be32f74892f381ae2a8905a", out BlueprintProgression FighterClassProgression, "FighterClassProgression", "when adding Immediate Repositioning Feature to Fighter UI groups")) return;
            IEnumerable<UIGroup> ui = FighterClassProgression.UIGroups.Where(group => group.m_Features.Contains(TSS_TotalCoverReference));
            if (ui.Count() == 0) Comment.Warning("Could not find any UI group containing a reference to the Tower Shield Total Cover blueprint inside the Fighter Class progression. Sanity check: " +
                $"Fighter Class progression blueprint is {FighterClassProgression.name} by guid {FighterClassProgression.AssetGuid}, " +
                $"Tower Shield Total Cover blueprint is {TSS_TotalCoverReference.Get().name} by guid {TSS_TotalCoverReference.Get().AssetGuid}");
            if (GiveImmediateRepositioningToTSS.GetValue())
            {
                BlueprintFeatureBaseReference ImmediateRepositioningFeatureReference = ImmediateRepositioningFeature.ToReference<BlueprintFeatureBaseReference>();
                lvl13features.Add(ImmediateRepositioningFeatureReference);
#if DEBUG
                Comment.Log("Added Immediate Repositioning feature to the Tower Shield Specialist archetype blueprint at lvl 13"); 
#endif
                foreach (UIGroup group in ui) group.m_Features.Add(ImmediateRepositioningFeatureReference);
#if DEBUG
                Comment.Log("Added Immediate Repositioning feature to the Fighter Progression blueprint UI groups at lvl 13"); 
#endif
            };

            if (RemoveTotalCoverFeatureFromTSS.GetValue())
            {
                lvl13features.Remove(TSS_TotalCoverReference);
#if DEBUG
                Comment.Log("Removed Total Cover feature from the Tower Shield Specialist archetype blueprint at lvl 13"); 
#endif
                foreach (UIGroup group in ui) group.m_Features.Add(TSS_TotalCoverReference);
#if DEBUG
                Comment.Log("Removed Total Cover feature from the Fighter Progression blueprint UI groups at lvl 13"); 
#endif
            }
        }
    }
}
