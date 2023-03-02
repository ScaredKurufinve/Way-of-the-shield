using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utils;
using UnityEngine;
using Way_of_the_shield.NewComponents;
using Way_of_the_shield.NewFeatsAndAbilities;

namespace Way_of_the_shield.Tweaks_and_Changes
{
    [HarmonyPatch]
    public class BackToBackTweak
    {
        [HarmonyPrepare]
        static bool Prepare()
        {
            if (!ChangeBackToBack.GetValue())
            {
                Comment.Log("ChangeBackToBack setting was disabled. BlueprintsCache_Init_Postfix_BackToBackNew patch won't be applied");
                return false;
            }
            else return true;
        }

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPostfix]
        static void BlueprintsCache_Init_Postfix_BackToBackNew()
        {
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log("Begin tweaking BackToBack"); 
#endif
            LocalizedString description = new() { Key = "BackToBackNew_Description", m_ShouldProcess = true };
            string circ = "when adding BackToBackNew";
            if (!RetrieveBlueprint("c920f2cd2244d284aa69a146aeefcb2c", out BlueprintFeature BTB, "BackToBack", circ)) return;
            BTB.ComponentsArray = BTB.Components.Where(c => c is not BackToBack).ToArray();
            BTB.AddComponent(new BackToBackNew());
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log($"Added BackToBackNew component to the {BTB.name} blueprint."); 
#endif
            BTB.m_Description = description;
            if (!RetrieveBlueprint("693964e674883e74b8d0005dbf4a4e6b", out BlueprintBuff CavalierTacticianBackToBackBuff, "CavalierTacticianBackToBackBuff", circ)) return;
            CavalierTacticianBackToBackBuff.m_Description = description;
        }
    }
}
