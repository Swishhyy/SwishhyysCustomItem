namespace SCI
{
    // Include the necessary namespaces.
    using Exiled.API.Features;              // Core Exiled API features for plugins.
    using Exiled.CustomItems.API;           // API for registering and managing custom items.
    using SCI.Custom.MedicalItems;          // Custom namespace containing the custom medical items.
    using System;                           // Provides fundamental classes and base classes.
    using SCI.Custom.Config;                // For configuration access
    using SCI.Services;                     // For WebhookService
    using SCI.Custom.Items.Grenades;
    using SCI.Custom.Throwables;           // Add this for the ImpactGrenade
    using SCI.Custom.Weapon;

    // Define the main plugin class which extends Exiled's Plugin base class using a generic Config type.
    public class Plugin : Plugin<SCI.Custom.Config.Config>
    {
        // Override the plugin's name property.
        public override string Name => "SCI";
        // Override the plugin's author property.
        public override string Author => "Swishhyy";
        // Override the plugin's version property.
        public override Version Version => new Version(2, 0, 0);

        // Public static instance for global access (singleton pattern)
        public static Plugin Instance { get; private set; }

        // Public webhook service property
        public WebhookService WebhookService { get; private set; }

        // Private fields to store instances of the custom item classes.
        private ExpiredSCP500Pills _expiredSCP500Pills;
        private AdrenalineSCP500Pills _adrenalineSCP500Pills;
        private SuicideSCP500Pills _suicideSCP500Pills;
        private ClusterGrenade _clusterGrenade;
        private ImpactGrenade _impactGrenade;
        private SmokeGrenade _smokeGrenade;
        private Railgun _railgun;

        // Define the minimum required version of the Exiled framework to run this plugin.
        private readonly Version requiredExiledVersion = new Version(9, 5, 1);

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

            // Log that the plugin has been successfully enabled.
            Log.Info($"{Name} has been enabled!");
            DebugLog("OnEnabled method called");

            // Call the base implementation to ensure any base setup is performed.
            base.OnEnabled();

            // Initialize WebhookService
            DebugLog("Initializing WebhookService");
            WebhookService = new WebhookService(Config.DiscordWebhook, Config.Debug);
            DebugLog("WebhookService initialized");

            // Create instances of the custom items using their corresponding configuration sections.
            DebugLog("Creating custom item instances with configuration");
            _expiredSCP500Pills = new ExpiredSCP500Pills(Config.ExpiredSCP500);
            _adrenalineSCP500Pills = new AdrenalineSCP500Pills(Config.AdrenalineSCP500);
            _suicideSCP500Pills = new SuicideSCP500Pills(Config.SuicideSCP500);
            _clusterGrenade = new ClusterGrenade(Config.ClusterGrenade);
            _impactGrenade = new ImpactGrenade(Config.ImpactGrenade);
            _smokeGrenade = new SmokeGrenade(Config.SmokeGrenade);
            _railgun = new Railgun(Config.Railgun);

            // Register the custom items with the Exiled framework so that they are recognized in-game.
            DebugLog("Registering custom items");
            _expiredSCP500Pills.Register();
            _adrenalineSCP500Pills.Register();
            _suicideSCP500Pills.Register();
            _clusterGrenade.Register();
            _impactGrenade.Register();
            _smokeGrenade.Register();
            _railgun.Register();

            // Log a debug message listing the registered custom items.
            Log.Debug($"Registered {Name} custom items: Expired SCP-500 Pills, Adrenaline Pills, Suicide Pills, Cluster Grenade, Impact Grenade");

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
            _railgun.Unregister();

            // Set the custom item instances to null to free resources.
            _expiredSCP500Pills = null;
            _adrenalineSCP500Pills = null;
            _suicideSCP500Pills = null;
            _clusterGrenade = null;
            _impactGrenade = null;
            _smokeGrenade = null;
            _railgun = null;

            WebhookService = null;

            // Log that the plugin has been disabled.
            Log.Info($"{Name} has been disabled!");

            // Clear the singleton instance
            Instance = null;

            DebugLog("OnDisabled method completed");
        }
    }
}