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
        internal const string Version = "1.1.1";

        private void Awake()
        {
            if(StudioAPI.InsideStudio) return;

            CharacterApi.RegisterExtraBehaviour<BecomeTrapController>(GUID);

            HarmonyInstance.Create(GUID).PatchAll(typeof(Hooks));
        }
    }
}
