using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Way_of_the_shield.NewFeatsAndAbilities.SacredShieldFeatures;
using static Way_of_the_shield.Main;
using static Way_of_the_shield.NewFeatsAndAbilities.SacredShieldFeatures.BastionOfGoodFeature;
using static Way_of_the_shield.NewFeatsAndAbilities.SacredShieldFeatures.HolyShield;
using static Way_of_the_shield.NewFeatsAndAbilities.SacredShieldFeatures.SacredShieldBond;

namespace Way_of_the_shield.NewArchetypes
{
    [HarmonyPatch]
    public class SacredShield
    {
        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPostfix]
        public static void CreateSacredShieldArchetype()
        {
            string circ = "when creating the Sacred  archetype blueprint";
            RetrieveBlueprint("bfa11238e7ae3544bbeb4d0b92e897ec", out BlueprintCharacterClass PaladinClass, "PaladinClass", circ);
            #region Create the archetype blueprint
            BlueprintArchetype SacredShieldArchetype = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("56F19F65E28B4C3FB03E425FB047E08A")),
                name = modName + "_SacredShieldArchetype",
                LocalizedName = new LocalizedString() { m_Key = "SacredShieldArchetype_DisplayName" },
                LocalizedDescription = new LocalizedString() { m_Key = "SacredShieldArchetype_Description" },
                LocalizedDescriptionShort = new LocalizedString() { m_Key = "Empty" },
                ReplaceStartingEquipment = true,
                m_StartingItems = new BlueprintItemReference[4],
                AddFeatures = new LevelEntry[10],
                RemoveFeatures = new LevelEntry[5],
                m_ParentClass = PaladinClass,
            };
            SacredShieldArchetype.AddToCache();
            #endregion
            #region Add Items
            if (RetrieveBlueprint("d7963e1fcf260c148877afd3252dbc91", out BlueprintItem ScalemailStandard, "ScalemailStandard", circ))
                SacredShieldArchetype.m_StartingItems[0] = ScalemailStandard.ToReference<BlueprintItemReference>();
            if (RetrieveBlueprint("d52566ae8cbe8dc4dae977ef51c27d91", out BlueprintItem PotionOfCureLightWounds, "PotionOfCureLightWounds", circ))
                SacredShieldArchetype.m_StartingItems[1] = PotionOfCureLightWounds.ToReference<BlueprintItemReference>();
            if (RetrieveBlueprint("533e10c8b4c6a4940a3767d096f4f05d", out BlueprintItem ColdIronLongsword, "ColdIronLongsword", circ))
                SacredShieldArchetype.m_StartingItems[2] = ColdIronLongsword.ToReference<BlueprintItemReference>();
            if (RetrieveBlueprint("f4cef3ba1a15b0f4fa7fd66b602ff32b", out BlueprintItem HeavyShield, "ScalemailStandard", circ))
                SacredShieldArchetype.m_StartingItems[3] = HeavyShield.ToReference<BlueprintItemReference>();
            #endregion
            #region Add Features
            CreateBastionOfGoodFeature();
            CreateHolyShieldAbilities();
            CreateSacredShieldBond();
            BlueprintFeatureBaseReference WeaponBondAdditionalUse = new() { deserializedGuid = AdditionalUsesFeature.AssetGuid };
            SacredShieldArchetype.AddFeatures[0] = new()
            {
                Level = 1,
                m_Features = new List<BlueprintFeatureBaseReference>
                {
                    BOGAbility1?.ToReference<BlueprintFeatureBaseReference>()   
                }
            };
            SacredShieldArchetype.AddFeatures[1] = new()
            {
                Level = 4,
                m_Features = new List<BlueprintFeatureBaseReference>
                {
                    SHAbility4?.ToReference<BlueprintFeatureBaseReference>()
                }
            };
            SacredShieldArchetype.AddFeatures[2] = new()
            {
                Level = 5,
                m_Features = new List<BlueprintFeatureBaseReference>
                {
                    SSFeature?.ToReference<BlueprintFeatureBaseReference>()
                }
            };
            SacredShieldArchetype.AddFeatures[3] = new()
            {
                Level = 8,
                m_Features = new List<BlueprintFeatureBaseReference>
                {
                    BondPlus2?.ToReference<BlueprintFeatureBaseReference>(),
                }
            };
            SacredShieldArchetype.AddFeatures[4] = new()
            {
                Level = 9,
                m_Features = new List<BlueprintFeatureBaseReference>
                {
                    WeaponBondAdditionalUse,
                }
            };
            SacredShieldArchetype.AddFeatures[5] = new()
            {
                Level = 11,
                m_Features = new List<BlueprintFeatureBaseReference>
                {
                    BOGAbility11?.ToReference<BlueprintFeatureBaseReference>(),
                    SHAbility11?.ToReference<BlueprintFeatureBaseReference>(),
                    BondPlus3?.ToReference<BlueprintFeatureBaseReference>(),
                }
            };
            SacredShieldArchetype.AddFeatures[6] = new()
            {
                Level = 13,
                m_Features = new List<BlueprintFeatureBaseReference>
                {
                    WeaponBondAdditionalUse,
                }
            };
            SacredShieldArchetype.AddFeatures[7] = new()
            {
                Level = 14,
                m_Features = new List<BlueprintFeatureBaseReference>
                {
                    BondPlus4?.ToReference<BlueprintFeatureBaseReference>(),
                }
            };
            SacredShieldArchetype.AddFeatures[8] = new()
            {
                Level = 17,
                m_Features = new List<BlueprintFeatureBaseReference>
                {
                    WeaponBondAdditionalUse,
                    BondPlus5?.ToReference<BlueprintFeatureBaseReference>(),
                }
            };
            SacredShieldArchetype.AddFeatures[9] = new()
            {
                Level = 20,
                m_Features = new List<BlueprintFeatureBaseReference>
                {
                    BOGAbility20?.ToReference<BlueprintFeatureBaseReference>(),
                    SHAbility20?.ToReference<BlueprintFeatureBaseReference>(),
                    BondPlus6?.ToReference<BlueprintFeatureBaseReference>(),
                }
            };
            #endregion
            #region Remove Features
            LevelEntry lvl1 = new() { Level = 1, m_Features = new() };
            LevelEntry lvl4 = new() { Level = 4, m_Features = new() };
            LevelEntry lvl5 = new() { Level = 5, m_Features = new() };
            LevelEntry lvl11 = new() { Level = 11, m_Features = new() };
            LevelEntry lvl20 = new() { Level = 20, m_Features = new() };
            if (RetrieveBlueprint("3a6db57fce75b0244a6a5819528ddf26", out BlueprintFeature PaladinSmiteEvil, "PaladinSmiteEvil", circ)) lvl1.m_Features.Add(PaladinSmiteEvil.ToReference<BlueprintFeatureBaseReference>());
            if (RetrieveBlueprint("cb6d55dda5ab906459d18a435994a760", out BlueprintFeature ChannelEnergyPaladinFeature, "ChannelEnergyPaladinFeature", circ)) lvl4.m_Features.Add(ChannelEnergyPaladinFeature.ToReference<BlueprintFeatureBaseReference>());
            if (RetrieveBlueprint("ad7dc4eba7bf92f4aba23f716d7a9ba6", out BlueprintFeature PaladinDivineBondSelection, "PaladinDivineBondSelection", circ)) lvl5.m_Features.Add(PaladinDivineBondSelection.ToReference<BlueprintFeatureBaseReference>());
            if (RetrieveBlueprint("9f13fdd044ccb8a439f27417481cb00e", out BlueprintFeature AuraOfJusticeFeature, "AuraOfJusticeFeature", circ)) lvl11.m_Features.Add(AuraOfJusticeFeature.ToReference<BlueprintFeatureBaseReference>());
            if (RetrieveBlueprint("eff3b63f744868845a2f511e9929f0de", out BlueprintFeature HolyChampion, "PaladinDivineBondSelection", circ)) lvl20.m_Features.Add(HolyChampion.ToReference<BlueprintFeatureBaseReference>());
            SacredShieldArchetype.RemoveFeatures[0] = lvl1;
            SacredShieldArchetype.RemoveFeatures[1] = lvl4;
            SacredShieldArchetype.RemoveFeatures[2] = lvl5;
            SacredShieldArchetype.RemoveFeatures[3] = lvl11;
            SacredShieldArchetype.RemoveFeatures[4] = lvl20;
            #endregion
            #region add features to UI groups
            if (RetrieveBlueprint("fd325cbba872e5f40b618970678db002", out BlueprintProgression PaladinProgression, "PaladinProgression", circ))
            {
                UIGroup g = PaladinProgression.UIGroups.Where(group => group.m_Features.Contains(ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("0f5c99ffb9c084545bbbe960b825d137").ToReference<BlueprintFeatureBaseReference>()))
                            .FirstOrDefault();
                if (g is not null)
                
                   g.m_Features.AddRange(new List<BlueprintFeatureBaseReference>()
                   {
                       BOGAbility1.ToReference<BlueprintFeatureBaseReference>(),
                       BOGAbility11.ToReference<BlueprintFeatureBaseReference>(),
                       BOGAbility20.ToReference<BlueprintFeatureBaseReference>(),
                   });
                else Comment.Log("Can't find the UI group containing Smite, {0}.", circ);

                g = PaladinProgression.UIGroups.Where(group => group.m_Features.Contains(ChannelEnergyPaladinFeature.ToReference<BlueprintFeatureBaseReference>()))
                            .FirstOrDefault();
                if (g is not null)
                    g.m_Features.AddRange(
                       new List<BlueprintFeatureBaseReference>()
                       {
                           SHAbility4.ToReference<BlueprintFeatureBaseReference>(),
                           SHAbility11.ToReference<BlueprintFeatureBaseReference>(),
                           SHAbility20.ToReference<BlueprintFeatureBaseReference>(),
                       });
                else Comment.Log("Can't find the UI group containing Channel Energy, {0}.", circ);
                g = PaladinProgression.UIGroups.Where(group => group.m_Features.Contains(f => f.deserializedGuid == BlueprintGuid.Parse("1c7cdc1605554954f838d85bbdd22d90")))
                            .FirstOrDefault();
                int i = -1;
                if (g is not null)
                {
                    g.m_Features.AddRange(
                       new List<BlueprintFeatureBaseReference>()
                       {
                           SSFeature?.ToReference<BlueprintFeatureBaseReference>(),
                           //WeaponBondAdditionalUse,
                           BondPlus2.ToReference<BlueprintFeatureBaseReference>(),
                           BondPlus3.ToReference<BlueprintFeatureBaseReference>(),
                           BondPlus4.ToReference<BlueprintFeatureBaseReference>(),
                           BondPlus5.ToReference<BlueprintFeatureBaseReference>(),
                           BondPlus6.ToReference<BlueprintFeatureBaseReference>(),
                       });

                     i = PaladinProgression.UIGroups.IndexOf(g);
                }
                else Comment.Log("Can't find the UI group containing Weapon Bond, {0}.", circ);
                UIGroup AdditionalUsesChupaChups = new() { m_Features = new List<BlueprintFeatureBaseReference>() { WeaponBondAdditionalUse } };
                var temporary = PaladinProgression.UIGroups.ToList();
                temporary.Insert(i + 1, AdditionalUsesChupaChups);
                PaladinProgression.UIGroups = temporary.ToArray();
            }
            #endregion
            PaladinClass.m_Archetypes = PaladinClass.m_Archetypes.AddToArray(SacredShieldArchetype.ToReference<BlueprintArchetypeReference>());
        }
    }
}
