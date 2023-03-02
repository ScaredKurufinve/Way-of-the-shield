using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;

namespace Way_of_the_shield.NewFeatsAndAbilities
{
    [HarmonyPatch]
    public static class OneHandedToggle
    {
        public static Sprite icon = LoadIcon("Icon_OneHandedToggle");
        [HarmonyPrepare]
        public static bool Prepare()
        {
            if ( Main.UMM is null || Main.TTTBase is null) return true;
            return !Main.CheckForModEnabled("TabletopTweaks-Base");
        }

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPostfix]
        [HarmonyAfter("TabletopTweaks-Base")]
        public static void CachePatch_Postfix()
        {

            BlueprintFeature FightDefensivelyFeature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("ca22afeb94442b64fb8536e7a9f7dc11");

#region Create OneHanded buff
            BlueprintBuff OneHandedBuff = new()
            {
                name = "OneHandedBuff",
                AssetGuid = new BlueprintGuid(new Guid("549016f34f4349fdb6b20ecb11e79a73")),
                m_DisplayName = new LocalizedString() { Key = "OneHandedBuff_DisplayName" },
                m_Description = new LocalizedString() { Key = "OneHandedBuff_Description" },
                m_Icon = icon,
                FxOnRemove = new()
            };
            OneHandedBuff.AddComponent(new AddMechanicsFeature() { m_Feature = MechanicsFeatureExtension.ForceOneHanded });
            OneHandedBuff.AddToCache();
#endregion
#region Create OneHanded ability
            BlueprintActivatableAbility OneHandedAbility = new()
            {
                name = "OneHandedAbility",
                AssetGuid = new BlueprintGuid(new Guid("15542aa4e3f542a4bbd66bcb13392798")),
                m_DisplayName = new LocalizedString() { Key = "OneHandedAbility_DisplayName" },
                m_Description = new LocalizedString() { Key = "OneHandedAbility_Description" },
                m_Buff = OneHandedBuff.ToReference<BlueprintBuffReference>(),
                IsOnByDefault = false,
                DoNotTurnOffOnRest = true,
                DeactivateImmediately = true,
                m_Icon = icon
            };
            OneHandedAbility.AddToCache();
#endregion
#region Create OneHanded Feature
            BlueprintFeature OneHandedFeature = new()
            {
                name = "OneHandedFeature",
                AssetGuid = new BlueprintGuid(new Guid("21a2cfc0fc894dd9ac1ab49e4c35ba38")),
                m_DisplayName = new LocalizedString() { Key = "OneHandedFeature_DisplayName" },
                m_Description = new LocalizedString() { Key = "OneHandedFeature_Description" },
                IsClassFeature = true,
                HideInUI = true,
                ReapplyOnLevelUp = true,
                Ranks = 1,
            };
            OneHandedFeature.AddComponent(new AddFacts() { m_Facts = new BlueprintUnitFactReference[] { OneHandedAbility.ToReference<BlueprintUnitFactReference>() } });
            OneHandedFeature.AddToCache();
#endregion
            var AddFacts = FightDefensivelyFeature.GetComponent<AddFacts>();
            if (!AddFacts.m_Facts.Any(x => x.Guid == OneHandedFeature.AssetGuid))
                AddFacts.m_Facts = AddFacts.m_Facts.Append(OneHandedFeature.ToReference<BlueprintUnitFactReference>()).ToArray();
        }


        [HarmonyPatch(typeof(ItemEntityWeapon), "HoldInTwoHands", MethodType.Getter)]
        [HarmonyPostfix]
        static void ItemEntityWeapon_HoldInTwoHands_Postfix(ItemEntityWeapon __instance, ref bool __result)
        {
            if (__result == false || __instance.Wielder is null) return;
            if (__instance.Wielder.Unit.Get<MechanicsFeatureExtension.MechanicsFeatureExtensionPart>()?.ForceOneHanded) __result = false;
        }

    }
}
