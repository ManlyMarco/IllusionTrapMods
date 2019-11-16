using System.Linq;
using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using UnityEngine;

namespace AI_ReverseTrap
{
    public class ReverseTrapController : CharaCustomFunctionController
    {
        private bool _forceMaleAnimations;

        public bool ForceMaleAnimations
        {
            get => _forceMaleAnimations;
            set
            {
                value = value && ChaControl.sex != ReverseTrap.MaleSex;

                if (_forceMaleAnimations != value)
                {
                    _forceMaleAnimations = value;

                    RefreshOverrideAnimations();
                }
            }
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            if (ChaControl.sex != ReverseTrap.MaleSex)
            {
                var data = new PluginData { data = { [nameof(ForceMaleAnimations)] = ForceMaleAnimations } };
                SetExtendedData(data);
            }
            else
                SetExtendedData(null);
        }

        protected override void OnReload(GameMode currentGameMode)
        {
            if (ChaControl.sex != ReverseTrap.MaleSex)
            {
                var data = GetExtendedData()?.data;
                ForceMaleAnimations = data != null && data.TryGetValue(nameof(ForceMaleAnimations), out var force) && force as bool? == true;
            }
            else
                ForceMaleAnimations = false;
        }

        internal void RefreshOverrideAnimations()
        {
            var animBody = ChaControl.animBody;

            if (ReverseTrap.MaleAnimations == null || animBody == null || animBody.runtimeAnimatorController == null) return;

            var overrideControler = animBody.runtimeAnimatorController as AnimatorOverrideController;
            if (overrideControler == null)
            {
                if (!ForceMaleAnimations) return;

                overrideControler = new AnimatorOverrideController(animBody.runtimeAnimatorController);
                animBody.runtimeAnimatorController = overrideControler;
            }
            else if (!ForceMaleAnimations)
            {
                animBody.runtimeAnimatorController = overrideControler.runtimeAnimatorController;
                return;
            }

            var animationClips = animBody.runtimeAnimatorController.animationClips.ToArray();

            foreach (var animationClip in animationClips)
            {
                if (ReverseTrap.ToMaleAnimationLookup.TryGetValue(animationClip.name, out var targetClipName))
                {
                    var replacement = ReverseTrap.MaleAnimations.FirstOrDefault(clip => clip.name == targetClipName);
                    if (replacement != null)
                    {
                        ReverseTrap.Logger.LogDebug($"Replacing animation {animationClip.name} with {replacement.name}");
                        overrideControler[animationClip] = replacement;
                    }
                    else
                    {
                        ReverseTrap.Logger.LogWarning($"Failed to replace animation {animationClip.name} with {targetClipName} because replacement clip was not found");
                    }
                }
            }
        }
    }
}
