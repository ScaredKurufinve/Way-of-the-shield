using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Way_of_the_shield.NewComponents
{
    [TypeId("591abc4700d84d56aec7085f37545add")]
    [AllowedOn(typeof(BlueprintBuff))]
    public class ContextActionRemoveBuffRanksCustom : ContextAction
    {
        public BlueprintBuffReference m_Buff;
        public int min;
        public ContextValue value;
        public bool ToCaster;
        public bool RemoveWhenZero = true;

        public BlueprintBuff Buff
        {
            get
            {
                return m_Buff;
            }
        }

        public override string GetCaption()
        {
            return $"ContextActionRemoveBuffRanksCustom {Buff?.NameSafe()}, ToCaster is {ToCaster}.";
        }
        public override void RunAction()
        {
#if DEBUG
            if (Debug.GetValue())
                Comment.Log("ContextActionRemoveBuffRanksCustom - start"); 
#endif

            MechanicsContext.Data data = ContextData<MechanicsContext.Data>.Current;
            if ((data?.Context) == null)
            {
                Comment.Error(this, "Unable to remove Buff: no context found");
                return;
            }
            UnitEntityData maybeCaster = Context.MaybeCaster;
            UnitEntityData unitEntityData = ToCaster ? maybeCaster : Target.Unit;
            if (unitEntityData == null)
            {
                Comment.Error(this, "Unable to remove Buff: no target found");
                return;
            }
            int v = value.Calculate(data.Context);
#if DEBUG
            if (Debug.GetValue())
                Comment.Log("ContextActionRemoveBuffRanksCustom - Will remove {0} ranks from {1}.", v, unitEntityData?.CharacterName); 
#endif
            Buff[] array = unitEntityData.Buffs.Enumerable.ToArray();
            foreach (Buff buff in array)
            {
                if (buff.Blueprint == Buff)
                {
                    bool isActive = buff.IsActive;
                    if (isActive)
                    {
                        buff.Deactivate();
                    }
                    
                    buff.Rank = Math.Max(buff.Rank - v, min);
                    if (buff.Rank < 1 && RemoveWhenZero)
                    {
                        EntityFactsManager manager = buff.Manager;
                        if (manager != null) manager.Remove(buff, true);
                        else Comment.Warning(this, $"ContextActionRemoveBuffRanksCustom - When removing buff {buff.Blueprint.name} from unit {buff.Owner?.CharacterName} there was no fact manager found.");
                    }
#if DEBUG
                        if (Debug.GetValue())
                            Comment.Log("ContextActionRemoveBuffRanksCustom - Current rank is " + buff.Rank); 
#endif

                    if (isActive)
                    {
                        buff.Activate();
                    }
                    break;
                }
            }
        }
    }
}
