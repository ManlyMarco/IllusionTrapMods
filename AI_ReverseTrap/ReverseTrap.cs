using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Harmony;
using BepInEx.Logging;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using UnityEngine;

namespace AI_ReverseTrap
{
    [BepInPlugin(GUID, "Reverse Trap", Version)]
    [BepInProcess("AI-Syoujyo")]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    public partial class ReverseTrap : BaseUnityPlugin
    {
        public const string GUID = "ReverseTrap";
        public const string Version = "1.0";

        internal static new ManualLogSource Logger { get; private set; }

        internal const int MaleSex = 0;
        internal static AnimationClip[] MaleAnimations { get; private set; }
        internal static readonly Dictionary<string, string> ToMaleAnimationLookup = new Dictionary<string, string>
        {
            {"Idle_00", "m_Idle_00"},
            {"mc_f_move_00", "mc_m_move_00"},
            {"mc_f_move_05", "mc_m_move_01"},
            {"mc_f_move_08_L", "mc_m_move_05_L"},
            {"mc_f_move_07_L", "mc_m_move_04_L"},
            {"Turn_Idle_00", "m_Turn_Idle_00"},
            {"mc_f_move_07_R", "mc_m_move_04_R"},
            {"mc_f_move_08_R", "mc_m_move_05_R"},
            {"mc_pf_move_02_S_in", "mc_m_move_02_S_in"},
            {"mc_pf_move_02_M_in", "mc_m_move_02_M_in"},
            {"mc_pf_move_02_L_in", "mc_m_move_02_L_in"},
            {"mc_pf_move_02_S_loop", "mc_m_move_02_S_loop"},
            {"mc_pf_move_02_M_loop", "mc_m_move_02_M_loop"},
            {"mc_pf_move_02_L_loop", "mc_m_move_02_L_loop"},
            {"mc_pf_move_03_S", "mc_m_move_03_S"},
            {"mc_pf_move_03_M", "mc_m_move_03_M"},
            {"mc_pf_move_03_L", "mc_m_move_03_L"},
            {"mc_f_action_00", "mc_m_action_00"},
            {"mc_pf_action_01", "mc_m_action_01"},
            {"mc_pf_action_02", "mc_m_action_02"},
            {"mc_pf_action_03", "mc_m_action_03"},
            {"mc_pf_action_04", "mc_m_action_04"},
            {"neko_01", "m_neko_00"},
            {"pf_neko_04_in", "m_neko_01_in"},
            {"pf_neko_04_loop", "m_neko_01_loop"},
            {"pf_neko_05", "m_neko_02"},
            {"pf_neko_06", "m_neko_03"},
            {"chair_Idle_00_M", "m_chair_Idle_00"},
            {"chair_16_S", "m_chair_00_S"},
            {"chair_16_M", "m_chair_00_M"},
            {"chair_16_L", "m_chair_00_L"},
            {"chair_15_S", "m_chair_01_S"},
            {"chair_15_M", "m_chair_01_M"},
            {"chair_15_L", "m_chair_01_L"},
            {"deskchair_Idle_00_M", "m_deskchair_Idle_00"},
            {"mc_pf_move_00_01", "mc_m_move_00_01"},
            {"mc_pf_move_05_01", "mc_m_move_01_01"},
            {"mc_pf_move_00_02", "mc_m_move_00_02"},
            {"mc_pf_move_05_02", "mc_m_move_01_02"},
            {"pf_Idle_00_03_S", "m_Idle_00_03_S"},
            {"pf_Idle_00_03_M", "m_Idle_00_03_M"},
            {"pf_Idle_00_03_L", "m_Idle_00_03_L"},
            {"mc_pf_move_00_03_S", "mc_m_move_00_03_S"},
            {"mc_pf_move_00_03_M", "mc_m_move_00_03_M"},
            {"mc_pf_move_00_03_L", "mc_m_move_00_03_L"}
        };

        private void Start()
        {
            Logger = base.Logger;

            try
            {
                // Load male animation clips for overriding
                var ab = AssetBundle.LoadFromFile(Application.dataPath + @"\..\abdata\animator\action\male\00.unity3d");
                var anim = ab.LoadAsset<RuntimeAnimatorController>("m_player.controller");
                MaleAnimations = anim.animationClips.ToArray();
                ab.Unload(false);
                Destroy(anim);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to read male player animation data - " + ex);
            }

            if (MaleAnimations != null && MaleAnimations.Any())
            {
                CharacterApi.RegisterExtraBehaviour<ReverseTrapController>(GUID);

                MakerAPI.RegisterCustomSubCategories += MakerAPI_RegisterCustomSubCategories;

                HarmonyWrapper.PatchAll(typeof(Hooks));
            }
        }

        private void MakerAPI_RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            if (MakerAPI.GetMakerSex() != MaleSex)
            {
                var makerToggle = e.AddControl(new MakerToggle(MakerConstants.Body.All, "Male walking animations", this));

                makerToggle.BindToFunctionController<ReverseTrapController, bool>(
                    controller => controller.ForceMaleAnimations,
                    (controller, value) => controller.ForceMaleAnimations = value);
            }
        }
    }
}
