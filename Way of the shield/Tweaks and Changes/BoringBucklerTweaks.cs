using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using UnityEngine;
using Way_of_the_shield.NewComponents;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Way_of_the_shield
{
    [HarmonyPatch]
    public static class BoringBucklerTweaks
    {

        public class Buckler1h_Component : CanUse2hWeaponAs1hBase, IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>, IInitiatorRulebookHandler<RuleAttackWithWeapon>
        {
            public static BlueprintBuff ShieldForbiddance;
            //static Buckler1h_Component()
            //{
            //    ShieldForbiddance = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("414f40680af64050a2a9dde3dede32ac");
            //   if (ShieldForbiddance is not null) return;
            //    ShieldForbiddance = new()
            //    {
            //        AssetGuid = new BlueprintGuid(new Guid("414f40680af64050a2a9dde3dede32ac")),
            //        name = "WayOfTheShield_ForbidShieldACforOneTurn",
            //        FxOnRemove = new(),
            //        m_Flags = BlueprintBuff.Flags.HiddenInUi
            //   };
            //    ShieldForbiddance.AddComponent(new AddMechanicsFeature() {m_Feature = MechanicsFeatureExtension.ShieldDenied });

            //}

            public override bool CanBeUsedAs2h(ItemEntityWeapon weapon)
            {
                return true;
            }

            public override bool CanBeUsedOn(ItemEntityWeapon weapon)
            {
                if (weapon is null) return false;
                if (weapon.Blueprint.Double) return false;
                ItemEntityShield shield = (weapon.HoldingSlot as HandSlot)?.PairSlot?.MaybeShield;
                if (shield is null) return false;
                if (shield.ArmorComponent.Blueprint.ProficiencyGroup == ArmorProficiencyGroup.Buckler) return true;
                else return false;
            }

            public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
            {
                if (evt.Initiator != Owner) return;
                ItemEntityWeapon weapon = evt.Weapon;
                if (!CanBeUsedOn(weapon)) return;
                if (!weapon.HoldInTwoHands) return;
                BlueprintWeaponType type = weapon.Blueprint.Type;

                if (
                     (  type.Category == WeaponCategory.Longbow
                     || type.Category == WeaponCategory.HeavyCrossbow
                     || type.Category == WeaponCategory.LightCrossbow
                     || type.Category == WeaponCategory.HandCrossbow
                     || type.Category == WeaponCategory.HeavyRepeatingCrossbow
                     || type.Category == WeaponCategory.LightRepeatingCrossbow
                     )
                   && !Owner.Ensure<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>().UnhinderingShield
                   )
                        evt.AddModifier(-1, Fact, ModifierDescriptor.Penalty); 
            }

            public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt) { }
            public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
            {
                if (evt.Weapon.HoldInTwoHands && evt.IsFirstAttack && Owner.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.UnhinderingShield) Owner.AddBuff(ShieldForbiddance, Fact.MaybeContext, new TimeSpan?(new Rounds(1).Seconds));
            }
            public void OnEventDidTrigger(RuleAttackWithWeapon evt) { }

            [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
            public static class CachePatch
            {

                //[HarmonyPrepare]
                //public static bool Prepare()
                //{
                //    if (AllowTwoHanded_as_OneHandedWhenBuckler.GetValue()) return true;
                //    else { Comment.Log("AllowTwoHanded_as_OneHandedWhenBuckler setting is disabled, patch AddBuckler1hToProfficiencyBlueprint won't be applied."); return false; };
                //}
                [HarmonyPostfix]
                public static void AddBuckler1hToBlueprint()
                {
#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log("Entered cache postfix to add Buckler1h component"); 
#endif
                    #region create 1h feature
                    BlueprintFeature Buckler1h_blueprint = new()
                    {
                        AssetGuid = new(new Guid("eb322fe21ec64a928ace4eaedb6d4339")),
                        name = "Buckler1h_Feature",
                        m_DisplayName = new LocalizedString() { Key = "Buckler1h_Name" },
                        HideInCharacterSheetAndLevelUp = true
                    };
                    Buckler1h_blueprint.AddComponent(new Buckler1h_Component());
                    Buckler1h_blueprint.AddToCache();
                    #endregion

                    if (!AllowTwoHanded_as_OneHandedWhenBuckler.GetValue())
                        { Comment.Log("AllowTwoHanded_as_OneHandedWhenBuckler setting is disabled, patch AddBuckler1hToProfficiencyBlueprint won't be applied."); return ; };

                    if (!RetrieveBlueprint("ca22afeb94442b64fb8536e7a9f7dc11", out BlueprintFeature FightDefensively, "FightDefensively", "when adding Buckler_1h to it")) return;
                    if (FightDefensively.Components.First(component => component is AddFacts) is not AddFacts AF)
                    {
                        Comment.Warning("Failed to find AddFacts component on the FightDefensively feature blueprint. Can not add Buckler1h component.");
                        return;
                    };
                    AF.m_Facts = AF.m_Facts.Append(Buckler1h_blueprint.ToReference<BlueprintUnitFactReference>()).ToArray();

                    if (RetrieveBlueprint("26fcc43f7d20374498d2e1643381d345", out BlueprintShieldType BucklerType, "BucklerType"))
                        BucklerType.m_DescriptionText = new LocalizedString() { Key = "Buckler_Description" };
                }
            }
        }
        public static BlueprintActivatableAbility BlueprintBucklerParryActivatableAbility 
        {
            get
            {
                if (!Created) AddBucklerParryToBucklerProfficiencyBlueprint();
                return m_BlueprintBucklerParryActivatableAbility;
            }
        }

        internal static bool Created;
        static BlueprintActivatableAbility m_BlueprintBucklerParryActivatableAbility;

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPostfix]
        public static void AddBucklerParryToBucklerProfficiencyBlueprint()
        {
            Sprite BucklerParryIcon = LoadIcon("Buckler");
            #region Create Shield Forbiddance blueprint
            BlueprintBuff ShieldForbiddance = new()
            {
                FxOnRemove = new(),
                m_Flags = BlueprintBuff.Flags.HiddenInUi,
            };
            ShieldForbiddance.AddComponent(new AddMechanicsFeature() { m_Feature = MechanicsFeatureExtension.ShieldDenied });
            ShieldForbiddance.AddToCache("414f40680af64050a2a9dde3dede32ac", "ForbidShieldACforOneTurn");
            Buckler1h_Component.ShieldForbiddance = ShieldForbiddance;
            #endregion
            #region Create BucklerParryBuff
            BlueprintBuff BucklerParryBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("a8f2b254bc2e4f8b95e12aa444287c58")),
                name = "BucklerParryBuff",
                m_DisplayName = new LocalizedString() { Key = "BucklerParry_DisplayName" },
                m_Description = new LocalizedString() { Key = "BucklerParry_Description" },
                m_DescriptionShort = new LocalizedString() { Key = "BucklerParry_ShortDescription" },
                Stacking = StackingType.Replace,
                m_Icon = BucklerParryIcon,
                FxOnRemove = new(),
                FxOnStart = new()
            };
            BucklerParryBuff.AddToCache();
            BucklerParryBuff.AddComponent(new OffHandParry.OffHandParryComponent() { category = WeaponCategory.WeaponLightShield });
            //BucklerParryBuff.AddComponent(new AddMechanicsFeature() { m_Feature = MechanicsFeatureExtension.ShieldDenied });
            BucklerParryBuff.AddComponent(new BuffDynamicDescriptionComponent_Parries(new() { m_Key = "BucklerParry_DynamicDescription" }));
            #endregion
            #region BucklerParryMainBuff
            BlueprintBuff BucklerParryMainBuff = new()
            {
                m_Flags = BlueprintBuff.Flags.HiddenInUi | BlueprintBuff.Flags.StayOnDeath,
                FxOnRemove = new(),
                FxOnStart = new()
            };
            BucklerParryMainBuff.AddToCache("dc65131392614eecba8b399e279c3915", "BucklerParryMainBuff");
            BucklerParryMainBuff.AddComponent(new AddInitiatorAttackWithWeaponTrigger()
            {
                ActionsOnInitiator = true,
                CheckWeaponRangeType = true,
                RangeType = WeaponRangeType.Melee,
                TriggerBeforeAttack = true,
                OnlyOnFirstAttack = true,
                Action = new()
                {
                    Actions = new GameAction[]
                    {
                        new ContextActionApplyBuff()
                        {
                            m_Buff = BucklerParryBuff.ToReference<BlueprintBuffReference>(),
                            UseDurationSeconds = true,
                            DurationSeconds = 7,
                            IsNotDispelable = true,
                            ToCaster = true,
                        }
                    }
                }
            });
            #endregion
            #region Create BucklerParryActivatableAbility
            BlueprintActivatableAbility BucklerParryActivatableAbility = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("a86db1a253d0446b857938314af1ae51")),
                name = "BucklerParryActivatableAbility",
                m_Buff = BucklerParryMainBuff.ToReference<BlueprintBuffReference>(),
                ActivationType = AbilityActivationType.Immediately,
                //DeactivateAfterFirstRound = true,
                DeactivateIfCombatEnded= true,
                m_DisplayName = new LocalizedString() { Key = "BucklerParry_DisplayName" },
                m_Description = new LocalizedString() { Key = "BucklerParry_Description" },
                m_DescriptionShort = new LocalizedString() { Key = "BucklerParry_ShortDescription" },
                DoNotTurnOffOnRest = true,
                m_Icon = BucklerParryIcon,
            };
            BucklerParryActivatableAbility.AddComponent(new ShieldEquippedRestriction() { categories = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.Buckler, ArmorProficiencyGroup.LightShield } });
            BucklerParryActivatableAbility.AddComponent(new RestrictionNonRangedWeapon());
            BucklerParryActivatableAbility.AddComponent(new DirectlyControlledUnlessFactRestriction() { m_Fact = new() { deserializedGuid = BlueprintGuid.Parse("ac8aaf29054f5b74eb18f2af950e752d") } }); //TwoWeaponFighting
            BucklerParryActivatableAbility.AddToCache();
            #endregion
            #region BucklerParryFeature
            BlueprintFeature BucklerParryFeature = new()
            {
                HideInUI = true,
            };
            BucklerParryFeature.AddToCache("e547039b227a42d7a8d98fad72f8717c", "BucklerParryFeature");
            BucklerParryFeature.AddComponent(new AddFacts() { m_Facts = new BlueprintUnitFactReference[] { BucklerParryActivatableAbility.ToReference<BlueprintUnitFactReference>() } });
            BlueprintUnitFactReference BucklerParryFeatureReference = BucklerParryFeature.ToReference<BlueprintUnitFactReference>();
            #endregion
            string circ = "when adding BucklerParry";
            #region Add buckler parry to ShieldBash restrictions
            if (!RetrieveBlueprint("3bb6b76ed5b38ab4f957c7f923c23b68", out BlueprintActivatableAbility ShieldBashAbility, "ShieldBashAbility", circ)) goto skipShieldBashAbility;
            IEnumerable<RestrictionOtherActivatables> ROA = ShieldBashAbility.GetComponents<RestrictionOtherActivatables>();
            if (ROA.Count() == 0)
            {
                var newRestriction = new RestrictionOtherActivatables()
                {
                    m_ActivatableAbilities = new BlueprintActivatableAbilityReference[] { BucklerParryActivatableAbility.ToReference<BlueprintActivatableAbilityReference>() }
                };
                ShieldBashAbility.AddComponent(newRestriction);
            }
            else 
            {
                if (ROA.Any(restriction => restriction.m_ActivatableAbilities.Contains(BucklerParryActivatableAbility))) { Comment.Log("Skipping ShieldBash"); goto skipShieldBashAbility; }
                    else { var firstRestriction = ROA.First(); firstRestriction.m_ActivatableAbilities = firstRestriction.m_ActivatableAbilities.AddToArray(BucklerParryActivatableAbility.ToReference<BlueprintActivatableAbilityReference>()); }
            }
#if DEBUG
            Comment.Log("Added ShieldedDefenseActivatableAbility to the ShieldBashAbility restrictions."); 
#endif
            ; skipShieldBashAbility:
            #endregion
            RetrieveBlueprint("cb8686e7357a68c42bdd9d4e65334633", out BlueprintFeature ShieldsProficiency, "ShieldProficiency", circ);
            if (!AddBucklerParry.GetValue()) goto skipParry;
            #region modify Buckler Proficiency blueprint
            if (!RetrieveBlueprint("7c28228ce4eed1543a6b670fd2a88e72", out BlueprintFeature BucklerProf, "Buckler Proficiency", circ)) goto SkipBucklerProf;
            AddFacts af  = BucklerProf.GetComponent<AddFacts>();
            if (af is null)
            {
                af = new();
                BucklerProf.AddComponent(af);
            }
            if (!af.m_Facts.Contains(BucklerParryFeatureReference)) af.m_Facts = af.m_Facts.AddToArray(BucklerParryFeatureReference);
            Comment.Log("Added BucklerParryActivatableFeature to the BucklerProficiency blueprint.");
            BucklerProf.AddComponent(new AddProficiencies() { ArmorProficiencies = new ArmorProficiencyGroup[] { }, WeaponProficiencies = new WeaponCategory[] { WeaponCategory.WeaponLightShield } });
            BucklerProf.m_Description = new() { m_Key = "BucklerProficiencyWithParry_Description" };
            if (!RetrieveBlueprint("121811173a614534e8720d7550aae253", out BlueprintFeature ShieldBashFeature, "ShieldBashFeature", circ)) goto SkipBucklerProf;
            ShieldBashFeature.Components = ShieldBashFeature.Components.Where(component => component is not PrerequisiteNotProficient noProf || !noProf.WeaponProficiencies.Contains(WeaponCategory.WeaponLightShield)).ToArray();
            Comment.Log("Emptied weapon proficiencies on the old Shield Bash blueprint");
            ; SkipBucklerProf:;
            #endregion
            #region modify Light Shield Proficiency blueprint
            if (!ShieldsProficiency) goto skipParry;
            //ShieldsProficiency.AddComponent(new AddProficiencies() { ArmorProficiencies = new ArmorProficiencyGroup[] { }, WeaponProficiencies = new WeaponCategory[] { WeaponCategory.WeaponLightShield, WeaponCategory.WeaponHeavyShield } });
             af = ShieldsProficiency.GetComponent<AddFacts>();
            if (af is null)
            {
                af = new() { m_Facts = new BlueprintUnitFactReference[] { } };
                ShieldsProficiency.AddComponent(af);
            }
            if (!af.m_Facts.Contains(BucklerParryFeatureReference)) af.m_Facts = af.m_Facts.AddToArray(BucklerParryFeatureReference);
            Comment.Log("Added BucklerParryActivatableFeature to the ShieldProficiency blueprint.");
            ;skipParry:;
            #endregion
            #region meddle with proficiencies
                #region Create new Buckler proficiency feature
            BlueprintFeature NewBuckler = new()
            {
                m_DisplayName = new() { m_Key = "NewBucklerProficiencyFeature_DisplayName" },
                m_Description = new() { m_Key = "NewBucklerProficiencyFeature_Description", ShouldProcess = true },
                HideInCharacterSheetAndLevelUp = true,
            };
            NewBuckler.AddComponent(new PrerequisiteClassLevel()
            {
                HideInUI = true,
                m_CharacterClass = new() { deserializedGuid = BlueprintGuid.Parse("26b10d4340839004f960f9816f6109fe") },
                Level = 1,
                Not = true,
                Group = Prerequisite.GroupType.All
            });
            NewBuckler.AddComponent(new PrerequisiteNotProficient()
            {
                ArmorProficiencies = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.Buckler },
                WeaponProficiencies = new WeaponCategory[] { },
                CheckInProgression = true,
                Group = Prerequisite.GroupType.All
            });
            NewBuckler.AddComponent(new PrerequisiteFeaturesFromList()
            {
                m_Features = new BlueprintFeatureReference[]
                {
                    new() { deserializedGuid = BlueprintGuid.Parse("ac8aaf29054f5b74eb18f2af950e752d") }, //TwoWeaponFighting
                    new() { deserializedGuid = BlueprintGuid.Parse("cb8686e7357a68c42bdd9d4e65334633")}   //ShieldsProficiency
                },
                Group = Prerequisite.GroupType.All,
                CheckInProgression = true,
            });
            NewBuckler.AddComponent(new AddFacts()
            {
                m_Facts = new BlueprintUnitFactReference[] {new() { deserializedGuid = BlueprintGuid.Parse("7c28228ce4eed1543a6b670fd2a88e72") } }
            });
            NewBuckler.AddToCache("8f75e3d5d1024385a3c5e7ca450591ca", "NewBucklerProficiencyFeature");
            NewBuckler.m_Icon = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("7c28228ce4eed1543a6b670fd2a88e72")?.m_Icon;
            #endregion
                #region do the ShieldsProficiency blueprint
            if (!RemoveBucklerProficiencies.GetValue() || !ShieldsProficiency) 
                goto skipProficiencies;
            if (ShieldsProficiency.Components.FirstOrDefault(c => c is AddProficiencies) is not AddProficiencies ap)
            {
                Comment.Warning("Failed to find the AddProficiencies component on the ShieldsProficiency blueprint when meddling with buckler proficiencies. Proceed to create new");
                ap = new()
                {
                    ArmorProficiencies = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.LightShield, ArmorProficiencyGroup.HeavyShield },
                    WeaponProficiencies = new WeaponCategory[] { },
                };
                ShieldsProficiency.AddComponent(ap);
            }
            else ap.ArmorProficiencies = ap.ArmorProficiencies.Where(p => p != ArmorProficiencyGroup.Buckler).ToArray();
            ShieldsProficiency.m_Description = new() { m_Key = "ShieldsProficiency_DescriptionWithoutBuckler" };
            #endregion
                #region return bucklers to classes
            string circ2 = "when returning bucklers to class proficiencies";
            (string, string)[] ClassProficiencies = new (string, string)[]
            {
                ("3d1cf37e3ce44e27ab3241fd750f6972", "InstinctualWarriorProficiencies"),
                ("fa3d3b2211a51994785d85e753f612d3", "BardProficiencies"),
                ("7aa59f5998e5baf4182cac3ff7998974", "BloodragerProficiencies"),
                ("a23591cc77086494ba20880f87e73970", "FighterProficiencies"),
                ("873f1f12bb43cda40bcc7d2878d16bf1", "HunterProficiencies"),
                ("e59db96fa83cefd4a9a8f211500d9522", "InquisitorProficiencies"),
                ("c5e479367d07d62428f2fe92f39c0341", "RangerProficiencies"),
                ("9fceea5f433969e44bb124ab3a95bb58", "DivineHunterProficiencies"),
                ("41cd5ff7ad1bc5848906e050b06d02dc", "SlayerProficiencies"),
            };
            (string, string)[] ShieldSelections = new (string, string)[]
            {
                ("79c6421dbdb028c4fa0c31b8eea95f16", "WarDomainGreaterFeatSelection"),
                ("da03141df23f3fe45b0c7c323a8e5a0e", "EldritchKnightFeatSelection"),
                ("90f105c8e31a6224ea319e6a810e4af8", "LoremasterCombatFeatSelection"),
                ("66befe7b24c42dd458952e3c47c93563", "MagusFeatSelection"),
                ("c5158a6622d0b694a99efb1d0025d2c1", "CombatTrick"),
                ("78fffe8e5d5bc574a9fd5efbbb364a03", "StudentOfWarCombatFeatSelection"),
                ("247a4068296e8be42890143f451b4b45", "BasicFeatSelection"),
                ("e10c4f18a6c8b4342afe6954bde0587b", "ExtraFeatMythicFeat"),
                ("a21acdafc0169f5488a9bd3256e2e65b", "DragonLevel2FeatSelection"),
            };

            (string, string)[] ShieldLessClasses = new (string, string)[]
            {
                ("acc15a2d19f13864e8cce3ba133a1979", "BarbarianProficiencies"),
                ("0a0f032ccfe411d4d86b298da4657e58", "CavalierProficiencies"),
                ("8c971173613282844888dc20d572cfc9", "ClericProficiencies"),
                ("0ef3fcce071b4b24a8c52dd0f55fd216", "DruidProficiencies"),
                ("9b45ccedba40f364fbc427b4e9c7b560", "OracleProficiency"),
                ("b10ff88c03308b649b50c31611c2fefb", "PaladinProficiencies"),
                ("64e3e3fb96c439b4f8d896339b2f358a", "SkaldProficiencies"),
                ("ad29d445f1534474db8295a61e42d08b", "WarpriestProficiencies"),
                ("05dfc7b4dd2840f99609c3b570343898", "StalwartDefenderProficiencies"),
                ("801edc0eaf2d36d4983f1fe5c0f79818", "BeastRiderProficiencies"),
                ("955a3cc721c067b4da06508526aabc55", "AngelfireApostleProficiencies"),
            };

            BlueprintUnitFactReference bucklerref = new() { deserializedGuid = BlueprintGuid.Parse("7c28228ce4eed1543a6b670fd2a88e72") };
            foreach (var (ID, name) in ClassProficiencies)
            {
                if (!RetrieveBlueprint(ID, out BlueprintFeature f, "name", circ2)) continue;
                af = f.Components.OfType<AddFacts>().FirstOrDefault();
                if (af is null)
                {
                    Comment.Warning("Did not find AddFacts component on the {0} blueprint {1}", name, circ2);
                    f.AddComponent(af = new());
                }
                if (!(af.m_Facts?.Contains(bucklerref) ?? false))
                    af.m_Facts = (af.m_Facts ?? new BlueprintUnitFactReference[] { }).AddToArray(bucklerref);
                Comment.Log($"Returned BucklerProficiency to proficiencies of the class {name} (guid {ID})");
            };
            foreach (var (ID, name) in ShieldLessClasses)
            {
                if (!RetrieveBlueprint(ID, out BlueprintFeature f, "name", circ2)) continue;
                f.m_Description = new() { m_Key = "WayOfTheShield_" + name + "_Shieldless", m_ShouldProcess = true };
                Comment.Log($"Changed the description of the class {name} (guid {ID}), {circ2}");
            };
            #endregion
            NewBuckler.AddFeatureToSelections(ShieldSelections, circ2);
            ; skipProficiencies:;
            #endregion
            m_BlueprintBucklerParryActivatableAbility = BucklerParryActivatableAbility;
            Created = true;
        }

        public class ShieldEquippedRestriction : ActivatableAbilityRestriction
        {
            public ArmorProficiencyGroup[] categories = new ArmorProficiencyGroup[] { };
            public override bool IsAvailable()
            {
               
                UnitBody body = Owner?.Body;
                if (body is null) return false;
                ItemEntityWeapon mainhand = body.PrimaryHand.MaybeWeapon;
                if (mainhand is not null && mainhand.HoldInTwoHands) return false;
                ItemEntityShield shield = body.SecondaryHand?.MaybeShield;
                if (shield is null || shield.WeaponComponent is null || shield.ArmorComponent is null) return false;
                foreach (var category in categories)
                {
                    if (shield.ArmorComponent.Blueprint.ProficiencyGroup == category
                        && Owner.Proficiencies.Contains(category))
                    {
                        return true;
                    }
                };
                return false;
            }
        }

        //[HarmonyPatch]
        //public static class BucklerModelReAttachment
        //{
        //    [HarmonyPatch(typeof(UnitViewHandSlotData), nameof(UnitViewHandSlotData.OffHandTransform), MethodType.Getter)]
        //    public static bool Prefix(UnitViewHandSlotData __instance, ref Transform __result)
        //    {
        //        Comment.Log($"BucklerModelReAttachment - item {__instance?.VisibleItem?.Blueprint.name} (object name '{__instance?.VisualModel?.name}') on unit { __instance?.Owner?.CharacterName}. " +
        //            $"Buckler? {(__instance?.VisibleItem?.Blueprint as BlueprintItemShield)?.Type.ProficiencyGroup == ArmorProficiencyGroup.Buckler}. " +
        //            $"Owner has forearm? {__instance.Character.transform.FindChildRecursive("L_ForeArm") is not null}.");
        //        if ((__instance?.VisibleItem?.Blueprint as BlueprintItemShield)?.Type.ProficiencyGroup == ArmorProficiencyGroup.Buckler)
        //        {
        //            __result = __instance.Character.transform.FindChildRecursive("L_ForeArm");
        //        }
        //        return __result is null;
        //    }

        //}
    }
}
