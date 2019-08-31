using BS_Utils.Utilities;
using CustomUI.GameplaySettings;
using IPA.Config;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace BeatSpearSupportMod
{
    public class BeatSpearSupport : MonoBehaviour
    {
        public const string Name = "BeatSpearSupport";
        public const string OneColorOption = "OneColor";
        public const string SwapHandsOption = "SwapHands";

        public bool OneColor { get; set; }
        public bool SwapHands { get; set; }

        private BS_Utils.Utilities.Config config;

        /// <summary>
        /// Called before Start or Updates by Unity infrastructure
        /// </summary>
        public void Awake()
        {
            config = new BS_Utils.Utilities.Config(Name);
            OneColor = config.GetBool(Name, OneColorOption, false, true);
            SwapHands = config.GetBool(Name, SwapHandsOption, false, true);
        }

        public void CreateBeatSpearSupportModifiers()
        {

            var oneColorOption = GameplaySettingsUI.CreateToggleOption("One Color", "Convert all cubes to one color!");
            var swapHandsOption = GameplaySettingsUI.CreateToggleOption("Swap Hands", "Reverse handedness in all maps, even One Saber maps!");

            oneColorOption.GetValue = OneColor;
            swapHandsOption.GetValue = SwapHands;
            oneColorOption.OnToggle += this.OnOneColorOptionToggle;
            swapHandsOption.OnToggle += this.OnSwapHandsOptionToggle;
        }

        public IEnumerator TransformBeatMap()
        {
            yield return new WaitForSecondsRealtime(0.1f);

            if (OneColor)
            {
                var gameplayCoreSceneSetup = Resources.FindObjectsOfTypeAll<GameplayCoreSceneSetup>().First();
                if (gameplayCoreSceneSetup == null) yield break;

                var dataModel = gameplayCoreSceneSetup.GetField<BeatmapDataModel>("_beatmapDataModel");
                var beatmapData = dataModel.beatmapData;

                BS_Utils.Gameplay.ScoreSubmission.DisableSubmission("BeatSpearSupport");

                NoteType undesiredNoteType = SwapHands ? NoteType.NoteB : NoteType.NoteA;
                Saber.SaberType desiredSaberType = SwapHands ? Saber.SaberType.SaberA : Saber.SaberType.SaberB;

                Console.WriteLine($"[BeatSpearSupport| TESTING] SWAPHANDS: {SwapHands}");

                foreach (BeatmapLineData line in beatmapData.beatmapLinesData)
                {
                    var objects = line.beatmapObjectsData;
                    foreach (BeatmapObjectData beatmapObject in objects)
                    {
                        if (beatmapObject.beatmapObjectType == BeatmapObjectType.Note)
                        {
                            var note = beatmapObject as NoteData;
                            if (note.noteType == undesiredNoteType)
                                note.SwitchNoteType();
                        }
                    }
                }

                var player = Resources.FindObjectsOfTypeAll<PlayerController>().FirstOrDefault();

                // Change saber type
                var saberObject = new GameObject("SaberTypeObject").AddComponent<SaberTypeObject>();
                saberObject.SetField("_saberType", desiredSaberType);

                Saber saberToSwap = SwapHands ? player.rightSaber : player.leftSaber;
                saberToSwap.SetField("_saberType", saberObject);
            }
        }

        private void OnOneColorOptionToggle(bool option)
        {
            OneColor = option;
            config.SetBool(Name, OneColorOption, option);
        }

        private void OnSwapHandsOptionToggle(bool option)
        {
            SwapHands = option;
            config.SetBool(Name, SwapHandsOption, option);
        }
    }
}
