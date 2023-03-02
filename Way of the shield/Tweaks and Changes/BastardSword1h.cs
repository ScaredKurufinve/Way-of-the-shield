using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Items;
using Kingmaker.UnitLogic.FactLogic;
using System.Collections.Generic;
using System.Linq;
using static Way_of_the_shield.Main;
using static Way_of_the_shield.Utilities;

namespace Way_of_the_shield
{
    [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
    public static class BastardSword1h
    {
        [HarmonyPostfix]
        public static void CachePatch()
        {
#if DEBUG
            if (Debug.GetValue())
                Comment.Log("Entered Blueprints Cache patch for exotic Bastard Sword One-handed component"); 
#endif
            if (!RetrieveBlueprint("57299a78b2256604dadf1ab9a42e2873", out BlueprintFeature BastardSwordProficiency, "BastardSwordProficiency", "when meddling with Bastard sowrd type handedness ")) return;
            if (!RetrieveBlueprint("203992ef5b35c864390b4e4a1e200629", out BlueprintFeature MartialWeaponProficiency, "MartialWeaponProficiency", "when meddling with Bastard sowrd type handedness ")) return;
            BastardSwordProficiency.AddComponent(new WeaponCategory1HandedComponent() { category = WeaponCategory.BastardSword });
            if (BastardSwordProficiency.ComponentsArray.FindOrDefault(c => c is PrerequisiteNotProficient) is not PrerequisiteNotProficient pnp)
            {
                Comment.Warning("Failed to find the PrerequisiteNotProficient component in the BastardSwordProficiency blueprint");
                return;
            };
            if (MartialWeaponProficiency.ComponentsArray.FindOrDefault(c => c is AddProficiencies) is not AddProficiencies prof)
            {
                Comment.Warning("Failed to find the AddProficiencies component in the MartialWeaponProficiency blueprint");
                return;
            };
            prof.WeaponProficiencies = prof.WeaponProficiencies.AddToArray(WeaponCategory.BastardSword);
            List<BlueprintComponent> l = BastardSwordProficiency.ComponentsArray.ToList();
            l.Remove(pnp);
            BastardSwordProficiency.ComponentsArray = l.ToArray();
            BastardSwordProficiency.AddComponent(new PrerequisiteProficiency() { ArmorProficiencies = new ArmorProficiencyGroup[] { }, WeaponProficiencies = new WeaponCategory[] { WeaponCategory.BastardSword } });

        }

        public class WeaponCategory1HandedComponent : CanUse2hWeaponAs1hBase
        {
            public override bool CanBeUsedAs2h(ItemEntityWeapon weapon)
            {
                return true;
            }

            public override bool CanBeUsedOn(ItemEntityWeapon weapon)
            {
                if (weapon is null || weapon.Blueprint.Category != category) return false;
                else return true;
            }

            public WeaponCategory category;
        }

    }
}
