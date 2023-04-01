using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using UnityEngine;

namespace Way_of_the_shield.NewFeatsAndAbilities
{
    [HarmonyPatch]
    public class ShieldedMage
    {
        static readonly List<(string, string)> selections = new()
        {
            ("247a4068296e8be42890143f451b4b45", "BasicFeatSelection"),
            ("90f105c8e31a6224ea319e6a810e4af8", "LoremasterCombatFeatSelection"),
            ("41c8486641f7d6d4283ca9dae4147a9f", "FighterFeatSelection")
        };


        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPostfix]
        [HarmonyAfter("TabletopTweaks-Base")]
        public static void CachePatch_CreateShieldedMageFeature()
        {
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log("Begin creating Shielded Mage feature"); 
#endif
            string circ = "when creating ShieldedMage feature";
            Sprite _Icon = LoadIcon("ShieldedMage");
            BlueprintFeature ShieldedMageFeature = new()
            {
                m_DisplayName = new() { m_Key = "ShieldedMageFeature_DisplayName" },
                m_Description = new() { m_Key = "ShieldedMageFeature_Description" },
                m_Icon = _Icon,
                IsClassFeature = true,
                Groups = new[] { FeatureGroup.Feat },
            };
            ShieldedMageFeature.AddToCache("2fb71f38f95e432a82bb08c6431972d0", "ShieldedMageFeature");
            if (Main.TTTBase is not null && RetrieveBlueprint("ef38e0fe68f14c88a9deacc421455d14", out BlueprintFeatureSelection ShieldMastery, "ShieldMasterySelection", "to add Shield Brace"))
                selections.Add((ShieldMastery.AssetGuid.ToString(), "TTT-ShieldMasterySelection"));
            ShieldedMageFeature.AddFeatureToSelections(selections, circ);
            ShieldedMageFeature.AddComponent(new ArcaneSpellFailureIncrease() { ToShield = true, Bonus = -15 });
            ShieldedMageFeature.AddComponent(new FeatureTagsComponent() { FeatureTags = FeatureTag.Defense | FeatureTag.Magic });
            ShieldedMageFeature.AddComponent(new PrerequisiteCasterType() { IsArcane = true});
            ShieldedMageFeature.AddComponent(new PrerequisiteStatValue()
            {
                Stat = StatType.BaseAttackBonus,
                Value = 3,
                HideInUI = false,
                Group = Prerequisite.GroupType.Any
            });
            List<BlueprintFeatureReference> prerequisites = new() { };
            if (RetrieveBlueprint("ac57069b6bf8c904086171683992a92a", out BlueprintFeature ShieldFocus, "ShieldFocus", circ))
                prerequisites.Add(ShieldFocus.ToReference<BlueprintFeatureReference>());
            if (RetrieveBlueprint("3c380607706f209499d951b29d3c44f3", out BlueprintFeature ArmorTraining, "ArmorTraining", circ))
                prerequisites.Add(ArmorTraining.ToReference<BlueprintFeatureReference>());

            ShieldedMageFeature.AddComponent(new PrerequisiteFeaturesFromList()
            {
                m_Features = prerequisites.ToArray(),
                HideInUI = false,
                Group = Prerequisite.GroupType.All
            });
            if (RetrieveBlueprint("48ac8db94d5de7645906c7d0ad3bcfbd", out BlueprintCharacterClass FighterClass, "FighterClass", circ))
                ShieldedMageFeature.AddComponent(new PrerequisiteClassLevel()
                {
                    Level = 1,
                    m_CharacterClass = FighterClass.ToReference<BlueprintCharacterClassReference>(),
                    HideInUI = false,
                    Group = Prerequisite.GroupType.Any
                });
        }
    }
}
