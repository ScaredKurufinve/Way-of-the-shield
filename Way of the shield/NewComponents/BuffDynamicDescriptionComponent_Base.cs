using HarmonyLib;
using Kingmaker.EntitySystem;
using Kingmaker.UI.MVVM._VM.Tooltip.Bricks;
using Kingmaker.UI.MVVM._VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using System;
using System.Collections;
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
        public static class Transpiler
        {

            public static TooltipBrickText TemporaryStorage;

            static Type EnumerableType;

            [HarmonyPatch(typeof(BlueprintsCache), nameof (BlueprintsCache.Init))]
            [HarmonyPostfix]
            static void ManualPatchOf_TooltipTemplateBuff_GetBody()
            {
                Main.harmony.Patch(original: TheTargetMethod(),
                                   transpiler: new HarmonyMethod(typeof(Transpiler).GetMethod(nameof(TooltipTemplateBuff_GetBody_InsertDynamicDescriptionCall))));
            }

            static MethodInfo TheTargetMethod()
            {
                var info = typeof(TooltipTemplateBuff).GetNestedType("<GetBody>d__10", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                if (info is null) return null;
                EnumerableType = info;
                var method = info.GetMethod(nameof(IEnumerator.MoveNext), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
#if DEBUG

                Comment.Log($"Type is {info?.Name} \n Method is {method.Name}"); 
#endif
                return method;

            }

            public static IEnumerable<CodeInstruction> TooltipTemplateBuff_GetBody_InsertDynamicDescriptionCall(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
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

                Label BeforeStacking = generator.DefineLabel();
                __instructions[index] = __instructions[index].WithLabels(BeforeStacking);


                CodeInstruction[] toInsert = new[]
                {
                    new (OpCodes.Ldloc_1),
                    new (OpCodes.Ldfld, typeof(TooltipTemplateBuff).GetField(nameof(TooltipTemplateBuff.Buff))),
                    CodeInstruction.Call(typeof(Transpiler), nameof(DynamicComponentCaller)),
                    new (OpCodes.Brfalse, BeforeStacking),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new (OpCodes.Ldsfld, typeof(Transpiler).GetField(nameof(TemporaryStorage), BindingFlags.Static | BindingFlags.Public)),
                    new (OpCodes.Stfld, EnumerableType?.GetField("<>2__current", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)),
                    new (OpCodes.Ldarg_0),
                    new (OpCodes.Ldc_I4_2),
                    new (OpCodes.Stfld, EnumerableType?.GetField("<>1__state", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)),
                    new (OpCodes.Ldc_I4_1),
                    new (OpCodes.Ret),
                    new CodeInstruction(OpCodes.Br, __instructions[index +2].operand)
                };
                __instructions.InsertRange(index, toInsert);
                return __instructions;
            }

            public static bool DynamicComponentCaller(Buff buff)
            {
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log("DynamicComponentCaller is called"); 
#endif

                var generator = buff.Components.Where(entityFactComponent => entityFactComponent.SourceBlueprintComponent is BuffDynamicDescriptionComponent_Base).FirstOrDefault();
                if (generator is not null)
                {
                    using (generator.RequestEventContext())
                        try
                        {
                        TemporaryStorage = new TooltipBrickText((generator.SourceBlueprintComponent as BuffDynamicDescriptionComponent_Base)?.GenerateDescription(), TooltipTextType.Simple);
                        }
                        catch (Exception ex) 
                        {
                            TemporaryStorage = null;
                            Exception exception= ex;
                            while (exception != null)
                            {
                                Comment.Log(exception.Message);
                                exception= exception.InnerException;
                            }
                            Comment.Error(ex.StackTrace);
                        }
                    if (TemporaryStorage is not null) 
                        return true;
                }
#if DEBUG
                if (Settings.Debug.GetValue())
                    Comment.Log("DynamicComponentCaller - result is null"); 
#endif
                return false;
            }
        }
    }


}
