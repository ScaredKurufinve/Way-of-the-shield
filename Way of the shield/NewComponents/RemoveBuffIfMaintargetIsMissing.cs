using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Way_of_the_shield.NewComponents
{
    [AllowedOn(typeof(BlueprintBuff), false)]
    [TypeId("ad6ac06205f14400989447c987cf3103")]
    public class RemoveBuffIfMaintargetIsMissing : UnitBuffComponentDelegate, IAreaHandler, IGlobalSubscriber, IUnitHandler, IUnitSpawnHandler
    {
        public void OnAreaBeginUnloading()
        {
        }

        // Token: 0x0600C482 RID: 50306 RVA: 0x0032E013 File Offset: 0x0032C213
        public void OnAreaDidLoad()
        {
            if (Context.MainTarget.Unit == null)
            {
                Buff.Remove();
            }
        }

        // Token: 0x0600C483 RID: 50307 RVA: 0x0032E033 File Offset: 0x0032C233
        public void HandleUnitSpawned(UnitEntityData entityData)
        {
        }

        // Token: 0x0600C484 RID: 50308 RVA: 0x0032E035 File Offset: 0x0032C235
        public void HandleUnitDestroyed(UnitEntityData entityData)
        {
            if (Context.MainTarget.Unit == null)
            {
                Buff.Remove();
            }
        }

        public void HandleUnitDeath(UnitEntityData entityData)
        {
            UnitEntityData target = Context.SourceAbilityContext?.ClickedTarget.Unit ?? Context.MainTarget.Unit;
            if (RemoveOnTargetDeath && target == entityData)
            {
                Buff.Remove();
            }
        }

        // Token: 0x0400824D RID: 33357
        public bool RemoveOnTargetDeath = true;
    }
}
