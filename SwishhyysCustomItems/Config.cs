namespace SCI.Custom.Config  // Use your existing namespace
{
    using System.ComponentModel;
    using Exiled.API.Interfaces;
    using SCI.Config;

    public class Config : IConfig
    {
        [Description("Whether the plugin is enabled or not")]
        public bool IsEnabled { get; set; } = true;

        [Description("Whether to enable debug logging")]
        public bool Debug { get; set; } = false;

        [Description("Discord webhook URL for notifications")]
        public string DiscordWebhook { get; set; } = string.Empty;

        public AdrenalineSCP500PillsConfig AdrenalineSCP500 { get; set; } = new AdrenalineSCP500PillsConfig();
        public SuicideSCP500PillsConfig SuicideSCP500 { get; set; } = new SuicideSCP500PillsConfig();
        public ClusterGrenadeConfig ClusterGrenade { get; set; } = new ClusterGrenadeConfig();
        public ImpactGrenadeConfig ImpactGrenade { get; set; } = new ImpactGrenadeConfig();
        public SmokeGrenadeConfig SmokeGrenade { get; set; } = new SmokeGrenadeConfig();
        public RailgunConfig Railgun { get; set; } = new RailgunConfig();
        public GrenadeLauncherConfig GrenadeLauncher { get; set; } = new GrenadeLauncherConfig();
        public ExpiredSCP500PillsConfig ExpiredSCP500 { get; set; } = new ExpiredSCP500PillsConfig();

    }
}
