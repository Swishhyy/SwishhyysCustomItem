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
using static SCI.Config.ExpiredSCP500PillsConfig;

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
        public override SpawnProperties SpawnProperties { get; set; } = new()
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
                // Quick validation
                if (!Check(ev.Player?.CurrentItem) || ev.Player == null ||
                    ev.Player.ReferenceHub?.playerEffectsController == null)
                {
                    Log.Debug("ExpiredSCP500Pills: Invalid state for pill usage");
                    return;
                }

                // Get random effect
                string category = GetRandomCategory();
                EffectType effectType = GetRandomEffectFromCategory(category);

                // Apply effect or heal fallback
                if (effectType != EffectType.None && TryApplyEffect(ev.Player, category, effectType, out string effectMessage))
                {
                    // Effect was applied successfully
                }
                else
                {
                    // Use healing fallback
                    float healAmount = UnityEngine.Random.Range(_config.HealFallback.MinHeal, _config.HealFallback.MaxHeal);
                    ev.Player.Health += healAmount;
                    effectMessage = $"You consumed an expired SCP-500 pill. It partially healed you for {healAmount:F0} HP.";
                }

                // Show message and consume item
                ev.Player.ShowHint(effectMessage, 5f);
                ev.Player.RemoveItem(ev.Player.CurrentItem);
            }
            catch (Exception ex)
            {
                Log.Error($"ExpiredSCP500Pills: Error in OnUsingItem: {ex.Message}");
            }
        }

        private bool TryApplyEffect(Player player, string category, EffectType effectType, out string message)
        {
            message = string.Empty;
            try
            {
                var settings = GetEffectSettings(category, effectType);
                if (settings == null) return false;

                // Get random intensity and duration
                byte intensity = (byte)UnityEngine.Random.Range(settings.MinIntensity, settings.MaxIntensity + 1);
                float duration = settings.CustomDuration > 0 ? settings.CustomDuration : _config.DefaultEffectDuration;

                // Apply the effect
                player.EnableEffect(effectType, intensity, duration);

                // Set appropriate message
                message = category is "SCP" or "Negative"
                    ? $"You consumed an expired SCP-500 pill. You feel strange {effectType} effects."
                    : $"You consumed an expired SCP-500 pill. You feel {effectType} effects.";

                Log.Debug($"Applied effect {effectType} (category: {category}) with intensity {intensity} to {player.Nickname}");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"ExpiredSCP500Pills: Error applying effect {effectType}: {ex.Message}");
                return false;
            }
        }

        // Helper methods for random selection
        private string GetRandomCategory()
        {
            if (_config?.CategoryChances == null || _config.CategoryChances.Count == 0)
                return "Positive";

            float totalChance = 0;
            float randomValue = UnityEngine.Random.Range(0f, 100f);

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
            var effects = GetEffectDictionaryForCategory(category);
            if (effects == null || effects.Count == 0)
                return EffectType.None;

            // Calculate normalized chances
            float totalChance = 0;
            float randomValue = UnityEngine.Random.Range(0f, 100f);
            float categoryTotalChance = effects.Sum(e => e.Value.Chance);
            float normalizationFactor = categoryTotalChance > 0 ? 100f / categoryTotalChance : 1f;

            // Select effect based on weighted chance
            foreach (var effect in effects)
            {
                totalChance += effect.Value.Chance * normalizationFactor;
                if (randomValue <= totalChance)
                    return effect.Key;
            }

            return EffectType.None;
        }

        // Simplified dictionary lookups
        private Dictionary<EffectType, EffectSettings> GetEffectDictionaryForCategory(string category) => category switch
        {
            "Positive" => _config.PositiveEffects,
            "Negative" => _config.NegativeEffects,
            "SCP" => _config.SCPEffects,
            _ => null
        };

        private EffectSettings GetEffectSettings(string category, EffectType effectType) =>
            GetEffectDictionaryForCategory(category)?.TryGetValue(effectType, out var settings) == true ? settings : null;
    }
}
