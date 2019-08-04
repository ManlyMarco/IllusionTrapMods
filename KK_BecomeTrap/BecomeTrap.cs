using BepInEx;
using Harmony;
using KKAPI.Chara;
using KKAPI.Studio;

namespace KK_BecomeTrap
{
    [BepInPlugin(GUID, "Koikatsu: Become Trap", Version)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID)]
    public partial class BecomeTrap : BaseUnityPlugin
    {
        public const string GUID = "marco.becometrap";
        internal const string Version = "2.0";

        internal static BecomeTrap Instance;

        private void Awake()
        {
            if(StudioAPI.InsideStudio) return;

            Instance = this;

            CharacterApi.RegisterExtraBehaviour<BecomeTrapController>(GUID);
            BecomeTrapGui.Initialize();
            HarmonyInstance.Create(GUID).PatchAll(typeof(Hooks));
        }
    }
}
