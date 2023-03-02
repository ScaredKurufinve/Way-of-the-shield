#undef Dynamic
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.Modding;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using Kingmaker.TextTools;
using System.Diagnostics;
using System.Collections.Generic;
#if !Dynamic
using UnityModManagerNet;
#endif

namespace Way_of_the_shield
{
#if DEBUG
    //[EnableReloading]
#endif
    public static class Main
    {

        internal static Harmony harmony;
        public static string modName = "";
#if Dynamic
        public static dynamic mod;
#endif
#if !Dynamic
        public static object mod;
#endif
        public static string modPath;
        static internal bool BeenLoaded = false;
        internal static Assembly[] allAssemblies;
        internal static Assembly UMM;
        internal static Assembly TTTCore;
        internal static Assembly TTTBase;
        internal static Assembly ModMenu;

        [OwlcatModificationEnterPoint]
        public static void LoadOwlcat(OwlcatModification modEntry)
        {
            mod = modEntry;
            if (!BeenLoaded)
            {
                Comment = modEntry.Logger;
                modName = modEntry.Manifest.UniqueName;
                modPath = modEntry.Path;
            }
            Load1();
        }
#if Dynamic
        public static void Load(dynamic modEntry)
#endif
#if !Dynamic
        public static void Load(object modEntry)
#endif
        {
            mod = modEntry;
            try
            {
#if !Dynamic
                if (modEntry is not UnityModManager.ModEntry mod1) return;
                if (BeenLoaded ) goto Load;
                Comment = LogChannelFactory.GetOrCreate(mod1.Info.DisplayName);
                modName = mod1.Info.Id;
                modPath = mod1.Path;
#endif

#if Dynamic
                if (BeenLoaded) goto Load;
                Comment = LogChannelFactory.GetOrCreate(mod.Info.DisplayName);
                modName = mod.Info.Id;
                modPath = mod.Path;
#endif

            }
            finally { }
            Load:
            Load1();
        }


        public static void Load1()
        {
//#if DEBUG
            Stopwatch timer = new();
            timer.Start();
//#endif
            Comment.Log(modName);
#region get assemblies
            allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            UMM = CheckForMod("UnityModManager");
            if (UMM != null)
            {
                TTTCore = CheckForMod("TabletopTweaks-Core");
                TTTBase = CheckForMod("TabletopTweaks-Base");
                ModMenu = CheckForMod("ModMenu");
            }
#endregion
            Comment.Log("TTT-Core is " + TTTCore?.FullName ?? "not found.");
            Comment.Log("TTT-Base is " + TTTBase?.FullName ?? "not found.");
            Comment.Log("ModMenu is " + ModMenu?.FullName ?? "not found.");
            harmony = new(modName); 
            try
            {
            if (initialized == false) Settings.Init();
            }
            catch(TypeLoadException ex)
            {
                Comment.Exception(ex, "F me. The type is " + ex.TypeName);
                Comment.Log(ex.Message);
            }
            //if (mod is UnityModManager.ModEntry modEntry)
            //{


            //};
            Harmony.DEBUG = true;
            if (BeenLoaded) harmony.UnpatchAll();
                harmony.PatchAll();
            //foreach (var patch in Harmony.GetPatchInfo(typeof(BlueprintsCache).GetMethod(nameof(BlueprintsCache.Init))).Postfixes.Where(p =>p.owner == harmony.Id)) Comment.Log("Patch is " + patch.PatchMethod);
            Comment.Log("Patched things up.");
            BeenLoaded = true;
            ModifierDescriptorComparer.Instance = new();
            TextTemplateEngine.AddTemplate("shield_check_set_bool" , new BoolSettingCheckerTemplate());
//#if DEBUG
            timer.Stop();
            Comment.Log($"Loading took {timer.ElapsedMilliseconds} ms");
//#endif
        }

        

        public static Assembly CheckForMod(string modName)
        {
            return allAssemblies.Where(ass => ass.FullName.Contains(modName)).FirstOrDefault();
        }

        public static bool CheckForModEnabled(string modName)
        {

#if !Dynamic
            var modEntries = UnityModManager.modEntries;
#endif

#if Dynamic
            var modEntries = UMM?
                .GetType("UnityModManagerNet.UnityModManager")?
                .GetField("modEntries", BindingFlags.Static | BindingFlags.Public)?
                .GetValue(null) as List<dynamic>;
#endif

            return (modEntries is not null && modEntries.Contains(mod => mod.Info.AssemblyName.Contains("TabletopTweaks-Base") && mod.Enabled));
        }


    }

    [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
    public static class LocalizationPatchForUMM
    {
        [HarmonyPrepare]
        public static bool UMMCheck()
        {
            if (Main.mod is OwlcatModification || Main.BeenLoaded) return false;
            else return true;
        }

        [HarmonyPrefix]
        public static void Prefix()
        {
            try
            {
                Comment.Log("Will try to apply localization for the " + LocalizationManager.CurrentLocale + " locale.");
                LocalizationPack localizationPack = LocalizationManager.LoadPack(Main.modPath + Path.DirectorySeparatorChar + "Localization" + Path.DirectorySeparatorChar + LocalizationManager.CurrentLocale + ".json", LocalizationManager.CurrentLocale);
                LocalizationPack currentPack = LocalizationManager.CurrentPack;
                if (localizationPack != null && currentPack != null)
                {
                    currentPack.AddStrings(localizationPack);
                    Comment.Log("Applied localization for the " + LocalizationManager.CurrentLocale + "locale.");
                }
                else Comment.Error("Failed to apply the localization. LocalizationPack is null? " + (localizationPack is null) + ". CurrentPack is null?" + (currentPack is null) + ".");
            }
            catch
            {
                Comment.Error("Failed to apply the localization");
            };
        }

        
    }
    //[HarmonyPatch(typeof(OwlcatModification), nameof(OwlcatModification.TryLoadBundle))]
    //public static class DumbBundleFix
    //{
    //    [HarmonyPrefix]
    //    public static bool Fix(OwlcatModification __instance, string bundleName, ref AssetBundle __result)
    //    {
    //        if (!__instance.Settings.BundlesLayout.GuidToBundle.Values.Contains(bundleName))
    //            __result = null;
    //        else __result = __instance.LoadBundle(bundleName);
    //        return false;
    //    }
    //}
}
