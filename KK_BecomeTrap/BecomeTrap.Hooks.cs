using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ActionGame;
using ActionGame.Chara;
using ActionGame.Communication;
using BepInEx;
using BepInEx.Logging;
using Harmony;
using Manager;

namespace KK_BecomeTrap
{
    public partial class BecomeTrap
    {
        private static class Hooks
        {
            private static BecomeTrapController GetController(Player player)
            {
                return player?.chaCtrl?.gameObject.GetComponent<BecomeTrapController>();
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Player), nameof(Player.LoadAnimator))]
            public static bool LoadAnimatorPrefix(Player __instance)
            {
                var ctrl = GetController(__instance);

                if (ctrl != null && ctrl.IsTrap)
                {
                    // If the player is a trap, load alternative animations
                    try
                    {
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

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ActionMap), nameof(ActionMap.Change), new[] { typeof(int), typeof(Scene.Data.FadeType) })]
            public static void MapChangePostfix(ActionMap __instance)
            {
                // Mark all maps as safe to be in (so we don't get kicked out) if the character is a trap
                __instance.StartCoroutine(MapChangeCo(__instance));
            }

            private static IEnumerator MapChangeCo(ActionMap instance)
            {
                BecomeTrapController ctrl;
                do
                {
                    yield return null;
                    ctrl = GetController(FindObjectOfType<Player>());
                }
                while (ctrl == null);

                if (ctrl.IsTrap)
                {
                    foreach (var param in instance.infoDic.Values)
                        param.isWarning = false;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Info), "GetListCommand", new[] { typeof(int), typeof(Info.Group), typeof(int) })]
            public static void GetListCommandPostfix(Info __instance, ref List<Info.BasicInfo> __result, int _stage, Info.Group _group, int _command)
            {
                // Make sure we are called from ActionGame.Communication.Info.GetIntroductionADV
                // Calling only StackTrace would be enough but this is much faster for most calls
                if (_stage >= 2 || _group != Info.Group.Introduction || _command != 0) return;

                var player = Game.Instance?.actScene?.Player;
                if (player == null) return;

                var controller = GetController(player);
                // Only applicable if the player is a trap
                if (controller == null || !controller.IsTrap) return;

                if (!new StackTrace().ToString().Contains("ActionGame.Communication.Info.GetIntroductionADV")) return;

                // Remove events that cause the girl to refuse to talk because you're tresspassing
                __result.RemoveAll(x => x.conditions == 1 || x.conditions == 3 || x.conditions == 32);
            }
        }
    }
}
