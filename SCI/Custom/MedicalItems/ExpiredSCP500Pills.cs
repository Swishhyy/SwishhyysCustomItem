using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using SCI.Config;

namespace SCI.Custom.MedicalItems
{
    public class ExpiredSCP500Pills(ExpiredSCP500PillsConfig config) : CustomItem
    {
        #region Configuration
        public override uint Id { get; set; } = 102;
        public override ItemType Type { get; set; } = ItemType.SCP500;
        public override string Name { get; set; } = "<color=#663399>Expired Pills</color>";
        public override string Description { get; set; } = "An expired bottle of SCP-500 pills with unpredictable effects.";
        public override float Weight { get; set; } = 0.5f;

        [CanBeNull]
        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties
        {
            Limit = 2,
            DynamicSpawnPoints =
            [
                new() { Chance = 15, Location = SpawnLocationType.InsideLczCafe },
                new() { Chance = 15, Location = SpawnLocationType.InsideLczWc },
                new() { Chance = 15, Location = SpawnLocationType.Inside914 },
                new() { Chance = 15, Location = SpawnLocationType.InsideGr18Glass },
                new() { Chance = 15, Location = SpawnLocationType.Inside096 }
            ],
        };

        private readonly ExpiredSCP500PillsConfig _config = config;

        #endregion

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
                // Verify this is our custom item
                if (!Check(ev.Player?.CurrentItem) || ev.Player == null)
                    return;

                // Check that player's effects controller exists
                if (ev.Player.ReferenceHub?.playerEffectsController == null)
                {
                    Log.Error("ExpiredSCP500Pills: Player effect controller is null");
                    return;
                }

                // Determine effect category and type
                string category = GetRandomCategory();
                EffectType effectType = GetRandomEffectFromCategory(category);

                // Variables to track effect application
                bool effectApplied = false;
                string effectMessage = string.Empty;

                // Apply effect if valid
                if (effectType != EffectType.None)
                {
                    try
                    {
                        EffectSettings settings = GetEffectSettings(category, effectType);

                        if (settings != null)
                        {
                            // Get random intensity within configured range
                            byte intensity = (byte)UnityEngine.Random.Range(settings.MinIntensity, settings.MaxIntensity + 1);

                            // Use custom duration if available, otherwise default
                            float duration = settings.CustomDuration > 0 ? settings.CustomDuration : _config.DefaultEffectDuration;

                            // Apply the effect
                            ev.Player.EnableEffect(effectType, intensity, duration);
                            effectApplied = true;

                            // Set appropriate message based on effect category
                            effectMessage = category is "SCP" or "Negative"
                                ? $"You consumed an expired SCP-500 pill. You feel strange {effectType} effects."
                                : $"You consumed an expired SCP-500 pill. You feel {effectType} effects.";

                            Log.Debug($"Applied effect {effectType} (category: {category}) with intensity {intensity} to {ev.Player.Nickname}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"ExpiredSCP500Pills: Error applying effect {effectType}: {ex.Message}");
                    }
                }

                // Use healing fallback if no effect was applied
                if (!effectApplied)
                {
                    float healAmount = UnityEngine.Random.Range(_config.HealFallback.MinHeal, _config.HealFallback.MaxHeal);
                    ev.Player.Health += healAmount;
                    effectMessage = $"You consumed an expired SCP-500 pill. It partially healed you for {healAmount:F0} HP.";
                }

                // Show message and remove item
                ev.Player.ShowHint(effectMessage, 5f);
                ev.Player.RemoveItem(ev.Player.CurrentItem);
            }
            catch (Exception ex)
            {
                Log.Error($"ExpiredSCP500Pills: Error in OnUsingItem: {ex.Message}");
            }
        }

        private string GetRandomCategory()
        {
            // Check for valid category chances
            if (_config?.CategoryChances == null || _config.CategoryChances.Count == 0)
                return "Positive";

            float totalChance = 0;
            float randomValue = UnityEngine.Random.Range(0f, 100f);

            // Select a category based on weighted chance
            foreach (var category in _config.CategoryChances)
            {
                totalChance += category.Value;

                if (randomValue <= totalChance)
                    return category.Key;
            }

            return "Positive";
        }

        private EffectType GetRandomEffectFromCategory(string category)
        {
            // Get effects for this category
            Dictionary<EffectType, EffectSettings> effects = GetEffectDictionaryForCategory(category);

            if (effects == null || effects.Count == 0)
                return EffectType.None;

            // Calculate normalized chances
            float totalChance = 0;
            float randomValue = UnityEngine.Random.Range(0f, 100f);

            // Sum total chance for normalization
            float categoryTotalChance = effects.Sum(e => e.Value.Chance);
            float normalizationFactor = categoryTotalChance > 0 ? 100f / categoryTotalChance : 1f;

            // Select effect based on weighted chance
            foreach (var effect in effects)
            {
                float effectNormalizedChance = effect.Value.Chance * normalizationFactor;
                totalChance += effectNormalizedChance;

                if (randomValue <= totalChance)
                    return effect.Key;
            }

            return EffectType.None;
        }

        private Dictionary<EffectType, EffectSettings> GetEffectDictionaryForCategory(string category) => category switch
        {
            "Positive" => _config.PositiveEffects,
            "Negative" => _config.NegativeEffects,
            "SCP" => _config.SCPEffects,
            _ => null
        };

        private EffectSettings GetEffectSettings(string category, EffectType effectType)
        {
            Dictionary<EffectType, EffectSettings> effects = GetEffectDictionaryForCategory(category);

            return effects != null && effects.TryGetValue(effectType, out EffectSettings settings)
                ? settings
                : null;
        }
    }
}
