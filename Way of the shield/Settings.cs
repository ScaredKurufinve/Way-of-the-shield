#undef Dynamic
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Modding;
using Kingmaker.Settings;
using Kingmaker.UI.SettingsUI;
using UnityEngine;
using static UnityEngine.GUILayout;
#if !Dynamic
using UnityModManagerNet;
#endif

namespace Way_of_the_shield
{
    [HarmonyPatch]
    public class Settings
    {
        internal const string settingsModName = "way-of-the-shield.";
        public static bool initialized = false;


        #region Setting Entities
        public static SettingsEntityBool AllowEquipNonProfficientItems                  = new (settingsModName + "allow-equip-non-profficient-items", true, false, true);
        public static SettingsEntityBool CheckForShieldLightness                        = new (settingsModName + "check-for-shield-lightness", true, false, true);
        public static SettingsEntityBool ForbidCloseFlanking                            = new (settingsModName + "forbid-close-flanking", true, false, true);
        public static SettingsEntityBool AllowCloseFlankingToEnemies                    = new (settingsModName + "allow-close-flanking-to-enemies", true, false, false);
        public static SettingsEntityBool ConcealmentAttackBonusOnBackstab               = new (settingsModName + "concealment-attack-bonus-on-backstab", true, false, false);
        public static SettingsEntityBool DenyShieldBonusOnBackstab                      = new (settingsModName + "deny-shield-bonus-on-backstab", true, false, false);
        public static SettingsEntityBool FlatFootedOnBackstab                           = new (settingsModName + "flat-footed-on-backstab", true, true, true);
        public static SettingsEntityBool AddBucklerParry                                = new (settingsModName + "add-buckler-parry", true, false, true);
        public static SettingsEntityBool AddShieldedDefense                             = new (settingsModName + "add-shielded-defense", true, false, true);
        public static SettingsEntityBool AllowTwoHanded_as_OneHandedWhenBuckler         = new (settingsModName + "allow-two-handed-as-one-handed-when-buckler", true, false, true);
        public static SettingsEntityBool AllowBucklerBashWhenTwoHandedWithUnhindering   = new (settingsModName + "allow-buckler-bash-when-two-handed-with-unhindering", false, false, false);
        public static SettingsEntityBool AllowTwoHandedSpears_as_OneHandedWhenMounted   = new (settingsModName + "allow-two-handed-spears-as-one-handed-when-mounted", true, false, true);
        public static SettingsEntityBool GiveImmediateRepositioningToTSS                = new (settingsModName + "give-immediate-repositioning-to-tss", true, false, true);
        public static SettingsEntityBool RemoveTotalCoverFeatureFromTSS                 = new (settingsModName + "remove-total-cover-feature-from-tss", true, false, true);
        public static SettingsEntityBool AllowShieldBashToAllWhoProficient              = new (settingsModName + "allow-shield-bash-without-improved-feature", true, false, true);
        public static SettingsEntityBool EnableSoftCover                                = new (settingsModName + "enable-soft-sover", true, false, true);
        public static SettingsEntityBool ChangeBackToBack                               = new (settingsModName + "change-back-to-back", true, false, true);
        public static SettingsEntityBool AddSoftCoverDenialToImprovedPreciseShot        = new (settingsModName + "add-soft-cover-denial-to-improved-precise-shot", true, false, true);
        public static SettingsEntityBool ChangeShieldWall                               = new (settingsModName + "change-shield-wall", true, false, true);
        public static SettingsEntityBool BuffSacredShieldEnhacementArray                = new (settingsModName + "buff-sacred-shield-enhacement-array", true, false, true);
        public static SettingsEntityBool ChangeShieldSpell                              = new (settingsModName + "change-shield-spell", true, false, true);
        public static SettingsEntityBool RemoveBucklerProficiencies                     = new (settingsModName + "remove-buckler-proficiencies", true, false, true);
        public static SettingsEntityBool FixRapiers                                     = new (settingsModName + "fix-rapiers", true, true, false);
#if DEBUG
        public static SettingsEntityBool Debug = new(settingsModName + "debug", false); 
#endif
        #endregion
        static UISettingsGroup SettingsGroup = ScriptableObject.CreateInstance<UISettingsGroup>();
        static internal List<(SettingsEntityBool, UISettingsEntityBool)> ListOfBoolSettings = new()
            {
                 new(AllowEquipNonProfficientItems, ScriptableObject.CreateInstance<UISettingsEntityBool>()),
                 new(CheckForShieldLightness, ScriptableObject.CreateInstance<UISettingsEntityBool>()),
                 new(ForbidCloseFlanking, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(AllowCloseFlankingToEnemies, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(ConcealmentAttackBonusOnBackstab, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(FlatFootedOnBackstab, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(DenyShieldBonusOnBackstab, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(ChangeBackToBack, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(AddShieldedDefense, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(AddBucklerParry, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(AllowTwoHanded_as_OneHandedWhenBuckler, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(AllowBucklerBashWhenTwoHandedWithUnhindering, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(AllowTwoHandedSpears_as_OneHandedWhenMounted, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(GiveImmediateRepositioningToTSS, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(RemoveTotalCoverFeatureFromTSS, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(AllowShieldBashToAllWhoProficient, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(EnableSoftCover, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(AddSoftCoverDenialToImprovedPreciseShot, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(ChangeShieldWall, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(BuffSacredShieldEnhacementArray, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(ChangeShieldSpell, ScriptableObject.CreateInstance<UISettingsEntityBool>()),
                 new(RemoveBucklerProficiencies, ScriptableObject.CreateInstance<UISettingsEntityBool>()),
                 new(FixRapiers, ScriptableObject.CreateInstance<UISettingsEntityBool>()),

                 #if DEBUG
                 new(Debug, ScriptableObject.CreateInstance<UISettingsEntityBool>()),  
                 #endif
        
            };


        static public void Init()
        {
            AddSettingsNames();
            List<UISettingsEntityBase> l = new();
            foreach (var (entity, visual) in ListOfBoolSettings) { visual.LinkSetting(entity); l.Add(visual); };
            SettingsGroup.SettingsList = l.ToArray();
#if DEBUG
            //Enable(Debug);
#endif

            if (Main.mod is OwlcatModification owlmod)
            {
                owlmod.OnGUI = CreateUnityGUISettings;
            }
            else
            {
#if !Dynamic
                if (Main.mod is UnityModManager.ModEntry mod) CreateUMMSettings(); 
            }
#endif
#if Dynamic

                try
                {
                    CreateUMMSettings();
                }
                catch(Exception ex)
                {
                    Exception e = ex;
                    while (e is not null)
                    {
                        Comment.Log(e.ToString());
                        e = e.InnerException;
                    }
                }
                
            }
#endif

            #region Add to ModMenu
            if (Main.ModMenu is null) goto skipModMenu;
            MethodInfo Wolfie = null;
            IEnumerable<Type> wolfies = Main.ModMenu.GetTypes().Where(tx => tx.Name.Contains("ModMenu"));
#if DEBUG
            if (Debug)
                Comment.Log("There are {0} wolfies", wolfies.Count()); 
#endif
            foreach (Type t in wolfies)
            {
                //Comment.Log("Cheking the {0} type.", t.Name);
                Wolfie = t.GetMethod(name: "AddSettings", types: new Type[] { typeof(UISettingsGroup) });
                if (Wolfie is not null) break;
            }
            if (Wolfie is null) Comment.Log("Did not find the ModMenu.AddSettings");
            else Wolfie.Invoke(null, new object[] { SettingsGroup });
            skipModMenu:
            #endregion

            initialized = true;
        }


        static public void Enable(SettingsEntityBool SettingName)
        {
            SettingName.SetValueAndConfirm(true);
        }

        static public void Disable(SettingsEntityBool SettingName)
        {
            SettingName.SetValueAndConfirm(false);
        }

        static void CreateUMMSettings()
        {

#if !Dynamic
            (Main.mod as UnityModManager.ModEntry).OnGUI = new(x => CreateUnityGUISettings());
#endif
#if Dynamic
            Main.mod.OnGUI = new(x => CreateUnityGUISettings());
#endif
        }
        static LocalizedString Empty = new() { m_Key = "Empty" };
        static LocalizedString Default = Empty;
        static LocalizedString Apply = Empty;
        static LocalizedString Cancel = Empty;
        static void AddSettingsNames()
        {
            foreach (var (entity, visual) in ListOfBoolSettings)
            {
                visual.m_Description = new() { Key = entity.Key + "_Description" };
                visual.m_TooltipDescription = new() { Key = entity.Key + "_TooltipDescription" };
            }
            Default = new LocalizedString() { Key = settingsModName + "SettingButton_" + "Default" };
            Apply = new LocalizedString() { Key = settingsModName + "SettingButton_" + "Apply" };
            Cancel = new LocalizedString() { Key = settingsModName + "SettingButton_" + "Cancel" };
            SettingsGroup.Title = new() { m_Key = "WayOfTheShield_SettingsGroup_Title" };
        }
        

        static void CreateUnityGUISettings()
        {
            float width = 180;
            Camera main = Camera.main;
            if (main is not null)
            {
                width = main.pixelHeight *5f / 6f;
            }
            BeginHorizontal();
            FlexibleSpace(); 
            if (Button(Default, Width(width *2 / 5))) 
            {
                foreach (var entity in SettingsGroup.SettingsList) 
                {
                    if (entity is UISettingsEntityBool b)
                        b.ResetToDefault(false);
                }
            }
            Space(15);
            if (Button(Apply, Width(width * 2 / 5))) { foreach (var (entity, _) in ListOfBoolSettings) entity.ConfirmTempValue(); SettingsController.SaveAll(); }
            Space(15);
            if (Button(Cancel, Width(width * 2 / 5))) foreach (var (entity, _) in ListOfBoolSettings) entity.RevertTempValue();
            FlexibleSpace();
            EndHorizontal();

            Space(5);
            foreach (var (entity, visual) in ListOfBoolSettings)
            {
                
                BeginHorizontal();
                Label(visual.Description, Width(width *2 /5));
                Space(5);
                Label(visual.TooltipDescription);
                FlexibleSpace();
                bool temp = Toggle(entity.GetTempValue(), "");
                if (temp != entity.m_TempValue) entity.SetTempValue( temp );
                EndHorizontal(); 
            }
            //GUILayout.EndArea();
        }
    }


}
