using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.RuleSystem.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Way_of_the_shield
{
    [HarmonyPatch(typeof(MeleeWeaponSizeChange), nameof(MeleeWeaponSizeChange.OnEventAboutToTrigger))]
    public class MeleeWeaponSizeChangeFix
    {
        [HarmonyPrefix]
        public static bool MeleeWeaponSizeChange_Fix(MeleeWeaponSizeChange __instance, RuleCalculateWeaponStats evt)
        {
            if (evt.Weapon.Blueprint.IsMelee && __instance.SizeCategoryChange != 0)
            {
                evt.IncreaseWeaponSize(__instance.SizeCategoryChange);
            }
            return false;
        }
    }
}
