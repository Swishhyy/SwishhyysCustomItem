namespace SCI
{
    using Exiled.API.Features;
    using Exiled.CustomItems.API;
    using Exiled.CustomItems.API.Features;
    using SCI.Config;
    using SCI.Custom.Items.Grenades;
    using SCI.Custom.MedicalItems;
    using SCI.Custom.Throwables;
    using SCI.Custom.Weapon;
    using SCI.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Main plugin class for SCI (Swishhyy's Custom Items)
    /// </summary>
    public class Plugin : Plugin<SCI.Custom.Config.Config>
    {
        #region Properties and Fields
        public override string Name => "SCI"; /// Gets the plugin name
        public override string Author => "Swishhyy"; // Gets the plugin author
        public override Version Version => new(3, 1, 0);// Gets the plugin version
        public static Plugin Instance { get; private set; } // Singleton instance for global access
        public WebhookService WebhookService { get; private set; } // Discord webhook service
        private readonly Version _requiredExiledVersion = new(9, 5, 1);  // Minimum required Exiled version
        private readonly Dictionary<string, object> _customItems = new(); // Dictionary to store custom items (more maintainable than individual fields)
        #endregion

        #region Helper Methods
        // Logs debug messages if debug mode is enabled
        public void DebugLog(string message)
        {
            if (Config.Debug)
            {
                Log.Debug($"[SCI Debug] {message}");
            }
        }

        // Gets a custom item from the dictionary
        private T GetItem<T>(string key) where T : class
        {
            return _customItems.TryGetValue(key, out object item) ? item as T : null;
        }
        #endregion

        #region Lifecycle Methods
        // Called when the plugin is enabled
        public override void OnEnabled()
        {
            // Set the singleton instance
            Instance = this;

            try
            {
                // Version check
                if (Exiled.Loader.Loader.Version < _requiredExiledVersion)
                {
                    Log.Error($"{Name} requires Exiled version {_requiredExiledVersion} or higher. Current version: {Exiled.Loader.Loader.Version}");
                    return;
                }

                Log.Info($"{Name} has been enabled!");
                DebugLog("OnEnabled method called");

                // Call base implementation
                base.OnEnabled();

                // Initialize configurations
                InitializeConfigurations();

                // Initialize services
                InitializeServices();

                // Create and register custom items
                InitializeCustomItems();

                DebugLog("OnEnabled method completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error($"Error enabling {Name}: {ex}");
            }
        }

        public override void OnDisabled() // Called when the plugin is disabled
        {
            try
            {
                DebugLog("OnDisabled method called");

                // Unregister custom items
                UnregisterAllItems();

                // Clean up services
                WebhookService = null;

                Log.Info($"{Name} has been disabled!");

                // Clear the singleton instance
                Instance = null;

                DebugLog("OnDisabled method completed");
            }
            catch (Exception ex)
            {
                Log.Error($"Error disabling {Name}: {ex}");
            }
        }
        #endregion

        #region Initialization Methods
        private void InitializeConfigurations() // Initializes configuration files
        {
            DebugLog("Generating individual config files");
            Utilities.ConfigWriter.GenerateAllConfigs(Config);

            DebugLog("Loading individual config files");
            Utilities.ConfigWriter.LoadAllConfigs(Config);
        }

        private void InitializeServices() // Initializes services
        {
            DebugLog("Initializing WebhookService");
            WebhookService = new WebhookService(Config.DiscordWebhook, Config.Debug);
            DebugLog("WebhookService initialized");
        }

        private void InitializeCustomItems() // Creates and registers all custom items
        {
            DebugLog("Creating custom item instances with configuration");

            _customItems["SCP500A"] = new SCP500A((SCP500A_Config)Config.SCP500A);
            _customItems["SCP500B"] = new SCP500B((SCP500B_Config)Config.SCP500B);
            _customItems["SCP500C"] = new SCP500C((SCP500C_Config)Config.SCP500C);
            _customItems["SCP500D"] = new SCP500D((SCP500D_Config)Config.SCP500D);
            _customItems["ClusterGrenade"] = new ClusterGrenade((ClusterGrenadeConfig)Config.ClusterGrenade);
            _customItems["ImpactGrenade"] = new ImpactGrenade((ImpactGrenadeConfig)Config.ImpactGrenade);
            _customItems["SmokeGrenade"] = new SmokeGrenade((SmokeGrenadeConfig)Config.SmokeGrenade);
            _customItems["BioGrenade"] = new BioGrenade((BioGrenadeConfig)Config.BioGrenade);
            _customItems["Railgun"] = new Railgun((RailgunConfig)Config.Railgun);
            _customItems["GrenadeLauncher"] = new GrenadeLauncher((GrenadeLauncherConfig)Config.GrenadeLauncher);
            //_customItems["HackingChip"] = new HackingChip((HackingChipConfig)Config.HackingChip);
            //_customItems["ReinforcementCall"] = new ReinforcementCall((ReinforcementCallConfig)Config.ReinforcementCall);

            RegisterEnabledItems(); // Register enabled items
        }

        private void RegisterEnabledItems() // Registers all enabled custom items
        {
            DebugLog("Registering custom items");

            RegisterItemIfEnabled<SCP500A>("SCP500A", Config.SCP500A.IsEnabled);
            RegisterItemIfEnabled<SCP500B>("SCP500B", Config.SCP500B.IsEnabled);
            RegisterItemIfEnabled<SCP500C>("SCP500C", Config.SCP500C.IsEnabled);
            RegisterItemIfEnabled<SCP500D>("SCP500D", Config.SCP500D.IsEnabled);
            RegisterItemIfEnabled<ClusterGrenade>("ClusterGrenade", Config.ClusterGrenade.IsEnabled);
            RegisterItemIfEnabled<ImpactGrenade>("ImpactGrenade", Config.ImpactGrenade.IsEnabled);
            RegisterItemIfEnabled<SmokeGrenade>("SmokeGrenade", Config.SmokeGrenade.IsEnabled);
            RegisterItemIfEnabled<BioGrenade>("BioGrenade", Config.BioGrenade.IsEnabled);
            RegisterItemIfEnabled<Railgun>("Railgun", Config.Railgun.IsEnabled);
            RegisterItemIfEnabled<GrenadeLauncher>("GrenadeLauncher", Config.GrenadeLauncher.IsEnabled);
            //RegisterItemIfEnabled<HackingChip>("HackingChip", Config.HackingChip.IsEnabled);
            //RegisterItemIfEnabled<ReinforcementCall>("ReinforcementCall", Config.ReinforcementCall.IsEnabled);

            LogRegisteredItems(); // Log registered items
        }

        private void RegisterItemIfEnabled<T>(string key, bool isEnabled) where T : CustomItem // Registers a specific item if it's enabled in config
        {
            if (isEnabled)
            {
                var item = GetItem<T>(key);
                item?.Register();
            }
        }

        private void LogRegisteredItems() // Logs all registered custom items
        {
            var registeredNames = _customItems.Keys
                .Where(k => k != "HackingChip" && k != "ReinforcementCall") // Skip commented items
                .Select(k => k.Replace("SCP500", "SCP-500 ").Replace("Grenade", " Grenade"))
                .ToList();

            Log.Debug($"Registered {Name} custom items: {string.Join(", ", registeredNames)}");
        }

        private void UnregisterAllItems() // Unregisters all custom items
        {
            DebugLog("Unregistering custom items");

            foreach (var kvp in _customItems) // Unregister each item
            {
                if (kvp.Value is CustomItem item)
                {
                    item.Unregister();
                }
            }
            _customItems.Clear(); // Clear the dictionary
        }
        #endregion
    }
}
