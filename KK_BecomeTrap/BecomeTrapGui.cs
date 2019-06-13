using System.Collections;
using BepInEx;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using UniRx;

namespace KK_BecomeTrap
{
    [BepInPlugin(BecomeTrap.GUID + "_GUI", "Koikatsu: Become Trap GUI", BecomeTrap.Version)]
    [BepInDependency(BecomeTrap.GUID)]
    public class BecomeTrapGui : BaseUnityPlugin
    {
        private MakerToggle _toggleEnabled;

        private void Start()
        {
            MakerAPI.RegisterCustomSubCategories += RegisterCustomSubCategories;
            MakerAPI.ChaFileLoaded += (sender, args) => StartCoroutine(ChaFileLoadedCo());
        }

        private IEnumerator ChaFileLoadedCo()
        {
            yield return null;

            if (_toggleEnabled != null)
            {
                var ctrl = GetMakerController();
                _toggleEnabled.Value = ctrl.IsTrap;
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
                _toggleEnabled = e.AddControl(new MakerToggle(MakerConstants.Parameter.Character, "Character is a trap or a futa (changes gameplay)", this));
                _toggleEnabled.ValueChanged.Subscribe(val => GetMakerController().IsTrap = val);
            }
        }
    }
}
