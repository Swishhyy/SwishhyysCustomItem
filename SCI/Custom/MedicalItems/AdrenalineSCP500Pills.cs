using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Enums;
using JetBrains.Annotations;
using SCI.Config;

namespace SCI.Custom.MedicalItems
{
    public class AdrenalineSCP500Pills(AdrenalineSCP500PillsConfig config) : CustomItem
    {
        #region Configuration
        public override uint Id { get; set; } = 101;
        public override ItemType Type { get; set; } = ItemType.SCP500;
        public override string Name { get; set; } = "<color=#8A2BE2>Adrenaline Pills</color>";
        public override string Description { get; set; } = "A small bottle of pills that gives you a boost of energy.";
        public override float Weight { get; set; } = 0.5f;

        private const string ActivationMessage = "<color=yellow>You feel a rush of adrenaline!</color>";
        private const string ExhaustionMessage = "<color=red>You feel exhausted after the adrenaline rush...</color>";
        private const string CooldownMessage = "You must wait before using another pill!";

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

        private readonly AdrenalineSCP500PillsConfig _config = config;
        private readonly Dictionary<string, DateTime> _cooldowns = [];

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

        private void OnUsingItem(UsingItemEventArgs ev)
        {
            try
            {
                // Verify this is our custom item
                if (!Check(ev.Player?.CurrentItem) || ev.Player == null)
                    return;

                // Check cooldown
                string userId = ev.Player.UserId;
                if (_cooldowns.TryGetValue(userId, out DateTime lastUsed))
                {
                    double secondsSinceLastUse = (DateTime.UtcNow - lastUsed).TotalSeconds;
                    if (secondsSinceLastUse < _config.Cooldown)
                    {
                        ev.Player.ShowHint(CooldownMessage, 5f);
                        return;
                    }
                }

                // Apply speed effect
                ev.Player.EnableEffect<CustomPlayerEffects.Scp207>(_config.EffectDuration);

                // Set effect intensity based on speed multiplier
                var scp207 = ev.Player.GetEffect<CustomPlayerEffects.Scp207>();
                if (scp207 != null)
                {
                    byte intensity = (byte)Math.Clamp(_config.SpeedMultiplier * 10, 0, 255);
                    scp207.Intensity = intensity;
                }

                // Restore stamina
                ev.Player.Stamina = _config.StaminaRestoreAmount;

                // Show message and remove item
                ev.Player.ShowHint(ActivationMessage, 5f);
                ev.Player.RemoveItem(ev.Player.CurrentItem);

                // Update cooldown
                _cooldowns[userId] = DateTime.UtcNow;

                // Schedule exhaustion effect after duration
                Timing.CallDelayed(_config.EffectDuration, () =>
                {
                    if (ev.Player is { IsAlive: true })
                    {
                        ev.Player.EnableEffect<CustomPlayerEffects.Exhausted>(_config.ExhaustionDuration);
                        ev.Player.ShowHint(ExhaustionMessage, 5f);
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error($"AdrenalineSCP500Pills: Error in OnUsingItem: {ex.Message}");
            }
        }
    }
}

