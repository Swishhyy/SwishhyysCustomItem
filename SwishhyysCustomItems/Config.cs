// Include the interfaces and classes necessary for configuring the plugin.
using Exiled.API.Interfaces;          // Provides the IConfig interface, which is used by EXILED for configuration files.
using SCI.Custom.Config;              // Imports the custom configuration classes for specific plugin features.
using System.ComponentModel;         // Allows the use of attributes (like Description) for providing metadata about the properties.

namespace SCI
{
    // The Config class implements the IConfig interface, making it the container for our plugin's configuration settings.
    public class Config : IConfig
    {
        // Flag to determine whether the plugin should be enabled.
        // The default value is set to true, meaning the plugin is enabled unless specified otherwise.
        [Description("Whether the plugin is enabled.")]
        public bool IsEnabled { get; set; } = true;

        // Flag for enabling or disabling debug messages in the console.
        // By default, debugging is off.
        [Description("Whether debug messages should be shown in the console.")]
        public bool Debug { get; set; } = false;

        // The Discord webhook URL for sending command usages to a Discord channel.
        [Description("Displays Command Usages in Discord.")]
        public string DiscordWebhook { get; set; } = "";

        // Configuration settings for the Expired SCP-500 Pills.
        // This property is of the custom type ExpiredSCP500PillsConfig and is initialized with default values.
        [Description("Configuration for Expired SCP-500 Pills")]
        public ExpiredSCP500PillsConfig ExpiredSCP500 { get; set; } = new ExpiredSCP500PillsConfig();

        // Configuration settings for the Adrenaline SCP-500 Pills.
        // It uses a custom class for storing configuration specific to the Adrenaline variant.
        public AdrenalineSCP500PillsConfig AdrenalineSCP500 { get; set; } = new AdrenalineSCP500PillsConfig();

        // Configuration settings for the Suicide SCP-500 Pills.
        // This property also uses a corresponding custom configuration class.
        public SuicideSCP500PillsConfig SuicideSCP500 { get; set; } = new SuicideSCP500PillsConfig();

        [Description("Configuration for Cluster Grenade")]
        public ClusterGrenadeConfig ClusterGrenade { get; set; } = new ClusterGrenadeConfig();

        
        [Description("Configuration for Impact Grenade")]
        public ImpactGrenadeConfig ImpactGrenade { get; set; } = new ImpactGrenadeConfig();
        
        [Description("Configuration for Smoke Grenades")]
        public SmokeGrenadeConfig SmokeGrenade { get; set; } = new SmokeGrenadeConfig();

        // Additional modular configurations can be added here in the future.
        // For example, you can uncomment and define another feature configuration as needed.
        // [Description("Configuration for another feature")]
        // public AnotherFeatureConfig AnotherFeature { get; set; } = new AnotherFeatureConfig();
    }
}
