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
using Kingmaker.EntitySystem.Persistence;
using System.Text;
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
        const string Shield = "Shield";
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
                Comment.SetStackTraceSeverity(LogSeverity.Error);
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
                Comment.SetStackTraceSeverity(LogSeverity.Error);
                modName = mod1.Info.Id;
                modPath = mod1.Path;
#endif

#if Dynamic
                if (BeenLoaded) goto Load;
                Comment = LogChannelFactory.GetOrCreate(mod.Info.DisplayName);
                modName = mod.Info.Id;
                modPath = mod.Path;
#endif
                harmony = new(Shield);
                harmony.Patch(
                    original: typeof(OwlcatModificationsManager).GetMethod(nameof(OwlcatModificationsManager.Start), BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
                    prefix: new HarmonyMethod(typeof(LocalizationPatchForUMM).GetMethod(nameof(LocalizationPatchForUMM.Patch))),
                    postfix: new HarmonyMethod(typeof(Main).GetMethod(nameof(Main.Load1))));
                return;

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
            Comment.Log($"Started loading the mod. Version is {Assembly.GetAssembly(typeof(Main)).GetName().Version}");
            harmony ??= new(Shield);
            harmony.UnpatchAll(Shield);
            #region get assemblies
            allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            UMM = CheckForMod("UnityModManager");
            if (UMM != null)
            {
                TTTCore = CheckForMod("TabletopTweaks-Core");
                TTTBase = CheckForMod("TabletopTweaks-Base");
                ModMenu = CheckForMod("ModMenu");
            }
            Comment.Log($"TTT-Core is {TTTCore?.GetName().Version.ToString() ?? "not found."}");
            Comment.Log($"TTT-Base is {TTTBase?.GetName().Version.ToString() ?? "not found."}");
            Comment.Log($"ModMenu is {ModMenu?.GetName().Version.ToString() ?? "not found."}");
#endregion
            try
            {
                if (initialized == false) Settings.Init();
            }
            catch(TypeLoadException ex)
            {
                Comment.Exception(ex, "F me. The type is " + ex.TypeName);
            }
            //if (mod is UnityModManager.ModEntry modEntry)
            //{


            //};
#if DEBUG
            Harmony.DEBUG = true; 
#endif
            if (!BeenLoaded)
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
            => allAssemblies.Where(ass => ass.GetName().Name.Contains(modName)).FirstOrDefault();
        

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

            return (modEntries is not null && modEntries.Contains(mod => mod.Info.AssemblyName.Contains(modName) && mod.Enabled));
        }


    }

    public static class LocalizationPatchForUMM
    {
        public static void Patch()
        {
            try
            {
                Comment.Log($"Will try to apply localization for the {LocalizationManager.CurrentLocale} locale.");
                string LocalizationFolder = Path.Combine(Main.modPath, "Localization");
                string LocalizationFileName = $"{LocalizationManager.CurrentLocale}.json";
                string LocalizationPath = Path.Combine(LocalizationFolder, LocalizationFileName);
                LocalizationPack localizationPack = LocalizationManager.LoadPack(LocalizationPath, LocalizationManager.CurrentLocale);
                LocalizationPack currentPack = LocalizationManager.CurrentPack;
                if (localizationPack != null && currentPack != null)
                {
                    currentPack.AddStrings(localizationPack);
                    Comment.Log($"Applied localization for the {LocalizationManager.CurrentLocale} locale.");
                }
                else
                {
                    StringBuilder warning = new();
                    warning.AppendLine($"Failed to apply the localization. LocalizationPack is null? {localizationPack is null}. CurrentPack is null? {currentPack is null}.");
                    bool flag = Directory.Exists(LocalizationFolder);
                    warning.AppendLine($"Localization folder does {(flag ? "" : "not ")}exist at {LocalizationFolder}.");
                    if (flag)
                    {
                        bool flag2 = File.Exists(LocalizationPath);
                        warning.AppendLine($"Localization file {LocalizationFileName} does {(flag2 ? "" : "not ")}exist inside the folder.");
                    }
                    Comment.Warning(warning.ToString());
                };
            }
            catch(Exception ex)
            {
                Comment.Exception(ex, "Failed to apply the localization");
            };
        }
    }
}
