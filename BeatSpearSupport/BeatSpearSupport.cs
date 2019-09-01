using CustomUI.GameplaySettings;
using CustomUI.Utilities;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace BeatSpearSupport
{
    public class BeatSpearSupport : MonoBehaviour
    {
        public const string assemblyName = "BeatSpearSupport";
        private const string iconResource = assemblyName + ".Resources.BeatSpear.png";

        public const string BeatSpearOnOption = "BeatSpearOn";
        public const string UseLeftSpearOption = "UseLeftSpear";

        private const string OneSaberModeName = "OneSaber";

        public bool BeatSpearOn { get; set; }
        public bool UseLeftSpear { get; set; }

        private BS_Utils.Utilities.Config config;

        /// <summary>
        /// Called before Start or Updates by Unity infrastructure
        /// </summary>
        public void Awake()
        {
            config = new BS_Utils.Utilities.Config(assemblyName);
            BeatSpearOn = config.GetBool(assemblyName, BeatSpearOnOption, false, true);
            UseLeftSpear = config.GetBool(assemblyName, UseLeftSpearOption, false, true);
        }

        public void CreateBeatSpearSupportModifiers()
        {
            var beatSpearIcon = UIUtilities.LoadSpriteFromResources(iconResource);

            var beatSpearMenu = GameplaySettingsUI.CreateSubmenuOption(
                GameplaySettingsPanels.ModifiersRight, 
                "Beat Spear Settings",
                "MainMenu",
                "BeatSpearMenu", 
                "Beat Spear Support options", 
                beatSpearIcon);

            var beatSpearToggle = GameplaySettingsUI.CreateToggleOption(
                GameplaySettingsPanels.ModifiersRight,
                "Enable Beat Spear",
                "BeatSpearMenu",
                "Enable Beat Spear Support for Standard and No Arrow maps.",
                beatSpearIcon);

            var useLeftToggle = GameplaySettingsUI.CreateToggleOption(
                GameplaySettingsPanels.ModifiersRight,
                "Use Left Spear",
                "BeatSpearMenu",
                "Use the left controller for the spear instead of the right");

            beatSpearToggle.GetValue = BeatSpearOn;
            useLeftToggle.GetValue = UseLeftSpear;
            beatSpearToggle.OnToggle += this.OnBeatSpearOptionToggle;
            useLeftToggle.OnToggle += this.OnUseLeftOptionToggle;
        }

        public IEnumerator TransformBeatMap()
        {
            yield return new WaitForSecondsRealtime(0.1f);

            if (BeatSpearOn)
            {
                // Check for game mode and early exit on One Saber or NoArrows
                GameplayCoreSceneSetupData data = BS_Utils.Plugin.LevelData?.GameplayCoreSceneSetupData;
                var beatmap = data.difficultyBeatmap;
                string serializedName = beatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
                if (serializedName == OneSaberModeName)
                {
                    // Do not transform for One Saber or legitimate No Arrows mode
                    Logging.Info($"No need to transform: {beatmap.level.songName} for spear as it is a One Saber map");
                    yield break;
                }

                // Get the in memory beat map
                var gameplayCoreSceneSetup = Resources.FindObjectsOfTypeAll<GameplayCoreSceneSetup>().First();
                if (gameplayCoreSceneSetup == null) yield break;

                var dataModel = gameplayCoreSceneSetup.GetField<BeatmapDataModel>("_beatmapDataModel");
                var beatmapData = dataModel.beatmapData;

                Logging.Info("Disabling submission on Beat Spear transformation.");
                BS_Utils.Gameplay.ScoreSubmission.DisableSubmission("BeatSpearSupport");

                // Transform the map to One Color and No Arrows
                NoteType undesiredNoteType = UseLeftSpear ? NoteType.NoteB : NoteType.NoteA;
                foreach (BeatmapLineData line in beatmapData.beatmapLinesData)
                {
                    var objects = line.beatmapObjectsData;
                    foreach (BeatmapObjectData beatmapObject in objects)
                    {
                        if (beatmapObject.beatmapObjectType == BeatmapObjectType.Note)
                        {
                            var note = beatmapObject as NoteData;
                            note.SetNoteToAnyCutDirection();

                            if (note.noteType == undesiredNoteType)
                                note.SwitchNoteType();
                        }
                    }
                }

                // Change the other saber to desired type
                Saber.SaberType desiredSaberType = UseLeftSpear ? Saber.SaberType.SaberA : Saber.SaberType.SaberB;
                var saberObject = new GameObject("SaberTypeObject").AddComponent<SaberTypeObject>();
                saberObject.SetField("_saberType", desiredSaberType);

                var player = Resources.FindObjectsOfTypeAll<PlayerController>().FirstOrDefault();
                Saber saberToSwap = UseLeftSpear ? player.rightSaber : player.leftSaber;
                saberToSwap.SetField("_saberType", saberObject);
            }
        }

        private void OnBeatSpearOptionToggle(bool option)
        {
            BeatSpearOn = option;
            config.SetBool(assemblyName, BeatSpearOnOption, option);
        }

        private void OnUseLeftOptionToggle(bool option)
        {
            UseLeftSpear = option;
            config.SetBool(assemblyName, UseLeftSpearOption, option);
        }
    }
}
