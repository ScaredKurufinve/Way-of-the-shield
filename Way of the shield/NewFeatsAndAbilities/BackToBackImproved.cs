using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Way_of_the_shield.NewComponents;

namespace Way_of_the_shield.NewFeatsAndAbilities
{
    [HarmonyPatch]
    public class BackToBackImproved
    {
        public static BlueprintFeature Feature
        {
            get
            {
                if (!Created) CreateBlueprint();
                return m_feature;
            }
            set{ }
        }
        static BlueprintFeature m_feature;
        public static bool Created;

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPostfix]
        public static void CreateBlueprint()
        {
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log("Begin creating BackToBackImproved feature blueprint"); 
#endif
            string circ = "when creating BackToBackImproved blueprint";
            bool flag = (ChangeBackToBack && (ConcealmentAttackBonusOnBackstab || DenyShieldBonusOnBackstab));
            BlueprintFeature blueprint = new()
            {
                m_DisplayName = new() { m_Key = "BackToBackImproved_DisplayName" },
                m_Description = new() { m_Key = ChangeBackToBack ? (flag ? "BackToBackImproved_Description" : "BackToBackImproved_SemiUnchanged_Description") : "BackToBackImproved_Unchanged_Description" },
                m_Icon = LoadIcon("BackToBackImproved"),

                Groups = new FeatureGroup[] { FeatureGroup.TeamworkFeat, FeatureGroup.CombatFeat, FeatureGroup.Feat},
        };
            blueprint.AddToCache("6314293bce0b4e1490ab10b1a7a5a318", "BackToBackImproved");
            if (flag) blueprint.AddComponent(new BackToBackImprovedComponent());
            else
            {
                Comment.Log("ChangeBackToBack setting is disabled, will be adding BackToBackBetter component to the BackToBackImproved feature blueprint instead of BackToBackImprovedComponent.");
                blueprint.AddComponent(new BackToBackBetter() { Radius = 5, m_BackToBackBetterFact = blueprint.ToReference<BlueprintUnitFactReference>() });
            }
            blueprint.AddComponent(new FeatureTagsComponent() { FeatureTags = FeatureTag.Defense | FeatureTag.Teamwork });
            blueprint.AddComponent(new PrerequisiteFeature() { m_Feature = new() { deserializedGuid = BlueprintGuid.Parse("c920f2cd2244d284aa69a146aeefcb2c") }, Group = Prerequisite.GroupType.All });
            blueprint.AddComponent(new PrerequisiteStatValue() { Stat = StatType.SkillPerception, Value = 3, Group = Prerequisite.GroupType.All });
            blueprint.AddComponent(new PrerequisiteStatValue() { Stat = StatType.Intelligence, Value = 3, Group = Prerequisite.GroupType.All });

            m_feature = blueprint;
            Created = true;


            if (!RetrieveBlueprint("c920f2cd2244d284aa69a146aeefcb2c", out BlueprintFeature BTB, "BackToBack", circ) ) return;
            if (BTB.IsPrerequisiteFor is not null && BTB.IsPrerequisiteFor.Contains(blueprint)) return;
            BTB.IsPrerequisiteFor ??= new();
            BTB.IsPrerequisiteFor.Add(blueprint.ToReference<BlueprintFeatureReference>());
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log($"Added {blueprint.name} to the {BTB.name} prerequisites.");
            blueprint.AddFeatureAsTeamwork();
#endif

        }
    }
}
