using System.Collections.Generic;
using System.Linq;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using UnityEngine;

namespace KK_BecomeTrap
{
    public static class BecomeTrapGui
    {
        private static readonly List<KeyValuePair<string, string>> _idleAnimations = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("Stand_17_01", "Hands behind"),
            new KeyValuePair<string, string>("Stand_19_01", "Hands in front"),
            new KeyValuePair<string, string>("Stand_27_01", "Hands crossed"),
            new KeyValuePair<string, string>("Stand_12_01", "Confident"),
            new KeyValuePair<string, string>("Stand_18_01", "Timid"),
            new KeyValuePair<string, string>("Stand_13_01", "Ladylike")
        };

        internal static string DefaultIdleAnimation => _idleAnimations[0].Key;

        internal static void Initialize()
        {
            MakerAPI.RegisterCustomSubCategories += RegisterCustomSubCategories;
        }

        private static void RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            if (MakerAPI.GetMakerSex() == 0)
            {
                var category = MakerConstants.Parameter.Character;

                var isTrap = e.AddControl(new MakerToggle(category, "Character is a trap or a futa (changes gameplay)", BecomeTrap.Instance));
                isTrap.BindToFunctionController<BecomeTrapController, bool>(
                    (controller) => controller.IsTrap,
                    (controller, value) => controller.IsTrap = value);

                var animType = e.AddControl(new MakerDropdown("Idle trap animation", _idleAnimations.Select(x => x.Value).ToArray(), category, 0, BecomeTrap.Instance));
                animType.BindToFunctionController<BecomeTrapController, int>(
                    (controller) => Mathf.Max(0, _idleAnimations.FindIndex(pair => pair.Key == controller.IdleAnimation)),
                    (controller, value) => controller.IdleAnimation = _idleAnimations[value].Key);
            }
        }
    }
}
