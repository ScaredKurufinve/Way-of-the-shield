using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using HarmonyLib;
using System.Text;
using static Kingmaker.Kingdom.KingdomStats;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utils;
using Kingmaker.EntitySystem;

namespace Way_of_the_shield.NewFeatsAndAbilities
{
    [HarmonyPatch]
    public static class UnhinderingShield
    {
        public static List<(string GUID, string name)> selections = new()
        {
                new ("247a4068296e8be42890143f451b4b45", "BasicFeatSelection"),
                new ("41c8486641f7d6d4283ca9dae4147a9f", "FighterFeatSelection"),
                new ("c5357c05cf4f8414ebd0a33e534aec50", "CrusaderFeat1"),
                new ("50dc57d2662ccbd479b6bc8ab44edc44", "CrusaderFeat10"),
                new ("2049abc955bf6fe41a76f2cb6ba8214a", "CrusaderFeat20"),
                new ("303fd456ddb14437946e344bad9a893b", "WarpriestFeatSelection"),
                new ("c5158a6622d0b694a99efb1d0025d2c1", "CombatTrick"),
        };
        static RulebookEvent.CustomDataKey AttackStatReplacementSources = new("WaytOfTheShield_AttackStatReplacementSources");

        [HarmonyAfter("TabletopTweaks-Base")]
        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPostfix]
        public static void BlueprintsCache_Patch()
        {
#if DEBUG
            Comment.Log("Begin creting the Unhindering Shield blueprint."); 
#endif
            #region Create the feature
            BlueprintFeature UnhinderingShield = new()
            {
                AssetGuid = new BlueprintGuid(new Guid("39470145204746cdb46cd95bcee4a92c")),
                name = "Way_of_the_shield-Unhindering_shield-Feature",
                m_DisplayName = new LocalizedString() { m_Key = "UnhinderingShieldFeature_DisplayName" },
                m_Description = new LocalizedString() { m_Key = "UnhinderingShieldFeature_Description" },
                m_DescriptionShort = new LocalizedString() { m_Key = "UnhinderingShieldFeature_ShortDescription" },
                m_Icon = LoadIcon("UnhinderingShield", 200, 64),
                Groups = new[] {FeatureGroup.Feat, FeatureGroup.CombatFeat }
            };
            UnhinderingShield.AddToCache();
            UnhinderingShield.AddComponent(new AddMechanicsFeature() { m_Feature = MechanicsFeatureExtension.UnhinderingShield });
            UnhinderingShield.AddComponent(new PrerequisiteProficiency()
            {
                ArmorProficiencies = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.Buckler },
                WeaponProficiencies = Array.Empty<WeaponCategory>(),
                HideInUI = false,
                Group = Prerequisite.GroupType.All
            });
            BlueprintFeatureReference ShieldFocusReference = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("ac57069b6bf8c904086171683992a92a")?.ToReference<BlueprintFeatureReference>();
            if (ShieldFocusReference.Get() is null) { Comment.Warning("WARNING. Failed to find the Shield Focus feature blueprint when creating prerequisites for Unhindering Shield"); }
            BlueprintFeatureReference ArmorTrainingReference = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("3c380607706f209499d951b29d3c44f3")?.ToReference<BlueprintFeatureReference>();
            if (ShieldFocusReference.Get() is null) { Comment.Warning("WARNING. Failed to find the Armor Training feature blueprint when creating prerequisites for Unhindering Shield"); }
            UnhinderingShield.AddComponent(new FeatureTagsComponent() { FeatureTags = 
                FeatureTag.Defense | 
                FeatureTag.Attack | 
                FeatureTag.Magic | 
                FeatureTag.ClassSpecific |
                FeatureTag.Melee});
            UnhinderingShield.AddComponent(new PrerequisiteFeaturesFromList()
            {
                m_Features = new BlueprintFeatureReference[]
                {
                        ShieldFocusReference,
                        ArmorTrainingReference
                },
                HideInUI = false,
                Group = Prerequisite.GroupType.All
            });

            BlueprintCharacterClassReference FighterClassReference = new();
            if (RetrieveBlueprint("48ac8db94d5de7645906c7d0ad3bcfbd", out BlueprintCharacterClass FighterClass, "FighterClass", "when creating prerequisites for Unhindering Shield")) FighterClassReference = FighterClass.ToReference<BlueprintCharacterClassReference>();
            UnhinderingShield.AddComponent(new PrerequisiteClassLevel()
            {
                Level = 4,
                m_CharacterClass = FighterClassReference,
                HideInUI = false,
                Group = Prerequisite.GroupType.Any
            });
            UnhinderingShield.AddComponent(new PrerequisiteStatValue()
            {
                Stat = StatType.BaseAttackBonus,
                Value = 6,
                HideInUI = false,
                Group = Prerequisite.GroupType.Any
            });
            #endregion

            if (Main.TTTBase is not null && Main.CheckForModEnabled("TabletopTweaks-Base"))
            {
                if (RetrieveBlueprint("ef38e0fe68f14c88a9deacc421455d14", out BlueprintFeatureSelection ShieldMastery, "ShieldMasterySelection", "to add Shield Brace"))
                    selections.Add((ShieldMastery.AssetGuid.ToString(), "TTT-ShieldMasterySelection"));
            }
            else
                Comment.Log("TTT-Base is not found as enabled, will not add Shield Brace to Shield Mastery Selection");
                #region Add the feature to selections
                BlueprintFeatureReference Reference = UnhinderingShield.ToReference<BlueprintFeatureReference>();
            if (Reference is null)
            {
                Comment.Warning("WARNING. Failed to create reference out of Unhindering Shield when adding to feat selection lists");
                return;
            }
            string circ = "when adding Unhindering Shield";
            foreach ((string GUID, string name) in selections)
            {
                if (!RetrieveBlueprint(GUID, out BlueprintFeatureSelection fs, name, circ)) continue;
                fs.m_AllFeatures = fs.m_AllFeatures.AddToArray(Reference);
#if DEBUG
                Comment.Log("Successfully added Shield Brace to " + name); 
#endif
            };
            #endregion
        }

        [HarmonyPatch(typeof(AttackStatReplacement), nameof(AttackStatReplacement.OnEventAboutToTrigger))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AttackStatReplacement_OnEventAboutToTrigger_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
        {
#if DEBUG
            Comment.Log("Entered DamageGrace transpiler for Unhindering Shield."); 
#endif
            List<CodeInstruction> _instructions = instructions.ToList();


            CodeInstruction[] toSearch = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldfld, typeof(AttackStatReplacement).GetField(nameof(AttackStatReplacement.ReplacementStat))),
                new CodeInstruction(OpCodes.Callvirt, typeof(RuleCalculateAttackBonusWithoutTarget).GetProperty(nameof(RuleCalculateAttackBonusWithoutTarget.AttackBonusStat)).SetMethod),
            };

            int index = IndexFinder(_instructions, toSearch, true);
            if (index == -1)
            {
                return instructions;
            };
            CodeInstruction[] toInsert2 = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldarg_0),
                CodeInstruction.Call((RuleCalculateAttackBonusWithoutTarget evt, AttackStatReplacement __instance) => AddReplacementSource(evt,  __instance))
            };
            _instructions.InsertRange(index, toInsert2);

            return _instructions;
        }
        static void AddReplacementSource(RuleCalculateAttackBonusWithoutTarget evt, AttackStatReplacement __instance)
        {
            if (!evt.TryGetCustomData(AttackStatReplacementSources, out (StatType stat, List<AttackStatReplacement> list) map))
            {
                map = new() { stat = __instance.ReplacementStat, list = new() { __instance} };
                evt.SetCustomData(AttackStatReplacementSources, map);
#if DEBUG
                if (Debug.GetValue()) 
                {
                    Comment.Log($"UnhinderingShield AttackStatReplacement AddReplacementSource - new map. Stat is {__instance.ReplacementStat}, source is {__instance.OwnerBlueprint.name}");
                }
#endif
                return;
            }

            if (map.stat == __instance.ReplacementStat)
            {
                map.list.Add(__instance);
                Comment.Log($"UnhinderingShield AttackStatReplacement AddReplacementSource - added entry. Stat is {__instance.ReplacementStat}, source is {__instance.OwnerBlueprint.name}");
            }
            else
            {
                map.stat = __instance.ReplacementStat;
                map.list.Clear();
                map.list.Add(__instance); 
                Comment.Log($"UnhinderingShield AttackStatReplacement AddReplacementSource - new map. Stat is {__instance.ReplacementStat}, source is {__instance.OwnerBlueprint.name}");
            }
        }

        [HarmonyPatch(typeof(RuleCalculateAttackBonusWithoutTarget), nameof(RuleCalculateAttackBonusWithoutTarget.OnTrigger))]
        [HarmonyPrefix]
        public static void RuleCalculateAttackBonusWithoutTarget_OnTrigger_PrefixToAddPenaltyFromShieldAndWeaponFinesse(RuleCalculateAttackBonusWithoutTarget __instance)
        {
            const string WeaponFinesseGuid = "90e54424d682d104ab36436bd527af09";
#if DEBUG
            if (Debug.GetValue())
            {
                StringBuilder sb = new($"UnhinderingShield RuleCalculateAttackBonusWithoutTarget OnTrigger - Entered for {__instance.Initiator.CharacterName}.");
                if (__instance.TryGetCustomData(AttackStatReplacementSources, out (StatType stat, List<AttackStatReplacement> list) map2))
                {
                    sb.Append($"\nSources for replacement are {string.Join(", ", map2.list.Select(component => component.OwnerBlueprint.name + "#" + component.OwnerBlueprint.AssetGuidThreadSafe))}. ");
                    ItemEntityShield maybeShield2 = __instance.Initiator?.Body.SecondaryHand.MaybeShield;
                    if (maybeShield2 != null)
                    {
                        sb.Append($"\nShield is {maybeShield2.Blueprint.name}. Type is {maybeShield2.Blueprint.Type.ProficiencyGroup}. ");
                        var contains = map2.list.Any(component => component.OwnerBlueprint.AssetGuidThreadSafe is WeaponFinesseGuid);
                        sb.Append($"Sources contain Weapon Finesse? {contains}. ");
                        if (contains)
                        {
                            sb.Append($"Initiator has Unhindering Shield? {__instance.Initiator.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.UnhinderingShield.Value.ToString() ?? "False"}. ");
                        }
                    }
                    else
                    {
                        sb.Append($" No shield in the off-hand found.");
                    }
                }
                else
                {
                    sb.Append(" No stat replacement");
                }
                Comment.Log(sb.ToString());
            }
#endif
            if (!__instance.TryGetCustomData(AttackStatReplacementSources, out (StatType stat, List<AttackStatReplacement> list) map))
                return;
            var WeaponFinesseComponent = map.list.FirstOrDefault(component => component.OwnerBlueprint.AssetGuidThreadSafe is WeaponFinesseGuid);
            if (!WeaponFinesseComponent || map.list.Count > 1)
                return;
            ItemEntityShield maybeShield = __instance.Initiator?.Body.SecondaryHand.MaybeShield;

            if (maybeShield is not null && (maybeShield.Blueprint.Type.ProficiencyGroup != ArmorProficiencyGroup.Buckler || !(__instance.Initiator.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.UnhinderingShield ?? false)))
            {
#if DEBUG
                if (Debug.GetValue())
                    Comment.Log($"UnhinderingShield RuleCalculateAttackBonusWithoutTarget OnTrigger - about to add penalty");
#endif
                int penalty = Rulebook.Trigger(new RuleCalculateArmorCheckPenalty(__instance.Initiator, maybeShield.ArmorComponent)).Result;
                //__instance.Result += penalty;
                __instance.AddModifier(
                    penalty,
                    __instance.Initiator.Descriptor.Progression.Features.Enumerable.FirstOrDefault(fact => fact.Blueprint.AssetGuidThreadSafe is WeaponFinesseGuid),
                    ModifierDescriptor.Penalty);
#if DEBUG
                if (Debug.GetValue())
                    Comment.Log($"UnhinderingShield RuleCalculateAttackBonusWithoutTarget OnTrigger - added {penalty}.");
#endif
            }
        }

        public static void RuleCalculateAttackBonusWithoutTarget_OnTrigger_Postfix(RuleCalculateAttackBonusWithoutTarget __instance)
        {
        }

        [HarmonyPatch(typeof(MonkNoArmorFeatureUnlock), nameof(MonkNoArmorFeatureUnlock.CheckEligibility))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> MonkNoArmorFeatureUnlock_CheckEligibility_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
#if DEBUG
            Comment.Log("Entered MonkNoArmorFeatureUnlock transpiler for Unhindering Shield."); 
#endif
            List<CodeInstruction> _instructions = instructions.ToList();

            CodeInstruction[] toSearch = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Callvirt, typeof(UnitBody).GetProperty(nameof(UnitBody.SecondaryHand)).GetMethod),
                new CodeInstruction(OpCodes.Callvirt, typeof(HandSlot).GetProperty(nameof(HandSlot.HasShield)).GetMethod),
            };

            int index = IndexFinder(_instructions, toSearch);
            if (index == -1)
            {
                return instructions;
            };

            _instructions[index - 1].operand = typeof(HandSlot).GetProperty(nameof(HandSlot.MaybeShield)).GetMethod;

            CodeInstruction[] toInsert = new CodeInstruction[]
            {
                CodeInstruction.Call(typeof(UnhinderingShield), nameof(MonkBuckler))
            };

            _instructions.InsertRange(index, toInsert);
            return _instructions;

        }

        [HarmonyPatch(typeof(MonkNoArmorAndMonkWeaponFeatureUnlock), nameof(MonkNoArmorAndMonkWeaponFeatureUnlock.CheckEligibility))]
        [HarmonyTranspiler]
        [HarmonyAfter("DarkCodex")]
        public static IEnumerable<CodeInstruction> MonkNoArmorAndMonkWeaponFeatureUnlock_CheckEligibility_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
#if DEBUG
            Comment.Log("Entered MonkNoArmorAndMonkWeaponFeatureUnlock transpiler for Unhindering Shield."); 
#endif
            List<CodeInstruction> _instructions = instructions.ToList();

            CodeInstruction[] toSearch = new[]
            {
                new CodeInstruction(OpCodes.Callvirt, typeof(UnitBody).GetProperty(nameof(UnitBody.SecondaryHand)).GetMethod),
                new CodeInstruction(OpCodes.Callvirt, typeof(HandSlot).GetProperty(nameof(HandSlot.HasShield)).GetMethod),
            };

            CodeInstruction[] toInsert = new[]
            {
                CodeInstruction.Call(typeof(UnhinderingShield), nameof(MonkBuckler))
            };

            int index = 0;
            int a = 0;
            for (int i = 0; i < 2; i++)
            {
                a = IndexFinder(_instructions.GetRange(index, _instructions.Count - index), toSearch);
                index += a;  

                _instructions[index - 1].operand = typeof(HandSlot).GetProperty(nameof(HandSlot.MaybeShield)).GetMethod;
                _instructions.InsertRange(index, toInsert);
            }
            return _instructions;

        }

        [HarmonyPatch(typeof(CannyDefensePermanent), nameof(CannyDefensePermanent.CheckArmor))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CannyDefensePermanent_CheckArmor_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
#if DEBUG
            Comment.Log("Entered CannyDefensePermanent CheckArmor transpiler for Unhindering Shield."); 
#endif
            List<CodeInstruction> _instructions = instructions.ToList();

            CodeInstruction[] toSearch = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Callvirt, typeof(UnitBody).GetProperty(nameof(UnitBody.SecondaryHand)).GetMethod),
                new CodeInstruction(OpCodes.Callvirt, typeof(HandSlot).GetProperty(nameof(HandSlot.HasShield)).GetMethod),
            };

            int index = IndexFinder(_instructions, toSearch);
            if (index == -1)
            {
                return instructions;
            };

            _instructions[index - 1].operand = typeof(HandSlot).GetProperty(nameof(HandSlot.MaybeShield)).GetMethod;

            CodeInstruction[] toInsert = new CodeInstruction[]
            {
                CodeInstruction.Call(typeof(UnhinderingShield), nameof(MonkBuckler))
            };

            _instructions.InsertRange(index, toInsert);
            return _instructions;

        }

        [HarmonyPatch(typeof(DuelistPreciseStrike), nameof(DuelistPreciseStrike.OnEventAboutToTrigger))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> DuelistPreciseStrike_OnEventAboutToTrigger_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
#if DEBUG
            Comment.Log("Entered DuelistPreciseStrike transpiler for Unhindering Shield."); 
#endif
            List<CodeInstruction> _instructions = instructions.ToList();

            CodeInstruction[] toSearch = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Callvirt, typeof(UnitBody).GetProperty(nameof(UnitBody.SecondaryHand)).GetMethod),
                new CodeInstruction(OpCodes.Callvirt, typeof(HandSlot).GetProperty(nameof(HandSlot.HasShield)).GetMethod),
            };

            int index = IndexFinder(_instructions, toSearch);
            if (index == -1)
            {
                return instructions;
            };

            _instructions[index - 1].operand = typeof(HandSlot).GetProperty(nameof(HandSlot.MaybeShield)).GetMethod;

            CodeInstruction[] toInsert = new CodeInstruction[]
            {
                CodeInstruction.Call(typeof(UnhinderingShield), nameof(MonkBuckler))
            };

            _instructions.InsertRange(index, toInsert);
            return _instructions;

        }

        [HarmonyPatch(typeof(ACBonusAgainstAttacks), nameof(ACBonusAgainstAttacks.Check))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ACBonusAgainstAttacks_Check_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
#if DEBUG
            Comment.Log("Entered ACBonusAgainstAttacks transpiler for Unhindering Shield."); 
#endif
            List<CodeInstruction> _instructions = instructions.ToList();

            CodeInstruction[] toSearch = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Callvirt, typeof(UnitBody).GetProperty(nameof(UnitBody.SecondaryHand)).GetMethod),
                new CodeInstruction(OpCodes.Callvirt, typeof(HandSlot).GetProperty(nameof(HandSlot.HasShield)).GetMethod),
            };

            int index = IndexFinder(_instructions, toSearch);
            if (index == -1)
            {
                return instructions;
            };

            _instructions[index - 1].operand = typeof(HandSlot).GetProperty(nameof(HandSlot.MaybeShield)).GetMethod;

            CodeInstruction[] toInsert = new CodeInstruction[]
            {
                CodeInstruction.Call(typeof(UnhinderingShield), nameof(MonkBuckler))
            };

            _instructions.InsertRange(index, toInsert);
            return _instructions;
        }

        [HarmonyPatch(typeof(UnitPartMagus), nameof(UnitPartMagus.HasOneHandedMeleeWeaponAndFreehand))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UnitPartMagus_HasOneHandedMeleeWeaponAndFreehand_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
#if DEBUG
            Comment.Log("Entered UnitPartMagus transpiler for Unhindering Shield."); 
#endif
            List<CodeInstruction> _instructions = instructions.ToList();

            CodeInstruction[] toSearch = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Callvirt, typeof(UnitBody).GetProperty(nameof(UnitBody.SecondaryHand)).GetMethod),
                new CodeInstruction(OpCodes.Callvirt, typeof(ItemSlot).GetProperty(nameof(ItemSlot.HasItem)).GetMethod),
            };

            int index = IndexFinder(_instructions, toSearch);
            if (index == -1)
            {
                return instructions;
            };

            _instructions[index - 1].operand = typeof(HandSlot).GetProperty(nameof(HandSlot.MaybeItem)).GetMethod;

            CodeInstruction[] toInsert = new CodeInstruction[]
            {
                CodeInstruction.Call(typeof(UnhinderingShield), nameof(MagusBuckler))
            };

            _instructions.InsertRange(index, toInsert);
            return _instructions;
        }
        public static bool MonkBuckler(ItemEntityShield shield)
        {
#if DEBUG
            if (Debug.GetValue())
                Comment.Log(
                    $"MonkBuckler - shield is null? {shield is null}. " + (shield is null ? "" : (
                    $"Proficiency group is {shield.Blueprint.Type.ProficiencyGroup}, " +
                    $"Shield Bash? {shield.Owner.State.Features.ShieldBash}. " + (shield.Owner.State.Features.ShieldBash ? "" : (
                    $"Has Unhindering Shield? {shield.Owner?.Unit.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.UnhinderingShield}. ")))) +
                    $"Total result is {shield is not null
                        && (shield.Blueprint.Type.ProficiencyGroup != ArmorProficiencyGroup.Buckler
                            || (!shield.Owner?.Unit.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.UnhinderingShield
                            || shield.Owner.State.Features.ShieldBash))}"); 
#endif

            return shield is not null
                    && (shield.Blueprint.Type.ProficiencyGroup != ArmorProficiencyGroup.Buckler
                        || (!shield.Owner?.Unit.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.UnhinderingShield                        
                        ||  shield.Owner.State.Features.ShieldBash ));
        }

        public static bool MagusBuckler(ItemEntity item)
        {
            //Comment.Log($"MagusBuckler - result is {item is not null && (item is not ItemEntityShield s || MonkBuckler(s))}.");
            return item is not null && (item is not ItemEntityShield shield || MonkBuckler(shield));
        }

        [HarmonyPatch]
        static class SwashbucklerPreciseStrikePatch
        {
            static MethodInfo target;

            static bool Prepare()
            {
                var assembly = Main.CheckForMod("Swashbuckler");
                System.Type type = null;
                assembly?.DefinedTypes.TryFind(t => t.Name.Equals("SwashbucklerPreciseStrike"), out type);
                if (type is not null)
                    target = type.GetMethod("OnEventAboutToTrigger");
                Comment.Log(
                    $"Preparing the patch in case Swashbuckler mod is present. " +
                    $"Assembly is null? {assembly is null}. " +
                    $"Type is null? {type is null}. " +
                    $"Target is null? {target is null}.");

                return target is not null;
            }

            static MethodInfo TargetMethod()
                => target;

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var _inst = instructions.ToList();

                CodeInstruction[] toSearch = new[]
                {
                    new CodeInstruction(OpCodes.Callvirt, typeof(BlueprintItemArmor).GetProperty(nameof(BlueprintItemArmor.ProficiencyGroup)).GetMethod),
                    new CodeInstruction(OpCodes.Ldc_I4_4),
                    new CodeInstruction(OpCodes.Beq_S)
                };


                int index1 = _inst.FindIndex(i => i.Calls(typeof(HandSlot).GetProperty(nameof(HandSlot.MaybeShield)).GetMethod));
                if (index1 == -1) return instructions;

                int index2 = IndexFinder(_inst, toSearch);
                if (index2 == -1) return instructions;

                _inst[index2 - 1].opcode = OpCodes.Brfalse_S;
                _inst.RemoveRange(index1+1, index2 - (index1+2));
                _inst.Insert(index1+1, new CodeInstruction(OpCodes.Call, typeof(UnhinderingShield).GetMethod(nameof(MonkBuckler))));

                return _inst;
            }
        }

    }
}
