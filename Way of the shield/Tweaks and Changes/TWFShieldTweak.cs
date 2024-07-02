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
                new (OpCodes.Call, typeof(ItemEntityWeapon).GetProperty(nameof(ItemEntityWeapon.IsShield)).GetMethod),
                new (OpCodes.Br_S)
            };

            int index = IndexFinder(_inst, toSearch, before: true);
            if (index == -1) return instructions;
            Comment.Log($"TWFShieldTweak - instructions are: \n{string.Join(", \n", _inst[index+0], _inst[index + 1], _inst[index + 2], _inst[index + 3])} ");
            _inst[index + toSearch.Length].MoveLabelsFrom(_inst[index]);
            _inst.RemoveRange(index, toSearch.Length);
            return _inst;

        }
    }
}
