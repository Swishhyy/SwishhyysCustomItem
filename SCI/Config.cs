using System.ComponentModel;
using Exiled.API.Interfaces;
using YamlDotNet.Serialization;

namespace SCI.Custom.Config
{
    public class Config : IConfig
    {
        [Description("Whether the plugin is enabled or not")]
        public bool IsEnabled { get; set; } = true;

        [Description("Whether to enable debug logging")]
        public bool Debug { get; set; } = false;

        [Description("Discord webhook URL for notifications")]
        public string DiscordWebhook { get; set; } = string.Empty;

        // These properties won't appear in the main config.yml file
        // They'll only be used internally to store loaded config values from individual files
        [YamlIgnore]
        public SCI.Config.ExpiredSCP500PillsConfig ExpiredSCP500 { get; set; } = new SCI.Config.ExpiredSCP500PillsConfig();

        [YamlIgnore]
        public SCI.Config.AdrenalineSCP500PillsConfig AdrenalineSCP500 { get; set; } = new SCI.Config.AdrenalineSCP500PillsConfig();

        [YamlIgnore]
        public SCI.Config.SuicideSCP500PillsConfig SuicideSCP500 { get; set; } = new SCI.Config.SuicideSCP500PillsConfig();
        [YamlIgnore]
        public SCI.Config.VanishingSCP500PillsConfig VanishingSCP500 { get; set; } = new SCI.Config.VanishingSCP500PillsConfig();

        [YamlIgnore]
        public SCI.Config.ClusterGrenadeConfig ClusterGrenade { get; set; } = new SCI.Config.ClusterGrenadeConfig();

        [YamlIgnore]
        public SCI.Config.ImpactGrenadeConfig ImpactGrenade { get; set; } = new SCI.Config.ImpactGrenadeConfig();

        [YamlIgnore]
        public SCI.Config.SmokeGrenadeConfig SmokeGrenade { get; set; } = new SCI.Config.SmokeGrenadeConfig();

        [YamlIgnore]
        public SCI.Config.RailgunConfig Railgun { get; set; } = new SCI.Config.RailgunConfig();

        [YamlIgnore]
        public SCI.Config.GrenadeLauncherConfig GrenadeLauncher { get; set; } = new SCI.Config.GrenadeLauncherConfig();
        [YamlIgnore]
        public SCI.Config.BioGrenadeConfig BioGrenade { get; set; } = new SCI.Config.BioGrenadeConfig();
        //[YamlIgnore]
        //public SCI.Config.HackingChipConfig HackingChip { get; set; } = new SCI.Config.HackingChipConfig();
        [YamlIgnore]
        public SCI.Config.ReinforcementCallConfig ReinforcementCall { get; set; } = new SCI.Config.ReinforcementCallConfig();
    }
}
