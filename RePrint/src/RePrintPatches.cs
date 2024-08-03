using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Klei.CustomSettings;
using KMod;
using UnityEngine;

// ReSharper disable InconsistentNaming
namespace RePrint
{
    public class RePrintPatches : UserMod2
    {
        private static KButton MakeReshuffleAllButton(KButton rejectBtnPrefab, KButton confirmBtnPrefab)
        {
            var reshuffleAllBtn = Util.KInstantiateUI<KButton>(
                rejectBtnPrefab.gameObject, confirmBtnPrefab.transform.parent.gameObject, true);
            reshuffleAllBtn.transform.SetAsFirstSibling();
            reshuffleAllBtn.GetComponentInChildren<LocText>().text = "Reshuffle All";
            var image = reshuffleAllBtn.GetComponent<KImage>();
            image.colorStyleSetting = confirmBtnPrefab.GetComponent<KImage>().colorStyleSetting;
            image.ApplyColorStyleSetting();
            return reshuffleAllBtn;
        }

        private static void ReshuffleAllContainers(ImmigrantScreen immigrantScreen)
        {
            var containers = (List<ITelepadDeliverableContainer>)Traverse.Create(immigrantScreen).Field(
                "containers").GetValue();
            foreach (var container in containers)
                switch (container)
                {
                    case CharacterContainer characterContainer when characterContainer != null:
                        ReshuffleContainer(characterContainer);
                        break;
                    case CarePackageContainer carePackageContainer when carePackageContainer != null:
                        ReshuffleContainer(carePackageContainer);
                        break;
                }
        }

        private static void ReshuffleContainer(ITelepadDeliverableContainer container)
        {
            Traverse.Create(container).Field("controller").Method("RemoveLast").GetValue();
            var reshuffleParams = new object[] { false };
            MethodInfo reshuffle = null;
            switch (container)
            {
                case CharacterContainer characterContainer when characterContainer != null:
                    reshuffle = typeof(CharacterContainer).GetMethod("Reshuffle", BindingFlags.Public | BindingFlags.Instance);
                    break;
                case CarePackageContainer carePackageContainer when carePackageContainer != null:
                    reshuffle = typeof(CarePackageContainer).GetMethod("Reshuffle", BindingFlags.NonPublic | BindingFlags.Instance);
                    break;
            }
            if (reshuffle != null) reshuffle.Invoke(container, reshuffleParams);
        }

        private static void ReBindReshuffleButton(ITelepadDeliverableContainer container)
        {
            var reshuffleButton = Traverse.Create(container).Field("reshuffleButton");
            reshuffleButton.Field("onClick").SetValue((System.Action)(() =>
                ReshuffleContainer(container)));
        }

        private static void InitializeContainers(ImmigrantScreen imScreen)
        {
            var disableProceedButton = imScreen.GetType().GetMethod("DisableProceedButton",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (disableProceedButton != null) disableProceedButton.Invoke(imScreen, null);
            var _containers = (List<ITelepadDeliverableContainer>)Traverse.Create(imScreen).Field(
                "containers").GetValue();
            if (_containers != null && _containers.Count > 0) return;
            var _containerParent = (GameObject)Traverse.Create(imScreen).Field("containerParent").GetValue();
            var _containerPrefab = (CharacterContainer)Traverse.Create(imScreen).Field("containerPrefab").GetValue();
            var _carePackageContainerPrefab = (CarePackageContainer)Traverse.Create(imScreen).Field(
                "carePackageContainerPrefab").GetValue();
            imScreen.OnReplacedEvent = null;
            var containers = new List<ITelepadDeliverableContainer>();
            var numberOfDuplicantOptions = 2;  // Todo: make this configurable via PLib integration
            var numberOfCarePackageOptions = 2;
            if (CustomGameSettings.Instance.GetCurrentQualitySetting(CustomGameSettingConfigs.CarePackages).id !=
                "Enabled")
            {
                numberOfCarePackageOptions = 0;
                numberOfDuplicantOptions = 4;
            }
            
            for (var index = 0; index < numberOfDuplicantOptions; ++index)
            {
                var characterContainer = Util.KInstantiateUI<CharacterContainer>(
                    _containerPrefab.gameObject, _containerParent);
                characterContainer.SetController(imScreen);
                containers.Add(characterContainer);
            }
            for (var index = 0; index < numberOfCarePackageOptions; ++index)
            {
                var packageContainer = Util.KInstantiateUI<CarePackageContainer>(
                    _carePackageContainerPrefab.gameObject, _containerParent);
                packageContainer.SetController(imScreen);
                containers.Add(packageContainer);
            }
            var selectedDeliverables = new List<ITelepadDeliverable>();
            Traverse.Create(imScreen).Field("selectedDeliverables").SetValue(selectedDeliverables);
            Traverse.Create(imScreen).Field("numberOfDuplicantOptions").SetValue(numberOfDuplicantOptions);
            Traverse.Create(imScreen).Field("numberOfCarePackageOptions").SetValue(numberOfCarePackageOptions);
            Traverse.Create(imScreen).Field("containers").SetValue(containers);
        }

        [HarmonyPatch(typeof(ImmigrantScreen), "OnPrefabInit")]
        public static class ImmigrantScreenOnPrefabInitPatch
        {
            public static void Postfix(ImmigrantScreen __instance, KButton ___rejectButton, KButton ___proceedButton)
            {
                var reshuffleAllBtn = MakeReshuffleAllButton(___rejectButton, ___proceedButton);
                reshuffleAllBtn.onClick += () => ReshuffleAllContainers(__instance);
            }
        }

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

        [HarmonyPatch(typeof(ImmigrantScreen), "Initialize")]
        public static class ImmigrantScreenInitializePatch
        {
            // Enable Reshuffle button for containers
            public static bool Prefix(Telepad telepad, ImmigrantScreen __instance)
            {
                InitializeContainers(__instance);
                var containers = (List<ITelepadDeliverableContainer>)Traverse.Create(__instance).Field(
                    "containers").GetValue();
                foreach (var container in containers)
                    switch (container)
                    {
                        case CharacterContainer characterContainer when characterContainer != null:
                            characterContainer.SetReshufflingState(true);
                            ReBindReshuffleButton(characterContainer);
                            break;
                        case CarePackageContainer carePackageContainer when carePackageContainer != null:
                            carePackageContainer.SetReshufflingState(true);
                            ReBindReshuffleButton(carePackageContainer);
                            break;
                    }

                Traverse.Create(__instance).Field("telepad").SetValue(telepad);
                return false; // skip the original method call
            }
        }
    }
}