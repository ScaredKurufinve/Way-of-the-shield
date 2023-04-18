using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using UnityEngine;
using static Way_of_the_shield.Main;

namespace Way_of_the_shield.NewFeatsAndAbilities.SacredShieldFeatures
{
    public class BastionOfGoodFeature
    {
        public static BlueprintFeature BOGAbility1;
        public static BlueprintFeature BOGAbility11;
        public static BlueprintFeature BOGAbility20;
        public static void CreateBastionOfGoodFeature()
        {
            string circ = "when creating the Bastion Of Good";
            Sprite Icon = LoadIcon("BastionOfGood");
            if (Icon is null) 
            {
                RetrieveBlueprint("3a6db57fce75b0244a6a5819528ddf26", out BlueprintFeature PaladinSmiteEvil, "PaladinSmiteEvil", circ);
                Icon = PaladinSmiteEvil?.Icon;
            }
            LocalizedString l_Displayname = new() { m_Key = "ShieldSmiteBuff_DisplayName" };
            #region Create ShieldSmiteAllyBuffHidden
                BlueprintBuff ShieldSmiteAllyBuffHidden = new()
                {
                    AssetGuid = new BlueprintGuid(new Guid("5240b74448e345d6b6d4b077c12e75b5")),
                    name = modName + "_ShieldSmiteAllyBuffHidden",
                    m_Flags = BlueprintBuff.Flags.StayOnDeath | BlueprintBuff.Flags.RemoveOnRest | BlueprintBuff.Flags.HiddenInUi,
                    Stacking = StackingType.Stack,
                    FxOnRemove = new(),
                };
            ShieldSmiteAllyBuffHidden.AddToCache();
            #endregion
            #region Create ShieldSmiteAllyBuff
            BlueprintBuff ShieldSmiteAllyBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("f7e1a37ceeaa41628cd1b04731eaee69")),
                name = modName + "_ShieldSmiteAllyBuff",
                m_Flags = BlueprintBuff.Flags.StayOnDeath | BlueprintBuff.Flags.RemoveOnRest,
                m_Icon = Icon,
                m_DisplayName = new LocalizedString() { Key = "ShieldSmiteAllyBuff_Displayname" },
                m_Description = new LocalizedString() { Key = "ShieldSmiteAllyBuff_Description" },
                FxOnRemove = new(),
                FxOnStart = new(),
            };
            ShieldSmiteAllyBuff.AddComponent(
                new AddFactContextActions()
                {
                    Activated = new()
                    {
                        Actions = new GameAction[1]
                        {
                            new ContextActionApplyBuff()
                            {
                                AsChild = true,
                                Permanent = true,
                                IsNotDispelable = true,
                                m_Buff = ShieldSmiteAllyBuffHidden.ToReference<BlueprintBuffReference>(),
                                //SameDuration = true,
                                //DurationValue = new(),
                            }
                        }
                    },
                    NewRound = new() { Actions= new GameAction[] {} },
                    Deactivated = new() { Actions = new GameAction[] { } },
                });
            ShieldSmiteAllyBuff.AddToCache();
            #endregion
            #region Create ShieldSmiteDebuffNoAroden
            BlueprintBuff ShieldSmiteDebuffNoAroden = new()
            {
                FxOnRemove = new(),
                FxOnStart = new(),
                m_Flags =   BlueprintBuff.Flags.HiddenInUi |
                            BlueprintBuff.Flags.StayOnDeath |
                            BlueprintBuff.Flags.RemoveOnRest |
                            BlueprintBuff.Flags.Harmful |
                            BlueprintBuff.Flags.RemoveOnRest,
                m_DisplayName = l_Displayname,
            };
            ShieldSmiteDebuffNoAroden.AddToCache("e9753adf1a5742fc96f91926054553b8", "ShieldSmiteDebuffNoAroden");
            ShieldSmiteDebuffNoAroden.AddComponent(new ACBonusAgainstTarget()
            {
                CheckCaster = true,
                CheckCasterFriend = false,
                Descriptor = ModifierDescriptor.Deflection,
                Value = new()
                {
                    ValueType = ContextValueType.Shared,
                    ValueShared = AbilitySharedValue.StatBonus,
                }
            });
            #endregion
            #region Create ShieldSmiteDebuffWithAroden
            BlueprintBuff ShieldSmiteDebuffWithAroden = new()
            {
                FxOnRemove = new(),
                FxOnStart = new(),
                m_Flags = BlueprintBuff.Flags.HiddenInUi |
                            BlueprintBuff.Flags.StayOnDeath |
                            BlueprintBuff.Flags.RemoveOnRest |
                            BlueprintBuff.Flags.Harmful |
                            BlueprintBuff.Flags.RemoveOnRest,
                m_DisplayName = l_Displayname,
            };
            ShieldSmiteDebuffWithAroden.AddToCache("03c8370ea11e4c988fe4c6a227c5181d", "ShieldSmiteDebuffWithAroden");
            ShieldSmiteDebuffWithAroden.AddComponent(new ACBonusAgainstTarget()
            {
                CheckCaster = true,
                CheckCasterFriend = false,
                Descriptor = ModifierDescriptor.Sacred,
                Value = new()
                {
                    ValueType = ContextValueType.Shared,
                    ValueShared = AbilitySharedValue.StatBonus,
                }
            });
            #endregion
            #region Create ShieldSmiteBuff blueprint
            BlueprintBuff ShieldSmiteBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("20e8cc979fb94234a9aa5bff6ea4289e")),
                name = modName + "_ShieldSmiteBuff",
                m_DisplayName = l_Displayname,
                m_Description = new LocalizedString() { m_Key = "Empty" },
                //m_DescriptionShort = new LocalizedString() { m_Key = "ShieldSmiteBuff_ShortDescription" },
                m_Icon = Icon,
                FxOnRemove = new(),
                IsClassFeature = true,
                Stacking = StackingType.Stack,
            };
            if (RetrieveBlueprint("b6570b8cbb32eaf4ca8255d0ec3310b0", out BlueprintBuff SmiteEvilBuff, "SmiteEvilBuff", circ))
                ShieldSmiteBuff.FxOnStart = SmiteEvilBuff.FxOnStart;
            ShieldSmiteBuff.AddComponent(new UniqueBuff());
            ShieldSmiteBuff.AddComponent(new RemoveBuffIfCasterIsMissing());
            ShieldSmiteBuff.AddComponent(new NewComponents.HalveDamageIfHasBuffFromCaster() { m_Buff = ShieldSmiteAllyBuffHidden.ToReference<BlueprintBuffReference>() });
            ShieldSmiteBuff.AddComponent(
                new AddFactContextActions()
                {
                    NewRound = new() { Actions = Array.Empty<GameAction>() },
                    Deactivated = new() { Actions = Array.Empty<GameAction>() },
                    Activated = new()
                    {
                        Actions = new[]
                        {
                            new Conditional()
                            {
                                ConditionsChecker = new()
                                {
                                    Conditions = new[]
                                    {
                                        new ContextConditionCasterHasFact()
                                        {
                                            m_Fact = new(){deserializedGuid = BlueprintGuid.Parse("36389cd62240b724f855920e2286d457") } // Scabbard_ArodensWrathFeature
                                        }
                                    }
                                },

                                IfTrue = new()
                                {
                                    Actions = new[]
                                    {
                                        new ContextActionApplyBuff()
                                        {
                                            AsChild = true,
                                            SameDuration = true,
                                            DurationValue = new(),
                                            m_Buff = ShieldSmiteDebuffWithAroden.ToReference<BlueprintBuffReference>()
                                        }
                                    }
                                },

                                IfFalse = new()
                                {
                                    Actions = new[]
                                    {
                                        new ContextActionApplyBuff()
                                        {
                                            AsChild = true,
                                            SameDuration = true,
                                            DurationValue = new(),
                                            m_Buff = ShieldSmiteDebuffNoAroden.ToReference<BlueprintBuffReference>()

                                        }
                                    }
                                }
                            }
                        }
                    }
                });
            ShieldSmiteBuff.AddComponent(
                new ContextCalculateSharedValue()
                {
                    ValueType = AbilitySharedValue.StatBonus,
                    Value = new()
                    {
                        DiceType = DiceType.One,
                        DiceCountValue = new()
                        {
                            ValueRank = AbilityRankType.Default,
                            ValueType = ContextValueType.Rank,
                        },
                        BonusValue = new()
                        {
                            ValueRank = AbilityRankType.ProjectilesCount,
                            ValueType = ContextValueType.Rank,

                        }
                    }

                });
            ShieldSmiteBuff.AddComponent(
                new ContextRankConfig()
                {
                    m_Type = AbilityRankType.ProjectilesCount,
                    m_BaseValueType = ContextRankBaseValueType.MaxClassLevelWithArchetype,
                    m_Class = new BlueprintCharacterClassReference[1] { new() { deserializedGuid = BlueprintGuid.Parse("bfa11238e7ae3544bbeb4d0b92e897ec") } },
                    Archetype = new() {deserializedGuid = BlueprintGuid.Parse("56F19F65E28B4C3FB03E425FB047E08A")},
                    m_Progression = ContextRankProgression.StartPlusDivStep,
                    m_StartLevel = -4,
                    m_StepLevel = 4,
                    m_UseMax = true,
                    m_Max = 5,

                });
            ShieldSmiteBuff.AddComponent(
                new ContextRankConfig()
                {
                    m_Type = AbilityRankType.Default,
                    m_BaseValueType = ContextRankBaseValueType.StatBonus,
                    m_Stat = StatType.Charisma,
                    m_Progression = ContextRankProgression.AsIs,
                    m_UseMin = true,
                    m_Min = 0,
                });
            ShieldSmiteBuff.AddToCache();
            #endregion
            #region Create ShieldSmiteAllyBuffPerfect
            BlueprintBuff ShieldSmiteAllyBuffPerfect = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("14bf28d3c74e4fb0a952d8f114633f66")),
                name = modName + "_ShieldSmiteAllyBuffPerfect",
                m_Flags = BlueprintBuff.Flags.StayOnDeath | BlueprintBuff.Flags.RemoveOnRest,
                m_Icon = Icon,
                m_DisplayName = new LocalizedString() { Key = "ShieldSmiteAllyBuffPerfect_Displayname" },
                m_Description = new LocalizedString() { Key = "ShieldSmiteAllyBuffPerfect_Description" },
                FxOnRemove = new(),
                FxOnStart = new(),
            };
            ShieldSmiteAllyBuffPerfect.AddComponent(
                new AddFactContextActions()
                {
                    Activated = new()
                    {
                        Actions = new GameAction[1]
                        {
                            new ContextActionApplyBuff()
                            {
                                AsChild = true,
                                Permanent = true,
                                IsNotDispelable = true,
                                m_Buff = ShieldSmiteAllyBuffHidden.ToReference<BlueprintBuffReference>(),
                                //SameDuration = true,
                                //DurationValue = new()

                            }
                        }
                    },
                    Deactivated = new() {Actions = new GameAction[] { } },
                    NewRound = new() { Actions = new GameAction[] { } },
                });
            ShieldSmiteAllyBuffPerfect.AddComponent(new NewComponents.AddRegerationFromTarget() 
                {m_checkCaster = true, 
                 m_checkedFact = ShieldSmiteBuff.ToReference<BlueprintBuffReference>(),
                 Heal = 10}
            );
            ShieldSmiteAllyBuffPerfect.AddToCache();
            #endregion
            #region Create ShieldSmite10AreaEffect blueprint
            BlueprintAbilityAreaEffect ShieldSmite10AreaEffect = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("631948517dec4519a38a0ad4bda8c341")),
                name = modName + "_ShieldSmite10AreaEffect",
                Shape = AreaEffectShape.Cylinder,
                Size = new(10),
                m_TargetType = BlueprintAbilityAreaEffect.TargetType.Ally,
                Fx = new(),
            };
            ShieldSmite10AreaEffect.AddComponent(
                new AbilityAreaEffectBuff()
                {
                    m_Buff = ShieldSmiteAllyBuff.ToReference<BlueprintBuffReference>(),
                    Condition = new()
                    {
                        Conditions = new Condition[]
                        {
                            new ContextConditionIsCaster()
                            {
                                Not = true
                            }
                        }
                    }
                });
            ShieldSmite10AreaEffect.AddToCache();
            #endregion
            #region Create ShieldSmite20AreaEffect blueprint
            BlueprintAbilityAreaEffect ShieldSmite20AreaEffect = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("8d2a1dafdaa84544a1f117c5283f0911")),
                name = modName + "_ShieldSmite20AreaEffect",
                Shape = AreaEffectShape.Cylinder,
                Size = new(20),
                m_TargetType = BlueprintAbilityAreaEffect.TargetType.Ally,
                Fx = new(),
            };
            ShieldSmite20AreaEffect.AddComponent(
                new AbilityAreaEffectBuff()
                {
                    m_Buff = ShieldSmiteAllyBuff.ToReference<BlueprintBuffReference>(),
                    Condition = new()
                    {
                        Conditions = new Condition[]
                        {
                            new ContextConditionIsCaster()
                            {
                                Not = true
                            }
                        }
                    }
                });
            ShieldSmite20AreaEffect.AddToCache();
            #endregion
            #region Create ShieldSmitePerfectAreaEffect blueprint
            BlueprintAbilityAreaEffect ShieldSmitePerfectAreaEffect = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("1e58d131750a442fa4c68848fa2b3167")),
                name = modName + "_ShieldSmitePerfectAreaEffect",
                Shape = AreaEffectShape.Cylinder,
                Size = new(20),
                m_TargetType = BlueprintAbilityAreaEffect.TargetType.Ally,
                Fx = new(),
            };
            ShieldSmitePerfectAreaEffect.AddComponent(
                new AbilityAreaEffectBuff()
                {
                    m_Buff = ShieldSmiteAllyBuffPerfect.ToReference<BlueprintBuffReference>(),
                    Condition = new()
                    {
                        Conditions = new Condition[]
                        {
                            new ContextConditionIsCaster()
                            {
                                Not = true
                            }
                        }
                    }
                });
            ShieldSmitePerfectAreaEffect.AddToCache();
            #endregion
            #region Create ShieldSmite10Buff
            BlueprintBuff ShieldSmite10Buff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("048e22da18ca42c1b0fb96db46e1096c")),
                name = modName + "_ShieldSmite10Buff",
                m_DisplayName = new LocalizedString() { Key = "BastionOfGoodFeature_DisplayName" },
                m_Description = new LocalizedString() { Key = "ShieldSmite10Buff_Description" },
                //m_DescriptionShort = new LocalizedString() { Key = "ShieldSmite10Buff_DescriptionShort" },
                m_Icon = Icon,
                IsClassFeature = true,
                FxOnRemove = new(),
            };
            ShieldSmite10Buff.AddComponent(new AddAreaEffect() { m_AreaEffect = ShieldSmite10AreaEffect.ToReference<BlueprintAbilityAreaEffectReference>() });
            ShieldSmite10Buff.AddToCache();
            #endregion
            #region Create ShieldSmite20Buff
            BlueprintBuff ShieldSmite20Buff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("d7e2ba2b7fde41178f6db286029ed054")),
                name = modName + "_ShieldSmite20Buff",
                m_DisplayName = new LocalizedString() { Key = "BastionOfGoodFeature_DisplayName" },
                m_Description = new LocalizedString() { Key = "ShieldSmite20Buff_Description" },
                //m_DescriptionShort = new LocalizedString() { Key = "ShieldSmite20Buff_DescriptionShort" },
                m_Icon = Icon,
                IsClassFeature = true,
                FxOnRemove = new(),
            };
            ShieldSmite20Buff.AddComponent(new AddAreaEffect() { m_AreaEffect = ShieldSmite20AreaEffect.ToReference<BlueprintAbilityAreaEffectReference>() });
            ShieldSmite20Buff.AddToCache();
            #endregion
            #region Create ShieldSmitePerfectBuff
            BlueprintBuff ShieldSmitePerfectBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("0404cc08e45246fdae240a79c8a7b515")),
                name = modName + "_ShieldSmitePerfectBuff",
                m_DisplayName = new LocalizedString() { Key = "BastionOfGoodFeature_DisplayName" },
                m_Description = new LocalizedString() { Key = "ShieldSmitePerfectBuff_Description" },
                //m_DescriptionShort = new LocalizedString() { Key = "ShieldSmitePerfectBuff_DescriptionShort" },
                m_Icon = Icon,
                IsClassFeature = true,
                FxOnRemove = new(),
            };
            ShieldSmitePerfectBuff.AddComponent(new AddAreaEffect() { m_AreaEffect = ShieldSmitePerfectAreaEffect.ToReference<BlueprintAbilityAreaEffectReference>() });
            ShieldSmitePerfectBuff.AddComponent(new NewComponents.AddRegerationFromTarget()
            {
                m_checkCaster = true,
                m_checkedFact = ShieldSmiteBuff.ToReference<BlueprintBuffReference>(),
                Heal = 10
            }
            );
            ShieldSmitePerfectBuff.AddToCache();
            #endregion
            #region Create BastionOfGoodCounterFeature
            BlueprintFeature BastionOfGoodCounterFeature = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("a456b09eb5064fa48295b5caf5191a41")),
                name = modName + "BastionOfGoodCounterFeature",
                HideInCharacterSheetAndLevelUp = true,
                HideInUI = true,
                IsClassFeature = true,
                Ranks = 3,
            };
            BastionOfGoodCounterFeature.AddToCache();
            #endregion
            #region Create BastionOfGoodAoEProviderBuff
            BlueprintBuff BastionOfGoodAoEProviderBuff = new()
            {
                AssetGuid = new BlueprintGuid( new Guid("ca005b615b554ddcb0bac1336c8b5751")),
                name = modName + "BastionOfGoodAoEProviderBuff",
                m_Flags = BlueprintBuff.Flags.HiddenInUi,
                FxOnRemove = new(),
            };
            BastionOfGoodAoEProviderBuff.AddComponent(
                new NewComponents.AuraFeatureComponentLadder()
                {
                    m_Buffs = new BlueprintBuffReference[3]
                    {
                        ShieldSmite10Buff.ToReference<BlueprintBuffReference>(),
                        ShieldSmite20Buff.ToReference<BlueprintBuffReference>(),
                        ShieldSmitePerfectBuff.ToReference<BlueprintBuffReference>(),
                    },
                    m_featureToCheck = BastionOfGoodCounterFeature.ToReference<BlueprintFeatureReference>()
                }) ;
            BastionOfGoodAoEProviderBuff.AddComponent(new NewComponents.RemoveBuffIfMaintargetIsMissing());
            BastionOfGoodAoEProviderBuff.AddToCache();
            #endregion
            #region Create ShieldSmiteAbility blueprint
            BlueprintAbility ShieldSmiteAbility = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("f421d2ab0f6f45e4a32b6460c1e692ee")),
                name = modName + "_ShieldSmiteAbility",
                m_DisplayName = new() { m_Key = "BastionOfGoodFeature_DisplayName" },
                m_Description = new() { m_Key = "BastionOfGoodFeature_ShortDescription" },
                m_Icon = Icon,
                Type = AbilityType.Supernatural,
                Range = AbilityRange.Medium,
                CanTargetEnemies = true,
                ShouldTurnToTarget = true,
                EffectOnEnemy = AbilityEffectOnUnit.Harmful,
                ActionType = UnitCommand.CommandType.Swift,
                Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Point,
            };

            if (RetrieveBlueprint("7bb9eb2042e67bf489ccd1374423cdec", out BlueprintAbility SmiteEvilAbility, "SmiteEvilAbility", circ))
            {
                if (SmiteEvilAbility.Components.TryFind(c => c is AbilityResourceLogic, out BlueprintComponent ARL))
                    ShieldSmiteAbility.AddComponent(ARL);
                if (SmiteEvilAbility.Components.TryFind(c => c is AbilitySpawnFx, out BlueprintComponent ASF))
                    ShieldSmiteAbility.AddComponent(ASF);
                if (SmiteEvilAbility.Components.TryFind(c => c is AbilityCasterAlignment, out BlueprintComponent ACA))
                    ShieldSmiteAbility.AddComponent(ACA);
                ShieldSmiteAbility.LocalizedDuration = SmiteEvilAbility.LocalizedDuration;
                ShieldSmiteAbility.LocalizedSavingThrow = SmiteEvilAbility.LocalizedSavingThrow;
            };
            GameAction[] buffingAction = new GameAction[2]
                                    {
                                        new ContextActionApplyBuff()
                                        {
                                            m_Buff = ShieldSmiteBuff.ToReference<BlueprintBuffReference>(),
                                            Permanent = true,
                                            DurationValue = new(),
                                        },
                                        new ContextActionOnContextCaster()
                                        {
                                            Actions = new()
                                            {
                                                Actions = new GameAction[1]
                                                {
                                                    new ContextActionApplyBuff()
                                                    {
                                                        m_Buff = BastionOfGoodAoEProviderBuff.ToReference<BlueprintBuffReference>(),
                                                        Permanent = true,
                                            DurationValue = new(),
                                                    }
                                                }
                                            }
                                        }
                                        
                                    };
            Conditional Main;
            if (RetrieveBlueprint("81b5d4099a8c5a7489c78d7f4150acd6", out BlueprintFeature HolyDevoteesWrathFeature, "HolyDevoteesWrathFeature", circ))
            {
                Main = new()
                {
                    ConditionsChecker = new()
                    {
                        Conditions = new Condition[1]
                        {
                            new ContextConditionCasterHasFact()
                            {
                                m_Fact = HolyDevoteesWrathFeature.ToReference<BlueprintUnitFactReference>()
                            }
                        }
                        
                    },
                    IfTrue = new ActionList()
                    {
                        Actions = new GameAction[1]
                        {
                            new Conditional()
                            {
                                ConditionsChecker = new()
                                {
                                    Conditions = new Condition[1]
                                    {
                                        new ContextConditionAlignment()
                                        {
                                            Not = true,
                                            Alignment = AlignmentComponent.Good
                                        }
                                    }
                                },
                                IfTrue = new()
                                {
                                    Actions = buffingAction
                                },
                                IfFalse = new(),
                            }
                        }
                    },
                    IfFalse = new()
                    {
                        Actions = new GameAction[1]
                        {
                            new Conditional()
                            {
                                ConditionsChecker = new()
                                {
                                    Conditions = new Condition[1]
                                    {
                                        new ContextConditionAlignment()
                                        {
                                            Alignment = AlignmentComponent.Evil
                                        }
                                    }
                                },
                                IfTrue = new()
                                {
                                    Actions = buffingAction
                                },
                                IfFalse = new()
                            }
                        }
                    }
                };
            }
            else
            {
                Main = new()
                {
                    ConditionsChecker = new()
                    {
                        Conditions = new Condition[1]
                        {
                            new ContextConditionAlignment()
                            {
                                Alignment = AlignmentComponent.Evil
                            }
                        }
                    },
                    IfTrue = new()
                    {
                        Actions = buffingAction
                    },
                    IfFalse = new()
                };
            }
            ShieldSmiteAbility.AddComponent(
                new AbilityEffectRunAction()
                {
                    Actions = new()
                    {
                        Actions = new GameAction[1]
                        {
                            Main
                        }
                    }
                });
            ShieldSmiteAbility.AddToCache();
            #endregion
            #region Create BastionOfGoodFeature blueprint
            BlueprintFeature BastionOfGoodFeature = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("16a2b29a9a5b4f0bb94a6405043fa258")),
                name = modName + "_BastionOfGoodFeature",
                m_DisplayName = new() { m_Key = "BastionOfGoodFeature_DisplayName" },
                m_Description = new() { m_Key = "BastionOfGoodFeature_Description" },
                m_DescriptionShort = new() { m_Key = "BastionOfGoodFeature_ShortDescription" },
                m_Icon = Icon,
                IsClassFeature = true,
            };
            BastionOfGoodFeature.AddComponent(new AddFacts()
            {
                m_Facts = new BlueprintUnitFactReference[2]
                {
                    BastionOfGoodCounterFeature.ToReference<BlueprintUnitFactReference>(),
                    ShieldSmiteAbility.ToReference<BlueprintUnitFactReference>()
                }
            });
            
            if (RetrieveBlueprint("b4274c5bb0bf2ad4190eb7c44859048b", out BlueprintAbilityResource SmiteEvilResource, "SmiteEvilResource", circ))
                BastionOfGoodFeature.AddComponent(
                    new AddAbilityResources()
                    {
                        m_Resource = SmiteEvilResource.ToReference<BlueprintAbilityResourceReference>(),
                        Amount = 0
                    });
            BastionOfGoodFeature.AddToCache();
            BOGAbility1 = BastionOfGoodFeature;
            #endregion
            #region Create Bastion Improved feature
            BlueprintFeature BastionOfGoodImprovedFeature = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("5249a6e441164be786db984b5c7ba17e")),
                name = modName + "_BastionOfGoodImprovedFeature",
                m_DisplayName = new() { m_Key = "BastionOfGoodImprovedFeature_DisplayName" },
                m_Description = new() { m_Key = "BastionOfGoodImprovedFeature_Description" },
                m_DescriptionShort = new() { m_Key = "BastionOfGoodImprovedFeature_ShortDescription" },
                m_Icon = Icon ,
                IsClassFeature = true,
            };
            BastionOfGoodImprovedFeature.AddComponent(new AddFacts()
            {
                m_Facts = new BlueprintUnitFactReference[1]
                {
                    BastionOfGoodCounterFeature.ToReference<BlueprintUnitFactReference>()
                }
            });
            BastionOfGoodImprovedFeature.AddToCache();
            BOGAbility11 = BastionOfGoodImprovedFeature;
            #endregion
            #region Create PerfectBastionFeature
            BlueprintFeature PerfectBastionFeature = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("0def38a6a9184fcfae3b66d54339397e")),
                name = modName + "_PerfectBastionFeature",
                m_DisplayName = new LocalizedString() { m_Key = "PerfectBastionFeature_Displayname" },
                m_Description = new LocalizedString() { m_Key = "PerfectBastionFeature_Description" },
                m_DescriptionShort = new LocalizedString() { m_Key = "PerfectBastionFeature_ShortDescription" },
                IsClassFeature = true,
                m_Icon = Icon,
            };
            PerfectBastionFeature.AddComponent(new AddFacts()
            {
                m_Facts = new BlueprintUnitFactReference[1]
                {
                    BastionOfGoodCounterFeature.ToReference<BlueprintUnitFactReference>()
                }
            });
            PerfectBastionFeature.AddToCache();

            BOGAbility20 = PerfectBastionFeature;
            #endregion

        }
    }
}
