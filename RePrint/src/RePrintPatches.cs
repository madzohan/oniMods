using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using KMod;
// using PeterHan.PLib.Core;

namespace RePrint
{
    public class RePrintPatches : UserMod2
    {
        // public override void OnLoad(Harmony harmony)
        // {
        //     var logVersion = false;
        //     Debug.Assert(logVersion = true); // '=', not '=='
        //     PUtil.InitLibrary(logVersion);
        //     base.OnLoad(harmony);
        // }

        [HarmonyPatch(typeof(ImmigrantScreen), "OnSpawn")]
        public static class ImmigrantScreenOnSpawnPatch
        {
            // Suppress "rejection confirm"
            public static void Postfix(ImmigrantScreen __instance)
            {
                var actions = Traverse.Create(__instance)
                    .Field("confirmRejectionBtn").Field("onClick").GetValue();
                Traverse.Create(__instance).Field("rejectButton").Field("onClick").SetValue(actions);
            }
        }

        // [HarmonyPatch(typeof(ImmigrantScreen), "OnRejectionConfirmed")]
        // public static class ImmigrantScreenOnRejectionConfirmedPatch
        // {
        //     // Following "rejection confirm" actions with a new ImmigrantScreen generated
        //     public static void Postfix(ImmigrantScreen __instance)
        //     {
        //         Immigration.Instance.timeBeforeSpawn = 0.0f;
        //         ImmigrantScreen.InitializeImmigrantScreen(__instance.Telepad);
        //     }
        // }

        [HarmonyPatch(typeof(ImmigrantScreen), "Initialize")]
        public static class ImmigrantScreenInitializePatch
        {
            // Enable Reshuffle button for containers
            public static bool Prefix(Telepad telepad, ImmigrantScreen __instance)
            {
                var initializeContainers = typeof(ImmigrantScreen).GetMethod("InitializeContainers",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (initializeContainers != null) initializeContainers.Invoke(__instance, null);

                var containers = (List<ITelepadDeliverableContainer>)Traverse.Create(__instance).Field(
                    "containers").GetValue();
                
                void Reshuffle(ITelepadDeliverableContainer container)
                {
                    var reshuffleParams = new object[] { false };
                    var removeLast = Traverse.Create(container).Field("controller").Method("RemoveLast");
                    removeLast.GetValue();
                    var containerType = container.GetType();
                    var reshuffle = containerType.GetMethod("Reshuffle", 
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    if (reshuffle == null) { return;}
                    reshuffle.Invoke(container, reshuffleParams);
                }
                void ReBindRerollButtonToReshuffle(ITelepadDeliverableContainer container)
                {
                    var reshuffleButton = Traverse.Create(container).Field("reshuffleButton");
                    reshuffleButton.Field("onClick").SetValue((System.Action)(() =>
                        Reshuffle(container)));
                }

                foreach (var container in containers)
                    switch (container)
                    {
                        case CharacterContainer characterContainer when characterContainer != null:
                            characterContainer.SetReshufflingState(true);
                            ReBindRerollButtonToReshuffle(characterContainer);
                            break;
                        case CarePackageContainer carePackageContainer when carePackageContainer != null:
                            carePackageContainer.SetReshufflingState(true);
                            ReBindRerollButtonToReshuffle(carePackageContainer);
                            break;
                    }

                Traverse.Create(__instance).Field("telepad").SetValue(telepad);
                return false; // skip the original method call
            }
        }
    }
}