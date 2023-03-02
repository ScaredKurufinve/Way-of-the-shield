using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.Mechanics.Facts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace Way_of_the_shield
{
    [HarmonyPatch]
    public static class ShieldWallFix
    {
        [HarmonyPrepare]
        public static bool Prepare()
        {
            if (ChangeShieldWall.GetValue()) return true;
            else { Comment.Log("ChangeShieldWall setting is disabled, patch ShieldWallFix won't be applied."); return false; };
        }
        
        [HarmonyPatch(typeof(BlueprintsCache), nameof (BlueprintsCache.Init))]
        [HarmonyPostfix]
        public static void SubstituteShieldWallComponent()
        {
            if (!RetrieveBlueprint("8976de442862f82488a4b138a0a89907", out BlueprintFeature bpShieldWall, "ShieldWall", "when substituting the ShieldWall component")) return;
            
            bpShieldWall.ComponentsArray = bpShieldWall.Components.Where(c => c is not ShieldWall).ToArray();
            bpShieldWall.AddComponent(new NewComponents.ShieldWallNew() { Radius = 1, m_ShieldWallFact = bpShieldWall.ToReference<BlueprintUnitFactReference>() });
            LocalizedString description = new () { m_Key = "ShieldWallRenewed_Description", m_ShouldProcess = true };
            LocalizedString descriptionShort = new () { m_Key = "ShieldWallRenewed_DescriptionShort", m_ShouldProcess = true };
            bpShieldWall.m_Description = description;
            bpShieldWall.m_DescriptionShort = descriptionShort;
            if (!RetrieveBlueprint("cc26546e4f73fe142b606b4759b4eb18", out BlueprintBuff CavalierTacticianShieldWallBuff, "CavalierTacticianShieldWallBuff", "when substituting the ShieldWall component")) return;
            CavalierTacticianShieldWallBuff.m_Description = description;
            CavalierTacticianShieldWallBuff.m_DescriptionShort = descriptionShort;

        }
    }
}
