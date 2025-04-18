using Exiled.API.Features;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SCI.Config;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SCI.Utilities
{
    public static class ConfigWriter
    {
        private static readonly string BaseConfigPath = Path.Combine(Paths.Configs, "SCI");

        // Category mappings based on filename patterns
        private static readonly Dictionary<string, string> CategoryPatterns = new()
        {
            { @"(?i)gun|weapon|launcher|railgun", "weapons" },
            { @"(?i)grenade|throwable|impact|cluster|smoke|bio", "throwables" },
            { @"(?i)scp500|pill|medical|adrenaline|heal|anti096", "medical" },
            { @"(?i)armor|wearable|vest|helmet", "wearables" },
            { @"(?i)misc|chip|call|utility", "misc" }
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
                string category = DetermineCategoryFromName(prop.Name);
                object configObj = prop.GetValue(mainConfig);
                if (configObj != null)
                {
                    GenerateConfig(prop.Name, configObj, category);
                }
            }

            Log.Info("Config generation completed");
        }

        /// <summary>
        /// Determines the category for a config item based on its name
        /// </summary>
        private static string DetermineCategoryFromName(string name)
        {
            foreach (var pattern in CategoryPatterns)
            {
                if (Regex.IsMatch(name, pattern.Key))
                {
                    return pattern.Value;
                }
            }
            return "misc"; // Default category
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
                // Get all categories (directories)
                var categoryDirs = Directory.GetDirectories(BaseConfigPath);
                if (categoryDirs.Length == 0)
                {
                    Log.Info("No category directories found. Looking for files in the base directory.");
                    LoadConfigsFromDirectory(BaseConfigPath, mainConfig);
                }
                else
                {
                    // Process each category directory
                    foreach (var categoryDir in categoryDirs)
                    {
                        LoadConfigsFromDirectory(categoryDir, mainConfig);
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
        /// Loads all configuration files from a specific directory
        /// </summary>
        private static void LoadConfigsFromDirectory(string directory, SCI.Custom.Config.Config mainConfig)
        {
            string category = Path.GetFileName(directory);
            var configFiles = Directory.GetFiles(directory, "*.yml");

            Log.Debug($"Found {configFiles.Length} config files in {category}");

            foreach (var configFile in configFiles)
            {
                string configName = Path.GetFileNameWithoutExtension(configFile);
                Log.Debug($"Processing config file: {configName}");

                // Try to find a matching property in the main config
                var prop = FindMatchingConfigProperty(configName);
                if (prop != null)
                {
                    try
                    {
                        // Get the property type
                        Type propType = prop.PropertyType;
                        object defaultValue = prop.GetValue(mainConfig);

                        // Load the config using the appropriate type
                        object loadedConfig = LoadConfigGeneric(propType, configName, category);

                        if (loadedConfig != null)
                        {
                            // Merge with defaults
                            MergeConfigsGeneric(propType, loadedConfig, defaultValue);

                            // Set the property value
                            prop.SetValue(mainConfig, loadedConfig);
                            Log.Debug($"Successfully loaded config: {configName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error loading config {configName}: {ex.Message}");
                    }
                }
                else
                {
                    Log.Debug($"No matching property found for config: {configName}");
                }
            }
        }

        /// <summary>
        /// Finds a property in the main config that matches the config filename
        /// </summary>
        private static PropertyInfo FindMatchingConfigProperty(string configName)
        {
            // Check exact match first
            var exactMatch = typeof(SCI.Custom.Config.Config).GetProperty(configName);
            if (exactMatch != null)
                return exactMatch;

            // Try normalized name matching (ignore case, remove special characters)
            string normalizedName = NormalizeName(configName);
            var properties = typeof(SCI.Custom.Config.Config).GetProperties();

            foreach (var prop in properties)
            {
                if (NormalizeName(prop.Name).Equals(normalizedName, StringComparison.OrdinalIgnoreCase))
                {
                    return prop;
                }
            }

            // Try fuzzy matching
            foreach (var prop in properties)
            {
                // Check if property name contains the config name or vice versa
                if (prop.Name.Contains(configName, StringComparison.OrdinalIgnoreCase) ||
                    configName.Contains(prop.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return prop;
                }
            }

            return null;
        }

        /// <summary>
        /// Normalizes a name for comparison (removes special characters, makes lowercase)
        /// </summary>
        private static string NormalizeName(string name)
        {
            return Regex.Replace(name, @"[^a-zA-Z0-9]", "").ToLowerInvariant();
        }

        /// <summary>
        /// Generic method to load a config file with the appropriate type
        /// </summary>
        private static object LoadConfigGeneric(Type configType, string itemName, string category)
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

                // Deserialize the YAML to the appropriate type
                object result = deserializer.Deserialize(yaml, configType);

                // Create a new instance if deserialization returned null
                if (result == null)
                {
                    Log.Debug($"Deserialized {itemName} config was null, creating new instance");
                    result = Activator.CreateInstance(configType);
                }

                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"Error loading config for {itemName}: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Generic method to merge configs to ensure no null collections
        /// </summary>
        private static void MergeConfigsGeneric(Type configType, object loadedConfig, object defaultConfig)
        {
            if (loadedConfig == null || defaultConfig == null)
                return;

            var properties = configType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

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
                    Log.Debug($"Replaced null {prop.Name} with default value in {configType.Name}");
                }
                // Special handling for dictionary properties to avoid nulls
                else if (prop.PropertyType.IsGenericType &&
                         (prop.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>) ||
                          prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    if (loadedValue == null)
                    {
                        prop.SetValue(loadedConfig, defaultValue);
                        Log.Debug($"Used default collection for {prop.Name} in {configType.Name}");
                    }
                }
            }
        }

        /// <summary>
        /// Generic method to generate a config file
        /// </summary>
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

        /// <summary>
        /// Generic method for strong-typed config loading, preserved for compatibility
        /// </summary>
        private static T LoadConfig<T>(string itemName, string category) where T : class
        {
            return LoadConfigGeneric(typeof(T), itemName, category) as T;
        }

        /// <summary>
        /// Generic method for strong-typed config merging, preserved for compatibility
        /// </summary>
        private static void MergeConfigs<T>(T loadedConfig, T defaultConfig) where T : class
        {
            MergeConfigsGeneric(typeof(T), loadedConfig, defaultConfig);
        }
    }
}
