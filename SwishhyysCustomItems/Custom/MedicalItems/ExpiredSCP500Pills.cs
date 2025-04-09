using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using SCI.Custom.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCI.Custom.MedicalItems
{
    public class ExpiredSCP500Pills : CustomItem
    {
        public override uint Id { get; set; } = 102;

        public override ItemType Type { get; set; } = ItemType.SCP500;

        public override string Name { get; set; } = "Expired SCP-500 Pills";

        public override string Description { get; set; } = "An expired bottle of SCP-500 pills with unpredictable effects.";

        public override float Weight { get; set; } = 0.5f;

        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties();

        // Reference to the configuration - fixed type name
        private readonly ExpiredSCP500PillsConfig _config;

        // Constructor that takes the config
        public ExpiredSCP500Pills(ExpiredSCP500PillsConfig config)
        {
            _config = config;
        }

        // Default constructor for compatibility
        public ExpiredSCP500Pills()
        {
            // Create a default config if none was provided
            _config = new ExpiredSCP500PillsConfig();
        }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
            base.UnsubscribeEvents();
        }

        public void OnUsingItem(UsingItemEventArgs ev)
        {
            try
            {
                // Check if the used item is this custom item (using CurrentItem instead of Item)
                if (!Check(ev.Player.CurrentItem))
                    return;

                // Check if player is null before proceeding
                if (ev.Player == null)
                {
                    Log.Error("ExpiredSCP500Pills: Player is null in OnUsingItem");
                    return;
                }

                // Make sure the player has valid effect components
                if (ev.Player.ReferenceHub?.playerEffectsController == null)
                {
                    Log.Error("ExpiredSCP500Pills: Player effect controller is null");
                    return;
                }

                // Use our randomization system with config values
                string category = GetRandomCategory();
                EffectType effectType = GetRandomEffectFromCategory(category);

                bool effectApplied = false;
                string effectMessage = "";

                if (effectType != EffectType.None)
                {
                    try
                    {
                        // Get settings for this effect from the config
                        EffectSettings settings = GetEffectSettings(category, effectType);

                        if (settings != null)
                        {
                            // Apply the effect with appropriate intensity and duration from config
                            byte intensity = (byte)UnityEngine.Random.Range(settings.MinIntensity, settings.MaxIntensity + 1);

                            // Use custom duration if specified, otherwise use default
                            float duration = settings.CustomDuration > 0 ? settings.CustomDuration : _config.DefaultEffectDuration;

                            // Apply the effect
                            ev.Player.EnableEffect(effectType, intensity, duration);
                            effectApplied = true;

                            // Customize message based on category
                            if (category == "SCP" || category == "Negative")
                                effectMessage = $"You consumed an expired SCP-500 pill. You feel strange {effectType} effects.";
                            else
                                effectMessage = $"You consumed an expired SCP-500 pill. You feel {effectType} effects.";

                            // Log effect application for debugging
                            Log.Debug($"Applied effect {effectType} (category: {category}) with intensity {intensity} to {ev.Player.Nickname}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"ExpiredSCP500Pills: Error applying effect {effectType}: {ex.Message}");
                    }
                }

                // If no effects were applied, use healing fallback from config
                if (!effectApplied)
                {
                    float healAmount = UnityEngine.Random.Range(_config.HealFallback.MinHeal, _config.HealFallback.MaxHeal);
                    ev.Player.Health += healAmount;
                    effectMessage = $"You consumed an expired SCP-500 pill. It partially healed you for {healAmount:F0} HP.";
                }

                // Show a hint to the player
                ev.Player.ShowHint(effectMessage, 5f);

                // Remove the item from the player's inventory (added this line to match AdrenalineSCP500Pills)
                ev.Player.RemoveItem(ev.Player.CurrentItem);
            }
            catch (Exception ex)
            {
                Log.Error($"ExpiredSCP500Pills: Error in OnUsingItem: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private string GetRandomCategory()
        {
            // Null check the config
            if (_config?.CategoryChances == null || _config.CategoryChances.Count == 0)
                return "Positive"; // Default fallback

            float totalChance = 0;
            float randomValue = UnityEngine.Random.Range(0f, 100f);

            foreach (var category in _config.CategoryChances)
            {
                totalChance += category.Value;
                if (randomValue <= totalChance)
                    return category.Key;
            }

            // Fallback to Positive if something goes wrong
            return "Positive";
        }

        private EffectType GetRandomEffectFromCategory(string category)
        {
            Dictionary<EffectType, EffectSettings> effects = GetEffectDictionaryForCategory(category);

            if (effects == null || effects.Count == 0)
                return EffectType.None;

            float totalChance = 0;
            float randomValue = UnityEngine.Random.Range(0f, 100f);

            // Calculate total chance within this category
            float categoryTotalChance = effects.Sum(e => e.Value.Chance);

            // Normalize to 100% if needed
            float normalizationFactor = categoryTotalChance > 0 ? 100f / categoryTotalChance : 1f;

            foreach (var effect in effects)
            {
                totalChance += effect.Value.Chance * normalizationFactor;
                if (randomValue <= totalChance)
                    return effect.Key;
            }

            return EffectType.None;
        }

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
                    return null;
            }
        }

        private EffectSettings GetEffectSettings(string category, EffectType effectType)
        {
            Dictionary<EffectType, EffectSettings> effects = GetEffectDictionaryForCategory(category);

            if (effects != null && effects.TryGetValue(effectType, out EffectSettings settings))
                return settings;

            return null;
        }
    }
}