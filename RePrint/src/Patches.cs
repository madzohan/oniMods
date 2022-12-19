using HarmonyLib;

namespace RePrint
{
    public class Patches
    {
        [HarmonyPatch(typeof(ImmigrantScreen))]
        [HarmonyPatch("OnRejectionConfirmed")]
        public class RejectPatch
        {
            public static void Postfix(ImmigrantScreen __instance)
            {
                ImmigrantScreen.InitializeImmigrantScreen(__instance.Telepad);
            }
        }
    }
}
