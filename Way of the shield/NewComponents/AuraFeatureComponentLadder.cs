using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using static Way_of_the_shield.Main;

namespace Way_of_the_shield.NewComponents
{
    public class AuraFeatureComponentLadder : AuraFeatureComponent
    {
        public BlueprintBuffReference[] m_Buffs;
        public BlueprintFeatureReference m_featureToCheck;
        public bool UseDuration = false;
        public ContextDurationValue Duration;
        
        public BlueprintBuff TrueBuff
        {
            get
            {
                int Length = m_Buffs.Length;
                if (Length == 0) return base.Buff;
                int Rank;
                BlueprintFeature bf = m_featureToCheck?.Get() ;
                if (bf != null)
                {
                    UnitEntityData caster = Fact.MaybeContext?.MaybeCaster ?? Fact.Owner;
                    if (caster is null)
                    {
                        Comment.Warning("Fact {0} on unit {1} is trying to call for aura ladder based on feature {2}, but the fact has no caster.",
                            Fact.Blueprint.name, Owner.CharacterName, bf.name);
                        return null;
                    }
                    Feature f = caster?.Progression.Features.GetFact(bf);
                    if (f is not null) Rank = f.GetRank();
                    else
                    {
                        Comment.Warning("Fact {0} on unit {1} is trying to call for aura ladder, but feature {2} is absent.",
                            Fact.Blueprint.name, Owner.CharacterName, bf.name);
                        return null;
                    }
                }
                else Rank = Fact.GetRank();
                if (Rank < 0) return Buff;
                if (Rank > Length) Rank = Length;
                BlueprintBuff result;
                do 
                {
                    Rank--;
                    if (Rank < 0) return Buff;
                    result = m_Buffs[Rank]?.Get(); 
                }
                while (result is null);
                return result;
            }
        }
        public override void OnActivate()
        {
            
            if (TrueBuff is not null)
                Data.AppliedBuff = Owner.AddBuff(TrueBuff, Fact.MaybeContext, (UseDuration ? ( Fact.MaybeContext is not null ?  Duration?.Calculate(Fact.MaybeContext).Seconds : null) : null));
        }
    }
}
