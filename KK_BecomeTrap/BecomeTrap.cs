using System;
using System.Collections.Generic;
using System.Linq;
using ActionGame;
using ActionGame.Chara;
using BepInEx;
using BepInEx.Logging;
using Harmony;
using MakerAPI.Chara;
using Manager;
using Sideloader;

namespace KK_BecomeTrap
{
    [BepInPlugin(GUID, "Koikatsu: Become Trap", Version)]
    [BepInDependency(Sideloader.Sideloader.GUID)]
    public class BecomeTrap : BaseUnityPlugin
    {
        public const string GUID = "marco.becometrap";
        internal const string Version = "1.0";
        
        private static BecomeTrapController GetController(Player player)
        {
            return player.chaCtrl?.gameObject.GetComponent<BecomeTrapController>();
        }

        private void Start()
        {
            HarmonyInstance.Create(GUID).PatchAll(typeof(BecomeTrap));

            CharacterApi.RegisterExtraBehaviour<BecomeTrapController>(GUID);

            var manifests = AccessTools.Field(typeof(Sideloader.Sideloader), "LoadedManifests").GetValue(GetComponent<Sideloader.Sideloader>()) as List<Manifest>;
            if(manifests == null || manifests.All(x => x.GUID != GUID))
                ShowZipmodError();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), nameof(Player.LoadAnimator))]
        public static bool LoadAnimatorPrefix(Player __instance)
        {
            Logger.Log(LogLevel.Message, "LoadAnimatorPrefix");

            var ctrl = GetController(__instance);

            if (ctrl != null && ctrl.IsTrap)
            {
                try
                {
                    Logger.Log(LogLevel.Message, "ReloadAssets");
                    __instance.motion.bundle = "action/animator/TrapAnimations.unity3d";
                    __instance.motion.asset = "player";
                    __instance.motion.LoadAnimator(__instance.animator);

                    return false;
                }
                catch (Exception ex)
                {
                    ShowZipmodError();
                    Logger.Log(LogLevel.Error, ex);
                }
            }

            return true;
        }

        private static void ShowZipmodError()
        {
            Logger.Log(LogLevel.Error | LogLevel.Message, "[BecomeTrap] Failed to load the animations! Make sure you have KK_BecomeTrap.zipmod in your mods folder!");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ActionMap), nameof(ActionMap.Change), new[] {typeof(int), typeof(Scene.Data.FadeType)})]
        public static void MapChangePostfix(ActionMap __instance)
        {
            var ctrl = GetController(FindObjectOfType<Player>());
            if (ctrl.IsTrap)
            {
                foreach (var param in __instance.infoDic.Values)
                    param.isWarning = false;
            }
        }
    }
}
