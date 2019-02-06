using System.Collections;
using BepInEx;
using MakerAPI;
using UniRx;

namespace KK_BecomeTrap
{
    [BepInProcess("Koikatu")]
    [BepInPlugin(BecomeTrap.GUID + "_GUI", "Koikatsu: Become Trap GUI", BecomeTrap.Version)]
    [BepInDependency(BecomeTrap.GUID)]
    public class BecomeTrapGui : BaseUnityPlugin
    {
        private MakerToggle _toggleEnabled;

        private void Start()
        {
            MakerAPI.MakerAPI.Instance.RegisterCustomSubCategories += RegisterCustomSubCategories;
            MakerAPI.MakerAPI.Instance.ChaFileLoaded += (sender, args) => StartCoroutine(ChaFileLoadedCo());
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
            return MakerAPI.MakerAPI.Instance.GetCharacterControl().gameObject.GetComponent<BecomeTrapController>();
        }

        private void RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            if (MakerAPI.MakerAPI.Instance.GetMakerSex() == 0)
            {
                _toggleEnabled = e.AddControl(new MakerToggle(MakerConstants.Parameter.Character, "Character is a trap", this));
                _toggleEnabled.ValueChanged.Subscribe(val => GetMakerController().IsTrap = val);
            }
        }
    }
}
