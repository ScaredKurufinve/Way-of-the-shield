using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Items;

namespace Way_of_the_shield.Tweaks_and_Changes
{
    [HarmonyPatch]
    public static class TWFShieldTweak
    {
        [HarmonyPrepare]
        static bool Prepare()
        {
#if DEBUG
            if (!CheckForShieldLightness.GetValue())
                Comment.Log("CheckForShieldLightness setting is disabled. TWFShieldTweak patch will not be applied");
#endif
            return CheckForShieldLightness.GetValue();
        }

        [HarmonyPatch(typeof(TwoWeaponFightingAttackPenalty), nameof(TwoWeaponFightingAttackPenalty.OnEventAboutToTrigger))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> TwoWeaponFightingAttackPenalty_Transpiler_RemoveShieldBullshit(IEnumerable<CodeInstruction> instructions)
        {
#if DEBUG
            if (Debug.GetValue())
                Comment.Log("TWFShieldTweak - Begin transpiling TwoWeaponFightingAttackPenalty.OnEventAboutToTrigger"); 
#endif
            var _inst = instructions.ToList();

            var toSearch = new CodeInstruction[]
            {
                new (OpCodes.Ldloc_1),
                new (OpCodes.Callvirt, typeof(ItemEntityWeapon).GetProperty(nameof(ItemEntityWeapon.IsShield)).GetMethod),
                new (OpCodes.Brtrue_S)
            };

            int index = IndexFinder(_inst, toSearch, before: true);
            if (index == -1) return instructions;
            _inst.RemoveRange(index, toSearch.Length);
            return _inst;

        }
    }
}
