using ExtensibleSaveFormat;
using MakerAPI;
using MakerAPI.Chara;

namespace KK_BecomeTrap
{
    public class BecomeTrapController : CharaCustomFunctionController
    {
        internal bool IsTrap { get; set; }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            if (ChaControl.sex == 0 && IsTrap)
            {
                var data = new PluginData();
                data.data.Add("IsTrap", IsTrap);
                data.version = 1;

                SetExtendedData(data);
            }
            else
                SetExtendedData(null);
        }

        protected override void OnReload(GameMode currentGameMode)
        {
            if (ChaControl.sex == 0)
            {
                var data = GetExtendedData();

                if (data != null
                    && data.data.TryGetValue("IsTrap", out var val)
                    && val is bool bVal)
                {
                    IsTrap = bVal;
                    return;
                }
            }

            IsTrap = false;
        }
    }
}
