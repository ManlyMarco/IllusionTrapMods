using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using Harmony;
using KKAPI.Chara;
using Sideloader;

namespace KK_BecomeTrap
{
    [BepInPlugin(GUID, "Koikatsu: Become Trap", Version)]
    [BepInDependency(Sideloader.Sideloader.GUID)]
    public partial class BecomeTrap : BaseUnityPlugin
    {
        public const string GUID = "marco.becometrap";
        internal const string Version = "1.1.1";

        private void Start()
        {
            HarmonyInstance.Create(GUID).PatchAll(typeof(BecomeTrap.Hooks));

            CharacterApi.RegisterExtraBehaviour<BecomeTrapController>(GUID);

            var manifests = AccessTools.Field(typeof(Sideloader.Sideloader), "LoadedManifests").GetValue(GetComponent<Sideloader.Sideloader>()) as List<Manifest>;
            if (manifests == null || manifests.All(x => x.GUID != GUID))
                ShowZipmodError();
        }

        private static void ShowZipmodError()
        {
            Logger.Log(LogLevel.Error | LogLevel.Message, "[BecomeTrap] Failed to load the animations! Make sure you have KK_BecomeTrap.zipmod in your mods folder!");
        }
    }
}
