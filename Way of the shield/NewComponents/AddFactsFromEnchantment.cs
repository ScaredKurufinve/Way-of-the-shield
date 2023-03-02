using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.UnitLogic;
using Kingmaker.EntitySystem;
using Kingmaker.Items;
using System.Linq;

namespace Way_of_the_shield.NewComponents
{
    [HarmonyPatch]
    [ComponentName("Add facts to the item wielder from enchantment")]
    [AllowedOn(typeof(BlueprintItemEnchantment), false)]
    [AllowMultipleComponents]
    [TypeId("f7d07a50707c444f8ee2c5e70a5be487")]
    public class AddFactsToEnchantmentWielder : ItemEnchantmentComponentDelegate
    {
        public BlueprintUnitFactReference[] Facts;
        public bool NeedsIdentification;

        //[HarmonyPatch(typeof(ItemEntity), nameof(ItemEntity.ReapplyFactsForWielder))]
        //[HarmonyPostfix]
        public static void ItemEntity_ReapplyFactsForWielder_Postfix(ItemEntity __instance)
        {

            if (__instance.Wielder is null || __instance is ItemEntityShield) return;
            BlueprintUnitFact entityFact;
            if (__instance.m_FactsAppliedToWielder is null) __instance.m_FactsAppliedToWielder = Enumerable.Empty<EntityFact>().ToArray();
            foreach (ItemEnchantment enchantment in __instance.Enchantments)
            {
#if DEBUG
                if (Debug.GetValue())
                    Comment.Log("Enchantment is " + enchantment.Name); 
#endif
                foreach (AddFactsToEnchantmentWielder component in enchantment.Blueprint.GetComponents<AddFactsToEnchantmentWielder>())
                {
                    if (component.Facts is null || component.NeedsIdentification && !__instance.IsIdentified) continue;
                    foreach (BlueprintUnitFactReference refer in component.Facts)
                    {
                        entityFact = refer?.Get();
                        if (entityFact is null) continue;
#if DEBUG
                        if (Debug.GetValue())
                            Comment.Log($"Fact is {entityFact.Name}."); 
#endif
                        EntityFact newFact = __instance.m_FactsAppliedToWielder.FirstItem((i) => i.Blueprint == entityFact) ?? __instance.Wielder.AddFact(entityFact, null, null);
                        newFact.SetSourceItem(__instance);
                        __instance.m_FactsAppliedToWielder = __instance.m_FactsAppliedToWielder.Append(newFact).ToArray();
                    };
                }
            }

        }


    }
}
