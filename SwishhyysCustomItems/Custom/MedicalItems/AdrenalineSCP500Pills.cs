using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using SCI.Custom.Config;
using System;
using System.Collections.Generic;

namespace SCI.Custom.MedicalItems
{
    public class AdrenalineSCP500Pills : CustomItem
    {
        public override uint Id { get; set; } = 101;

        public override ItemType Type { get; set; } = ItemType.SCP500;

        public override string Name { get; set; } = "Adrenaline Pills";

        public override string Description { get; set; } = "A small bottle of pills that gives you a boost of energy.";

        public override float Weight { get; set; } = 0.5f;

        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties();

        // Reference to the configuration
        private readonly AdrenalineSCP500PillsConfig _config;

        // Dictionary to track cooldowns per player
        private readonly Dictionary<string, DateTime> cooldowns = new Dictionary<string, DateTime>();

        // Constructor that takes the config
        public AdrenalineSCP500Pills(AdrenalineSCP500PillsConfig config)
        {
            _config = config;
        }

        // Default constructor for compatibility
        public AdrenalineSCP500Pills()
        {
            // Create a default config if none was provided
            _config = new AdrenalineSCP500PillsConfig();
        }

        protected override void SubscribeEvents()
        {
            // Subscribe to the UsingItem event
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;

            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            // Unsubscribe from the UsingItem event
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;

            base.UnsubscribeEvents();
        }

        private void OnUsingItem(UsingItemEventArgs ev)
        {
            // Check if the used item is this custom item
            if (!Check(ev.Player.CurrentItem))
                return;

            // Get the player's unique identifier
            string userId = ev.Player.UserId;

            // Check for cooldown
            if (cooldowns.TryGetValue(userId, out DateTime lastUsed))
            {
                if ((DateTime.UtcNow - lastUsed).TotalSeconds < _config.Cooldown)
                {
                    ev.Player.ShowHint(_config.CooldownMessage, _config.HintDuration);
                    return;
                }
            }

            // Apply movement speed boost using the Scp207 effect
            ev.Player.EnableEffect<CustomPlayerEffects.Scp207>(_config.EffectDuration);

            // Adjust the intensity of the effect to set the speed multiplier
            var scp207 = ev.Player.GetEffect<CustomPlayerEffects.Scp207>();
            if (scp207 != null)
            {
                // Ensure intensity is within byte range
                byte intensity = (byte)Math.Max(0, Math.Min(_config.SpeedMultiplier * 10, 255));
                scp207.Intensity = intensity;
            }

            // Restore stamina to full (or to the configured amount)
            ev.Player.Stamina = _config.StaminaRestoreAmount;

            // Provide feedback to the player - using ShowHint instead of Broadcast
            ev.Player.ShowHint(_config.ActivationMessage, _config.HintDuration);

            // Remove the item from the player's inventory
            ev.Player.RemoveItem(ev.Player.CurrentItem);

            // Update the cooldown timer
            cooldowns[userId] = DateTime.UtcNow;

            // Apply side effects after the effect duration ends
            Timing.CallDelayed(_config.EffectDuration, () =>
            {
                if (ev.Player != null && ev.Player.IsAlive)
                {
                    // Apply exhaustion effect
                    ev.Player.EnableEffect<CustomPlayerEffects.Exhausted>(_config.ExhaustionDuration);
                    // Use ShowHint instead of Broadcast
                    ev.Player.ShowHint(_config.ExhaustionMessage, _config.HintDuration);
                }
            });
        }
    }
}
