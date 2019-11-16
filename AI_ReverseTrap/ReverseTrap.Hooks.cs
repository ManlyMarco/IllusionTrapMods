using AIChara;
using AIProject;
using HarmonyLib;
using UnityEngine;

namespace AI_ReverseTrap
{
    public partial class ReverseTrap
    {
        private static class Hooks
        {
            [HarmonyPostfix]
            // void ActorAnimation.SetAnimatorController(RuntimeAnimatorController rac)
            [HarmonyPatch(typeof(ActorAnimation), nameof(ActorAnimation.SetAnimatorController), typeof(RuntimeAnimatorController))]
            private static void SetAnimatorPost(ActorAnimation __instance)
            {
                if (__instance.Actor != null && __instance.Actor.ChaControl != null)
                {
                    var ctrl = __instance.Actor.ChaControl.GetComponent<ReverseTrapController>();
                    ctrl?.RefreshOverrideAnimations();
                }
            }

            [HarmonyPostfix]
            // RuntimeAnimatorController LoadAnimation(string assetBundleName, string assetName, string manifestName)
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.LoadAnimation), typeof(string), typeof(string), typeof(string))]
            private static void SetAnimatorPost2(ChaControl __instance)
            {
                var ctrl = __instance.GetComponent<ReverseTrapController>();
                ctrl?.RefreshOverrideAnimations();
            }
        }
    }
}
