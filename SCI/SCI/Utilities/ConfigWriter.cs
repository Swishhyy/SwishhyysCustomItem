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

        // Define category mappings based on the namespaces and type names
        private static readonly Dictionary<string, string> CategoryMappings = new()
        {
            { "SCI.Config.WeaponsConfig", "weapons" },
            { "SCI.Config.ThrowablesConfig", "throwables" },
            { "SCI.Config.MedicalItemsConfig", "medical" },
            { "SCI.Config.MiscConfig", "misc" },
            { "SCI.Config.WearablesConfig", "wearables" }
        };

        // Type name patterns to determine categories
        private static readonly Dictionary<string, string> TypePatterns = new()
        {
            { "Weapon|Gun|Launcher|Railgun", "weapons" },
            { "Grenade|Throwable|Impact|Cluster|Smoke", "throwables" },
            { "SCP500|Pills|Medical|Adrenaline|Heal", "medical" },
            { "Armor|Wearable|Vest|Helmet", "wearables" }
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
            var typeNamespace = configType.Namespace ?? string.Empty;
            var typeName = configType.Name;

            Log.Debug($"Determining category for type {typeName} in namespace {typeNamespace}");

            // Check if it's defined in a specific namespace that maps to a category
            foreach (var mapping in CategoryMappings)
            {
                if (typeNamespace.StartsWith(mapping.Key))
                {
                    return mapping.Value;
                }
            }

            // If not found by namespace, check by type name patterns
            foreach (var pattern in TypePatterns)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(typeName, pattern.Key,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    return pattern.Value;
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

                // Add support for any other configs that might be in the directory
                LoadAdditionalConfigs(mainConfig);
            }
            catch (Exception ex)
            {
                Log.Error($"Error loading configs: {ex.Message}\n{ex.StackTrace}");
            }

            Log.Info("Config loading completed");
        }

        /// <summary>
        /// Loads any additional configs that might be in the directories but not explicitly handled
        /// </summary>
        private static void LoadAdditionalConfigs(SCI.Custom.Config.Config mainConfig)
        {
            try
            {
                // Get all category directories
                var categoryDirs = Directory.GetDirectories(BaseConfigPath);

                foreach (var categoryDir in categoryDirs)
                {
                    string category = Path.GetFileName(categoryDir);
                    var configFiles = Directory.GetFiles(categoryDir, "*.yml");

                    foreach (var configFile in configFiles)
                    {
                        string configName = Path.GetFileNameWithoutExtension(configFile);

                        // Skip files we've already explicitly handled
                        if (HasExplicitHandler(configName))
                            continue;

                        Log.Debug($"Attempting to load additional config: {configName} from category {category}");

                        // Try to find a matching property in the main config
                        var prop = typeof(SCI.Custom.Config.Config).GetProperty(configName);
                        if (prop != null)
                        {
                            // Get the property type and try to load the config
                            Type propType = prop.PropertyType;
                            object defaultValue = prop.GetValue(mainConfig);

                            try
                            {
                                // Use a generic method to load the config with the right type
                                var loadMethod = typeof(ConfigWriter).GetMethod("LoadConfig",
                                    BindingFlags.NonPublic | BindingFlags.Static)
                                    .MakeGenericMethod(propType);

                                object loadedConfig = loadMethod.Invoke(null, new object[] { configName, category });

                                if (loadedConfig != null)
                                {
                                    // Use the MergeConfigs method to ensure collections are preserved
                                    var mergeMethod = typeof(ConfigWriter).GetMethod("MergeConfigs",
                                        BindingFlags.NonPublic | BindingFlags.Static)
                                        .MakeGenericMethod(propType);

                                    mergeMethod.Invoke(null, new[] { loadedConfig, defaultValue });

                                    // Set the property value
                                    prop.SetValue(mainConfig, loadedConfig);
                                    Log.Debug($"Successfully loaded additional config: {configName}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Debug($"Error loading additional config {configName}: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error loading additional configs: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a config name is already handled explicitly in the LoadAllConfigs method
        /// </summary>
        private static bool HasExplicitHandler(string configName)
        {
            string[] explicitHandlers =
            {
                "ExpiredSCP500", "AdrenalineSCP500", "SuicideSCP500",
                "ClusterGrenade", "ImpactGrenade", "SmokeGrenade",
                "Railgun", "GrenadeLauncher"
            };

            return explicitHandlers.Contains(configName);
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
                         (prop.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>) ||
                          prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    if (loadedValue == null)
                    {
                        prop.SetValue(loadedConfig, defaultValue);
                        Log.Debug($"Used default collection for {prop.Name} in {typeof(T).Name}");
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
