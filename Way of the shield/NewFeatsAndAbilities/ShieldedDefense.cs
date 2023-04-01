using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Properties;
using System;
using UnityEngine;
using Way_of_the_shield.NewComponents;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using static Way_of_the_shield.Main;
using static Way_of_the_shield.BoringBucklerTweaks;

namespace Way_of_the_shield.NewFeatsAndAbilities
{
    [HarmonyPatch]
    public static class ShieldedDefense
    {
        public static BlueprintFeature ShieldedDefenseFeature { get { if (!Created && !InProcess) BlueprintCache_Init_Patch(); return m_ShieldedDefenseFeature; } }
        static BlueprintFeature m_ShieldedDefenseFeature;

        internal static bool Created;
        internal static bool InProcess;

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPostfix]
        public static void BlueprintCache_Init_Patch()
        {
            InProcess = true;
#if DEBUG
            Comment.Log($"Begin creating Shielded Defense (flag is {Created})");
#endif
            if (Created) return;
            string circ = "when creating Shielded Defense";
            LocalizedString Name = new() { Key = "ShieldedDefenseActivatableAbility_DisplayName" };
            Sprite DefenseIcon = LoadIcon("Heavy");
            #region Create ShieldedDefenseProperty
            BlueprintUnitProperty ShieldedDefenseProperty = new()
            {
                BaseValue = 2,
            };
            ShieldedDefenseProperty.AddComponent(new ShieldedDefensePropertyGetter());
            ShieldedDefenseProperty.AddToCache("fa65cbcf6bba4f3992261173381a3874", "ShieldedDefenseProperty");
            #endregion
            #region Create ShieldedDefenseEffectBuff
            BlueprintBuff ShieldedDefenseEffectBuff = new()
            {
                name = modName + "_ShieldedDefenseEffectBuff",
                AssetGuid = new(new Guid("29397505648a462a874872b132a00b75")),
                m_DisplayName = new() { Key = "ShieldedDefenseActivatableAbility_DisplayName" },
                m_Description = new() { m_Key = "ShieldedDefenseActivatableAbility_Description" },
                m_DescriptionShort = new() { m_Key = "ShieldedDefenseActivatableAbility_ShortDescription" },
                FxOnRemove = new(),
                FxOnStart = new(),
                Stacking = StackingType.Rank,
                Ranks = 100,
                m_Icon = DefenseIcon,
            };
            ShieldedDefenseEffectBuff.AddComponent(new AddMechanicsFeature() { m_Feature = MechanicsFeatureExtension.ForceDualWieldingPenalties });
            ShieldedDefenseEffectBuff.AddComponent(new BuffSetRanksOnApply()
            {
                Value = new()
                {
                    ValueType = ContextValueType.CasterCustomProperty,
                    m_CustomProperty = ShieldedDefenseProperty.ToReference<BlueprintUnitPropertyReference>(),
                }
            });
            ShieldedDefenseEffectBuff.AddComponent(new ShieldedDefenseAcBonus()
            {
                Descriptor = ModifierDescriptor.Shield,
                StackMode = Kingmaker.EntitySystem.Stats.ModifiableValue.StackMode.ForceStack,
                Value = new()
                {
                    ValueType = ContextValueType.Rank,
                    ValueRank  = AbilityRankType.StatBonus,
                }
            });
            ShieldedDefenseEffectBuff.AddComponent(new ContextRankConfig()
            {
                m_Type = AbilityRankType.StatBonus,
                m_BaseValueType = ContextRankBaseValueType.CasterBuffRank,
                m_Buff = new() {deserializedGuid = BlueprintGuid.Parse("29397505648a462a874872b132a00b75") },
                m_BuffRankMultiplier = 1,
                m_Progression = ContextRankProgression.Div2,
            }); // AC bonus config
            ShieldedDefenseEffectBuff.AddComponent(new AddTargetAttackWithWeaponTrigger
            {
                DoNotPassAttackRoll = true,
                Categories = new WeaponCategory[]{ },
                OnlyHit = false,
                ActionOnSelf = new()
                {
                    Actions = new GameAction[]
                    {
                        new ContextActionRemoveBuffRanksCustom()
                        {
                            min = 0,
                            value = new(){ ValueType = ContextValueType.Rank, ValueRank = AbilityRankType.ProjectilesCount},
                            m_Buff = new() {deserializedGuid = BlueprintGuid.Parse("29397505648a462a874872b132a00b75") },
                            ToCaster = true,
                            RemoveWhenZero = false,
                        }
                    }
                },
                ActionsOnAttacker = new()
                {
                    Actions = new GameAction[] {}
                }
            });
            ShieldedDefenseEffectBuff.AddComponent(new ContextRankConfig()
            {
                m_Type = AbilityRankType.ProjectilesCount,
                m_UseMin = true,
                m_Min = 1,
                m_BaseValueType = ContextRankBaseValueType.FeatureRank,
                m_Progression = ContextRankProgression.StartPlusDivStep,
                m_Feature = Mythic_ShieldedDefense.Feature.ToReference<BlueprintFeatureReference>(),
                m_StepLevel = -1,
                m_StartLevel = 1,
            }); // Stack removal config
            ShieldedDefenseEffectBuff.AddComponent(new BuffDynamicDecriptionComponent_Charges(new LocalizedString() { m_Key = "ShieldedDefenseEffectBuff_DynamicDecription" }, new () { m_Progression = ContextRankProgression.Div2 }));
            ShieldedDefenseEffectBuff.AddToCache();
            #endregion
            #region Create ShieldedDefenseMainBuff blueprint
            BlueprintBuff ShieldedDefenseMainBuff = new()
            {
                name = modName + "_ShieldedDefenseVisibleBuff",
                AssetGuid = new(new Guid("807ca5b2501444aab22a0f08a64691e1")),
                m_DisplayName = new() { Key = "ShieldedDefenseActivatableAbility_DisplayName" },
                m_Description = new() { m_Key = "ShieldedDefenseActivatableAbility_Description" },
                m_DescriptionShort = new() { m_Key = "ShieldedDefenseActivatableAbility_ShortDescription" },
                m_Icon = DefenseIcon,
                FxOnRemove = new(),
                FxOnStart = new(),
                m_Flags = BlueprintBuff.Flags.StayOnDeath | BlueprintBuff.Flags.HiddenInUi,
            };

            /*ShieldedDefenseVisibleBuff.AddComponent(new BuffExtraEffects()
            {
                m_CheckedBuff = FightDefensivelyBuff?.ToReference<BlueprintBuffReference>(),
                m_ExtraEffectBuff = ShieldedDefenseEffectBuff.ToReference<BlueprintBuffReference>()
            });*/
            ShieldedDefenseMainBuff.AddToCache();
            #endregion
            #region Insert ShieldedDefenseMainBuff into FightDefensivelyBuff
            if (!RetrieveBlueprint("6ffd93355fb3bcf4592a5d976b1d32a9", out BlueprintBuff FightDefensivelyBuff, "FightDefensivelyBuff", "when inserting ShieldedDefenseVisibleBuff"))
            {
                Comment.Log("ERROR: Can't insert ShieldedDefenseVisibleBuff into FightDefensivelyBuff! Shielded Defense will not work!");
                goto SkipInsertion;
            }
            AddFactContextActions addFactContextActions = FightDefensivelyBuff.Components.OfType<AddFactContextActions>()?.FirstOrDefault();
            if (addFactContextActions is null)
            {
                Comment.Log("WARNING: Couldn't find an existing AddFactContextActions component on the FightDefensivelyBuff blueprint when inserting ShieldedDefenseVisibleBuff. This is sus, creating a new one.");
                addFactContextActions = new();
            }
            addFactContextActions.Deactivated ??= new();
            addFactContextActions.Activated ??= new();
            addFactContextActions.Activated.Actions = addFactContextActions.NewRound.Actions.AddToArray(
                new Conditional()
                {
                    name = $"{FightDefensivelyBuff}_addFactContextActions_NewRound_CheckForShieldedDefenseEffectBuff",
                    ConditionsChecker = new()
                    {
                        Conditions = new Condition[]
                        {
                            new ContextConditionCasterHasFact() {m_Fact = new(){deserializedGuid = BlueprintGuid.Parse("807ca5b2501444aab22a0f08a64691e1") } }
                        }
                    },
                    IfTrue = new()
                    {
                        Actions = new GameAction[] { new ContextActionApplyBuff() { m_Buff = ShieldedDefenseEffectBuff.ToReference<BlueprintBuffReference>(), ToCaster = true, Permanent = true, } }
                    },
                    IfFalse = new() { }
                });;
            addFactContextActions.NewRound ??= new();
            addFactContextActions.NewRound.Actions = addFactContextActions.NewRound.Actions.AddToArray(
                new Conditional()
                {
                    ConditionsChecker = new()
                    {
                        Conditions = new Condition[]
                        {
                            new ContextConditionCasterHasFact() {m_Fact = new(){deserializedGuid = BlueprintGuid.Parse("807ca5b2501444aab22a0f08a64691e1") } }
                        }
                    },
                    IfTrue = new()
                    {
                        Actions = new GameAction[] { new ContextActionApplyBuff() { m_Buff = ShieldedDefenseEffectBuff.ToReference<BlueprintBuffReference>(), ToCaster = true, Permanent = true,} }
                    }
                });
                
            addFactContextActions.Deactivated.Actions = addFactContextActions.Deactivated.Actions.AddToArray(new ContextActionRemoveBuff() { m_Buff = ShieldedDefenseEffectBuff.ToReference<BlueprintBuffReference>() });

        SkipInsertion:
            #endregion
            #region Create ShieldedDefenseActivatableAbility blueprint
            BlueprintActivatableAbility ShieldedDefenseActivatableAbility = new()
            {
                name = modName + "ShieldedDefenseActivatableAbility",
                AssetGuid = new(new Guid("7ca07f3bdbf145c2a975b11d9690c0b3")),
                m_DisplayName = Name,
                m_Description = new() { m_Key = "ShieldedDefenseActivatableAbility_Description" },
                m_DescriptionShort = new() { m_Key = "ShieldedDefenseActivatableAbility_ShortDescription" },
                m_Icon = DefenseIcon,
                ActivationType = AbilityActivationType.Immediately,
                DeactivateImmediately = true,
                DoNotTurnOffOnRest = true,
                m_Buff = ShieldedDefenseMainBuff.ToReference<BlueprintBuffReference>(),
            };
            ShieldedDefenseActivatableAbility.AddToCache();
            ShieldedDefenseActivatableAbility.AddComponent(new RestrictionOtherActivatables() { Require = true, m_ActivatableAbilities = new[] { new BlueprintActivatableAbilityReference() { deserializedGuid = BlueprintGuid.Parse("09d742e8b50b0214fb71acfc99cc00b3") } } }); // FightDefensivelyToggleAbility
            ShieldedDefenseActivatableAbility.AddComponent(new ShieldEquippedRestriction() { categories = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.LightShield, ArmorProficiencyGroup.HeavyShield, ArmorProficiencyGroup.TowerShield } });
            #endregion
            #region Create ShieldedDefenseFeature
            BlueprintFeature BlueprintShieldedDefenseFeature = new()
            {
                HideInUI = true,
                m_DisplayName = Name,
            };
            BlueprintShieldedDefenseFeature.AddToCache("76c121e7d9644d77948fe031b0eafe21", "ShieldedDefenseFeature");
            m_ShieldedDefenseFeature = BlueprintShieldedDefenseFeature;
            BlueprintShieldedDefenseFeature.AddComponent(new AddFacts() { m_Facts = new BlueprintUnitFactReference[] { ShieldedDefenseActivatableAbility.ToReference<BlueprintUnitFactReference>() } });
            #endregion
            if (AddShieldedDefense.GetValue() is false) goto skipMythicPet;
            #region Add buckler parry to ShieldBash restrictions
            if (!RetrieveBlueprint("3bb6b76ed5b38ab4f957c7f923c23b68", out BlueprintActivatableAbility ShieldBashAbility, "ShieldBashAbility", circ)) goto skipShieldBashAbility;
            IEnumerable<RestrictionOtherActivatables> ROA = ShieldBashAbility.GetComponents<RestrictionOtherActivatables>();
            if (ROA.Count() == 0)
            {
                var newRestriction = new RestrictionOtherActivatables()
                {
                    m_ActivatableAbilities = new BlueprintActivatableAbilityReference[] { ShieldedDefenseActivatableAbility.ToReference<BlueprintActivatableAbilityReference>() }
                };
                ShieldBashAbility.AddComponent(newRestriction);
            }
            else
            {
                if (ROA.Any(restriction => restriction.m_ActivatableAbilities.Contains(ShieldedDefenseActivatableAbility)))  goto skipShieldBashAbility;
                else {var firstRestriction = ROA.First(); firstRestriction.m_ActivatableAbilities = firstRestriction.m_ActivatableAbilities.AddToArray(ShieldedDefenseActivatableAbility.ToReference<BlueprintActivatableAbilityReference>()); }
            }
#if DEBUG
            Comment.Log("Added ShieldedDefenseActivatableAbility to the ShieldBashAbility restrictions."); 
#endif
            skipShieldBashAbility:
            #endregion
            if (!RetrieveBlueprint("cb8686e7357a68c42bdd9d4e65334633", out BlueprintFeature ShieldProficiency, "ShieldProficiency", circ)) goto skipShieldProficiency;
            AddFacts af = ShieldProficiency.GetComponent<AddFacts>();
            if (af is null)
            {
                af = new() { m_Facts = new BlueprintUnitFactReference[] { } };
                ShieldProficiency.AddComponent(af);
            }
            if (!af.m_Facts.Any(f => f.Guid == ShieldedDefenseActivatableAbility.AssetGuid))
                af.m_Facts = af.m_Facts.AddToArray(ShieldedDefenseActivatableAbility.ToReference<BlueprintUnitFactReference>());

#if DEBUG
            Comment.Log("Added ShieldedDefenseActivatableAbility to the ShieldProficiency blueprint."); 
#endif
        skipShieldProficiency:
            if (!RetrieveBlueprint("82fbdd5eb5ac73b498c572cc71bda48f", out BlueprintFeature ElementalBastionFeature, "ElementalBastionFeature", circ)) goto skipELementalBastion;
            if (!(ElementalBastionFeature.Components.Any(c => c is AddFacts af && af.m_Facts.Any(f => f.Guid == ShieldedDefenseActivatableAbility.AssetGuid)))) 
            ElementalBastionFeature.AddComponent(new AddFacts() { m_Facts = new BlueprintUnitFactReference[] { ShieldedDefenseActivatableAbility.ToReference<BlueprintUnitFactReference>() } });
#if DEBUG
            Comment.Log("Added ShieldedDefenseActivatableAbility to the ElementalBastionFeature blueprint."); 
#endif
        skipELementalBastion:
            if (!RetrieveBlueprint("94fe0ca10f17bf143a7d2bd2ab2acdda", out BlueprintFeature MythicPetEmptyFeature, "MythicPetEmptyFeature", circ)) goto skipMythicPet;
            if (!(MythicPetEmptyFeature.Components.Any(c => c is AddFacts af && af.m_Facts.Any(f => f.Guid == ShieldedDefenseActivatableAbility.AssetGuid))))
                MythicPetEmptyFeature.AddComponent(new AddFacts() { m_Facts = new BlueprintUnitFactReference[] { ShieldedDefenseActivatableAbility.ToReference<BlueprintUnitFactReference>() } });
#if DEBUG
            Comment.Log("Added ShieldedDefenseActivatableAbility to the MythicPetEmptyFeature blueprint.");
#endif
        skipMythicPet:;
            Created = true;
            InProcess = false;

        }
    }
}

