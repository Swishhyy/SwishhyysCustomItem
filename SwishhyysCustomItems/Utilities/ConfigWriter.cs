using Exiled.API.Features;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SCI.Config;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Collections.Generic;

namespace SCI.Utilities
{
    public static class ConfigWriter
    {
        private static readonly string BaseConfigPath = Path.Combine(Paths.Configs, "SCI");

        // Define category mappings based on the namespaces
        private static readonly Dictionary<string, string> CategoryMappings = new Dictionary<string, string>
        {
            { "SCI.Config.WeaponsConfig", "weapons" },
            { "SCI.Config.ThrowablesConfig", "throwables" },
            { "SCI.Config.MedicalItemsConfig", "medical" },
            { "SCI.Config.MiscConfig", "misc" }
        };

        /// <summary>
        /// Generates individual configuration files for all custom items
        /// </summary>
        public static void GenerateAllConfigs(SCI.Custom.Config.Config mainConfig)
        {
            Log.Info("Generating individual config files for all items...");

            // Ensure the directory exists
            if (!Directory.Exists(BaseConfigPath))
            {
                Directory.CreateDirectory(BaseConfigPath);
                Log.Info($"Created directory: {BaseConfigPath}");
            }

            // Use reflection to get all config properties from mainConfig
            var configProperties = typeof(SCI.Custom.Config.Config)
                .GetProperties()
                .Where(p => p.Name != "IsEnabled" && p.Name != "Debug" && p.Name != "DiscordWebhook")
                .ToList();

            foreach (var prop in configProperties)
            {
                string category = GetCategoryFromConfigType(prop.PropertyType);
                object configObj = prop.GetValue(mainConfig);
                if (configObj != null)
                {
                    GenerateConfig(prop.Name, configObj, category);
                }
            }

            Log.Info("Config generation completed");
        }

        private static string GetCategoryFromConfigType(Type configType)
        {
            // First, try to determine category based on which file the type is defined in
            var typeAssemblyName = configType.Assembly.GetName().Name;
            var typeNamespace = configType.Namespace ?? string.Empty;
            var typeFullName = configType.FullName ?? string.Empty;

            Log.Debug($"Determining category for type {configType.Name} in namespace {typeNamespace}");

            // Check specific types first for special cases
            if (configType.Name == "GrenadeLauncherConfig")
            {
                return "weapons";
            }
            else if (configType.Name == "RailgunConfig")
            {
                return "weapons";
            }
            else if (configType.Name == "TacoBellStickConfig")
            {
                return "misc";
            }

            // Check if it's defined in a specific config file
            if (typeNamespace == "SCI.Config")
            {
                // Check based on naming patterns in the type name
                if (configType.Name.Contains("Weapon") || configType.Name.Contains("Gun") || configType.Name.Contains("Launcher"))
                {
                    return "weapons";
                }
                if (configType.Name.Contains("Grenade") || configType.Name.Contains("Throwable"))
                {
                    return "throwables";
                }
                if (configType.Name.Contains("SCP500") || configType.Name.Contains("Pills") || configType.Name.Contains("Medical"))
                {
                    return "medical";
                }
            }

            // Default to misc if we can't determine otherwise
            return "misc";
        }

        /// <summary>
        /// Loads all individual config files and updates the main config object
        /// </summary>
        public static void LoadAllConfigs(SCI.Custom.Config.Config mainConfig)
        {
            Log.Info("Loading individual config files...");

            if (!Directory.Exists(BaseConfigPath))
            {
                Log.Info($"Config directory does not exist: {BaseConfigPath}. Using defaults.");
                return;
            }

            try
            {
                // Medical Items
                var expiredConfig = LoadConfig<ExpiredSCP500PillsConfig>("ExpiredSCP500", "medical");
                if (expiredConfig != null)
                {
                    // Make sure we preserve any default collections that might be null after deserialization
                    MergeConfigs(expiredConfig, mainConfig.ExpiredSCP500);
                    mainConfig.ExpiredSCP500 = expiredConfig;
                }

                var adrenalineConfig = LoadConfig<AdrenalineSCP500PillsConfig>("AdrenalineSCP500", "medical");
                if (adrenalineConfig != null)
                {
                    MergeConfigs(adrenalineConfig, mainConfig.AdrenalineSCP500);
                    mainConfig.AdrenalineSCP500 = adrenalineConfig;
                }

                var suicideConfig = LoadConfig<SuicideSCP500PillsConfig>("SuicideSCP500", "medical");
                if (suicideConfig != null)
                {
                    MergeConfigs(suicideConfig, mainConfig.SuicideSCP500);
                    mainConfig.SuicideSCP500 = suicideConfig;
                }

                // Throwables
                var clusterConfig = LoadConfig<ClusterGrenadeConfig>("ClusterGrenade", "throwables");
                if (clusterConfig != null)
                {
                    MergeConfigs(clusterConfig, mainConfig.ClusterGrenade);
                    mainConfig.ClusterGrenade = clusterConfig;
                }

                var impactConfig = LoadConfig<ImpactGrenadeConfig>("ImpactGrenade", "throwables");
                if (impactConfig != null)
                {
                    MergeConfigs(impactConfig, mainConfig.ImpactGrenade);
                    mainConfig.ImpactGrenade = impactConfig;
                }

                var smokeConfig = LoadConfig<SmokeGrenadeConfig>("SmokeGrenade", "throwables");
                if (smokeConfig != null)
                {
                    MergeConfigs(smokeConfig, mainConfig.SmokeGrenade);
                    mainConfig.SmokeGrenade = smokeConfig;
                }

                // Weapons
                var railgunConfig = LoadConfig<RailgunConfig>("Railgun", "weapons");
                if (railgunConfig != null)
                {
                    MergeConfigs(railgunConfig, mainConfig.Railgun);
                    mainConfig.Railgun = railgunConfig;
                }

                var grenadeLauncherConfig = LoadConfig<GrenadeLauncherConfig>("GrenadeLauncher", "weapons");
                if (grenadeLauncherConfig != null)
                {
                    MergeConfigs(grenadeLauncherConfig, mainConfig.GrenadeLauncher);
                    mainConfig.GrenadeLauncher = grenadeLauncherConfig;
                }

                // Misc Items - Load static properties
                var tacoBellConfig = LoadConfig<TacoBellStickConfig>("TacoBellStick", "misc");
                if (tacoBellConfig != null)
                {
                    MergeConfigs(tacoBellConfig, mainConfig.TacoBellStick);
                    mainConfig.TacoBellStick = tacoBellConfig;

                    // Handle static properties
                    var properties = typeof(TacoBellStickConfig).GetProperties(BindingFlags.Public | BindingFlags.Static);
                    foreach (var property in properties)
                    {
                        var loadedValue = property.GetValue(tacoBellConfig);
                        if (loadedValue != null)
                        {
                            property.SetValue(null, loadedValue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error loading configs: {ex.Message}\n{ex.StackTrace}");
            }

            Log.Info("Config loading completed");
        }

        /// <summary>
        /// Merges properties from loaded config into default config to ensure no null collections
        /// </summary>
        private static void MergeConfigs<T>(T loadedConfig, T defaultConfig) where T : class
        {
            if (loadedConfig == null || defaultConfig == null)
                return;

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                var loadedValue = prop.GetValue(loadedConfig);
                var defaultValue = prop.GetValue(defaultConfig);

                // If the loaded property is null but default has a value, use the default
                if (loadedValue == null && defaultValue != null)
                {
                    prop.SetValue(loadedConfig, defaultValue);
                    Log.Debug($"Replaced null {prop.Name} with default value in {typeof(T).Name}");
                }
                // Special handling for dictionary properties to avoid nulls
                else if (prop.PropertyType.IsGenericType &&
                         prop.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    if (loadedValue == null)
                    {
                        prop.SetValue(loadedConfig, defaultValue);
                        Log.Debug($"Used default dictionary for {prop.Name} in {typeof(T).Name}");
                    }
                }
            }
        }

        private static void GenerateConfig<T>(string itemName, T config, string category) where T : class
        {
            try
            {
                string categoryPath = Path.Combine(BaseConfigPath, category);
                if (!Directory.Exists(categoryPath))
                {
                    Directory.CreateDirectory(categoryPath);
                }

                string configPath = Path.Combine(categoryPath, $"{itemName}.yml");

                // Create a YAML serializer with proper settings
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                    .Build();

                // Serialize the object to YAML
                string yaml = serializer.Serialize(config);

                // Add a comment to the YAML file as a text header
                string yamlWithComment = $"# {itemName} configuration - Last updated: {DateTime.Now}\n{yaml}";

                File.WriteAllText(configPath, yamlWithComment);
                Log.Debug($"Generated config file: {configPath} in category: {category}");
            }
            catch (Exception ex)
            {
                Log.Error($"Error generating config for {itemName}: {ex.Message}");
            }
        }

        private static T LoadConfig<T>(string itemName, string category) where T : class
        {
            try
            {
                string configPath = Path.Combine(BaseConfigPath, category, $"{itemName}.yml");

                if (!File.Exists(configPath))
                {
                    Log.Debug($"Config file does not exist: {configPath}");
                    return null;
                }

                string yaml = File.ReadAllText(configPath);

                // Create a YAML deserializer
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();

                // Deserialize the YAML
                T result = deserializer.Deserialize<T>(yaml);

                // Create a new instance if deserialization returned null
                if (result == null)
                {
                    Log.Debug($"Deserialized {itemName} config was null, creating new instance");
                    result = Activator.CreateInstance<T>();
                }

                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"Error loading config for {itemName}: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }
    }
}
