using CustomUI.GameplaySettings;
using CustomUI.Utilities;
using UnityEngine;

namespace BeatSpearSupport
{
    public class BeatSpearModifiersUI : MonoBehaviour
    {
        private const string beatSpearIconResource = Plugin.assemblyName + ".Resources.BeatSpear.png";
        private const string oneColorSpearIconResource = Plugin.assemblyName + ".Resources.OneColor.png";

        public void CreateBeatSpearSupportModifiers()
        {
            var beatSpearIcon = UIUtilities.LoadSpriteFromResources(beatSpearIconResource);
            var oneColorIcon = UIUtilities.LoadSpriteFromResources(oneColorSpearIconResource);

            var beatSpearMenu = GameplaySettingsUI.CreateSubmenuOption(
                GameplaySettingsPanels.ModifiersRight,
                "Beat Spear Settings",
                "MainMenu",
                "BeatSpearMenu",
                "Beat Spear Support options",
                beatSpearIcon);

            var twoControllersToggle = GameplaySettingsUI.CreateToggleOption(
                GameplaySettingsPanels.ModifiersRight,
                "Two Controller Spear",
                "BeatSpearMenu",
                "Use two hand controllers to control the spear. NOT COMPATIBLE WITH DARTHMAUL",
                beatSpearIcon);

            var beatSpearToggle = GameplaySettingsUI.CreateToggleOption(
                GameplaySettingsPanels.ModifiersRight,
                "One Color",
                "BeatSpearMenu",
                "Enable One Color for Standard and No Arrow maps.",
                oneColorIcon);

            var leftHandedToggle = GameplaySettingsUI.CreateToggleOption(
                GameplaySettingsPanels.ModifiersRight,
                "Left Spear",
                "BeatSpearMenu",
                "Use the left saber and its color as the spear");

            var removeOtherToggle = GameplaySettingsUI.CreateToggleOption(
                GameplaySettingsPanels.ModifiersRight,
                "Remove Other Spear",
                "BeatSpearMenu",
                "Remove the offhand spear. NOT COMPATIBLE WITH DARTHMAUL");

            twoControllersToggle.GetValue = ConfigOptions.instance.TwoControllers;
            beatSpearToggle.GetValue = ConfigOptions.instance.OneColor;
            leftHandedToggle.GetValue = ConfigOptions.instance.LeftHanded;
            removeOtherToggle.GetValue = ConfigOptions.instance.RemoveOtherSpear;

            twoControllersToggle.OnToggle += (option) => ConfigOptions.instance.TwoControllers = option;
            beatSpearToggle.OnToggle += (option) => ConfigOptions.instance.OneColor = option;
            leftHandedToggle.OnToggle += (option) => ConfigOptions.instance.LeftHanded = option;
            removeOtherToggle.OnToggle += (option) => ConfigOptions.instance.RemoveOtherSpear = option;
        }
    }
}
