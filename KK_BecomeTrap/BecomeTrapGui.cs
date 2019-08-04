using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using UniRx;
using UnityEngine;

namespace KK_BecomeTrap
{
    [BepInPlugin(BecomeTrap.GUID + "_GUI", "Koikatsu: Become Trap GUI", BecomeTrap.Version)]
    [BepInDependency(BecomeTrap.GUID)]
    public class BecomeTrapGui : BaseUnityPlugin
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

        private MakerDropdown _animType;
        private MakerToggle _toggleEnabled;

        private IEnumerator ChaFileLoadedCo()
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

        private void RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            if (MakerAPI.GetMakerSex() == 0)
            {
                var category = MakerConstants.Parameter.Character;

                _toggleEnabled = e.AddControl(new MakerToggle(category, "Character is a trap or a futa (changes gameplay)", this));
                _toggleEnabled.ValueChanged.Subscribe(val => GetMakerController().IsTrap = val);

                _animType = e.AddControl(new MakerDropdown("Idle trap animation", _idleAnimations.Select(x => x.Value).ToArray(), category, 0, this));
                _animType.ValueChanged.Subscribe(i => GetMakerController().IdleAnimation = _idleAnimations[i].Key);
            }
        }

        private void Start()
        {
            MakerAPI.RegisterCustomSubCategories += RegisterCustomSubCategories;
            MakerAPI.ChaFileLoaded += (sender, args) => StartCoroutine(ChaFileLoadedCo());
        }
    }
}
