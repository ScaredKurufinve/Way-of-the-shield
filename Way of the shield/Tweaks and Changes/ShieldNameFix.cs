using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.UI.Tooltips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Way_of_the_shield.Tweaks_and_Changes
{
    [HarmonyPatch(typeof(ItemEntity), nameof(ItemEntity.Name), MethodType.Getter)]
    static class ShieldNameFix
    {
        [HarmonyBefore("TabletopTweaks-Base")]
        [HarmonyPrefix]
        static bool Prefix(ItemEntity __instance, ref string __result)
        {
            if (__instance is not ItemEntityWeapon weapon) return true;
            if (!weapon.IsShield) return true;
            __result = weapon.Shield.Name;
            return false;
        }
    }
    
}
