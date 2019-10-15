using BS_Utils.Utilities;

namespace BeatSpearSupport
{
    public class ConfigOptions : PersistentSingleton<ConfigOptions>
    {
        public const string TwoControllersOption = "TwoControllers";
        public const string OneColorOption = "OneColor";
        public const string LeftHandedOption = "UseLeftSpear";
        public const string RemoveOtherSpearOption = "RemoveOtherSpear";

        public bool TwoControllers { get => twoControllers; set { twoControllers = value; this.config.SetBool(Plugin.assemblyName, TwoControllersOption, value); } }
        public bool OneColor { get => oneColor; set { oneColor = value; this.config.SetBool(Plugin.assemblyName, OneColorOption, value); } }
        public bool LeftHanded { get => useOtherSpear; set { useOtherSpear = value; this.config.SetBool(Plugin.assemblyName, LeftHandedOption, value); } }
        public bool RemoveOtherSpear { get => removeOtherSpear; set { removeOtherSpear = value; this.config.SetBool(Plugin.assemblyName, RemoveOtherSpearOption, value); } }

        private Config config;

        private bool twoControllers;
        private bool oneColor;
        private bool useOtherSpear;
        private bool removeOtherSpear;


        /// <summary>
        /// Called before Start or Updates by Unity infrastructure
        /// </summary>
        public void Awake()
        {
            config = new Config(Plugin.assemblyName);
            TwoControllers = config.GetBool(Plugin.assemblyName, TwoControllersOption, false, true);
            OneColor = config.GetBool(Plugin.assemblyName, OneColorOption, false, true);
            LeftHanded = config.GetBool(Plugin.assemblyName, LeftHandedOption, false, true);
            RemoveOtherSpear = config.GetBool(Plugin.assemblyName, RemoveOtherSpearOption, false, true);
        }
    }
}
