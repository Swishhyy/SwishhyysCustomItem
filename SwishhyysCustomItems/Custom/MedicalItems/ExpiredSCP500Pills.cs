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
            _config = config;
        }

        // Default constructor for compatibility; initializes with a default configuration.
        public ExpiredSCP500Pills()
        {
            // Instantiate a default config if none is provided.
            _config = new ExpiredSCP500PillsConfig();
        }

        // Subscribe to relevant events when the item is registered.
        protected override void SubscribeEvents()
        {
            // Attach the OnUsingItem method to the UsingItem event.
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
            // Ensure any base class subscriptions are also executed.
            base.SubscribeEvents();
        }

        // Unsubscribe from events when the item is unregistered to prevent memory leaks.
        protected override void UnsubscribeEvents()
        {
            // Detach the OnUsingItem method from the UsingItem event.
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
            // Ensure any base class unsubscriptions are also processed.
            base.UnsubscribeEvents();
        }

        // Event handler called when a player uses an item.
        public void OnUsingItem(UsingItemEventArgs ev)
        {
            try
            {
                // Verify that the item being used is this custom item.
                if (!Check(ev.Player.CurrentItem))
                    return;

                // Ensure the player object is valid.
                if (ev.Player == null)
                {
                    Log.Error("ExpiredSCP500Pills: Player is null in OnUsingItem");
                    return;
                }

                // Check that the player's effects controller exists to safely apply effects.
                if (ev.Player.ReferenceHub?.playerEffectsController == null)
                {
                    Log.Error("ExpiredSCP500Pills: Player effect controller is null");
                    return;
                }

                // Determine which category of effects to apply based on a random selection weighted by config chances.
                string category = GetRandomCategory();
                // From the category, pick a specific effect to apply.
                EffectType effectType = GetRandomEffectFromCategory(category);

                // Variables to track whether any effect was applied and what feedback message to show the player.
                bool effectApplied = false;
                string effectMessage = "";

                // If a valid effect was selected, attempt to apply it.
                if (effectType != EffectType.None)
                {
                    try
                    {
                        // Retrieve the settings (intensity, duration, and chance) for the chosen effect.
                        EffectSettings settings = GetEffectSettings(category, effectType);

                        if (settings != null)
                        {
                            // Choose a random intensity within the range specified in the settings.
                            byte intensity = (byte)UnityEngine.Random.Range(settings.MinIntensity, settings.MaxIntensity + 1);

                            // Determine the duration to apply: use custom duration if specified; otherwise, use default duration.
                            float duration = settings.CustomDuration > 0 ? settings.CustomDuration : _config.DefaultEffectDuration;

                            // Apply the chosen effect with calculated intensity and duration.
                            ev.Player.EnableEffect(effectType, intensity, duration);
                            effectApplied = true;

                            // Set a custom message based on the effect category.
                            if (category == "SCP" || category == "Negative")
                                effectMessage = $"You consumed an expired SCP-500 pill. You feel strange {effectType} effects.";
                            else
                                effectMessage = $"You consumed an expired SCP-500 pill. You feel {effectType} effects.";

                            // Log the effect application for debugging purposes.
                            Log.Debug($"Applied effect {effectType} (category: {category}) with intensity {intensity} to {ev.Player.Nickname}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log any errors encountered during effect application.
                        Log.Error($"ExpiredSCP500Pills: Error applying effect {effectType}: {ex.Message}");
                    }
                }

                // If no effect was applied, use a fallback healing mechanism.
                if (!effectApplied)
                {
                    // Determine a random healing amount within the fallback range specified in the config.
                    float healAmount = UnityEngine.Random.Range(_config.HealFallback.MinHeal, _config.HealFallback.MaxHeal);
                    // Increase the player's health by the determined healing amount.
                    ev.Player.Health += healAmount;
                    // Set the feedback message to inform the player of the healing.
                    effectMessage = $"You consumed an expired SCP-500 pill. It partially healed you for {healAmount:F0} HP.";
                }

                // Display a hint to the player with the effect or healing message for 5 seconds.
                ev.Player.ShowHint(effectMessage, 5f);

                // Remove the consumed item from the player's inventory.
                ev.Player.RemoveItem(ev.Player.CurrentItem);
            }
            catch (Exception ex)
            {
                // Log any unhandled exceptions during the item usage event.
                Log.Error($"ExpiredSCP500Pills: Error in OnUsingItem: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // Selects a random category based on weighted chances defined in the config.
        private string GetRandomCategory()
        {
            // Check that there are category chances defined in the config; if not, default to "Positive".
            if (_config?.CategoryChances == null || _config.CategoryChances.Count == 0)
                return "Positive"; // Default fallback category.

            // Initialize total chance and generate a random value between 0 and 100.
            float totalChance = 0;
            float randomValue = UnityEngine.Random.Range(0f, 100f);

            // Iterate through each category and its associated chance.
            foreach (var category in _config.CategoryChances)
            {
                totalChance += category.Value;
                // When the random value falls within the cumulative chance range, return that category.
                if (randomValue <= totalChance)
                    return category.Key;
            }

            // Fallback to "Positive" if no category was selected.
            return "Positive";
        }

        // Chooses a random effect within the specified category based on their weighted chances.
        private EffectType GetRandomEffectFromCategory(string category)
        {
            // Retrieve the dictionary of effects for the given category.
            Dictionary<EffectType, EffectSettings> effects = GetEffectDictionaryForCategory(category);

            // If no effects are available, return None.
            if (effects == null || effects.Count == 0)
                return EffectType.None;

            // Initialize total chance and generate a random value between 0 and 100.
            float totalChance = 0;
            float randomValue = UnityEngine.Random.Range(0f, 100f);

            // Calculate the total chance sum for the effects within this category.
            float categoryTotalChance = effects.Sum(e => e.Value.Chance);

            // Normalize the chances to a 100-point scale if necessary.
            float normalizationFactor = categoryTotalChance > 0 ? 100f / categoryTotalChance : 1f;

            // Iterate through each effect in the category.
            foreach (var effect in effects)
            {
                // Accumulate the weighted chance.
                totalChance += effect.Value.Chance * normalizationFactor;
                // If the random value is within the current range, select this effect.
                if (randomValue <= totalChance)
                    return effect.Key;
            }

            // If none selected, return None.
            return EffectType.None;
        }

        // Retrieves the dictionary of effects for a given category.
        private Dictionary<EffectType, EffectSettings> GetEffectDictionaryForCategory(string category)
        {
            switch (category)
            {
                case "Positive":
                    return _config.PositiveEffects;
                case "Negative":
                    return _config.NegativeEffects;
                case "SCP":
                    return _config.SCPEffects;
                default:
                    // Return null if the category does not match any defined configurations.
                    return null;
            }
        }

        // Retrieves the settings for a specific effect type within a category.
        private EffectSettings GetEffectSettings(string category, EffectType effectType)
        {
            // First, get the dictionary of effects for the specified category.
            Dictionary<EffectType, EffectSettings> effects = GetEffectDictionaryForCategory(category);

            // Try to retrieve the settings for the specific effect type.
            if (effects != null && effects.TryGetValue(effectType, out EffectSettings settings))
                return settings;

            // Return null if no settings are found.
            return null;
        }
    }
}
