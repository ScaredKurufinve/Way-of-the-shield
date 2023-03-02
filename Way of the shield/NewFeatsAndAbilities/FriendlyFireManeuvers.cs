using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using System;
using static Way_of_the_shield.Main;
using static Way_of_the_shield.Utilities;

namespace Way_of_the_shield
{
    [HarmonyPatch]
    public static class FriendlyFireManeuvers
    {
        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPostfix]
        public static void BluprintCache_Postfix_AddFriendlyFireFeature()
        {
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log("Begin Creating the Friendly Fire Maneuvers feature blueprint"); 
#endif
            BlueprintFeature FriendlyFireManeuversFeature = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("0183efa3fbb343d9b5085fafb63338b6")),
                name = Main.modName + "_FriendlyFireManeuversFeature",
                m_DisplayName = new LocalizedString() { m_Key = "FriendlyFireManeuversFeature_DisplayName" },
                m_Description = new LocalizedString() { m_Key = "FriendlyFireManeuversFeature_Description" },
                m_DescriptionShort = new LocalizedString() { m_Key = "FriendlyFireManeuversFeature_ShortDescription" },
                Ranks = 1,
                Groups = new FeatureGroup[] { FeatureGroup.Feat | FeatureGroup.CombatFeat | FeatureGroup.TeamworkFeat },
                m_Icon = LoadIcon("FriendlyFire"),
                HideInCharacterSheetAndLevelUp = false,
                HideInUI = false
            };
            FriendlyFireManeuversFeature.AddComponent(new NewComponents.RemoveOthersFromSoftCover()
            {
                CheckWeaponType = WeaponTypesForSoftCoverDenial.Ranged,
                OnlyAlly = true,
                CheckFacts = true,
                FactsToCheck = new BlueprintUnitFactReference[] { FriendlyFireManeuversFeature.ToReference<BlueprintUnitFactReference>() }
            });
            if (RetrieveBlueprint("0da0c194d6e1d43419eb8d990b28e0ab", out BlueprintFeature PointBlankShot, "PointBlankShot", "when creating FriendlyFireManeuversFeature."))
                FriendlyFireManeuversFeature.AddComponent(new PrerequisitePlayerHasFeature() { m_Feature = PointBlankShot.ToReference<BlueprintFeatureReference>() });
            if (RetrieveBlueprint("8f3d1e6b4be006f4d896081f2f889665", out BlueprintFeature PreciseShot, "PreciseShot", "when creating FriendlyFireManeuversFeature."))
                FriendlyFireManeuversFeature.AddComponent(new PrerequisitePlayerHasFeature() { m_Feature = PreciseShot.ToReference<BlueprintFeatureReference>() });
            //NewComponents.SavingBonusAgainstAlliesIfAllyHasFactAndSimpleProjectile savingThrowBonus = new()
            //{
            //    savingThrowType = SavingThrowType.Reflex,
            //    Value = 4,
            //    Bonus = 0,
            //    ModifierDescriptor = ModifierDescriptor.Circumstance,
            //    EnablingFeature = FriendlyFireManeuversFeature.ToReference<BlueprintUnitFactReference>(),
            //};
            NewComponents.SavingThrowBonusConditional savingThrowBonus = new()
            {
                savingThrowType = SavingThrowType.Reflex,
                Value = 4,
                Bonus = 0,
                ModifierDescriptor = ModifierDescriptor.Circumstance,
                Conditions = new ConditionsChecker()
                {
                    Operation = Operation.And,
                    Conditions = new Condition[]
                                                        {
                                                            new ContextConditionIsCaster() {Not = true },
                                                            new ContextConditionCasterIsPartyEnemy() {Not = true },
                                                            new NewComponents.ContextConditionProjectileType() {projTypes = new AbilityProjectileType[]{AbilityProjectileType.Line, AbilityProjectileType.Simple } },
                                                            new ContextConditionCasterHasFact() {m_Fact = FriendlyFireManeuversFeature.ToReference<BlueprintUnitFactReference>() }
                                                        }
                }
            };

            FriendlyFireManeuversFeature.AddComponent(savingThrowBonus);
            FriendlyFireManeuversFeature.AddComponent(new FeatureTagsComponent() { FeatureTags = FeatureTag.Attack | FeatureTag.Defense | FeatureTag.Ranged | FeatureTag.SavingThrows | FeatureTag.Teamwork });
            FriendlyFireManeuversFeature.AddToCache();
            FriendlyFireManeuversFeature.AddFeatureAsTeamwork(PackRagerGuids: ("4284269a86c94d44a0bd5ecfda581e9f", "e6022a7dfdb74c009912af8f68f2034f", "cf3d683689134902afd8068ccfef45a2", "63b9b940834347ce953a5090f17bb2b5", "5da9646f8e214e93aeeeb54ae2e0cb99"),
                                                              CavalierGuid: "d298b0083a624aec8b31fbf1dac1e1a9",
                                                              VanguardGuids: ("bf6cffb3c07e4fcc99372a99021e01d8", "67c48e49a0ca408e9f8dcfe0e2de801f"));
        }
    }
}
