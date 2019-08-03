using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ActionGame;
using ActionGame.Chara;
using ActionGame.Communication;
using BepInEx.Logging;
using Harmony;
using Manager;
using UnityEngine;
using Logger = BepInEx.Logger;
using Motion = Illusion.Game.Elements.EasyLoader.Motion;

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

            /// <summary>
            /// If the player is a trap, load alternative animations
            /// </summary>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Motion), nameof(Motion.LoadAnimator), new[] { typeof(Animator) })]
            public static void LoadAnimatorHook(Motion __instance, Animator animator)
            {
                if (__instance.bundle == "action/animator/00.unity3d" && __instance.asset == "player")
                {
                    Logger.Log(LogLevel.Debug, "[BecomeTrap] Replacing player animations");

                    var playerClips = animator.runtimeAnimatorController.animationClips;

                    // Names of animations to replace stock male animations with, same index as in stock animationClips
                    var replacements = new[]
                    {
                        "mc_m_talk_00_00",
                        "f_tachi_00_08_01",
                        "f_tachi_00_13",
                        "f_cheer_00_02",
                        "m_tachi_00_02",
                        "m_suwari_00_03",
                        "m_Lewd_00_00",
                        "m_Lewd_00_01",
                        "f_suwari_00_11_00_S",
                        "f_cheer_00_01",
                        "m_call_00",
                        // todo Add option to change standing animation
                        "Stand_17_01",
                        "f_aruki_00_00_01",
                        "f_hasiru_00_01_01",
                        "f_syagami_00_02",
                        "mc_m_squat_walk_00_00_01",
                    };

                    if (playerClips.Length != replacements.Length)
                    {
                        Logger.Log(LogLevel.Error, "Player runtimeAnimatorController.animationClips has different size than the replacement list!");
                        return;
                    }

                    var allAssets = __instance.GetAllAssets<Object>();
                    var stockClips = allAssets.OfType<RuntimeAnimatorController>().SelectMany(x => x.animationClips);
                    var overrideClips = allAssets.OfType<AnimatorOverrideController>().SelectMany(
                            controller =>
                            {
                                var list = new List<KeyValuePair<AnimationClip, AnimationClip>>(controller.overridesCount);
                                controller.GetOverrides(list);
                                return list.Select(x => x.Value);
                            });
                    var allClipList = stockClips.Concat(overrideClips).ToList();

                    // Have to make a new override controller because directly changing animationClips array doesn't work
                    var overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);

                    for (var i = 0; i < playerClips.Length; i++)
                        overrideController[playerClips[i]] = allClipList.First(x => x.name == replacements[i]);

                    animator.runtimeAnimatorController = overrideController;

                    __instance.UnloadBundle();
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ActionMap), nameof(ActionMap.Change), new[] { typeof(int), typeof(Scene.Data.FadeType) })]
            public static void MapChangePostfix(ActionMap __instance)
            {
                var inMainGame = Singleton<Game>.IsInstance() && Singleton<Game>.Instance.actScene != null;
                if (!inMainGame) return;

                // Mark all maps as safe to be in (so we don't get kicked out) if the character is a trap
                __instance.StartCoroutine(MapChangeCo(__instance));
            }

            private static IEnumerator MapChangeCo(ActionMap instance)
            {
                BecomeTrapController ctrl = null;

                yield return new WaitUntil(() => (ctrl = GetController(Singleton<Game>.Instance.actScene.Player)) != null);

                if (ctrl.IsTrap)
                {
                    foreach (var param in instance.infoDic.Values)
                        param.isWarning = false;
                }
            }

            /// <summary>
            /// Remove events that cause the girl to refuse to talk because you're tresspassing
            /// </summary>
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
