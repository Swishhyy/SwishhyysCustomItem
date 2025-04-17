namespace SCI
{
    // Include the necessary namespaces.
    using Exiled.API.Features;              // Core Exiled API features for plugins.
    using Exiled.CustomItems.API;           // API for registering and managing custom items.
    using SCI.Custom.MedicalItems;          // Custom namespace containing the custom medical items.
    using System;                           // Provides fundamental classes and base classes.               // For configuration access
    using SCI.Services;                     // For WebhookService
    using SCI.Custom.Items.Grenades;
    using SCI.Custom.Throwables;           // Add this for the ImpactGrenade
    using SCI.Custom.Weapon;
    using SCI.Custom.Misc;

    // Define the main plugin class which extends Exiled's Plugin base class using a generic Config type.
    public class Plugin : Plugin<SCI.Custom.Config.Config>
    {
        // Override the plugin's name property.
        public override string Name => "SCI";
        // Override the plugin's author property.
        public override string Author => "Swishhyy";
        // Override the plugin's version property.
        public override Version Version => new(3, 1, 0);

        // Public static instance for global access (singleton pattern)
        public static Plugin Instance { get; private set; }

        // Public webhook service property
        public WebhookService WebhookService { get; private set; }

        // Private fields to store instances of the custom item classes.
        private ExpiredSCP500Pills _expiredSCP500Pills;
        private AdrenalineSCP500Pills _adrenalineSCP500Pills;
        private SuicideSCP500Pills _suicideSCP500Pills;
        private VanishingSCP500Pills _vanishingSCP500Pills;
        private ClusterGrenade _clusterGrenade;
        private ImpactGrenade _impactGrenade;
        private SmokeGrenade _smokeGrenade;
        private Railgun _railgun;
        private GrenadeLauncher _grenadeLauncher;
        private BioGrenade _bioGrenade;
        //private HackingChip _hackingChip;
        private ReinforcementCall _reinforcementCall;

        // Define the minimum required version of the Exiled framework to run this plugin.
        private readonly Version requiredExiledVersion = new(9, 5, 1);

        // Helper method for debug logging throughout the plugin
        public void DebugLog(string message)
        {
            if (Config.Debug)
            {
                Log.Debug($"[SCI Debug] {message}");
            }
        }

        // This method is called when the plugin is enabled.
        public override void OnEnabled()
        {
            // Set the singleton instance
            Instance = this;

            // Check if the current Exiled framework version meets the minimum required version.
            if (Exiled.Loader.Loader.Version < requiredExiledVersion)
            {
                // Log an error and return early to avoid running on an unsupported version.
                Log.Error($"{Name} requires Exiled version {requiredExiledVersion} or higher. Current version: {Exiled.Loader.Loader.Version}");
                return;
            }

            // Log that the plugin has been enabled.
            Log.Info($"{Name} has been enabled!");
            DebugLog("OnEnabled method called");

            // Call the base implementation to ensure any base setup is performed.
            base.OnEnabled();

            // Generate individual config files on startup
            DebugLog("Generating individual config files");
            Utilities.ConfigWriter.GenerateAllConfigs(Config);

            // Load configs from individual files (this will override the default values)
            DebugLog("Loading individual config files");
            Utilities.ConfigWriter.LoadAllConfigs(Config);

            // Initialize WebhookService
            DebugLog("Initializing WebhookService");
            WebhookService = new WebhookService(Config.DiscordWebhook, Config.Debug);
            DebugLog("WebhookService initialized");

            // Create instances of the custom items using their corresponding configuration sections.
            // Explicitly cast each config object to its proper type
            DebugLog("Creating custom item instances with configuration");
            _expiredSCP500Pills = new ExpiredSCP500Pills((SCI.Config.ExpiredSCP500PillsConfig)Config.ExpiredSCP500);
            _adrenalineSCP500Pills = new AdrenalineSCP500Pills((SCI.Config.AdrenalineSCP500PillsConfig)Config.AdrenalineSCP500);
            _suicideSCP500Pills = new SuicideSCP500Pills((SCI.Config.SuicideSCP500PillsConfig)Config.SuicideSCP500);
            _clusterGrenade = new ClusterGrenade((SCI.Config.ClusterGrenadeConfig)Config.ClusterGrenade);
            _impactGrenade = new ImpactGrenade((SCI.Config.ImpactGrenadeConfig)Config.ImpactGrenade);
            _smokeGrenade = new SmokeGrenade((SCI.Config.SmokeGrenadeConfig)Config.SmokeGrenade);
            _railgun = new Railgun((SCI.Config.RailgunConfig)Config.Railgun);
            _grenadeLauncher = new GrenadeLauncher((SCI.Config.GrenadeLauncherConfig)Config.GrenadeLauncher);
            _vanishingSCP500Pills = new VanishingSCP500Pills((SCI.Config.VanishingSCP500PillsConfig)Config.VanishingSCP500);
            _bioGrenade = new BioGrenade((SCI.Config.BioGrenadeConfig)Config.BioGrenade);
            //_hackingChip = new HackingChip((SCI.Config.HackingChipConfig)Config.HackingChip);
            _reinforcementCall = new ReinforcementCall((SCI.Config.ReinforcementCallConfig)Config.ReinforcementCall);

            // Register the custom items with the Exiled framework so that they are recognized in-game.
            DebugLog("Registering custom items");
            _expiredSCP500Pills.Register();
            _adrenalineSCP500Pills.Register();
            _suicideSCP500Pills.Register();
            _clusterGrenade.Register();
            _impactGrenade.Register();
            _smokeGrenade.Register();
            _railgun.Register();
            _grenadeLauncher.Register();
            _vanishingSCP500Pills.Register();
            _bioGrenade.Register();
            //_hackingChip.Register();
            _reinforcementCall.Register();

            // Log a debug message listing the registered custom items.
            Log.Debug($"Registered {Name} custom items: Expired SCP-500 Pills, Adrenaline Pills, Suicide Pills, Cluster Grenade, Impact Grenade, Smoke Grenade, Railgun, Grenade Launcher");

            DebugLog("OnEnabled method completed successfully");
        }

        // This method is called when the plugin is disabled.
        public override void OnDisabled()
        {
            DebugLog("OnDisabled method called");

            // Unregister each custom item if they have been initialized (using the null-conditional operator).
            DebugLog("Unregistering custom items");
            _expiredSCP500Pills?.Unregister();
            _adrenalineSCP500Pills?.Unregister();
            _suicideSCP500Pills?.Unregister();
            _clusterGrenade?.Unregister();
            _impactGrenade?.Unregister();
            _smokeGrenade?.Unregister();
            _railgun?.Unregister();
            _grenadeLauncher?.Unregister();
            _vanishingSCP500Pills?.Unregister();
            _bioGrenade?.Unregister();
            //_hackingChip?.Unregister();
            _reinforcementCall?.Unregister();

            // Set the custom item instances to null to free resources.
            _expiredSCP500Pills = null;
            _adrenalineSCP500Pills = null;
            _suicideSCP500Pills = null;
            _clusterGrenade = null;
            _impactGrenade = null;
            _smokeGrenade = null;
            _railgun = null;
            _grenadeLauncher = null;
            _vanishingSCP500Pills = null;
            _bioGrenade = null;
            //_hackingChip = null;
            _reinforcementCall = null;

            // Unregister the WebhookService to clean up resources.
            WebhookService = null;

            // Log that the plugin has been disabled.
            Log.Info($"{Name} has been disabled!");

            // Clear the singleton instance
            Instance = null;

            DebugLog("OnDisabled method completed");
        }
    }
}