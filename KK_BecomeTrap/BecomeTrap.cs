using BepInEx;
using BepInEx.Harmony;
using BepInEx.Logging;
using KKAPI.Chara;
using KKAPI.Studio;

namespace KK_BecomeTrap
{
    [BepInPlugin(GUID, "Koikatsu: Become Trap", Version)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public partial class BecomeTrap : BaseUnityPlugin
    {
        public const string GUID = "marco.becometrap";
        public const string Version = "2.0";

        internal static BecomeTrap Instance;
        internal static new ManualLogSource Logger;

        private void Awake()
        {
            Instance = this;
            Logger = base.Logger;

            if (StudioAPI.InsideStudio) return;

            CharacterApi.RegisterExtraBehaviour<BecomeTrapController>(GUID);
            BecomeTrapGui.Initialize();
            HarmonyWrapper.PatchAll(typeof(Hooks));
        }
    }
}
