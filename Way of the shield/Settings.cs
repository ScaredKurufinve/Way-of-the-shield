#undef Dynamic
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Modding;
using Kingmaker.Settings;
using Kingmaker.UI.SettingsUI;
using UnityEngine;
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
        public static SettingsEntityBool AllowEquipNonProfficientItems                   ;
        public static SettingsEntityBool ForbidCloseFlanking                             ;
        public static SettingsEntityBool AllowCloseFlankingToEnemies                     ;
        public static SettingsEntityBool ConcealmentAttackBonusOnBackstab                ;
        public static SettingsEntityBool DenyShieldBonusOnBackstab                       ;
        public static SettingsEntityBool FlatFootedOnBackstab                            ;
        public static SettingsEntityBool AddBucklerParry                                 ;
        public static SettingsEntityBool AllowTwoHanded_as_OneHandedWhenBuckler          ;
        public static SettingsEntityBool AllowBucklerBashWhenTwoHandedWithUnhindering    ;
        public static SettingsEntityBool AllowTwoHandedSpears_as_OneHandedWhenMounted    ;
        public static SettingsEntityBool GiveImmediateRepositioningToTSS                 ;
        public static SettingsEntityBool RemoveTotalCoverFeatureFromTSS                  ;
        public static SettingsEntityBool AllowShieldBashToAllWhoProficient               ;
        public static SettingsEntityBool EnableSoftCover                                 ;
        public static SettingsEntityBool ChangeBackToBack                                ;
        public static SettingsEntityBool AddSoftCoverDenialToImprovedPreciseShot         ;
        public static SettingsEntityBool ChangeShieldWall                                ;
        public static SettingsEntityBool BuffSacredShieldEnhacementArray                 ;
        public static SettingsEntityBool ChangeShieldSpell                               ;
        public static SettingsEntityBool RemoveBucklerProficiencies                      ;
        public static SettingsEntityBool FixRapiers                                      ;
#if DEBUG
        public static SettingsEntityBool Debug = new(settingsModName + "debug", false); 
#endif
        #endregion
        static UISettingsGroup SettingsGroup = ScriptableObject.CreateInstance<UISettingsGroup>();
        static internal List<(SettingsEntityBool, UISettingsEntityBool)> ListOfBoolSettings;


        static public void Init()
        {
            DoInit();
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

        static void DoInit()
        {
            AllowEquipNonProfficientItems = new(settingsModName + "allow-equip-non-profficient-items", true, false, true);
            ForbidCloseFlanking = new(settingsModName + "forbid-close-flanking", true, false, true);
            AllowCloseFlankingToEnemies = new(settingsModName + "allow-close-flanking-to-enemies", true, false, false);
            ConcealmentAttackBonusOnBackstab = new(settingsModName + "concealment-attack-bonus-on-backstab", true, false, false);
            DenyShieldBonusOnBackstab = new(settingsModName + "deny-shield-bonus-on-backstab", true, false, false);
            FlatFootedOnBackstab = new(settingsModName + "flat-footed-on-backstab", true, true, true);
            AddBucklerParry = new(settingsModName + "add-buckler-parry", true, false, true);
            AllowTwoHanded_as_OneHandedWhenBuckler = new(settingsModName + "allow-two-handed-as-one-handed-when-buckler", true, false, true);
            AllowBucklerBashWhenTwoHandedWithUnhindering = new(settingsModName + "allow-buckler-bash-when-two-handed-with-unhindering", false, false, false);
            AllowTwoHandedSpears_as_OneHandedWhenMounted = new(settingsModName + "allow-two-handed-spears-as-one-handed-when-mounted", true, false, true);
            GiveImmediateRepositioningToTSS = new(settingsModName + "give-immediate-repositioning-to-tss", true, false, true);
            RemoveTotalCoverFeatureFromTSS = new(settingsModName + "remove-total-cover-feature-from-tss", true, false, true);
            AllowShieldBashToAllWhoProficient = new(settingsModName + "allow-shield-bash-without-improved-feature", true, false, true);
            EnableSoftCover = new(settingsModName + "enable-soft-sover", true, false, true);
            ChangeBackToBack = new(settingsModName + "change-back-to-back", true, false, true);
            AddSoftCoverDenialToImprovedPreciseShot = new(settingsModName + "add-soft-cover-denial-to-improved-precise-shot", true, false, true);
            ChangeShieldWall = new(settingsModName + "change-shield-wall", true, false, true);
            BuffSacredShieldEnhacementArray = new(settingsModName + "buff-sacred-shield-enhacement-array", true, false, true);
            ChangeShieldSpell = new(settingsModName + "change-shield-spell", true, false, true);
            RemoveBucklerProficiencies = new(settingsModName + "remove-buckler-proficiencies", true, false, true);
            FixRapiers = new(settingsModName + "fix-rapiers", true, true, false);

            ListOfBoolSettings = new()
            {
                 new(AllowEquipNonProfficientItems, ScriptableObject.CreateInstance<UISettingsEntityBool>()),
                 new(ForbidCloseFlanking, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(AllowCloseFlankingToEnemies, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(ConcealmentAttackBonusOnBackstab, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(FlatFootedOnBackstab, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(DenyShieldBonusOnBackstab, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
                 new(ChangeBackToBack, ScriptableObject.CreateInstance < UISettingsEntityBool >()),
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
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Default)) 
            {
                foreach (var entity in SettingsGroup.SettingsList) 
                {
                    if (entity is UISettingsEntityBool b)
                        b.ResetToDefault(false);
                }
            }
            if (GUILayout.Button(Apply)) { foreach (var (entity, _) in ListOfBoolSettings) entity.ConfirmTempValue(); SettingsController.SaveAll(); }
            if (GUILayout.Button(Cancel)) foreach (var (entity, _) in ListOfBoolSettings) entity.RevertTempValue();
            GUILayout.EndHorizontal();

            foreach (var (entity, visual) in ListOfBoolSettings)
            {
                
                GUILayout.BeginHorizontal();
                GUILayout.Label(visual.Description);
                GUILayout.FlexibleSpace();
                GUILayout.Label(visual.TooltipDescription, GUILayout.Width(width));
                bool temp = GUILayout.Toggle(entity.GetTempValue(), "");
                if (temp != entity.m_TempValue) entity.SetTempValue( temp );
                GUILayout.EndHorizontal(); 
            }
            //GUILayout.EndArea();
        }
    }


}
