using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using System.IO;
using static Way_of_the_shield.Main;
using Kingmaker.Blueprints.Facts;
using Kingmaker.TextTools;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using Kingmaker.Cheats;

namespace Way_of_the_shield
{
    [HarmonyPatch]
    public static class Utilities
    {
        public static LogChannel Comment;

        [HarmonyPatch(typeof(BonusSourceStrings), nameof(BonusSourceStrings.GetText))]
        public static class BonusTypeExtenstions
        {
            static readonly (BonusType, LocalizedString, int)[] entries_start = new (BonusType, LocalizedString, int)[]
            {
               (BonusType.None, new() { Key = "BonusType_NonProficientWeapon_name" }, 160),
               (BonusType.None, new() { Key = "BonusType_Backstab_name" }, 161),
            };

            public static Dictionary<int, (BonusType type, LocalizedString name)> entries = new();



            static BonusTypeExtenstions()
            {
                foreach ((BonusType type, LocalizedString name, int number) in entries_start)
                {

                    entries.Add(
                        key: number,
                        value: ((BonusType)Enum.ToObject(typeof(BonusType), number), name));


#if DEBUG
                    if (Settings.Debug.GetValue())
                        Comment.Log("converted Localized String " + type + " into a BonustType = " + number); 
#endif

                }
            }


            public static BonusType GetBonusType(int n)
            {
                if (entries.TryGetValue(n, out (BonusType type, LocalizedString name) found)) return found.type;

                Comment.Error("Requsted a BonusType by number " + n + " and it wasn't found");
                return BonusType.None;
            }

            [HarmonyPrefix]
            public static bool Prefix(BonusType bonusType, ref string __result)
            {

                if ((int)bonusType < 160) return true;
                foreach ((BonusType type, LocalizedString name) in entries.Values)
                {
                    if (bonusType == type)
                    {
                        __result = name;
                        return false;
                    }
                }
                return true;
            }

            public static readonly Type unitFactType = typeof(MainUnitFact);

            [HarmonyPatch(typeof(StatModifiersBreakdown), nameof(StatModifiersBreakdown.GetBonusSourceText), new Type[] { typeof(IUIDataProvider), typeof(bool) })]
            [HarmonyPrefix]
            public static bool ResolveCharName_StatModifiersBreakdown_GetBonusSourceText_Prefix(IUIDataProvider source, ref string __result)
            {

                if (source is not null && source.GetType() == unitFactType)
                {
                    string name = (source as UnitFact)?.Owner?.CharacterName;
                    if (!String.IsNullOrEmpty(name))
                    {
                        __result = name;
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch]
        public static class ModifierDescriptorExtension
        {

            const int startIndex = 80;
            static ModifierDescriptorExtension()
            {
                original = EnumUtils.GetValues<ModifierDescriptor>().ToArray();
                dummies = new ModifierDescriptor[startIndex];
                for (int i = 0; i < original.Length - 1; i++) dummies[i] = original[i];
                for (int i = original.Length - 1; i < startIndex; i++) dummies[i] = (ModifierDescriptor)(startIndex -1);
                newDescriptors = new ModifierDescriptor[]
                {
                    SoftCover,
                    NotProficientWithArmor,
                    NotProficientWithShield
                };
                remade = new();
                remade = dummies.ToList();
                remade.AddRange(newDescriptors);
                amount = remade.Count - 1 - EnumUtils.GetMaxValue<ModifierDescriptor>();
#if DEBUG
                if (Settings.Debug.GetValue())
                    for (int i = 0; i < remade.Count() - 1; i++) Comment.Log(i + ": " + remade[i].ToString()); 
#endif
            }


            public const ModifierDescriptor SoftCover = (ModifierDescriptor)(startIndex + 0);
            public const ModifierDescriptor NotProficientWithArmor = (ModifierDescriptor)(startIndex + 1);
            public const ModifierDescriptor NotProficientWithShield = (ModifierDescriptor)(startIndex + 2);

            internal static ModifierDescriptor[] original;
            internal static ModifierDescriptor[] dummies;
            public static ModifierDescriptor[] newDescriptors;

            public static List<ModifierDescriptor> remade;

            public static int amount;
            public static AbilityModifierEntry[] names = new AbilityModifierEntry[]
            {
                new () {Key = SoftCover, Name = new LocalizedString() {Key = "ModifierDescriptor_SoftCover" }},
                new () {Key = NotProficientWithArmor, Name = new LocalizedString() {Key = "ModifierDescriptor_NonProficientArmor_name" }},
                new () {Key = NotProficientWithShield, Name = new LocalizedString() {Key = "ModifierDescriptor_NonProficientShield_name" }},
            };


            [HarmonyPatch(typeof(ModifierDescriptorComparer), MethodType.Constructor)]
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> InsertNewModifierDescriptor(IEnumerable<CodeInstruction> instructions)
            {
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log("Entered the ModifierDescriptorComparer constructor transpiler to insert new descriptors."); 
#endif
                List<CodeInstruction> _instructions = instructions.ToList();

                CodeInstruction[] toSearch1 = new CodeInstruction[]
                {
                    new CodeInstruction( OpCodes.Call, typeof(EnumUtils).GetMethod(nameof(EnumUtils.GetMaxValue)).MakeGenericMethod(typeof(ModifierDescriptor)) )
                };

                int index1 = IndexFinder(_instructions, toSearch1);
                if (index1 == -1)
                {
                    Comment.Error("Failed to find the GetMaxValue<ModifierDescriptor>()");
                    return instructions;
                }
                CodeInstruction[] toInsert1 = new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldsfld, typeof(ModifierDescriptorExtension).GetField(nameof(amount))),
                    new CodeInstruction(OpCodes.Add)
                };

                _instructions.InsertRange(index1, toInsert1);

                CodeInstruction[] toSearch2 = new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Stloc_0)
                };

                int index2 = IndexFinder(_instructions, toSearch2, true);
                if (index2 == -1)
                {
                    return instructions;
                }

                CodeInstruction[] toInsert2 = new CodeInstruction[]
                {
                    CodeInstruction.Call(typeof(ModifierDescriptorExtension), nameof(AddDescriptors1))
                };

                _instructions.InsertRange(index2, toInsert2);

                CodeInstruction[] toSearch3 = new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Stloc_3)
                };
                int index3 = IndexFinder(_instructions, toSearch3);
                if (index3 == -1)
                {
                    Comment.Error("Failed to find the EnumUtils.GetValues<ModifierDescriptor>()");
                    return instructions;
                }
                CodeInstruction[] toInsert3 = new CodeInstruction[]
                {
                    CodeInstruction.Call(typeof(ModifierDescriptorExtension), nameof(AddDescriptors2))
                };

                _instructions.InsertRange(index3 - 2, toInsert3);

                return _instructions;
            }

            [HarmonyPatch(typeof(ModifierDescriptorComparer), MethodType.Constructor)]
            [HarmonyPostfix]
            public static void ApplyTTTCoreToModifierDescriptorComparer()
            {
#if DEBUG
                Comment.Log("Entered the ApplyTTTCoreToModifierDescriptorComparer"); 
#endif
                if (TTTCore is not null) TTTCorePatchesForSortedDescriptors()?.Invoke(null, null);
            }

            [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
            [HarmonyPostfix]
            public static void NewDescriptorNames()
            {
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log("Entered the BlueprintCache Init patch NewDescriptorNames"); 
#endif
                LocalizedTexts.Instance.AbilityModifiers.Entries = LocalizedTexts.Instance.AbilityModifiers.Entries.AddRangeToArray(names);

#if DEBUG
                if (Settings.Debug.GetValue())
                    foreach (ModifierDescriptor descriptor in ModifierDescriptorComparer.SortedValues) Comment.Log((int)descriptor + ": " + LocalizedTexts.Instance.AbilityModifiers.GetName(descriptor)); 

#endif
            }

#pragma warning disable IDE0060 // Удалите неиспользуемый параметр
            static List<ModifierDescriptor> AddDescriptors1(List<ModifierDescriptor> original)
#pragma warning restore IDE0060 // Удалите неиспользуемый параметр
            {
#if DEBUG
                Comment.Log("I'm inside AddDescriptors1."); 
#endif
                return remade;
            }
#pragma warning disable IDE0060 // Удалите неиспользуемый параметр
            static IEnumerable<ModifierDescriptor> AddDescriptors2(IEnumerable<ModifierDescriptor> original)
#pragma warning restore IDE0060 // Удалите неиспользуемый параметр
            {
#if DEBUG
                Comment.Log("I'm inside AddDescriptors2"); 
#endif
                return remade;
            }

            static MethodInfo TTTCorePatchesForSortedDescriptors()
            {
                Type t = TTTCore?.GetTypes().Where(type => type.FullName.Contains("AdditionalModifierDescriptors")).FirstOrDefault();
                MethodInfo returnValue = t?.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)?.Where(method => method.Name == "Update_ModifierDescriptorComparer_SortedValues")?.FirstOrDefault();
#if DEBUG
                if (returnValue is null) Comment.Log("Did not find the Update_ModifierDescriptorComparer_SortedValues method");
                else Comment.Log("found the Update_ModifierDescriptorComparer_SortedValues method"); 
#endif
                return returnValue;
            }
        }
        public static class EnchantPoolExtensions
        {
            public const EnchantPoolType DivineShield = (EnchantPoolType)20;
        }

        public class BoolSettingCheckerTemplate : TextTemplate
        {
            public override int MinParamenters
            {
                get
                {
                    return 2;
                }
            }
            public override int MaxParamenters
            {
                get
                {
                    return 3;
                }
            }

            public override string Generate(bool capitalized, List<string> parameters)
            {
                
                if (parameters.Count < MinParamenters)
                {
                    return string.Empty;
                }
                if (ListOfBoolSettings.Any(setting => setting.Item1.Key.Equals(settingsModName + parameters[0].ToLower()) && setting.Item1.GetValue() is true))
                    return parameters[1];
                else
                    return parameters.Count < 3 ? "" : parameters[2];
                
            }
        }

        public static int IndexFinder(IEnumerable<CodeInstruction> original, CodeInstruction[] RangeToSearch, bool before = false)
        {
            if (RangeToSearch.Count() >= original.Count()) { Comment.Error("IndexFinder was given a range to search longer than the original"); return -1; };
#if DEBUG
            if (Settings.Debug.GetValue())
            {
                Comment.Log("");
                Comment.Log("Searching for instructions:");
                foreach (CodeInstruction i in RangeToSearch) Comment.Log(i.ToString());
                Comment.Log("");
            } 
#endif


            int length = RangeToSearch.Count();
            CodeInstruction[] orArray = original.ToArray();
            int index = -1;
            bool found;
            int limit = original.Count() - length + 1;
            for (int i = 0; i < limit; i++)
            {
                found = true;
                for (int b = 0; b < length; b++)
                {
                    if (!(orArray[i + b].Is(RangeToSearch[b])))
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    index = before ? i : i + length;
                    break;
                }
            }

            if (index == -1) Comment.Error("Failed to find an index when transpiling");
#if DEBUG
            if (Settings.Debug.GetValue())
            {
                Comment.Log("Index = " + index);
            } 
#endif

            return index;

        }

        public static bool Is(this CodeInstruction original, CodeInstruction toCompare)
        {
            OpCode? cmpCode = toCompare.opcode;
            object cmpOperand = toCompare.operand;
            if (cmpCode is null && cmpOperand is null) return true;
            if (cmpOperand is null) return original.opcode == toCompare.opcode;
            if (cmpCode is null) return original.OperandIs(toCompare.opcode);
            return (original.opcode == toCompare.opcode) && (original.OperandIs(cmpOperand));
        }
        public static bool GetValue(this UISettingsEntityBool s) { return s.Setting.GetValue(); }

        public static void AddComponent(this BlueprintScriptableObject blueprint, BlueprintComponent component)
        {            
            List<string> names = blueprint.ComponentsArray.Select(x => x.name).ToList();
            string name_prototype = "";

            if (!String.IsNullOrEmpty(component.name))
            {
                if (!names.Contains(component.name)) goto Name;
                name_prototype = component.name;
                goto Iterate;
            };

            string bp_name = blueprint.name;
            string cmp_type = component.GetType().Name;
            string mod_name = modName;
            name_prototype += "_" + mod_name;
            if (!(String.IsNullOrEmpty(bp_name))) name_prototype += "_" + bp_name;
            if (!(String.IsNullOrEmpty(cmp_type))) name_prototype += "_" + cmp_type;

            Iterate: int i = 0;
            while (names.Contains(name_prototype + i))
            {
                i++;
            }
            component.name = name_prototype + "_" + i;
            Name: component.OwnerBlueprint = blueprint;
        //    if (blueprint.Components is null)
        //        blueprint.Components = new BlueprintComponent[1] { component };
        //    else
            blueprint.Components = blueprint.Components.Append(component).ToArray();


        }
        public static Sprite LoadIcon(string IconName, float PixelsPerUnit = 100, int size = 64)
        {
            if (String.IsNullOrEmpty(IconName)) return null;
            byte[] bytes = new byte[] { };
            string path = Path.Combine(modPath, "Icons", IconName + ".png");
            try
            {
                bytes = File.ReadAllBytes(path);
            }
            catch (FileNotFoundException)
            {
                Comment.Error($"Failed to load an Icon from {path}");
                return null;
            }
            Sprite sprite;
            int TrueSize =(int) (size * PixelsPerUnit / 100);
            try
            {
                Texture2D texture = new(TrueSize, TrueSize, TextureFormat.RGBA32, false);
                texture.LoadImage(bytes);
                sprite = Sprite.Create(texture, new Rect(0, 0, TrueSize, TrueSize), new Vector2(0, 0), PixelsPerUnit);
            }
            catch (ArgumentException)
            {
                Comment.Error($"Failed to create a sprite for IconName {IconName}");
                sprite = null;
            }
            finally
            {
            }
                return sprite;
        }

        public static bool RetrieveBlueprint<T>(string GUID, out T blueprint, string name = null, string circumstances = null) where T : BlueprintScriptableObject
        {
            blueprint = null;
            BlueprintScriptableObject bp_base;
            if (!Guid.TryParse(GUID, out Guid guid))
            {
                Comment.Error($"ERROR! Failed to parse string {GUID} into a GUID while retrieving a blueprint requested with the name {name}{(circumstances.IsNullOrEmpty() ? "" : " " + circumstances)}.");
                return false;
            }
            try
            {
                bp_base = ResourcesLibrary.TryGetBlueprint<BlueprintScriptableObject>(new BlueprintGuid(guid));
            }
            catch
            {
                Comment.Error($"ERROR! Failed to retrieve blueprint {name} by GUID {GUID}{(circumstances.IsNullOrEmpty() ? "" : " " + circumstances)}");
                return false;
            }

            if (bp_base is null)
            {
                Comment.Error($"ERROR! Failed to retrieve blueprint {name} by GUID {GUID}{(circumstances.IsNullOrEmpty() ? "" : " " + circumstances)}");
                return false;
            }
            blueprint = bp_base as T;
            if (blueprint is not null) return true;
            else
            {
                Comment.Error($"ERROR! Failed to convert blueprint {bp_base.name} of type {bp_base.GetType()} by GUID {GUID} requested with the name {name} into the type {typeof(T)}{(circumstances.IsNullOrEmpty() ? "" : " " + circumstances)}.");
                return false;
            }
        }
        public static void AddFeatureToSelections(this BlueprintFeature feature, IEnumerable<(string guid, string BlueprintName)> selections, string circumstances = "")
        {
            string featureName = (!String.IsNullOrEmpty(feature.m_DisplayName) ? feature.m_DisplayName : feature.name);
            BlueprintFeatureReference featureToList = feature.ToReference<BlueprintFeatureReference>();
            if (featureToList is null)
            {
                Comment.Warning("WARNING. Failed to create reference out of {0} feature when adding to feat selection lists", new object[] { featureName });
                return;
            }
            foreach ((string GUID, string name) in selections)
            {
                if (!RetrieveBlueprint(GUID, out BlueprintFeatureSelection fs, name, "when adding the " + featureName + " feature to selections")) continue;
                fs.m_AllFeatures ??= new BlueprintFeatureReference[0];
                fs.m_AllFeatures = fs.m_AllFeatures.AddToArray(featureToList);
#if DEBUG
                Comment.Log($"Successfully added {featureName} to {name}{", " + circumstances}"); 
#endif
            };
        }
        public static bool AddFeatureToSelections(string GUID, IEnumerable<(string guid, string BlueprintName)> selections, string circumstance = "")
        {
            if (!Guid.TryParse(GUID, out Guid id))
            {
                Comment.Error("Failed to parse Guid {0} while adding to feature selections {1}", GUID, circumstance);
                return false;
            }
            string circ = "when adding the blueprint to feature selections";
            BlueprintUnitFact fact = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>(new BlueprintGuid(id));
            if (fact is null)
            {
                Comment.Error("Failed to convert the blueprint {0} into a Feature while adding to feature selections {1}", GUID, circumstance);
                return false;
            }
            BlueprintUnitFactReference reference = fact.ToReference<BlueprintUnitFactReference>();
            AddFacts af;
            foreach (var (guid, BlueprintName) in selections)
            {
                if (!RetrieveBlueprint(guid, out BlueprintFeatureSelection bp, BlueprintName, circ)) continue;
                af = bp.Components.FirstOrDefault(c => c is AddFacts) as AddFacts;
                if (af is null)
                {
                    af = new() ;
                    bp.AddComponent(af);
                }
                af.m_Facts ??= new BlueprintUnitFactReference[] { };
                af.m_Facts.AddToArray(reference);
            }
            return true;
        }



        public static BlueprintList moddedBP = new() { Entries = new() };
        public static void AddToCache(this BlueprintScriptableObject bp, string GUID, string name)
        {
            if (!Guid.TryParse(GUID, out Guid guid))
            {
                Comment.Error("Failed to parse blueprint GUID: " + GUID + " when adding a blueprint with the name " + name);
            }
            bp.AssetGuid = new BlueprintGuid(guid);
            bp.name = modName + "_" + name;
            bp.AddToCache();
        }
        public static void AddToCache(this BlueprintScriptableObject bp) 
        {

            if (ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints.TryGetValue(bp.AssetGuid, out var oldValue))
            {
                var oldName = oldValue.Blueprint?.name;
                if (oldName is not null && oldName.Equals(bp.name))
                {
                    ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(bp.AssetGuid, bp);
                    //oldValue.Blueprint = bp;
                    //ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints[bp.AssetGuid] = oldValue;
                    Comment.Log($"WARNING. Substituting old blueprint {oldName} of guid {bp.AssetGuid} with a new one. If this happens not upon reload, this is an error");
                    return;
                }

                Comment.Error("While adding blueprint {0} with guid {1}, another blueprint with the same guid has been found!", bp, bp.AssetGuid);
                return;
            }
            try
            {
                ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(bp.AssetGuid, bp);
                if (!moddedBP.Entries.Any(entry => entry.Guid == bp.AssetGuid.ToString())) moddedBP.Entries.Add(new () {Name = bp.name, Guid = bp.AssetGuid.ToString(), m_Type = bp.GetType(), TypeFullName = bp.GetType().FullName });
                Comment.Log("Added blueprint {1} with the guid {0} to the cache.", bp.AssetGuid, bp);
            }
            catch 
            {
                StackTrace trace = new();
                StackFrame frame = trace.GetFrame(1);
                Comment.Error("Failed to add a blueprint to the cache! Calling method is {0}, {1}", frame.GetMethod().Name , frame.GetMethod().DeclaringType.Name); 
            }
            
        }

        public static void AddFeatureAsTeamwork(this BlueprintFeature feature,
                                                (string BuffGuid, string AreaEffectGuid, string AreaBuffGuid, string SwitchBuffGuid, string ToggleAbilityGuid) PackRagerGuids = default((string, string, string, string, string)),
                                                string CavalierGuid = null,
                                                (string BuffGuid, string AbilityGuid) VanguardGuids = default((string, string)),
                                                bool DoNotAdd = false)
        {
            string name = feature.name.Replace(modName + "_", "").Replace("Feature", "");
            Comment.Log($"Begin adding {name} as a teamwork feature. IS NOT ADDED TO SELECTIONS.");
            feature.AddFeatureToSelections(selections);
            BlueprintFeatureReference r = feature.ToReference<BlueprintFeatureReference>();
            BlueprintUnitFactReference r2 = feature.ToReference<BlueprintUnitFactReference>();
            AbilityApplyFact aaf;
            ShareFeaturesWithPet sfwp;
            AddFactsFromCaster affc;
            #region Teamwork feature to Pack Rager
            #region Create Pack Rager Buff
            if (String.IsNullOrEmpty(PackRagerGuids.BuffGuid)) goto skipPackRager;
            BlueprintBuff PackRagerBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid(PackRagerGuids.BuffGuid)),
                name = "PackRager_" + name + "Buff",
                IsClassFeature = true,
                Stacking = StackingType.Replace,
                m_Icon = ResourcesLibrary.TryGetBlueprint<BlueprintActivatableAbility>("4230d0ca826cb6b4fb6db6cdb318ec8e")?.Icon,
                FxOnRemove = new(),
                m_Description = feature.m_Description,
                m_DisplayName = feature.m_DisplayName,
                m_DescriptionShort = feature.m_DescriptionShort,
            };
            PackRagerBuff.AddComponent(new AddTemporaryFeat() { m_Feat = r });
            PackRagerBuff.AddToCache();
            Comment.Log("Added {0}  to the BlueprintsCache", PackRagerBuff.name);
            #endregion
            #region Create Pack Rager area effect
            if (String.IsNullOrEmpty(PackRagerGuids.AreaEffectGuid)) goto skipPackRager;
            BlueprintAbilityAreaEffect PackRagerAreaEffect = new()
            {
                AssetGuid = new BlueprintGuid(new Guid(PackRagerGuids.AreaEffectGuid)),
                name = "PackRager_" + name + "Area",
                m_TargetType = BlueprintAbilityAreaEffect.TargetType.Ally,
                SpellResistance = false,
                AffectEnemies = false,
                AffectDead = false,
                AggroEnemies = false,
                Shape = AreaEffectShape.Cylinder,
                Size = new Feet(50),
                Fx = new(),
                CanBeUsedInTacticalCombat = false,
                m_SizeInCells = 0,
                Components = new BlueprintComponent[] { }
            };
            PackRagerAreaEffect.AddComponent(new AbilityAreaEffectRunAction()
            {
                UnitMove = new(),
                UnitExit = new()
                {
                    Actions = new GameAction[] { new ContextActionRemoveBuff() {
                                                                                        m_Buff = PackRagerBuff.ToReference<BlueprintBuffReference>()} }
                },
                UnitEnter = new()
                {
                    Actions = new GameAction[] { new ContextActionApplyBuff() {
                                                                                          m_Buff = PackRagerBuff.ToReference<BlueprintBuffReference>(),
                                                                                          Permanent = true,
                                                                                          DurationSeconds = new(),
                                                                                          IsFromSpell = false,
                                                                                          AsChild = true} }
                },
                Round = new()
            });
            PackRagerAreaEffect.AddToCache();
            Comment.Log("Added {0}  to the BlueprintsCache", PackRagerAreaEffect.name);
            #endregion
            #region Create Pack Rager area buff
            if (String.IsNullOrEmpty(PackRagerGuids.AreaBuffGuid)) goto skipPackRager;
            BlueprintBuff PackRagerAreaBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid(PackRagerGuids.AreaBuffGuid)),
                name = "PackRager_" + name + "AreaBuff",
                m_Icon = feature.m_Icon,
                IsClassFeature = true,
                m_Flags = BlueprintBuff.Flags.StayOnDeath,
                Stacking = StackingType.Replace,
                FxOnRemove = new(),
                m_Description = feature.m_Description,
                m_DisplayName = feature.m_DisplayName,
                m_DescriptionShort = feature.m_DescriptionShort,
            };
            PackRagerAreaBuff.AddComponent(new AddAreaEffect() { m_AreaEffect = PackRagerAreaEffect.ToReference<BlueprintAbilityAreaEffectReference>() });
            PackRagerAreaBuff.AddToCache();
            Comment.Log("Added {0}  to the BlueprintsCache", PackRagerAreaBuff.name);
            #endregion
            #region create Pack Rager switch buff
            if (String.IsNullOrEmpty(PackRagerGuids.SwitchBuffGuid)) goto skipPackRager;
            BlueprintBuff PackRagerSwitchBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid(PackRagerGuids.SwitchBuffGuid)),
                name = "PackRager_" + name + "Buff",
                m_Icon = feature.m_Icon,
                m_DisplayName = feature.m_DisplayName,
                m_Description = feature.m_Description,
                m_DescriptionShort = feature.m_DescriptionShort,
                IsClassFeature = true,
                m_Flags = BlueprintBuff.Flags.StayOnDeath,
                FxOnRemove = new(),
                Stacking = StackingType.Replace,
                ComponentsArray = new BlueprintComponent[] { },
            };
            PackRagerSwitchBuff.AddComponent(new BuffExtraEffects()
            {
                m_CheckedBuff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613").ToReference<BlueprintBuffReference>(),
                m_ExtraEffectBuff = PackRagerAreaBuff.ToReference<BlueprintBuffReference>()
            });
            PackRagerSwitchBuff.AddToCache();
            Comment.Log("Added {0} to the blueprint cache.", PackRagerSwitchBuff.name);
            #endregion
            #region Create Pack Rager toggle ability
            if (String.IsNullOrEmpty(PackRagerGuids.ToggleAbilityGuid)) goto skipPackRager;
            BlueprintActivatableAbility PackRagerToggleAbility = new()
            {
                AssetGuid = new BlueprintGuid(new Guid(PackRagerGuids.ToggleAbilityGuid)),
                name = "PackRager_" + name + "ToggleAbility",
                m_DisplayName = feature.m_DisplayName,
                m_Description = feature.m_Description,
                m_DescriptionShort = feature.m_DescriptionShort,
                m_Icon = PackRagerBuff.Icon,
                Group = ActivatableAbilityGroup.RagingTactician,
                WeightInGroup = 1,
                IsOnByDefault = true,
                DeactivateImmediately = true,
                ActivationType = AbilityActivationType.Immediately,
                m_Buff = PackRagerSwitchBuff.ToReference<BlueprintBuffReference>()
            };
            PackRagerToggleAbility.AddToCache();
            Comment.Log("Added {0}  to the BlueprintsCache", PackRagerToggleAbility.name);
            #endregion
            #region add Pack Rager toggle ability to PackRagerRagingTacticianBaseFeature
            if (PackRagerRagingTacticianBaseFeature is null || DoNotAdd) goto skipPackRager;
            PackRagerRagingTacticianBaseFeature.AddComponent(new AddFeatureIfHasFact()
            {
                m_CheckedFact = r2,
                m_Feature = PackRagerToggleAbility.ToReference<BlueprintUnitFactReference>()
            });
            Comment.Log("Added {0} to the Raging Tactician Base Feature components array.", PackRagerToggleAbility.name);
        #endregion
        #endregion
        skipPackRager:
            #region Teamwork feature to Cavalier
            #region Create Cavalier buff
            if (String.IsNullOrEmpty(CavalierGuid)) goto skipCavalier;
            BlueprintBuff CavalierBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid(CavalierGuid)),
                name = "CavalierTactician_" + name + "Buff",
                m_DisplayName = feature.m_DisplayName,
                m_Description = feature.m_Description,
                m_DescriptionShort = feature.m_DescriptionShort,
                m_Icon = feature.m_Icon,
                FxOnRemove = new(),
                FxOnStart = new(),
                IsClassFeature = true,
                Stacking = StackingType.Ignore,
                Components = new BlueprintComponent[] { },
            };
            CavalierBuff.AddComponent(new AddFeatureIfHasFact() { Not = true, m_CheckedFact = r2, m_Feature = r2 });
            CavalierBuff.AddToCache();
            feature.AddComponent(new AddFacts() { m_Facts = new[] {CavalierBuff.ToReference<BlueprintUnitFactReference>() } });
            #endregion
            #region Add Cavalier buff to Cavalier ability
            if (CavalierTacticianAbility is null) goto skipCavalier;
            aaf = CavalierTacticianAbility.Components.Select(c => c as AbilityApplyFact).FirstOrDefault();
            if (aaf is null) goto skipCavalier;
            aaf.m_Facts = aaf.m_Facts.AddToArray(CavalierBuff.ToReference<BlueprintUnitFactReference>());
            Comment.Log("Added {0} to the Cavalier Tactician Ability.", CavalierBuff.name);
            #endregion
            #region Add Cavalier buff to Cavalier Swift ability
            if (CavalierTacticianAbilitySwift is null) goto skipCavalier;
            aaf = CavalierTacticianAbilitySwift.Components.Select(c => c as AbilityApplyFact).FirstOrDefault();
            if (aaf is null) goto skipCavalier;
            aaf.m_Facts = aaf.m_Facts.AddToArray(CavalierBuff.ToReference<BlueprintUnitFactReference>());
            Comment.Log("Added {0} to the Cavalier Tactician Ability.", CavalierBuff.name);
            #endregion
            #region add to CavalierTacticianSupportFeature
            if (CavalierTacticianSupportFeature is null || DoNotAdd) goto skipCavalier;
            CavalierTacticianSupportFeature.Components.AddToArray(new AddFeatureIfHasFact() { m_CheckedFact = r2, m_Feature = CavalierBuff.ToReference<BlueprintUnitFactReference>() });
            Comment.Log("Added {0} to the Cavalier Tactician Support Feature.", CavalierBuff.name);
        #endregion
        #endregion
        skipCavalier:
            #region Teamwork feature to Hunter Tactics
            sfwp = HunterTactics?.ComponentsArray.FindOrDefault(c => c is ShareFeaturesWithPet) as ShareFeaturesWithPet;
            if (sfwp is not null || !DoNotAdd) sfwp.m_Features = sfwp.m_Features.AddToArray(r);
            Comment.Log("Added {0} to the Hunter Tactics.", name);
            #endregion
            #region Teamwork feature to Monster Tactics 
            affc = MonsterTacticsBuff?.ComponentsArray.FindOrDefault(c => c is AddFactsFromCaster) as AddFactsFromCaster;
            if (affc is not null || !DoNotAdd) affc.m_Facts = affc.m_Facts.AddToArray(r2);
            Comment.Log("Added {0} to the Monster Tactics.", name);
            #endregion
            #region Teamwork feature to Sacred Huntsmaster
            sfwp = SacredHuntsmasterTactics?.ComponentsArray.FindOrDefault(c => c is ShareFeaturesWithPet) as ShareFeaturesWithPet;
            if (sfwp is not null || !DoNotAdd) sfwp.m_Features = sfwp.m_Features.AddToArray(r);
            Comment.Log("Added {0} to theSacred Huntsmaster.", name);
            #endregion
            #region Teamwork feature to Tactical Leader
            affc = TacticalLeaderFeatShareBuff?.ComponentsArray.FindOrDefault(c => c is AddFactsFromCaster) as AddFactsFromCaster;
            if (affc is not null || !DoNotAdd) affc.m_Facts = affc.m_Facts.AddToArray(r2);
            Comment.Log("Added {0} to the Tactical Leader.", name);
            #endregion
            #region Teamwork feature to Battle Prowess
            affc = BattleProwessEffectBuff?.ComponentsArray.FindOrDefault(c => c is AddFactsFromCaster) as AddFactsFromCaster;
            if (affc is not null || !DoNotAdd) affc.m_Facts = affc.m_Facts.AddToArray(r2);
            Comment.Log("Added {0} to the Battle Prowess.", name);
            #endregion
            #region Teamwork feature to Vanguard Tactician
            #region Create Vanguard buff
            if (String.IsNullOrEmpty(VanguardGuids.BuffGuid)) goto skipVanguardTactician;
            BlueprintBuff VanguardBuff = new()
            {
                AssetGuid = new BlueprintGuid(new Guid(VanguardGuids.BuffGuid)),
                name = "VanguardTactician_" + name + "Buff",
                m_DisplayName = feature.m_DisplayName,
                m_Description = feature.m_Description,
                m_DescriptionShort = feature.m_DescriptionShort,
                m_Icon = feature.m_Icon,
                IsClassFeature = true,
                Stacking = StackingType.Replace,
                FxOnRemove = new(),
                Components = new BlueprintComponent[] { },
            };
            VanguardBuff.AddComponent(new AddFactsFromCaster() { m_Facts = new BlueprintUnitFactReference[] { r2 } });
            VanguardBuff.AddToCache();
            Comment.Log("Added {0} to the Blueprints cache.", VanguardBuff.name);
            #endregion
            #region Create Vanguard Tactician ability
            if (String.IsNullOrEmpty(VanguardGuids.AbilityGuid)) goto skipVanguardTactician;
            RetrieveBlueprint("61e818d575ef4ff49a4ecbe03106add9", out BlueprintAbilityResource vtr, "VanguardTacticianResource", "when adding " + feature.name + " to Vanguard Tactian abilities.");
            BlueprintAbility VanguardAbility = new()
            {
                AssetGuid = new BlueprintGuid(new Guid(VanguardGuids.AbilityGuid)),
                name = "VanguardTactician_" + name + "Ability",
                m_DisplayName = feature.m_DisplayName,
                m_Description = feature.m_DescriptionShort,
                m_DescriptionShort = feature.m_DescriptionShort,
                LocalizedDuration = new(),
                LocalizedSavingThrow = new(),
                m_Icon = feature.m_Icon,
                Type = AbilityType.Extraordinary,
                Range = AbilityRange.Personal,
                CanTargetSelf = true,
                EffectOnAlly = AbilityEffectOnUnit.None,
                EffectOnEnemy = AbilityEffectOnUnit.None,
                m_Parent = VanguardTacticianBaseAbility?.ToReference<BlueprintAbilityReference>(),
                Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Omni,
                ActionType = Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                Components = new BlueprintComponent[] { }
            };
            VanguardAbility.AddComponent(new AbilityResourceLogic()
            {
                m_RequiredResource = vtr?.ToReference<BlueprintAbilityResourceReference>(),
                m_IsSpendResource = true,
                Amount = 1
            });
            VanguardAbility.AddComponent(new AbilityShowIfCasterHasFact() { m_UnitFact = r2 });
            VanguardAbility.AddComponent(new AbilityTargetsAround()
            {
                m_Radius = new(30),
                m_TargetType = TargetType.Ally,
                m_SpreadSpeed = new(0)
            });
            VanguardAbility.AddComponent(new ContextRankConfig()
            {
                m_BaseValueType = ContextRankBaseValueType.ClassLevel,
                m_Progression = ContextRankProgression.Div2,
                m_Max = 20,
                m_Class = new BlueprintCharacterClassReference[] { ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("c75e0971973957d4dbad24bc7957e4fb")?.ToReference<BlueprintCharacterClassReference>() }
            });
            VanguardAbility.AddComponent(new AbilityEffectRunAction()
            {
                Actions = new()
                {
                    Actions = new GameAction[]
                    {
                        new Conditional()
                        {
                            ConditionsChecker = new()
                            {
                                Operation = Operation.And,
                                Conditions = new Condition[]{ new ContextConditionHasFact(){Not = true, m_Fact = r2}}
                            },
                            IfTrue = new()
                            {
                                Actions = new GameAction[]
                                {
                                    new ContextActionApplyBuff()
                                    {
                                        m_Buff = VanguardBuff.ToReference<BlueprintBuffReference>(),
                                        AsChild = true,
                                        DurationValue = new()
                                        {
                                            Rate = DurationRate.Rounds,
                                            DiceType = DiceType.One,
                                            DiceCountValue = new() {ValueType = ContextValueType.Rank, Value = 0},
                                            BonusValue = new() {ValueType = ContextValueType.Simple, Value = 3},
                                            m_IsExtendable = true
                                        }
                                    }
                                }
                            },
                            IfFalse = new()
                        }
                    }
                }
            });
            VanguardAbility.AddToCache();
            Comment.Log("Added {0} to the Blueprints cache.", VanguardAbility.name);
            #endregion
            #region Add Vanguard Ability to Vanguard Tatician base ability
            if (VanguardTacticianBaseAbility is null || !DoNotAdd) goto skipVanguardTactician;
            AbilityVariants av = VanguardTacticianBaseAbility.Components.FindOrDefault(c => c is AbilityVariants) as AbilityVariants;
            if (av is not null) av.m_Variants = av.m_Variants.AddToArray(VanguardAbility.ToReference<BlueprintAbilityReference>());
            Comment.Log("Added {0} to the  Vanguard Tatician base ability.", VanguardAbility.name);
        #endregion
        #endregion
        skipVanguardTactician:;
        }
        static readonly HashSet<(string, string)> selections = new()
        {
            new ("247a4068296e8be42890143f451b4b45", "BasicFeatSelection"),
            new ("41c8486641f7d6d4283ca9dae4147a9f", "FighterFeatSelection"),
            new ("c5357c05cf4f8414ebd0a33e534aec50", "CrusaderFeat1"),
            new ("50dc57d2662ccbd479b6bc8ab44edc44", "CrusaderFeat10"),
            new ("2049abc955bf6fe41a76f2cb6ba8214a", "CrusaderFeat20"),
            new ("303fd456ddb14437946e344bad9a893b", "WarpriestFeatSelection"),
            new ("dd17090d14958ef48ba601688b611970", "CavalierBonusFeatSelection"),
            new ("94ebbd6472c19fa4ea7196eaff11a740", "PackRagerTeamworkFeatureSelection"),
            new ("7bc55b5e381358c45b42153b8b2603a6", "CavalierTacticianFeatSelection"),
            new ("ef1cd58e0b7fc7f45baedb09407a1cd1", "GendarmeFeatSelection"),
            new ("cf2ca457ffb585a4995fd79441167a72", "DevilbanePriestTeamworkFeatSelection"),
            new ("79c6421dbdb028c4fa0c31b8eea95f16", "WarDomainGreaterFeatSelection"),
            new ("da03141df23f3fe45b0c7c323a8e5a0e", "EldritchKnightFeatSelection"),
            new ("01046afc774beee48abde8e35da0f4ba", "HunterTeamworkFeatSelection"),
            new ("d87e2f6a9278ac04caeb0f93eff95fcb", "TeamworkFeat"),
            new ("90f105c8e31a6224ea319e6a810e4af8", "LoremasterCombatFeatSelection"),
            new ("66befe7b24c42dd458952e3c47c93563", "MagusFeatSelection"),
            new ("8e627812dc034b9db12fa396fdc9ec75", "ArcaneRiderFeatSelection"),
            new ("c5158a6622d0b694a99efb1d0025d2c1", "CombatTrick"),
            new ("29b480a26a88f9e47a10d8c9fab84ee6", "BattleProwessSelection"),
            new ("64960cdba39692243bef11da263ab7f3", "BattleScionTeamworkFeat"),
            new ("78fffe8e5d5bc574a9fd5efbbb364a03", "StudentOfWarCombatFeatSelection"),
            new ("303fd456ddb14437946e344bad9a893b", "WarpriestFeatSelection"),
            new ("cfad18f581584ac4ba066df067956477", "LifeBondingFriendshipSelection"),
            new ("69a33d6ced23446e819667149d088898", "LifeBondingFriendshipSelection1"),
            new ("e10c4f18a6c8b4342afe6954bde0587b", "ExtraFeatMythicFeat"),
            new ("a21acdafc0169f5488a9bd3256e2e65b", "DragonLevel2FeatSelection")
        };
        #region TeamworkFeatures
        static BlueprintFeature PackRagerRagingTacticianBaseFeature;
        static BlueprintFeature CavalierTacticianSupportFeature;
        static BlueprintAbility CavalierTacticianAbility;
        static BlueprintAbility CavalierTacticianAbilitySwift;
        static BlueprintFeature HunterTactics;
        static BlueprintBuff MonsterTacticsBuff;
        static BlueprintFeature SacredHuntsmasterTactics;
        static BlueprintBuff TacticalLeaderFeatShareBuff;
        static BlueprintBuff BattleProwessEffectBuff;
        static BlueprintAbility VanguardTacticianBaseAbility;

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPriority(800)]
        [HarmonyPostfix]
        public static void BlueprintsCache_Init_PatchRetrieveAllTacticianFeatures()
        {
            string circ = "when retrieving all tactican features";
            RetrieveBlueprint("54efaa577ffe5114eb839d1bee8eda95", out PackRagerRagingTacticianBaseFeature, "PackRagerRagingTacticianBaseFeature", circ);
            RetrieveBlueprint("37c496c0c2f04544b83a8d013409fd47", out CavalierTacticianSupportFeature, "CavalierTacticianSupportFeature", circ);
            RetrieveBlueprint("3ff8ef7ba7b5be0429cf32cd4ddf637c", out CavalierTacticianAbility, "CavalierTacticianAbility", circ);
            RetrieveBlueprint("78b8d3fd0999f964f82d1c5ec30900e8", out CavalierTacticianAbilitySwift, "CavalierTacticianAbility", circ);
            RetrieveBlueprint("1b9916f7675d6ef4fb427081250d49de", out HunterTactics, "HunterTactics", circ);
            RetrieveBlueprint("e1f437048db80164792155102375b62c", out SacredHuntsmasterTactics, "SacredHuntsmasterTactics", circ);
            RetrieveBlueprint("81ddc40b935042844a0b5fb052eeca73", out MonsterTacticsBuff, "MonsterTacticsBuff", circ);
            RetrieveBlueprint("a603a90d24a636c41910b3868f434447", out TacticalLeaderFeatShareBuff, "TacticalLeaderFeatShareBuff", circ);
            RetrieveBlueprint("8c8cb2f8d83035e45843a88655da8321", out BattleProwessEffectBuff, "BattleProwessEffectBuff", circ);
            RetrieveBlueprint("00af3b5f43aa7ae4c87bcfe4e129f6e8", out VanguardTacticianBaseAbility, "VanguardTacticianBaseAbility", circ);
        }
        #endregion

        public static readonly Vector2[] results = new Vector2[2]
        {
            new Vector2(0, 0),
            new Vector2(0, 0)
        };
        static Vector2 buffer = new(0f, 0f);
        static public int LineCircleIntersect(Vector2 lineStart, Vector2 lineEnd, Vector2 circleCenter, float circleRadius, bool ordered = true)
        {
            Array.Clear(results, 0, 2);

            if (lineEnd.x - lineStart.x is > -0.005f and < 0.005f) // if the line is vertical
            {
                Comment.Log($"Shortcut X");
                var distance_x = Mathf.Abs(circleCenter.x - lineStart.x) - circleRadius;

                if (distance_x > 0.005f) // distance from the center of the circle to the vertical line is greater than the radius, hence no intersection
                    return 0;

                results[0].x = lineStart.x;
                results[0].y = circleCenter.y;
                if (distance_x is > -0.005f and < 0.005f) // if a vertical line meets the circle exactly at the radius end
                    return 1;
                var blah = Mathf.Sqrt(circleRadius * circleRadius - (circleCenter.x - lineStart.x) * (circleCenter.x - lineStart.x));
                results[0].y += blah;
                results[1].x = lineStart.x;
                results[1].y = circleCenter.y - blah;
                if (ordered)
                    CheckOrder();
                return 2;
            }

            if (lineEnd.y - lineStart.y is > -0.005f and < 0.005f) // if the line is horizontal
            {
                Comment.Log($"Shortcut Y");
                var distance_y = Mathf.Abs(circleCenter.y - lineStart.y) - circleRadius;

                if (distance_y > 0.005f) // distance from the center of the circle to the horizontal line is greater than the radius, hence no intersection
                    return 0;

                results[0].x = circleCenter.x;
                results[0].y = lineStart.y;
                if (distance_y is > -0.005f and < 0.005f) // if a horizontal line meets the circle exactly at the radius end
                    return 1;
                var blah = Mathf.Sqrt(circleRadius * circleRadius - (circleCenter.y - lineStart.y) * (circleCenter.y - lineStart.y));
                results[0].x += blah;
                results[1].x = circleCenter.x;
                results[1].y = lineStart.y - blah;
                if (ordered)
                    CheckOrder();
                return 2;
            }


            lineStart -= circleCenter; 
            lineEnd -= circleCenter;

            float offsetX = lineEnd.x - lineStart.x;
            float offsetY = lineEnd.y - lineStart.y;
            float segmentLengthSqr = offsetX * offsetX + offsetY * offsetY;
            float determ = lineStart.x * lineEnd.y - lineStart.y * lineEnd.x;
            float incSqr = circleRadius * circleRadius * segmentLengthSqr - determ * determ;
            if (incSqr < -0.0f)
                return 0;
            var incidence = Mathf.Sqrt(incSqr);
            Comment.Log($"Incidence sqaured is {incidence}");
            var a = determ * offsetY;
            var b = Sign(offsetY) * offsetX * incidence;
            var c = -determ * offsetX;
            var d = Math.Abs(offsetY) * incidence;

            results[0].x = ((a + b) / segmentLengthSqr) + circleCenter.x;
            results[0].y = ((c + d) / segmentLengthSqr) + circleCenter.y;

            if (incidence < 0.01f)
                return 1;
            
            results[1].x = ((a - b) / segmentLengthSqr) + circleCenter.x;            
            results[1].y = ((c - d) / segmentLengthSqr) + circleCenter.y;

            if (ordered)
                CheckOrder();            
            return 2;

            ; float Sign (float Input)
            {
                if (Input < 0)
                    return -1f;
                else return 1f;
            }

            ; void CheckOrder()
            {
                var compare = Vector2.SqrMagnitude(results[0] - lineStart) - Vector2.SqrMagnitude(results[1] - lineStart);
                if (compare > 0.0f)
                {
                    buffer = results[0];
                    results[0] = results[1];
                    results[1] = buffer;
                }

            }
        }

        static readonly (Vector2 lineStart, Vector2 lineEnd, Vector2 circleCenter, float circleRadius)[] test = new[]
        {
            (new Vector2(10f, 0f), new Vector2(10f, 20f), new Vector2(10f, 10f), 5f),
            (new Vector2(7.5f, 0f), new Vector2(7.5f, 20f), new Vector2(10f, 10f), 5f),
            (new Vector2(5f, 0f), new Vector2(5f, 20f), new Vector2(10f, 10f), 5f),
            (new Vector2(2.5f, 0f), new Vector2(2.5f, 20f), new Vector2(10f, 10f), 5f),
            (new Vector2(6f, 0f), new Vector2(6f, 20f), new Vector2(10f, 10f), 5f),
            (new Vector2(0f, 0f), new Vector2(20f, 20f), new Vector2(10f, 10f), 5f),
            (new Vector2(0f, 10f), new Vector2(20f, 10f), new Vector2(10f, 10f), 5f),
            (new Vector2(10f, 20f), new Vector2(10f, 0f), new Vector2(10f, 10f), 5f),
            (new Vector2(10f, 0f), new Vector2(10f, 1f), new Vector2(10f, 10f), 5f),
        };

        //[HarmonyPatch(typeof(BlueprintsCache), nameof (BlueprintsCache.Init))]
        //[HarmonyPrefix]
        static void test2()
        {
            Comment.Log("LineCircleIntersect - begin test");
            foreach (var t in test)
            {
                var i = LineCircleIntersect(t.lineStart, t.lineEnd, t.circleCenter, t.circleRadius);
                if (i == 0)
                    Comment.Log($"No intersect for line from ({t.lineStart}) to ({t.lineEnd})");
                else if (i == 1)
                    Comment.Log($"Single intersect for line from ({t.lineStart}) to ({t.lineEnd}) at ({results[0]})");
                else
                    Comment.Log($"Two intersects for line from ({t.lineStart}) to ({t.lineEnd}) at ({results[0]}) and at ({results[1]})");
            }
        }
        



        public enum WeaponTypesForSoftCoverDenial
        {
            Any = 0,
            Reach = 1,
            Ranged = 2
        }

        [HarmonyPatch(typeof(Kingmaker.Cheats.Utilities), nameof(Kingmaker.Cheats.Utilities.GetAllBlueprints))]
        internal static class CheatDataPatch
        {
            static bool added = false;

            internal static void Prefix (ref bool __state)
            {
                if (Kingmaker.Cheats.Utilities.s_BlueprintList is null || !added) __state = true;
                else __state = false;
            }

            internal static void Postfix(ref bool __state)
            {
                if (!__state) return;
                if (Kingmaker.Cheats.Utilities.s_BlueprintList is not BlueprintList list)
                {
                    Comment.Log("WARNING! CheatDataPatch has set state to true, but the blueprint list is still null");
                    return;
                }
                list.Entries.AddRange(moddedBP.Entries);
                added = true;
            }

        }
    }
}
