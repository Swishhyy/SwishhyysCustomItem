using Exiled.API.Interfaces;
using SCI.Custom.Config;
using System.ComponentModel;

namespace SCI
{
    public class Config : IConfig
    {
        [Description("Whether the plugin is enabled.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Whether debug messages should be shown in the console.")]
        public bool Debug { get; set; } = false;

        [Description("Configuration for Expired SCP-500 Pills")]
        public ExpiredSCP500PillsConfig ExpiredSCP500 { get; set; } = new ExpiredSCP500PillsConfig();
        public AdrenalineSCP500PillsConfig AdrenalineSCP500 { get; set; } = new AdrenalineSCP500PillsConfig();
        public SuicideSCP500PillsConfig SuicideSCP500 { get; set; } = new SuicideSCP500PillsConfig();

        // You can add more modular configs here in the future
        // [Description("Configuration for another feature")]
        // public AnotherFeatureConfig AnotherFeature { get; set; } = new AnotherFeatureConfig();
    }
}