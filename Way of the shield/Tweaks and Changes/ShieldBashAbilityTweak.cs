using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Way_of_the_shield.NewComponents;
using static Kingmaker.Blueprints.Area.FactHolder;

namespace Way_of_the_shield.Tweaks_and_Changes
{
    [HarmonyPatch]
    public class ShieldBashAbilityTweak
    {
        //[HarmonyPrepare]
        //public static bool Prepare()
        //{
        //    if (!AllowShieldBashToAllWhoProficient.GetValue())
        //    { Comment.Log("AllowShieldBashToAllWhoProficient is disabled. BlueprintsCache_Init_Patch_MoveShieldBashAbilityToProficiencyBP patch will not be applied"); return false; }
        //    else return true;
        //}


        [HarmonyPostfix]
        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        public static void BlueprintsCache_Init_Patch_MoveShieldBashAbilityToProficiencyBP()
        {
#if DEBUG
            if (Debug.GetValue())
                Comment.Log("ShieldBashAbilityTweak - Inside BlueprintsCache_Init_Patch_MoveShieldBashAbilityToProficiencyBP");
#endif
            string BashAbilityGUID = "3bb6b76ed5b38ab4f957c7f923c23b68";
            #region Create NewShieldBash blueprint
            BlueprintFeature NewShieldBash = new()
            {
                IsClassFeature = true,
                HideInUI = true,
            };
            NewShieldBash.AddComponent(new AddFacts()
            {
                m_Facts = new[] 
                {
                    new BlueprintUnitFactReference(){deserializedGuid = BlueprintGuid.Parse(BashAbilityGUID)} //ShieldBashAbility
                }
            });
            NewShieldBash.AddToCache("f42adaab0f24462c87a7875c259ffccb", "NewShieldBashFeature");
            #endregion
            if (!AllowShieldBashToAllWhoProficient.GetValue())
            { 
                Comment.Log("AllowShieldBashToAllWhoProficient is disabled. BlueprintsCache_Init_Patch_MoveShieldBashAbilityToProficiencyBP patch will not be applied"); 
                return; 
            }
            if (!RetrieveBlueprint(BashAbilityGUID, out BlueprintActivatableAbility ShieldBashAbility, "ShieldBashAbility", "when tweaking Shield Bash")) return;
            string circ = "when tweaking Shield Bash";
            if (!RetrieveBlueprint("121811173a614534e8720d7550aae253", out BlueprintFeature ShieldBashFeature, "ShieldBashFeature", circ)) return;
            #region Restrict Bash to controllable units
            ShieldBashAbility.AddComponent(new DirectlyControlledUnlessFactRestriction() { m_Fact = ShieldBashFeature.ToReference<BlueprintUnitFactReference>()});
#if DEBUG
            if (Debug.GetValue())
                Comment.Log("ShieldBashAbilityTweak - Restricted Shield Bash to directly controlled units.");
#endif
            #endregion
            #region ShieldBash feature blueprint tweaks
            IEnumerable<AddFacts> AFlist = ShieldBashFeature.Components.OfType<AddFacts>();
            if (AFlist.Count () < 1) { Comment.Error($"Failed to find any AddFacts components on the {ShieldBashFeature.name} blueprint {circ}."); return; }
            if (!AFlist.TryFind(c => c.m_Facts.Contains(ShieldBashAbility as BlueprintUnitFact), out AddFacts af)) 
                { Comment.Error($"Failed to find any AddFacts component on the {ShieldBashFeature.name} blueprint containing guid {BashAbilityGUID} {circ}."); return; }
            af.m_Facts = af.m_Facts
                .Where(f => f.deserializedGuid != BlueprintGuid.Parse(BashAbilityGUID))
                .AddItem(NewShieldBash.ToReference<BlueprintUnitFactReference>())
                .ToArray();
            ShieldBashFeature.m_DisplayName = new LocalizedString() { m_Key = "OldShieldBashFeature_DisplayName" };
            ShieldBashFeature.m_Description = new LocalizedString() { m_Key = "OldShieldBashFeature_Description" };
#if DEBUG
            if (Debug.GetValue())
                Comment.Log("ShieldBashAbilityTweak - Removed the Shield Bash ability reference from the Shield Bash Improved blueprint");
#endif
            #endregion
            #region Add shield bash to the proficiency if setting is enabled
            if (!AllowShieldBashToAllWhoProficient.GetValue())
            { Comment.Log("AllowShieldBashToAllWhoProficient is disabled. BlueprintsCache_Init_Patch_MoveShieldBashAbilityToProficiencyBP patch will not be applied"); goto skipAddingTrigger; }
            if (!RetrieveBlueprint("cb8686e7357a68c42bdd9d4e65334633", out BlueprintFeature ShieldsProficiency, "ShieldsProficiency", circ)) return;
            AddFacts afProf = ShieldsProficiency.Components.OfType<AddFacts>().FirstOrDefault();
            if (afProf is null)
            {
                afProf = new AddFacts() { m_Facts = new BlueprintUnitFactReference[] { } };
                ShieldsProficiency.AddComponent(afProf);
            }
            afProf.m_Facts = afProf.m_Facts.AddToArray(new BlueprintUnitFactReference() { deserializedGuid = BlueprintGuid.Parse("f42adaab0f24462c87a7875c259ffccb") });
#if DEBUG
            if (Debug.GetValue())
                Comment.Log("ShieldBashAbilityTweak - Added the Shield Bash ability reference to the Shield Proficiency blueprint");
#endif
            #endregion
            #region slap the shield denial onto ShieldBashBuff
            if (!RetrieveBlueprint("5566971fdebf7fe468a497bbee0d3ed5", out BlueprintBuff ShieldBashBuff, "ShieldBashBuff", circ)) goto skipAddingTrigger;
            if (ShieldBashBuff.ComponentsArray.Any(component => component is AddInitiatorShieldBashTrigger)) goto skipAddingTrigger;
            ShieldBashBuff.AddComponent(new AddInitiatorShieldBashTrigger()
            {
                OnlyOnFirstBashAttack = true,
                ActionsOnInitiator = true,
                Actions = new()
                {
                    Actions = new GameAction[]
                    {
                        new Conditional()
                        {
                            name = $"{ShieldBashBuff.name}_AddInitiatorShieldBashTrigger_Conditional",
                            ConditionsChecker = new()
                            {
                                Conditions = new Condition[]
                                {
                                    new ContextConditionCasterHasFact()
                                    {
                                        m_Fact = new(){deserializedGuid = BlueprintGuid.Parse("121811173a614534e8720d7550aae253")} // Improved Shield Bash
                                    }
                                }
                            },

                            IfFalse = new() {Actions = new GameAction[]
                            {
                                new ContextActionApplyBuff()
                                {
                                    m_Buff = new(){deserializedGuid = BlueprintGuid.Parse("414f40680af64050a2a9dde3dede32ac")}, // Shield Forbiddance
                                    UseDurationSeconds = true,
                                    DurationSeconds = 6,
                                    //ToCaster = true,
                                } }
                            },
                            IfTrue = new () {Actions = new GameAction[]
                            { 
                            }},
                        }
                    }
                }
            });

        skipAddingTrigger:;
#if DEBUG
            if (Debug.GetValue())
                Comment.Log("ShieldBashAbilityTweak - Added the trigger causing Shield Forbiddance to to the ShieldBashBuff blueprint");
#endif
            #endregion
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ActivatableAbilityCollection), nameof(ActivatableAbilityCollection.OnFactDidAttach))]
        public static IEnumerable<CodeInstruction> ActivatableAbilityCollection_OnFactDidAttach_PatchToNotActivateOnAttach(IEnumerable<CodeInstruction> instructions)
        {
#if DEBUG
            if (Debug.GetValue())
                Comment.Log("Entered ActivatableAbilityCollection_OnFactDidAttach_PatchToNotActivateOnAttach");
#endif

            var _inst = instructions.ToList();

            CodeInstruction[] toSearch = new[]
            {
                new CodeInstruction(OpCodes.Ldfld, typeof(BlueprintActivatableAbility).GetField(nameof(BlueprintActivatableAbility.IsOnByDefault))),
                new CodeInstruction(OpCodes.Brfalse_S)
            };

            int index = IndexFinder(_inst, toSearch);
            if (index == -1)
            {
                Comment.Error("ActivatableAbilityCollection_OnFactDidAttach_PatchToNotActivateOnAttach - failed to find IsOnByDefault");
                return instructions;
            }

            CodeInstruction[] toInsert = new[] 
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                CodeInstruction.Call((ActivatableAbility fact) => CheckShieldBashForDefaultOn(fact)),
                new CodeInstruction(_inst[index-1])
            };

            _inst.InsertRange(index, toInsert);
            return _inst;
        }

        public static bool CheckShieldBashForDefaultOn(ActivatableAbility fact)
        {
            return fact.Blueprint.AssetGuidThreadSafe is not "3bb6b76ed5b38ab4f957c7f923c23b68" //ShieldBashAbility
                || !fact.Owner.IsPlayerFaction;
        }
    }
}
