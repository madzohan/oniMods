using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace RePrint
{
    public class Patches
    {
        [HarmonyPatch(typeof(ImmigrantScreen), "OnSpawn")]
        public class ImmigrantScreenOnSpawnPatch
        {
            // Suppress "rejection confirm"
            public static void Postfix(ImmigrantScreen __instance)
            {
                var actions = Traverse.Create(__instance)
                    .Field("confirmRejectionBtn").Field("onClick").GetValue();
                Traverse.Create(__instance).Field("rejectButton").Field("onClick").SetValue(actions);
            }
        }

        [HarmonyPatch(typeof(ImmigrantScreen), "OnRejectionConfirmed")]
        public class ImmigrantScreenOnRejectionConfirmedPatch
        {
            // Following "rejection confirm" actions with a new ImmigrantScreen generated
            public static void Postfix(ImmigrantScreen __instance)
            {
                Immigration.Instance.timeBeforeSpawn = 0.0f;
                ImmigrantScreen.InitializeImmigrantScreen(__instance.Telepad);
            }
        }

        [HarmonyPatch(typeof(ImmigrantScreen), "Initialize")]
        public class ImmigrantScreenInitializePatch
        {
            // Enable Reshuffle button for containers
            public static bool Prefix(Telepad telepad, ImmigrantScreen __instance)
            {
                // this.InitializeContainers()
                var initializeContainers = typeof(ImmigrantScreen).GetMethod("InitializeContainers",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (initializeContainers != null) initializeContainers.Invoke(__instance, null);

                var containers = (List<ITelepadDeliverableContainer>)Traverse.Create(__instance).Field(
                    "containers").GetValue();

                foreach (var container in containers)
                    switch (container)
                    {
                        case CharacterContainer characterContainer when characterContainer != null:
                            characterContainer.SetReshufflingState(true);
                            // Debug.Log("called characterContainer.SetReshufflingState(true)");
                            break;
                        case CarePackageContainer carePackageContainer when carePackageContainer != null:
                            // for some reason reshuffle for care packages doesn't work https://forums.kleientertainment.com/klei-bug-tracker/oni_so/feels-like-carepackagecontainerreshufflebutton-click-bind-is-broken-r38207/
                            // so it is disabled until fixed
                            // carePackageContainer.SetReshufflingState(false);
                            // Debug.Log("called carePackageContainer.SetReshufflingState(true)");
                            break;
                    }

                Traverse.Create(__instance).Field("telepad").SetValue(telepad);
                return false; // skip the original method call
            }
        }
    }
}