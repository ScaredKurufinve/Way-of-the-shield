using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Way_of_the_shield.NewComponents;

namespace Way_of_the_shield.NewFeatsAndAbilities
{
    [HarmonyPatch]
    public static class Mythic_BucklerParry
    {
        public static BlueprintFeature Mythic_BucklerParryFeature {
            get
            {
                if (!Created) CreateBlueprint();
                return m_Mythic_BucklerParryFeature;
            } 
        }

        internal static bool Created;
        static BlueprintFeature m_Mythic_BucklerParryFeature;

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPostfix]
        internal static void CreateBlueprint()
        {
            #region Create Blueprint
            Sprite Icon = null;
            string circ = "when creating the Mythic_BucklerParryFeature blueprint";
            BlueprintFeature bp = new()
            {
                m_DisplayName = new() { m_Key = "Mythic_BucklerParryFeature_DisplayName" },
                m_Description = new() { m_Key = "Mythic_BucklerParryFeature_Description" },
                m_Icon = Icon,
                Groups = new FeatureGroup[] {FeatureGroup.MythicFeat}
            };
            bp.AddToCache("0a1c1d2138964c97ba8f48d7843ea685", "Mythic_BucklerParryFeature");
            bp.AddComponent(new FeatureTagsComponent() { FeatureTags = FeatureTag.Defense | FeatureTag.Melee});
            bp.AddComponent(new AdditionalParriesComponent()
            {
                Conditions = new()
                {
                    Conditions = new Condition[] 
                    {
                        new ContextConditionIsWeaponEquipped()
                        {
                            CheckOnCaster = true,
                            CheckWeaponCategory = true,
                            Category = WeaponCategory.WeaponLightShield,
                        }
                    }
                },
                Bonus= new()
                {
                    ValueType = ContextValueType.Rank,
                    ValueRank = AbilityRankType.Default
                },
                Penalized = false,
                Haste = false,
            });
            bp.AddComponent(new ContextRankConfig()
            {
                m_Type = AbilityRankType.Default,
                m_BaseValueType = ContextRankBaseValueType.FeatureRank,
                m_Feature = new() { deserializedGuid = BlueprintGuid.Parse("6948b379c0562714d9f6d58ccbfa8faa") }, //TwoWeaponFightingBasicMechanics
                m_Progression  = ContextRankProgression.BonusValue,
                m_StepLevel = -1,
            });
            bp.AddComponent(new PrerequisiteStatValue() { Stat = StatType.Dexterity, Value = 13, Group = Prerequisite.GroupType.All });
            bp.AddComponent(new PrerequisiteFeature() { m_Feature = new() { deserializedGuid = BlueprintGuid.Parse("ac8aaf29054f5b74eb18f2af950e752d") }, Group = Prerequisite.GroupType.All }); // TwoWeaponFighting
            bp.AddComponent(new PrerequisiteProficiency()
            {
                ArmorProficiencies= new ArmorProficiencyGroup[] {},
                WeaponProficiencies = new WeaponCategory[] { WeaponCategory.WeaponLightShield}
            });
            m_Mythic_BucklerParryFeature = bp;
            Created = true;
            #endregion
            #region Add to mythic selection
            BlueprintFeatureSelection mythicSelection = ResourcesLibrary.GetRoot()?.SystemMechanics.MythicFeatSelection;
            if (mythicSelection is null)
            {
                Comment.Error("Failed to retrieve the mythic selection blueprint from cache root " + circ) ;
                return;
            }
            if (!mythicSelection.m_AllFeatures.Contains(bp)) mythicSelection.m_AllFeatures = mythicSelection.m_AllFeatures.AddToArray(bp.ToReference<BlueprintFeatureReference>());
            #endregion
            #region Add to prerequisites
            if (!RetrieveBlueprint("ac8aaf29054f5b74eb18f2af950e752d", out BlueprintFeature TwoWeaponFighting, "TwoWeaponFighting", circ)) goto SkipTWF;
            var l1 = TwoWeaponFighting.IsPrerequisiteFor ??= new List<BlueprintFeatureReference>() { };
            if (!l1.Contains(bp))
                l1.Add(bp.ToReference<BlueprintFeatureReference>());
            SkipTWF:;
            if (!RetrieveBlueprint("7c28228ce4eed1543a6b670fd2a88e72", out BlueprintFeature BucklerProficiency, "BucklerProficiency", circ)) goto SkipProfBuc;
            l1 = BucklerProficiency.IsPrerequisiteFor ??= new List<BlueprintFeatureReference>() { };
            if (!l1.Contains(bp))
                l1.Add(bp.ToReference<BlueprintFeatureReference>());
            SkipProfBuc:;
            if (!RetrieveBlueprint("cb8686e7357a68c42bdd9d4e65334633", out BlueprintFeature ShieldsProficiency, "ShieldProficiency", circ)) goto SkipProfLShield;
            l1 = ShieldsProficiency.IsPrerequisiteFor ??= new List<BlueprintFeatureReference>() { };
            if (!l1.Contains(bp))
                l1.Add(bp.ToReference<BlueprintFeatureReference>());
            SkipProfLShield:;

            #endregion
        }
    }
}
