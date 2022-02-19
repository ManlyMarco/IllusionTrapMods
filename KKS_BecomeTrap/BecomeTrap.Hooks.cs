using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ActionGame;
using ActionGame.Chara;
using ActionGame.Communication;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Motion = Illusion.Game.Elements.EasyLoader.Motion;
using Object = UnityEngine.Object;

namespace KK_BecomeTrap
{
    public partial class BecomeTrap
    {
        private static class Hooks
        {
            private static BecomeTrapController GetController(Player player)
            {
                if (player == null || player.chaCtrl == null) return null;
                return player.chaCtrl.gameObject.GetComponent<BecomeTrapController>();
            }

            /// <summary>
            /// If the player is a trap, load alternative animations
            /// </summary>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Motion), nameof(Motion.LoadAnimator), typeof(Animator))]
            public static void LoadAnimatorHook(Motion __instance, Animator animator)
            {
                if (__instance.bundle != "action/animator/00.unity3d" || __instance.asset != "player") return;

                var inMainGame = ActionScene.initialized;
                if (!inMainGame) return;

                var ctrl = GetController(ActionScene.instance.Player);

                if (ctrl == null || !ctrl.IsTrap) return;

                Logger.Log(LogLevel.Debug, "Replacing player animations");

                var playerClips = animator.runtimeAnimatorController.animationClips;

                // Names of animations to replace stock male animations with, same index as in stock animationClips
                // Some male animations need to stay if they use props because they are animated separately and wouldn't match other anims
                var replacements = new[]
                {
                    "f_talk_00_00_01"          ,
                    "f_suwari_00_11_00_S"      ,
                    "m_call_00"                ,
                    "ks_m_action_00"           ,
                    "ks_f_sunaijiri_00_00_loop",
                    "ks_m_action_02"           ,
                    "ks_m_action_03"           ,
                    "m_twoshot_00_kiss"        ,
                    "m_twoshot_00_00_S"        ,
                    "m_twoshot_00_00"          ,
                    "m_twoshot_00_00"          ,
                    string.IsNullOrEmpty(ctrl.IdleAnimation) ? BecomeTrapGui.DefaultIdleAnimation : ctrl.IdleAnimation,
                    "f_aruki_00_00_01"         ,
                    "f_hasiru_00_00_01"        ,
                    "f_syagami_00_01"          ,
                    "mc_m_squat_walk_00_00_01" ,
                };

                if (playerClips.Length != replacements.Length)
                {
                    Logger.Log(LogLevel.Error, "Player runtimeAnimatorController.animationClips has different size than the replacement list!");
                    return;
                }

                // Gather all available AnimationClips. They can't be accessed directly so need to spawn controllers and grab clips from those
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
                {
                    try
                    {
                        overrideController[playerClips[i]] = allClipList.First(x => x.name == replacements[i]);
                    }
                    catch (InvalidOperationException)
                    {
                        Logger.Log(LogLevel.Error, $"Failed to find animation clip {replacements[i]}! Using DefaultIdleAnimation instead.");
                        overrideController[playerClips[i]] = allClipList.First(x => x.name == BecomeTrapGui.DefaultIdleAnimation);
                    }
                }

                animator.runtimeAnimatorController = overrideController;

                __instance.UnloadBundle();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ActionMap), nameof(ActionMap.ChangeAsync), typeof(int), typeof(FadeCanvas.Fade), typeof(bool))]
            public static void MapChangePostfix(ActionMap __instance)
            {
                var inMainGame = ActionScene.initialized;
                if (!inMainGame) return;

                // Mark all maps as safe to be in (so we don't get kicked out) if the character is a trap
                __instance.StartCoroutine(MapChangeCo(__instance));
            }

            private static IEnumerator MapChangeCo(ActionMap instance)
            {
                BecomeTrapController ctrl = null;

                yield return null;
                yield return new WaitUntil(() => (ctrl = GetController(ActionScene.instance.Player)) != null);

                if (ctrl.IsTrap)
                {
                    // todo somehow return to default if player turns trap feature off?
                    foreach (var param in instance.infoDic.Values)
                        param.isWarning = false;
                }
            }

            /// <summary>
            /// Remove events that cause the girl to refuse to talk because you're tresspassing
            /// </summary>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Info), nameof(ActionGame.Communication.Info.GetListCommand), typeof(int), typeof(Info.Group), typeof(int))]
            public static void GetListCommandPostfix(Info __instance, ref List<Info.BasicInfo> __result, int _stage, Info.Group _group, int _command)
            {
                // Make sure we are called from ActionGame.Communication.Info.GetIntroductionADV
                // Calling only StackTrace would be enough but this is much faster for most calls
                if (_stage >= 2 || _group != ActionGame.Communication.Info.Group.Introduction || _command != 0) return;

                // Prevent crashing when speaking to the guide or other NPCs
                if (__instance.isNPC) return;

                if (!ActionScene.initialized) return;

                var controller = GetController(ActionScene.instance.Player);
                // Only applicable if the player is a trap
                if (controller == null || !controller.IsTrap) return;

                if (!new StackTrace().ToString().Contains("ActionGame.Communication.Info.GetIntroductionADV")) return;

                // Remove events that cause the girl to refuse to talk because you're tresspassing
                __result.RemoveAll(x =>
                    x.conditions == 1 ||
                    x.conditions == 2 ||
                    x.conditions == 3 ||
                    x.conditions == 4 ||
                    x.conditions == 30 ||
                    x.conditions == 31);
            }
        }
    }
}
