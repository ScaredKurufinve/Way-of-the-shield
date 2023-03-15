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

namespace Way_of_the_shield.NewFeatsAndAbilities
{
    [HarmonyPatch]
    public static class UnhinderingShield
    {
        public static HashSet<(string GUID, string name)> selections = new()
        {
                new ("247a4068296e8be42890143f451b4b45", "BasicFeatSelection"),
                new ("41c8486641f7d6d4283ca9dae4147a9f", "FighterFeatSelection"),
                new ("c5357c05cf4f8414ebd0a33e534aec50", "CrusaderFeat1"),
                new ("50dc57d2662ccbd479b6bc8ab44edc44", "CrusaderFeat10"),
                new ("2049abc955bf6fe41a76f2cb6ba8214a", "CrusaderFeat20"),
                new ("303fd456ddb14437946e344bad9a893b", "WarpriestFeatSelection"),
                new ("c5158a6622d0b694a99efb1d0025d2c1", "CombatTrick"),
        };
        public static bool ShieldFlag = false;
        public static bool ShieldFlag2 = false;

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
            UnhinderingShield.AddComponent(new FeatureTagsComponent() { FeatureTags = FeatureTag.Defense | FeatureTag.Attack | FeatureTag.Magic | FeatureTag.ClassSpecific });
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
            UnhinderingShield.AddToCache();
            #endregion

            if (RetrieveBlueprint("ef38e0fe68f14c88a9deacc421455d14", out BlueprintFeatureSelection ShieldMastery, "ShieldMasterySelection", "to add Shield Brace"))
                selections.Add((ShieldMastery.AssetGuid.ToString(), "TTT-ShieldMasterySelection"));
            #region Add the feature to selections
            BlueprintFeatureReference Reference = UnhinderingShield.ToReference<BlueprintFeatureReference>();
            if (Reference is null)
            {
                Comment.Warning("WARNING. Failed to create reference out of Unhindering Shield when adding to feat selection lists");
                return;
            }
            BlueprintFeatureSelection fs;
            string circ = "when adding Unhindering Shield";
            foreach ((string GUID, string name) in selections)
            {
                Comment.Log("Trying to retrieve " + name + " to add Shield Brace");
                if (!RetrieveBlueprint(GUID, out fs, name, circ)) continue;
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
                new CodeInstruction(OpCodes.Callvirt, typeof(UnitPartDamageGrace).GetMethod(nameof(UnitPartDamageGrace.HasEntry))),
                new CodeInstruction(OpCodes.Br_S),
            };

            CodeInstruction[] toSearch2 = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldfld, typeof(AttackStatReplacement).GetField(nameof(AttackStatReplacement.ReplacementStat))),
                new CodeInstruction(OpCodes.Callvirt, typeof(RuleCalculateAttackBonusWithoutTarget).GetProperty(nameof(RuleCalculateAttackBonusWithoutTarget.AttackBonusStat)).SetMethod),
            };

            int index = IndexFinder(instructions, toSearch);
            if (index == -1)
            {
                Comment.Warning("WARNING. Failed to find the Ensure<UnitPartDamageGrace>().HasEntry(evt.Weapon.Blueprint.Category)) in the AttackStatReplacement when transpiling for Unhindering Shield");
                return instructions;
            };

            Label label = gen.DefineLabel();
            _instructions[index - 1].labels.Add(label);

            CodeInstruction[] toInsert = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Brfalse_S, label),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Stsfld, typeof(UnhinderingShield).GetField(nameof(ShieldFlag)))
            };
            _instructions.InsertRange(index, toInsert);


            int index2 = IndexFinder(_instructions, toSearch);
            if (index2 == -1)
            {
                return instructions;
            };
            CodeInstruction[] toInsert2 = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Stsfld, typeof(UnhinderingShield).GetField(nameof(ShieldFlag)))
            };
            _instructions.InsertRange(index2, toInsert2);

            return _instructions;
        }

        [HarmonyPatch(typeof(AttackStatReplacement), nameof(AttackStatReplacement.OnEventAboutToTrigger))]
        [HarmonyPostfix]
        public static void AttackStatReplacement_OnEventAboutToTrigger_Postfix(AttackStatReplacement __instance, RuleCalculateAttackBonusWithoutTarget evt)
        {
#if DEBUG
            if (Debug.GetValue())
                Comment.Log($"Entered AttackStatReplacement_OnEventAboutToTrigger_Postfix for {evt.Initiator.CharacterName}. Flags are {ShieldFlag} and {ShieldFlag2}."); 
#endif
            if (ShieldFlag && ShieldFlag2)
            {

                ItemEntityShield maybeShield = __instance.Owner?.Body.SecondaryHand.MaybeShield;
                if (maybeShield is not null && (maybeShield.Blueprint.Type.ProficiencyGroup != ArmorProficiencyGroup.Buckler || !__instance.Owner.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.UnhinderingShield))
                {
                    evt.AddModifier(maybeShield.ArmorComponent.Blueprint.ArmorChecksPenalty, __instance.Fact, ModifierDescriptor.Penalty);
                }
            }
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
        public static IEnumerable<CodeInstruction> MonkNoArmorAndMonkWeaponFeatureUnlock_CheckEligibility_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
#if DEBUG
            Comment.Log("Entered MonkNoArmorAndMonkWeaponFeatureUnlock transpiler for Unhindering Shield."); 
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
            if (shield is null) return false;
            if (shield.Blueprint.Type.ProficiencyGroup != ArmorProficiencyGroup.Buckler || !shield.Owner?.Unit.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.UnhinderingShield) return true;
            else return false;
        }
        public static bool MagusBuckler(ItemEntity item)
        {
            if (item is null) return false;
#pragma warning disable IDE0019 
            ItemEntityShield shield = item as ItemEntityShield;
#pragma warning restore IDE0019 
            if (shield is null) return true;
            if (shield.Blueprint.Type.ProficiencyGroup != ArmorProficiencyGroup.Buckler || !shield.Owner?.Unit.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.UnhinderingShield) return true;
            else return false;
        }

    }
}
