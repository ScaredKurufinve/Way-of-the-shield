using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Items.Slots;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints.Classes;

namespace Way_of_the_shield.NewComponents
{
    [HarmonyPatch]
    public class CouchedSpears
    {
        [TypeId("a7d850915c644a3aa8332d4b76b61764")]
        public class CouchedSpears_Component : CanUse2hWeaponAs1hBase
        {
            public override bool CanBeUsedAs2h(ItemEntityWeapon weapon)
            {
                return false;
            }

            public override bool CanBeUsedOn(ItemEntityWeapon weapon)
            {
                if (Fact.Owner.Unit.GetSaddledUnit() is null) return false;

                if (weapon is null) { return false; };

                return weapon.Blueprint.FighterGroup == WeaponFighterGroupFlags.Spears;
            }
        }

        [HarmonyPrepare]
        public static bool Prepare()
        {
            if (AllowTwoHandedSpears_as_OneHandedWhenMounted.GetValue()) return true;
            else { Comment.Log("AllowTwoHandedSpears_as_OneHandedWhenMounted setting is disabled, patch AddBuckler1hToProfficiencyBlueprint won't be applied."); return false; };
        }

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPostfix]
        public static void AddCouchedSpears1hToMountedCombat()
        {
#if DEBUG
            if (Debug.GetValue())
                Comment.Log("Begin adding the CouchedSpear component to the Mounted Combat feature blueprint.");
#endif
            if (!RetrieveBlueprint("f308a03bea0d69843a8ed0af003d47a9", out BlueprintFeature MountedCombat, "MountedCombat", "when adding Couched spears")) return;

            MountedCombat.AddComponent(new CouchedSpears_Component());
            MountedCombat.m_Description = new() { Key = "MountedCombat_description", m_ShouldProcess = true };
        }
    }
}
