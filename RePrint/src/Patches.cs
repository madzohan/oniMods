using HarmonyLib;

namespace RePrint
{
    public class Patches
    {
        [HarmonyPatch(typeof(ImmigrantScreen))]
        [HarmonyPatch("OnSpawn")]
        public class OnSpawnPatch
        // Suppress "rejection confirm"
        {
            public static void Postfix(ImmigrantScreen __instance)
            {
                var actions = Traverse.Create(__instance)
                    .Field("confirmRejectionBtn").Field("onClick").GetValue();
                Traverse.Create(__instance).Field("rejectButton").Field("onClick").SetValue(actions);
            }
        }
        
        [HarmonyPatch(typeof(ImmigrantScreen))]
        [HarmonyPatch("OnRejectionConfirmed")]
        public class OnRejectionConfirmedPatch
        // Following "rejection confirm" actions with a new ImmigrantScreen generated
        {
            public static void Postfix(ImmigrantScreen __instance)
            {
                ImmigrantScreen.InitializeImmigrantScreen(__instance.Telepad);
            }
        }
    }
}
