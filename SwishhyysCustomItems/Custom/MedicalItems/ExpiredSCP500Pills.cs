// Using directives: Bring in the necessary namespaces for game features, events, configurations, and Unity operations.
using Exiled.API.Enums;                              // Provides enumerations like DamageType and custom EffectType.
using Exiled.API.Features;                           // Core API features for interacting with players, logging, etc.
using Exiled.API.Features.Spawn;                     // Allows item spawn configurations.
using Exiled.CustomItems.API.Features;               // Base classes and features for custom items.
using Exiled.Events.EventArgs.Player;                // Event arguments for player-related events (like UsingItem).
using SCI.Custom.Config;                             // Custom configuration classes for this plugin.
using System;                                        // Provides basic system functionalities.
using System.Collections.Generic;                    // Provides generic collection types, such as Dictionary.
using System.Linq;                                   // Enables LINQ queries for collections.
using UnityEngine;                                   // Provides Unity classes for vectors, random numbers, math functions, etc.

namespace SCI.Custom.MedicalItems
{
    // This class defines an Expired SCP-500 Pills custom item.
    public class ExpiredSCP500Pills : CustomItem
    {
        // Unique identifier for this custom item.
        public override uint Id { get; set; } = 102;

        // Defines the in-game item type for this custom item.
        public override ItemType Type { get; set; } = ItemType.SCP500;

        // The displayed name for the custom item.
        public override string Name { get; set; } = "Expired SCP-500 Pills";

        // A short description of the custom item.
        public override string Description { get; set; } = "An expired bottle of SCP-500 pills with unpredictable effects.";

        // Weight of the item, which might affect in-game physics or inventory constraints.
        public override float Weight { get; set; } = 0.5f;

        // Controls the spawn properties for this item (such as spawn chance, location, etc.).
        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties();

        // Readonly field to hold the configuration for how this item behaves.
        private readonly ExpiredSCP500PillsConfig _config;

        // Constructor that receives a configuration object for customized behavior.
        public ExpiredSCP500Pills(ExpiredSCP500PillsConfig config)
        {
            Plugin.Instance?.DebugLog($"ExpiredSCP500Pills constructor with config called");
            _config = config;
            Plugin.Instance?.DebugLog($"ExpiredSCP500Pills initialized with config: DefaultEffectDuration={_config.DefaultEffectDuration}, CategoryChances count={_config?.CategoryChances?.Count ?? 0}");
        }

        // Default constructor for compatibility; initializes with a default configuration.
        public ExpiredSCP500Pills()
        {
            Plugin.Instance?.DebugLog("ExpiredSCP500Pills default constructor called");
            // Instantiate a default config if none is provided.
            _config = new ExpiredSCP500PillsConfig();
            Plugin.Instance?.DebugLog("ExpiredSCP500Pills initialized with default config");
        }

        // Subscribe to relevant events when the item is registered.
        protected override void SubscribeEvents()
        {
            Plugin.Instance?.DebugLog("ExpiredSCP500Pills.SubscribeEvents called");
            // Attach the OnUsingItem method to the UsingItem event.
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
            // Ensure any base class subscriptions are also executed.
            base.SubscribeEvents();
            Plugin.Instance?.DebugLog("ExpiredSCP500Pills event subscriptions completed");
        }

        // Unsubscribe from events when the item is unregistered to prevent memory leaks.
        protected override void UnsubscribeEvents()
        {
            Plugin.Instance?.DebugLog("ExpiredSCP500Pills.UnsubscribeEvents called");
            // Detach the OnUsingItem method from the UsingItem event.
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
            // Ensure any base class unsubscriptions are also processed.
            base.UnsubscribeEvents();
            Plugin.Instance?.DebugLog("ExpiredSCP500Pills event unsubscriptions completed");
        }

        // Event handler called when a player uses an item.
        public void OnUsingItem(UsingItemEventArgs ev)
        {
            Plugin.Instance?.DebugLog($"ExpiredSCP500Pills.OnUsingItem called for player: {ev.Player?.Nickname ?? "unknown"}");

            try
            {
                // Verify that the item being used is this custom item.
                if (!Check(ev.Player.CurrentItem))
                {
                    Plugin.Instance?.DebugLog("OnUsingItem: Item check failed, not our custom item");
                    return;
                }

                Plugin.Instance?.DebugLog("OnUsingItem: Item check passed, processing expired SCP-500 pills usage");

                // Ensure the player object is valid.
                if (ev.Player == null)
                {
                    Log.Error("ExpiredSCP500Pills: Player is null in OnUsingItem");
                    Plugin.Instance?.DebugLog("OnUsingItem: Player is null, aborting");
                    return;
                }

                // Check that the player's effects controller exists to safely apply effects.
                if (ev.Player.ReferenceHub?.playerEffectsController == null)
                {
                    Log.Error("ExpiredSCP500Pills: Player effect controller is null");
                    Plugin.Instance?.DebugLog("OnUsingItem: Player effect controller is null, aborting");
                    return;
                }

                // Determine which category of effects to apply based on a random selection weighted by config chances.
                Plugin.Instance?.DebugLog("OnUsingItem: Selecting random effect category");
                string category = GetRandomCategory();
                Plugin.Instance?.DebugLog($"OnUsingItem: Selected category: {category}");

                // From the category, pick a specific effect to apply.
                Plugin.Instance?.DebugLog($"OnUsingItem: Selecting random effect from category: {category}");
                EffectType effectType = GetRandomEffectFromCategory(category);
                Plugin.Instance?.DebugLog($"OnUsingItem: Selected effect: {effectType}");

                // Variables to track whether any effect was applied and what feedback message to show the player.
                bool effectApplied = false;
                string effectMessage = "";

                // If a valid effect was selected, attempt to apply it.
                if (effectType != EffectType.None)
                {
                    Plugin.Instance?.DebugLog($"OnUsingItem: Valid effect selected ({effectType}), attempting to apply");

                    try
                    {
                        // Retrieve the settings (intensity, duration, and chance) for the chosen effect.
                        Plugin.Instance?.DebugLog($"OnUsingItem: Getting settings for effect {effectType} in category {category}");
                        EffectSettings settings = GetEffectSettings(category, effectType);

                        if (settings != null)
                        {
                            Plugin.Instance?.DebugLog($"OnUsingItem: Settings found. MinIntensity={settings.MinIntensity}, MaxIntensity={settings.MaxIntensity}, CustomDuration={settings.CustomDuration}");

                            // Choose a random intensity within the range specified in the settings.
                            byte intensity = (byte)UnityEngine.Random.Range(settings.MinIntensity, settings.MaxIntensity + 1);
                            Plugin.Instance?.DebugLog($"OnUsingItem: Random intensity selected: {intensity}");

                            // Determine the duration to apply: use custom duration if specified; otherwise, use default duration.
                            float duration = settings.CustomDuration > 0 ? settings.CustomDuration : _config.DefaultEffectDuration;
                            Plugin.Instance?.DebugLog($"OnUsingItem: Effect duration will be: {duration} seconds");

                            // Apply the chosen effect with calculated intensity and duration.
                            Plugin.Instance?.DebugLog($"OnUsingItem: Applying effect {effectType} with intensity {intensity} for {duration} seconds to {ev.Player.Nickname}");
                            ev.Player.EnableEffect(effectType, intensity, duration);
                            effectApplied = true;

                            // Set a custom message based on the effect category.
                            if (category == "SCP" || category == "Negative")
                            {
                                effectMessage = $"You consumed an expired SCP-500 pill. You feel strange {effectType} effects.";
                                Plugin.Instance?.DebugLog($"OnUsingItem: Using negative/SCP effect message: {effectMessage}");
                            }
                            else
                            {
                                effectMessage = $"You consumed an expired SCP-500 pill. You feel {effectType} effects.";
                                Plugin.Instance?.DebugLog($"OnUsingItem: Using positive effect message: {effectMessage}");
                            }

                            // Log the effect application for debugging purposes.
                            Log.Debug($"Applied effect {effectType} (category: {category}) with intensity {intensity} to {ev.Player.Nickname}");
                        }
                        else
                        {
                            Plugin.Instance?.DebugLog($"OnUsingItem: No settings found for effect {effectType} in category {category}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log any errors encountered during effect application.
                        Log.Error($"ExpiredSCP500Pills: Error applying effect {effectType}: {ex.Message}");
                        Plugin.Instance?.DebugLog($"OnUsingItem: Exception while applying effect: {ex.Message}\n{ex.StackTrace}");
                    }
                }
                else
                {
                    Plugin.Instance?.DebugLog("OnUsingItem: No valid effect selected (EffectType.None)");
                }

                // If no effect was applied, use a fallback healing mechanism.
                if (!effectApplied)
                {
                    Plugin.Instance?.DebugLog("OnUsingItem: No effect was applied, using healing fallback");

                    // Determine a random healing amount within the fallback range specified in the config.
                    float healAmount = UnityEngine.Random.Range(_config.HealFallback.MinHeal, _config.HealFallback.MaxHeal);
                    Plugin.Instance?.DebugLog($"OnUsingItem: Random heal amount: {healAmount:F1} HP (range: {_config.HealFallback.MinHeal}-{_config.HealFallback.MaxHeal})");

                    // Increase the player's health by the determined healing amount.
                    Plugin.Instance?.DebugLog($"OnUsingItem: Player health before healing: {ev.Player.Health:F1} HP");
                    ev.Player.Health += healAmount;
                    Plugin.Instance?.DebugLog($"OnUsingItem: Player health after healing: {ev.Player.Health:F1} HP");

                    // Set the feedback message to inform the player of the healing.
                    effectMessage = $"You consumed an expired SCP-500 pill. It partially healed you for {healAmount:F0} HP.";
                    Plugin.Instance?.DebugLog($"OnUsingItem: Using healing fallback message: {effectMessage}");
                }

                // Display a hint to the player with the effect or healing message for 5 seconds.
                Plugin.Instance?.DebugLog($"OnUsingItem: Showing hint to player: {effectMessage}");
                ev.Player.ShowHint(effectMessage, 5f);

                // Remove the consumed item from the player's inventory.
                Plugin.Instance?.DebugLog($"OnUsingItem: Removing item from player {ev.Player.Nickname}'s inventory");
                ev.Player.RemoveItem(ev.Player.CurrentItem);

                Plugin.Instance?.DebugLog("OnUsingItem: Method completed successfully");
            }
            catch (Exception ex)
            {
                // Log any unhandled exceptions during the item usage event.
                Log.Error($"ExpiredSCP500Pills: Error in OnUsingItem: {ex.Message}\n{ex.StackTrace}");
                Plugin.Instance?.DebugLog($"OnUsingItem: Unhandled exception: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // Selects a random category based on weighted chances defined in the config.
        private string GetRandomCategory()
        {
            Plugin.Instance?.DebugLog("GetRandomCategory method called");

            // Check that there are category chances defined in the config; if not, default to "Positive".
            if (_config?.CategoryChances == null || _config.CategoryChances.Count == 0)
            {
                Plugin.Instance?.DebugLog("GetRandomCategory: CategoryChances is null or empty, returning default 'Positive'");
                return "Positive"; // Default fallback category.
            }

            // Initialize total chance and generate a random value between 0 and 100.
            float totalChance = 0;
            float randomValue = UnityEngine.Random.Range(0f, 100f);
            Plugin.Instance?.DebugLog($"GetRandomCategory: Random value is {randomValue:F2} (out of 100)");

            // Iterate through each category and its associated chance.
            foreach (var category in _config.CategoryChances)
            {
                totalChance += category.Value;
                Plugin.Instance?.DebugLog($"GetRandomCategory: Category {category.Key} has chance {category.Value:F2}, cumulative chance is now {totalChance:F2}");

                // When the random value falls within the cumulative chance range, return that category.
                if (randomValue <= totalChance)
                {
                    Plugin.Instance?.DebugLog($"GetRandomCategory: Selected category: {category.Key}");
                    return category.Key;
                }
            }

            // Fallback to "Positive" if no category was selected.
            Plugin.Instance?.DebugLog("GetRandomCategory: No category selected in loop, returning default 'Positive'");
            return "Positive";
        }

        // Chooses a random effect within the specified category based on their weighted chances.
        private EffectType GetRandomEffectFromCategory(string category)
        {
            Plugin.Instance?.DebugLog($"GetRandomEffectFromCategory called with category: {category}");

            // Retrieve the dictionary of effects for the given category.
            Dictionary<EffectType, EffectSettings> effects = GetEffectDictionaryForCategory(category);

            // If no effects are available, return None.
            if (effects == null || effects.Count == 0)
            {
                Plugin.Instance?.DebugLog($"GetRandomEffectFromCategory: No effects found for category {category}, returning EffectType.None");
                return EffectType.None;
            }

            Plugin.Instance?.DebugLog($"GetRandomEffectFromCategory: Found {effects.Count} effects in category {category}");

            // Initialize total chance and generate a random value between 0 and 100.
            float totalChance = 0;
            float randomValue = UnityEngine.Random.Range(0f, 100f);
            Plugin.Instance?.DebugLog($"GetRandomEffectFromCategory: Random value is {randomValue:F2} (out of 100)");

            // Calculate the total chance sum for the effects within this category.
            float categoryTotalChance = effects.Sum(e => e.Value.Chance);
            Plugin.Instance?.DebugLog($"GetRandomEffectFromCategory: Total raw chance sum for all effects: {categoryTotalChance:F2}");

            // Normalize the chances to a 100-point scale if necessary.
            float normalizationFactor = categoryTotalChance > 0 ? 100f / categoryTotalChance : 1f;
            Plugin.Instance?.DebugLog($"GetRandomEffectFromCategory: Normalization factor: {normalizationFactor:F4}");

            // Iterate through each effect in the category.
            foreach (var effect in effects)
            {
                // Accumulate the weighted chance.
                float effectNormalizedChance = effect.Value.Chance * normalizationFactor;
                totalChance += effectNormalizedChance;
                Plugin.Instance?.DebugLog($"GetRandomEffectFromCategory: Effect {effect.Key} has normalized chance {effectNormalizedChance:F2}, cumulative chance is now {totalChance:F2}");

                // If the random value is within the current range, select this effect.
                if (randomValue <= totalChance)
                {
                    Plugin.Instance?.DebugLog($"GetRandomEffectFromCategory: Selected effect: {effect.Key}");
                    return effect.Key;
                }
            }

            // If none selected, return None.
            Plugin.Instance?.DebugLog("GetRandomEffectFromCategory: No effect selected in loop, returning EffectType.None");
            return EffectType.None;
        }

        // Retrieves the dictionary of effects for a given category.
        private Dictionary<EffectType, EffectSettings> GetEffectDictionaryForCategory(string category)
        {
            Plugin.Instance?.DebugLog($"GetEffectDictionaryForCategory called with category: {category}");

            switch (category)
            {
                case "Positive":
                    Plugin.Instance?.DebugLog($"GetEffectDictionaryForCategory: Returning PositiveEffects dictionary with {_config.PositiveEffects?.Count ?? 0} effects");
                    return _config.PositiveEffects;
                case "Negative":
                    Plugin.Instance?.DebugLog($"GetEffectDictionaryForCategory: Returning NegativeEffects dictionary with {_config.NegativeEffects?.Count ?? 0} effects");
                    return _config.NegativeEffects;
                case "SCP":
                    Plugin.Instance?.DebugLog($"GetEffectDictionaryForCategory: Returning SCPEffects dictionary with {_config.SCPEffects?.Count ?? 0} effects");
                    return _config.SCPEffects;
                default:
                    // Return null if the category does not match any defined configurations.
                    Plugin.Instance?.DebugLog($"GetEffectDictionaryForCategory: Unknown category {category}, returning null");
                    return null;
            }
        }

        // Retrieves the settings for a specific effect type within a category.
        private EffectSettings GetEffectSettings(string category, EffectType effectType)
        {
            Plugin.Instance?.DebugLog($"GetEffectSettings called with category: {category}, effectType: {effectType}");

            // First, get the dictionary of effects for the specified category.
            Dictionary<EffectType, EffectSettings> effects = GetEffectDictionaryForCategory(category);

            // Try to retrieve the settings for the specific effect type.
            if (effects != null && effects.TryGetValue(effectType, out EffectSettings settings))
            {
                Plugin.Instance?.DebugLog($"GetEffectSettings: Found settings for effect {effectType} in category {category}");
                return settings;
            }

            // Return null if no settings are found.
            Plugin.Instance?.DebugLog($"GetEffectSettings: No settings found for effect {effectType} in category {category}, returning null");
            return null;
        }
    }
}
