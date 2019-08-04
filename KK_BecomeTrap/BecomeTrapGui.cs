using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using UniRx;
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

        private static MakerDropdown _animType;
        private static MakerToggle _toggleEnabled;

        internal static string DefaultIdleAnimation => _idleAnimations[0].Key;

        internal static void Initialize()
        {
            MakerAPI.RegisterCustomSubCategories += RegisterCustomSubCategories;
            MakerAPI.ChaFileLoaded += (sender, args) => BecomeTrap.Instance.StartCoroutine(ChaFileLoadedCo());
            MakerAPI.MakerExiting += MakerExiting;
        }

        private static IEnumerator ChaFileLoadedCo()
        {
            yield return null;

            if (_toggleEnabled != null)
            {
                var ctrl = GetMakerController();
                _toggleEnabled.Value = ctrl.IsTrap;
                _animType.Value = Mathf.Max(0, _idleAnimations.FindIndex(pair => pair.Key == ctrl.IdleAnimation));
            }
        }

        private static BecomeTrapController GetMakerController()
        {
            return MakerAPI.GetCharacterControl().gameObject.GetComponent<BecomeTrapController>();
        }

        private static void MakerExiting(object sender, EventArgs e)
        {
            _toggleEnabled = null;
            _animType = null;
        }

        private static void RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            if (MakerAPI.GetMakerSex() == 0)
            {
                var category = MakerConstants.Parameter.Character;

                _toggleEnabled = e.AddControl(new MakerToggle(category, "Character is a trap or a futa (changes gameplay)", BecomeTrap.Instance));
                _toggleEnabled.ValueChanged.Subscribe(val => GetMakerController().IsTrap = val);

                _animType = e.AddControl(new MakerDropdown("Idle trap animation", _idleAnimations.Select(x => x.Value).ToArray(), category, 0, BecomeTrap.Instance));
                _animType.ValueChanged.Subscribe(i => GetMakerController().IdleAnimation = _idleAnimations[i].Key);
            }
        }
    }
}
