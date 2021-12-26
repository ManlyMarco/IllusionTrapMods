using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using KKAPI.Chara;
using KKAPI.Studio;

namespace KK_BecomeTrap
{
    [BepInPlugin(GUID, "Koikatsu: Become Trap", Version)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public partial class BecomeTrap : BaseUnityPlugin
    {
        public const string GUID = "marco.becometrap";
        public const string Version = "2.2";

        internal static BecomeTrap Instance;
        internal static new ManualLogSource Logger;

        private void Awake()
        {
            Instance = this;
            Logger = base.Logger;

            if (StudioAPI.InsideStudio) return;

            CharacterApi.RegisterExtraBehaviour<BecomeTrapController>(GUID);
            BecomeTrapGui.Initialize();
            Harmony.CreateAndPatchAll(typeof(Hooks));
        }
    }
}
