//#undef DEBUG
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Way_of_the_shield.NewComponents
{
    [HarmonyPatch]
    [TypeId("a6d261e170e846c5b546b5d4069b4cc3")]
    [AllowedOn(typeof(BlueprintBuff))]
    public class BuffSetRanksOnApply : UnitBuffComponentDelegate
    {
        public ContextValue Value;
        int LastResult;

        public void SetRanks()
        {
#if DEBUG
            if (Debug.GetValue())
                Comment.Log("Inside BuffSetRanksOnApply.SetRanks"); 
#endif
            if (Value is null)
            {
                Comment.Error("BuffSetRanksOnApply: Value is null when applying {0} Buff to {1}", Buff.Blueprint?.name ?? Buff.Blueprint?.AssetGuid.ToString(), Buff.Owner?.CharacterName);
                return;
            }
            if (Buff.Context is null)
            {
                Comment.Warning("BuffSetRanksOnApply: Context is null when applying {0} Buff to {1}", Buff.Blueprint?.name ?? Buff.Blueprint?.AssetGuid.ToString(), Buff.Owner?.CharacterName);
                return;
            }
            int result = LastResult = Value.Calculate(Buff.Context);
#if DEBUG
            if (Debug.GetValue())
                Comment.Log("BuffSetRanksOnApply: Buff {0} on unit {1} will be set to rank {2}", Buff.Blueprint?.name ?? Buff.Blueprint?.AssetGuid.ToString(), Buff.Owner?.CharacterName, result); 
#endif

            Buff.SetRank(result);
        }

        [HarmonyPatch(typeof(UnitHelper), nameof(UnitHelper.AddBuff), new Type[] {typeof(UnitDescriptor), typeof(BlueprintBuff), typeof(MechanicsContext), typeof(TimeSpan?) })]
        [HarmonyPostfix]
        public static void SetRanksPostfix(Buff __result)
        {
            __result?.CallComponents<BuffSetRanksOnApply>(setRanks => setRanks.SetRanks());
        }
    }
}
