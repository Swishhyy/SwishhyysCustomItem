namespace SCI
{
    // Include the necessary namespaces.
    using Exiled.API.Features;              // Core Exiled API features for plugins.
    using Exiled.CustomItems.API;           // API for registering and managing custom items.
    using SCI.Custom.MedicalItems;          // Custom namespace containing the custom medical items.
    using System;                         // Provides fundamental classes and base classes.
    using SCI.Custom.Config;              // For configuration access

    // Define the main plugin class which extends Exiled's Plugin base class using a generic Config type.
    public class Plugin : Plugin<Config>
    {
        // Override the plugin's name property.
        public override string Name => "SCI";
        // Override the plugin's author property.
        public override string Author => "Swishhyy";
        // Override the plugin's version property.
        public override Version Version => new Version(2, 0, 0);

        // Public static instance for global access (singleton pattern)
        public static Plugin Instance { get; private set; }

        // Private fields to store instances of the custom item classes.
        private ExpiredSCP500Pills _expiredSCP500Pills;
        private AdrenalineSCP500Pills _adrenalineSCP500Pills;
        private SuicideSCP500Pills _suicideSCP500Pills;

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

            // Create instances of the custom items using their corresponding configuration sections.
            DebugLog("Creating custom item instances with configuration");
            _expiredSCP500Pills = new ExpiredSCP500Pills(Config.ExpiredSCP500);
            _adrenalineSCP500Pills = new AdrenalineSCP500Pills(Config.AdrenalineSCP500);
            _suicideSCP500Pills = new SuicideSCP500Pills(Config.SuicideSCP500);

            // Register the custom items with the Exiled framework so that they are recognized in-game.
            DebugLog("Registering custom items");
            _expiredSCP500Pills.Register();
            _adrenalineSCP500Pills.Register();
            _suicideSCP500Pills.Register();

            // Log a debug message listing the registered custom items.
            Log.Debug($"Registered {Name} custom items: Expired SCP-500 Pills, Adrenaline Pills, Suicide Pills");

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

            // Set the custom item instances to null to free resources.
            _expiredSCP500Pills = null;
            _adrenalineSCP500Pills = null;
            _suicideSCP500Pills = null;

            // Log that the plugin has been disabled.
            Log.Info($"{Name} has been disabled!");

            // Clear the singleton instance
            Instance = null;

            DebugLog("OnDisabled method completed");
        }
    }
}
