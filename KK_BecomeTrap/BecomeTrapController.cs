using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;

namespace KK_BecomeTrap
{
    public class BecomeTrapController : CharaCustomFunctionController
    {
        internal bool IsTrap { get; set; }

        internal string IdleAnimation { get; set; }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            if (ChaControl.sex == 0 && IsTrap)
            {
                var data = new PluginData();
                data.data.Add("IsTrap", IsTrap);
                data.data.Add("IdleAnimation", IdleAnimation);
                data.version = 1;

                SetExtendedData(data);
            }
            else
                SetExtendedData(null);
        }

        protected override void OnReload(GameMode currentGameMode)
        {
            IsTrap = false;
            IdleAnimation = null;

            // Only males
            if (ChaControl.sex == 0)
            {
                var data = GetExtendedData();

                if (data != null)
                {
                    IsTrap = data.data.TryGetValue("IsTrap", out var val) && val is bool isTrap && isTrap;
                    IdleAnimation = data.data.TryGetValue("IdleAnimation", out var val2) && val2 is string anim ? anim : null;
                }
            }
        }
    }
}
