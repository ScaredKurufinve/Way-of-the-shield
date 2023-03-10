using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Blueprints.Classes.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Kingmaker;
using Kingmaker.Blueprints.JsonSystem.Converters;
using System.Diagnostics;

namespace Way_of_the_shield.NewFeatsAndAbilities
{
    public class Mythic_ShieldedDefense
    {
        public static BlueprintFeature Feature
        {
            get { if (!Created && !InProcess) CreateBlueprint(); return m_feature; }
            set { }
        }
        static BlueprintFeature m_feature;

        internal static bool Created;
        internal static bool InProcess;
        internal static void CreateBlueprint()
        {
            InProcess = true;
            Sprite MythicIcon = LoadIcon("Mythic_ShieldedDefense");

            //LoadIcon("Mythic_ShieldedDefense");
            string circ = "when creating Mythic_ShieldedDefense blueprint";
            BlueprintFeature blueprint = new()
            {
                m_DisplayName = new() { m_Key = "Mythic_ShieldedDefense_m_DisplayName" },
                m_Description = new() { m_Key = "Mythic_ShieldedDefense_m_Description" },
                m_Icon = MythicIcon,
                Groups = new FeatureGroup[] {FeatureGroup.MythicFeat},
                Ranks = 1,
            };
            blueprint.AddToCache("3d621329f6874035b4be27edc6b84f25", "Mythic_ShieldedDefense");
            m_feature = blueprint;
            blueprint.AddComponent(new FeatureTagsComponent() { FeatureTags = FeatureTag.Defense });
            blueprint.AddComponent(new PrerequisiteFeature()
            {
                m_Feature = ShieldedDefense.ShieldedDefenseFeature.ToReference<BlueprintFeatureReference>(),
                Group = Prerequisite.GroupType.All
            });
            if (!RetrieveBlueprint("dbec636d84482944f87435bd31522fcc", out BlueprintFeature ShieldMaster, "ShieldMaster", circ)) goto skipShieldMasterPrerequisite;
            blueprint.AddComponent(new PrerequisiteFeature()
            {
                m_Feature = ShieldMaster.ToReference<BlueprintFeatureReference>(),
                Group  = Prerequisite.GroupType.All
            });
            ShieldMaster.IsPrerequisiteFor ??= new List<BlueprintFeatureReference>();
            ShieldMaster.IsPrerequisiteFor.Add(blueprint.ToReference<BlueprintFeatureReference>());
            skipShieldMasterPrerequisite:

            BlueprintFeatureSelection MythicFeatSelection = ResourcesLibrary.GetRoot().SystemMechanics.MythicFeatSelection;
            if (MythicFeatSelection is null) { Comment.Error("Failed to retrieve MythicFeatSelection blueprint from root " + circ); goto skipMythicFeatSlection; };
            MythicFeatSelection.m_AllFeatures = MythicFeatSelection.m_AllFeatures.AddToArray(blueprint.ToReference<BlueprintFeatureReference>());
            skipMythicFeatSlection:

            Created = true;
            InProcess = false;
        }
    }
}
