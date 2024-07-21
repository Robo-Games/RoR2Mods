using UncappedChances.Effects;
using BepInEx.Configuration;

namespace UncappedChances
{
    public static class Configs
    {
        public static ConfigFile ModConfig;
        public static string ConfigFolderPath { get => System.IO.Path.Combine(BepInEx.Paths.ConfigPath, MainPlugin.pluginInfo.Metadata.GUID); }
        private const string Section_Crit = "Crit";
        private const string Section_Bleed = "Bleed";
        private const string Section_Collapse = "Collapse";
        private const string Section_Ghors = "Ghors Tome";
        private const string Section_Sticky = "Sticky Bomb";
        private const string Label_EnableChange = "Enable Changes";
        private const string Desc_Enable = "Enables changes for this item.";

        public static void Setup()
        {
            ModConfig = new ConfigFile(System.IO.Path.Combine(ConfigFolderPath, $"ModConfig.cfg"), true);
            Read_AllConfigs();
        }

        private static void Read_AllConfigs()
        {
            Crit.Enable = ModConfig.Bind(Section_Crit, Label_EnableChange, Crit.EnableDefault, Desc_Enable).Value;
            Crit.HarderSuccessive = ModConfig.Bind(Section_Crit, "Lower Successive Crits", Crit.HarderSuccessiveDefault, "Each successive crit has a low chance to proc.").Value;
            Crit.MultiplicativeCrit = ModConfig.Bind(Section_Crit, "Multiplicative Crits", Crit.MultiplicativeCritDefault, "Each crit multiplies the previous, ie 2-4-8 instead of 2-4-6").Value;

            Bleed.Enable = ModConfig.Bind(Section_Bleed, Label_EnableChange, Bleed.EnableDefault, Desc_Enable).Value;
            Bleed.HarderSuccessive = ModConfig.Bind(Section_Bleed, "Lower Successive Stacks", Bleed.HarderSuccessiveDefault, "Each successive bleed stack has a low chance to proc.").Value;

            Collapse.Enable = ModConfig.Bind(Section_Collapse, Label_EnableChange, Collapse.EnableDefault, Desc_Enable).Value;
            Collapse.HarderSuccessive = ModConfig.Bind(Section_Collapse, "Lower Successive Stacks", Collapse.HarderSuccessiveDefault, "Each successive collapse stack has a low chance to proc.").Value;

            GhorsTome.Enable = ModConfig.Bind(Section_Ghors, Label_EnableChange, GhorsTome.EnableDefault, Desc_Enable).Value;
            GhorsTome.HarderSuccessive = ModConfig.Bind(Section_Ghors, "Lower Successive Drops", GhorsTome.HarderSuccessiveDefault, "Each successive drop has a low chance to proc.").Value;

            StickyBomb.Enable = ModConfig.Bind(Section_Sticky, Label_EnableChange, StickyBomb.EnableDefault, Desc_Enable).Value;
            StickyBomb.HarderSuccessive = ModConfig.Bind(Section_Sticky, "Lower Successive Bombs", StickyBomb.HarderSuccessiveDefault, "Each successive sticky bomb has a low chance to proc.").Value;
            StickyBomb.SingleBomb = ModConfig.Bind(Section_Sticky, "One Bomb", StickyBomb.SingleBombDefault, "Instead of applying multiple bombs, apply 1 with the total damage.").Value;

        }
    }
}