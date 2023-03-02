using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Way_of_the_shield
{
    [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
    public class SpellShieldTweaks
    {
        [HarmonyPrepare]
        public static bool Prepare()
        {
            if (ChangeShieldSpell.GetValue()) return true;
            else { Comment.Log("ChangeShieldSpell setting is disabled, patch UnitCombatState_IsFlanked_Patch won't be applied."); return false; };
        }

        [HarmonyPostfix]
        public static void BlueprintsCache_Init_PatchForSpellShieldTweaks()
        {
#if DEBUG
            if (Debug.GetValue())
                Comment.Log("Begin tweaking the shield spell."); 
#endif
            string circ = "when tweaking the shield spell";
            if (!RetrieveBlueprint("9c0fa9b438ada3f43864be8dd8b3e741", out BlueprintBuff MageShieldBuff, "MageShieldBuff", circ)) return;
            if (!RetrieveBlueprint("ef768022b0785eb43a18969903c537c4", out BlueprintAbility MageShield, "MageShield", circ)) return;
            if (!RetrieveBlueprint("f60d0cd93edc65c42ad31e34a905fb2f", out BlueprintSpellList AlchemistSpellList, "AlchemistSpellList", circ)) return;
            AddStatBonus asb = MageShieldBuff.ComponentsArray.FindOrDefault(component => component is AddStatBonus a && a.Descriptor == ModifierDescriptor.Shield) as AddStatBonus;
            asb.Descriptor = ModifierDescriptor.UntypedStackable;
            MageShield.ComponentsArray = MageShield.ComponentsArray.Where(component => !(component is SpellListComponent c && c.m_SpellList.deserializedGuid == BlueprintGuid.Parse("f60d0cd93edc65c42ad31e34a905fb2f") )).ToArray();
            AlchemistSpellList.SpellsByLevel[1].m_Spells.RemoveAll(spell => spell.deserializedGuid == MageShield.AssetGuid);
            LocalizedString desc = new() { m_Key = "MageShield_description", m_ShouldProcess = true };
            MageShield.m_Description = desc;

        }
    }
}
