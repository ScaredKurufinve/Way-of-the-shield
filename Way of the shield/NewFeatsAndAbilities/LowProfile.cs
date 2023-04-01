using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Designers.Mechanics.Facts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Way_of_the_shield.NewComponents;

namespace Way_of_the_shield.NewFeatsAndAbilities
{
    [HarmonyPatch]
    public class LowProfile
    {
        static List<(string guid, string name)> selections = new()
        {
            new ("247a4068296e8be42890143f451b4b45", "BasicFeatSelection"),
            new ("90f105c8e31a6224ea319e6a810e4af8", "LoremasterCombatFeatSelection"),
            new ("41c8486641f7d6d4283ca9dae4147a9f", "FighterFeatSelection")
        };

        [HarmonyPatch(typeof (BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPostfix]
        public static void BlueprintsCache_Init_Postfix_CreateLowProfile()
        {
            BlueprintFeature feature = new()
            {
                m_DisplayName = new() { m_Key = "LowProfileFeature_DisplayName" },
                m_Description = new() { m_Key = "LowProfileFeature_Description" },
                Groups = new[] {FeatureGroup.Feat, FeatureGroup.CombatFeat, FeatureGroup.Racial },
                IsClassFeature = true,
            };
            feature.AddToCache("b30ed2f9c55d409cb89d9bc34e4aafd2", "LowProfileFeature");
            feature.AddComponent(new RemoveSelfFromSoftCover());
            feature.AddComponent(new ACBonusAgainstAttacks() { AgainstRangedOnly = true, 
                                                               Value = 1,
                                                               Descriptor = ModifierDescriptor.Dodge}
            );
            feature.AddComponent(new PrerequisiteStatValue(){ Stat = Kingmaker.EntitySystem.Stats.StatType.Dexterity,
                                                              Value = 13}
            );
            feature.AddComponent(new PrerequisiteFeaturesFromList()
            {
                Amount = 1,
                m_Features = new[] { new BlueprintFeatureReference() { deserializedGuid = BlueprintGuid.Parse("ef35a22c9a27da345a4528f0d5889157") }, //gnome race
                                     new BlueprintFeatureReference() { deserializedGuid = BlueprintGuid.Parse("b0c3ef2729c498f47970bb50fa1acd30") } }
            }
            );
            feature.AddComponent(new FeatureTagsComponent() { FeatureTags = Kingmaker.Blueprints.Classes.Selection.FeatureTag.Defense });
            feature.AddFeatureToSelections(selections);
        }
    }
}
