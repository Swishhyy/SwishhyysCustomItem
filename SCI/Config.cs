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

        // Dynamic property access for configs loaded by ConfigWriter
        [YamlIgnore]
        public SCI.Config.SCP500C_Config SCP500C { get; set; } = new();

        [YamlIgnore]
        public SCI.Config.SCP500A_Config SCP500A { get; set; } = new();

        [YamlIgnore]
        public SCI.Config.SCP500B_Config SCP500B { get; set; } = new();

        [YamlIgnore]
        public SCI.Config.SCP500D_Config SCP500D { get; set; } = new();

        [YamlIgnore]
        public SCI.Config.ClusterGrenadeConfig ClusterGrenade { get; set; } = new();

        [YamlIgnore]
        public SCI.Config.ImpactGrenadeConfig ImpactGrenade { get; set; } = new();

        [YamlIgnore]
        public SCI.Config.SmokeGrenadeConfig SmokeGrenade { get; set; } = new();

        [YamlIgnore]
        public SCI.Config.RailgunConfig Railgun { get; set; } = new();

        [YamlIgnore]
        public SCI.Config.GrenadeLauncherConfig GrenadeLauncher { get; set; } = new();

        [YamlIgnore]
        public SCI.Config.BioGrenadeConfig BioGrenade { get; set; } = new();

        // To be enabled when implemented:
        //[YamlIgnore]
        //public SCI.Config.HackingChipConfig HackingChip { get; set; } = new();
        //[YamlIgnore]
        //public SCI.Config.ReinforcementCallConfig ReinforcementCall { get; set; } = new();
    }
}
