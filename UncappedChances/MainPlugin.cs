using BepInEx;
using UncappedChances.Effects;
using R2API;

namespace UncappedChances
{
    [BepInDependency(ItemAPI.PluginGUID)]

    [BepInDependency(LanguageAPI.PluginGUID)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class MainPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "ZetaDaemon";
        public const string PluginName = "UncappedChances";
        public const string PluginVersion = "1.0.3";

        internal static BepInEx.Logging.ManualLogSource ModLogger;
        public static PluginInfo pluginInfo;

        private void Awake()
        {
            ModLogger = this.Logger;
            pluginInfo = Info;
            Configs.Setup();
            EnableChanges();
            SharedHooks.Setup();
        }
        private void EnableChanges()
        {
            new Crit();
            new Bleed();
            new Collapse();
        }
    }
}
