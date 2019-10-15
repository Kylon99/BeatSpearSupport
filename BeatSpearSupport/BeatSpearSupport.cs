using CustomUI.Utilities;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace BeatSpearSupport
{
    public class BeatSpearSupport : MonoBehaviour
    {
        private const KeyCode leftTrigger = KeyCode.JoystickButton14;
        private const KeyCode rightTrigger = KeyCode.JoystickButton15;

        private PlayerController playerController;
        private MainSettingsModel mainSettingsModel;
        private XRNode previousForwardHand;

        private void Awake()
        {
            this.playerController = FindObjectOfType<PlayerController>();
            mainSettingsModel = Resources.FindObjectsOfTypeAll<MainSettingsModel>().FirstOrDefault();
            previousForwardHand = ConfigOptions.instance.LeftHanded ? XRNode.RightHand : XRNode.LeftHand;

            var pauseAnimationController = Object.FindObjectOfType<PauseAnimationController>();
            if (pauseAnimationController != null) pauseAnimationController.resumeFromPauseAnimationDidFinishEvent += this.ResumeFromPauseAnimationDidFinishEvent;
        }

        private void Update()
        {
            const float handleLength = 0.75f;
            const float handleLengthSquared = 0.5625f;

            if (!ConfigOptions.instance.TwoControllers) { return; }

            // Determine the forward hand
            if (Input.GetKeyDown(leftTrigger)) { previousForwardHand = XRNode.LeftHand; }
            if (Input.GetKeyDown(rightTrigger)) { previousForwardHand = XRNode.RightHand; }

            XRNode forwardHandNode = previousForwardHand;
            XRNode rearHandNode = forwardHandNode == XRNode.RightHand ? XRNode.LeftHand : XRNode.RightHand;

            // Get positions and rotations of hands
            (Vector3 position, Quaternion rotation) forwardHand = GetXRNodePosRos(forwardHandNode);
            (Vector3 position, Quaternion rotation) rearHand = GetXRNodePosRos(rearHandNode);
            Vector3 forward = (forwardHand.position - rearHand.position).normalized;
            Vector3 up = forwardHand.rotation * Vector3.one;

            // Determine final saber position
            Vector3 saberPosition;
            float handSeparationSquared = (forwardHand.position - rearHand.position).sqrMagnitude;
            if (handSeparationSquared > handleLengthSquared)
            {
                // Clamp the saber at the extent of the forward hand
                saberPosition = forwardHand.position;
            }
            else
            {
                // Allow the saber to be pushed forward by the rear hand
                saberPosition = rearHand.position + (forward * handleLength);
            }

            // Apply transforms to saber
            Saber saberToTransform = ConfigOptions.instance.LeftHanded ? playerController.leftSaber : playerController.rightSaber;
            saberToTransform.transform.position = saberPosition;
            saberToTransform.transform.rotation = Quaternion.LookRotation(forward, up);
        }

        private void ResumeFromPauseAnimationDidFinishEvent()
        {
            if (ConfigOptions.instance.TwoControllers || ConfigOptions.instance.RemoveOtherSpear)
            {
                Saber saberToHide = ConfigOptions.instance.LeftHanded ? this.playerController.rightSaber : this.playerController.leftSaber;
                saberToHide.gameObject.SetActive(false);
            }
        }

        private (Vector3, Quaternion) GetXRNodePosRos(XRNode node)
        {
            var pos = InputTracking.GetLocalPosition(node);
            var rot = InputTracking.GetLocalRotation(node);

            var roomCenter = mainSettingsModel.roomCenter;
            var roomRotation = Quaternion.Euler(0, mainSettingsModel.roomRotation, 0);
            pos = roomRotation * pos;
            pos += roomCenter;
            rot = roomRotation * rot;
            return (pos, rot);
        }

        /// <summary>
        /// To be invoked every time when starting the GameCore scene.
        /// </summary>
        /// <remarks>
        /// Note that no delay is done with this method
        /// </remarks>
        public void BeginGameCoreScene()
        {
            if (ConfigOptions.instance.OneColor)
            {
                this.TransformToOneColor();
            }

            if (ConfigOptions.instance.TwoControllers)
            {
                Logging.Info("Disabling submission on Two Controller option");
                BS_Utils.Gameplay.ScoreSubmission.DisableSubmission(Plugin.assemblyName);
                previousForwardHand = ConfigOptions.instance.LeftHanded ? XRNode.RightHand : XRNode.LeftHand;
            }

            if (ConfigOptions.instance.TwoControllers || ConfigOptions.instance.RemoveOtherSpear)
            {
                Saber saberToHide = ConfigOptions.instance.LeftHanded ? this.playerController.rightSaber : this.playerController.leftSaber;
                saberToHide.gameObject.SetActive(false);
            }
        }

        private void TransformToOneColor()
        {
            const string OneSaberModeName = "OneSaber";

            // Check for game mode and early exit on One Saber or NoArrows
            GameplayCoreSceneSetupData data = BS_Utils.Plugin.LevelData?.GameplayCoreSceneSetupData;
            var beatmap = data.difficultyBeatmap;
            string serializedName = beatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            if (serializedName == OneSaberModeName)
            {
                // Do not transform for One Saber or legitimate No Arrows mode
                Logging.Info($"No need to transform: {beatmap.level.songName} for spear as it is a One Saber map");
                return;
            }

            // Get the in memory beat map
            var gameplayCoreSceneSetup = Resources.FindObjectsOfTypeAll<GameplayCoreSceneSetup>().First();
            if (gameplayCoreSceneSetup == null) return;

            var dataModel = gameplayCoreSceneSetup.GetField<BeatmapDataModel>("_beatmapDataModel");
            var beatmapData = dataModel.beatmapData;

            Logging.Info("Disabling submission on One Color No Arrows transformation");
            BS_Utils.Gameplay.ScoreSubmission.DisableSubmission(Plugin.assemblyName);

            // Transform the map to One Color and No Arrows
            NoteType undesiredNoteType = ConfigOptions.instance.LeftHanded ? NoteType.NoteB : NoteType.NoteA;
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
            Saber.SaberType desiredSaberType = ConfigOptions.instance.LeftHanded ? Saber.SaberType.SaberA : Saber.SaberType.SaberB;
            var saberObject = new GameObject("SaberTypeObject").AddComponent<SaberTypeObject>();
            saberObject.SetField("_saberType", desiredSaberType);

            var player = Resources.FindObjectsOfTypeAll<PlayerController>().FirstOrDefault();
            Saber saberToSwap = ConfigOptions.instance.LeftHanded ? player.rightSaber : player.leftSaber;
            saberToSwap.SetField("_saberType", saberObject);
        }
    }
}
