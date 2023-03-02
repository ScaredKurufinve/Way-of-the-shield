using Kingmaker.Blueprints.Classes;
using static Way_of_the_shield.Main;
using static Way_of_the_shield.Utilities;

namespace Way_of_the_shield
{
    [HarmonyPatch]
    public static class FixImprovedPreciseShot
    {
        [HarmonyPrepare]
        public static bool CheckSettings()
        {
            if (AddSoftCoverDenialToImprovedPreciseShot.GetValue()) return true;
            else { Comment.Log("AddSoftCoverDenialToImprovedPreciseStrike setting is disabled, patch FixImprovedPreciseShot won't be applied."); return false; };
        }

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPostfix]
        public static void FixImprovedPreciseShot_BlueprintsCache_Init_Postfix()
        {
#if DEBUG
            if (Settings.Debug.GetValue())
                Comment.Log("Entered the BLueprintCache Init postfix to add SoftCoverDenial to Improved Precise Shot"); 
#endif
            if (!RetrieveBlueprint("46f970a6b9b5d2346b10892673fe6e74", out BlueprintFeature ImprovedPreciseShot, "ImprovedPreciseShot", "when adding SoftCoverDenial to it.")) return;
            ImprovedPreciseShot.AddComponent(new SoftCover.SoftCoverDenialComponent());
            ImprovedPreciseShot.m_Description = new() { Key = "ImprovedPreciseShot_Description" };
            Comment.Log("Successfully added SoftCoverDenial to Improved Precise Shot");
        }
    }
}
