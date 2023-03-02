using Kingmaker.EntitySystem;
using Kingmaker.UI.MVVM._VM.Tooltip.Bricks;
using Kingmaker.UI.MVVM._VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.UI.Tooltips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Way_of_the_shield.NewComponents
{
    public abstract class BuffDynamicDescriptionComponent_Base : UnitBuffComponentDelegate
    {
        public abstract string GenerateDescription();




        [HarmonyPatch]
        static class Transpiler
        {
            static Type EnumerableType;

            [HarmonyPatch(typeof(BlueprintsCache), nameof (BlueprintsCache.Init))]
            [HarmonyPostfix]
            static void ManualPatchOf_TooltipTemplateBuff_GetBody()
            {
                Main.harmony.Patch(TheTargetMethod(), transpiler: new HarmonyMethod(typeof(Transpiler).GetMethod(nameof(TooltipTemplateBuff_GetBody_InsertDynamicDescriptionCall), BindingFlags.NonPublic | BindingFlags.Static)));
            }


            static MethodInfo TheTargetMethod()
            {
                var info = typeof(TooltipTemplateBuff).GetNestedType("<GetBody>d__9", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                if (info is null) return null;
                EnumerableType = info;
                var method = info.GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                return method;

            }

            static IEnumerable<CodeInstruction> TooltipTemplateBuff_GetBody_InsertDynamicDescriptionCall(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                List<CodeInstruction> __instructions = instructions.ToList();

                CodeInstruction[] toSearch = new CodeInstruction[]
                {
                    new (OpCodes.Ldloc_1),
                    new (OpCodes.Ldfld, typeof(TooltipTemplateBuff).GetField(nameof(TooltipTemplateBuff.m_Stacking))),
                    new (OpCodes.Brfalse_S)
                };

                int index = IndexFinder(__instructions, toSearch, before: true);
                if (index == -1) return instructions;

                Label label = generator.DefineLabel();
                __instructions[index].WithLabels(label);

                CodeInstruction[] toInsert = new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new (OpCodes.Ldloc_1),
                    new (OpCodes.Ldfld, typeof(TooltipTemplateBuff).GetField(nameof(TooltipTemplateBuff.Buff))),
                    CodeInstruction.Call(typeof(Transpiler), nameof(DynamicComponentCaller)),
                    new (OpCodes.Stfld, EnumerableType?.GetField("<>2__current", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)),
                    new (OpCodes.Ldarg_0),
                    new (OpCodes.Ldc_I4_2),
                    new (OpCodes.Stfld, EnumerableType?.GetField("<>1__state", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)),
                    new (OpCodes.Ldc_I4_1),
                    new (OpCodes.Ret),
                    new (OpCodes.Ldarg_0),
                    new (OpCodes.Ldc_I4_M1),
                    new (OpCodes.Stfld, EnumerableType?.GetField("<>1__state", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)),
                    new CodeInstruction(OpCodes.Br, __instructions[index +2].operand)
                };
                __instructions.InsertRange(index, toInsert);
                return __instructions;
            }

            static TooltipBrickText DynamicComponentCaller(Buff buff)
            {
                string result = "";

                var generator = buff.Components.Where(entityFactComponent => entityFactComponent.SourceBlueprintComponent is BuffDynamicDescriptionComponent_Base).FirstOrDefault();
                if (generator is not null)
                {
                    using (generator.RequestEventContext())
                    {
                        result = (generator.SourceBlueprintComponent as BuffDynamicDescriptionComponent_Base).GenerateDescription();
                    }
                }
                return new TooltipBrickText(result, TooltipTextType.Simple);
            }
        }
    }


}
