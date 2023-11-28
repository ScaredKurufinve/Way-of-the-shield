#undef DEBUG
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.FactLogic;
using System.Reflection;

namespace Way_of_the_shield
{
    [HarmonyPatch]
    public static class MechanicsFeatureExtension
    {
        public const AddMechanicsFeature.MechanicsFeatureType UnhinderingShield = (AddMechanicsFeature.MechanicsFeatureType)3000;
        public const AddMechanicsFeature.MechanicsFeatureType TowerShieldDefense = (AddMechanicsFeature.MechanicsFeatureType)3001;
        public const AddMechanicsFeature.MechanicsFeatureType ForceOneHanded = (AddMechanicsFeature.MechanicsFeatureType)3002;
        public const AddMechanicsFeature.MechanicsFeatureType BucklerBash = (AddMechanicsFeature.MechanicsFeatureType)3003;
        public const AddMechanicsFeature.MechanicsFeatureType ShieldDenied = (AddMechanicsFeature.MechanicsFeatureType)3004;
        public const AddMechanicsFeature.MechanicsFeatureType ForceDualWieldingPenalties = (AddMechanicsFeature.MechanicsFeatureType)3005;
        public const AddMechanicsFeature.MechanicsFeatureType ImprovedShieldBash = (AddMechanicsFeature.MechanicsFeatureType)3006;

        const int Length = 7;
        static readonly FieldInfo[] flagArray = new FieldInfo[Length]
        {
            typeof(MechanicsFeatureExtensionPart).GetField(nameof(MechanicsFeatureExtensionPart.UnhinderingShield)),
            typeof(MechanicsFeatureExtensionPart).GetField(nameof(MechanicsFeatureExtensionPart.TowerShieldBrace)),
            typeof(MechanicsFeatureExtensionPart).GetField(nameof(MechanicsFeatureExtensionPart.ForceOneHanded)),
            typeof(MechanicsFeatureExtensionPart).GetField(nameof(MechanicsFeatureExtensionPart.BucklerBash)),
            typeof(MechanicsFeatureExtensionPart).GetField(nameof(MechanicsFeatureExtensionPart.ShieldDenied)),
            typeof(MechanicsFeatureExtensionPart).GetField(nameof(MechanicsFeatureExtensionPart.ForceDualWieldingPenalties)),
            typeof(MechanicsFeatureExtensionPart).GetField(nameof(MechanicsFeatureExtensionPart.ImprovedShieldBash)),
        };



        public class MechanicsFeatureExtensionPart : EntityPart
        {
            
            public CountableFlag UnhinderingShield = new();
            public CountableFlag TowerShieldBrace = new();
            public CountableFlag ForceOneHanded = new();
            public CountableFlag BucklerBash = new();
            public CountableFlag ShieldDenied = new();
            public CountableFlag ForceDualWieldingPenalties = new();
            public CountableFlag ImprovedShieldBash = new();
        }


        [HarmonyPatch(typeof(AddMechanicsFeature.Runtime), nameof(AddMechanicsFeature.Runtime.GetFeature))]
        [HarmonyPrefix]
        public static bool GetMechanicsFeature_Prefix(ref CountableFlag __result, UnitEntityData unit, AddMechanicsFeature.MechanicsFeatureType type)
        {

#if DEBUG
            if (Settings.Debug.GetValue()) Comment.Log("Entered AddMechanicsFeature.Runtime.GetFeature prefix"); 
#endif
            if (type is < UnhinderingShield or >= UnhinderingShield + Length) return true;
            __result = flagArray[(int)type - 3000].GetValue(unit.Ensure<MechanicsFeatureExtensionPart>()) as CountableFlag;
#if DEBUG
            if (Settings.Debug.GetValue()) Comment.Log("Mechanics feature = " + (int)type); 
#endif
            return __result is null;


        }


    }
}
