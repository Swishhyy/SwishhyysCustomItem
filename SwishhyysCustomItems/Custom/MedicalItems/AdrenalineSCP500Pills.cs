// Using directives: import the necessary namespaces for spawning items, custom item functionality, event arguments, timing, configuration, and collections.
using Exiled.API.Features.Spawn;                   // Provides functionality related to item spawn properties.
using Exiled.CustomItems.API.Features;             // Contains base classes and features for custom items.
using Exiled.Events.EventArgs.Player;              // Provides event argument types, such as those used in the UsingItem event.
using MEC;                                         // Allows for calling delayed actions using Timing.CallDelayed.
using SCI.Custom.Config;                           // Provides access to custom configuration classes.
using System;                                      // Provides basic system functionality.
using System.Collections.Generic;                  // Provides generic collection types like Dictionary.

namespace SCI.Custom.MedicalItems
{
    // Defines the AdrenalineSCP500Pills custom item class, inheriting from CustomItem.
    public class AdrenalineSCP500Pills : CustomItem
    {
        // Unique identifier for this custom item.
        public override uint Id { get; set; } = 101;

        // Specifies the type of item (SCP500 in this case) for in-game identification.
        public override ItemType Type { get; set; } = ItemType.SCP500;

        // The display name of the custom item.
        public override string Name { get; set; } = "Adrenaline Pills";

        // A short description of what the custom item does.
        public override string Description { get; set; } = "A small bottle of pills that gives you a boost of energy.";

        // The weight of the item which might affect in-game mechanics like inventory management.
        public override float Weight { get; set; } = 0.5f;

        // Spawn properties that define how and where the item spawns in the game.
        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties();

        // Readonly field to store the custom configuration for this item.
        private readonly AdrenalineSCP500PillsConfig _config;

        // Dictionary to track cooldowns for each player by their UserId.
        private readonly Dictionary<string, DateTime> cooldowns = new Dictionary<string, DateTime>();

        // Constructor that takes a custom configuration object.
        public AdrenalineSCP500Pills(AdrenalineSCP500PillsConfig config)
        {
            _config = config;
        }

        // Default constructor for compatibility; initializes the custom configuration with default values.
        public AdrenalineSCP500Pills()
        {
            _config = new AdrenalineSCP500PillsConfig();
        }

        // Method called when the item is registered. Subscribes to the UsingItem event.
        protected override void SubscribeEvents()
        {
            // Subscribe to the event when a player uses an item.
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
            // Ensure base event subscriptions are executed.
            base.SubscribeEvents();
        }

        // Method called when the item is unregistered. Unsubscribes from the UsingItem event.
        protected override void UnsubscribeEvents()
        {
            // Unsubscribe from the UsingItem event to prevent memory leaks.
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
            // Ensure base unsubscription processes are executed.
            base.UnsubscribeEvents();
        }

        // Event handler invoked when a player uses an item.
        private void OnUsingItem(UsingItemEventArgs ev)
        {
            // Verify that the item the player is using is this custom item.
            if (!Check(ev.Player.CurrentItem))
                return;

            // Retrieve the player's unique identifier.
            string userId = ev.Player.UserId;

            // Check if the player is on cooldown for using this item.
            if (cooldowns.TryGetValue(userId, out DateTime lastUsed))
            {
                // If the cooldown period has not elapsed, show a message and exit.
                if ((DateTime.UtcNow - lastUsed).TotalSeconds < _config.Cooldown)
                {
                    ev.Player.ShowHint(_config.CooldownMessage, _config.HintDuration);
                    return;
                }
            }

            // Apply a movement speed boost effect (here using the Scp207 effect as a substitute) for the duration defined in the config.
            ev.Player.EnableEffect<CustomPlayerEffects.Scp207>(_config.EffectDuration);

            // Retrieve the applied Scp207 effect to adjust its intensity.
            var scp207 = ev.Player.GetEffect<CustomPlayerEffects.Scp207>();
            if (scp207 != null)
            {
                // Calculate the intensity based on the configured speed multiplier, ensuring it stays within the valid byte range (0 to 255).
                byte intensity = (byte)Math.Max(0, Math.Min(_config.SpeedMultiplier * 10, 255));
                // Set the intensity of the effect.
                scp207.Intensity = intensity;
            }

            // Restore the player's stamina to full or to the specified amount defined in the configuration.
            ev.Player.Stamina = _config.StaminaRestoreAmount;

            // Display an activation message hint to the player for a duration specified by the config.
            ev.Player.ShowHint(_config.ActivationMessage, _config.HintDuration);

            // Remove the item from the player's inventory as it is consumed on use.
            ev.Player.RemoveItem(ev.Player.CurrentItem);

            // Update the cooldown timer for this player by setting the current UTC time.
            cooldowns[userId] = DateTime.UtcNow;

            // Schedule a delayed call to apply side effects after the main effect duration finishes.
            Timing.CallDelayed(_config.EffectDuration, () =>
            {
                // Verify that the player is still valid and alive before applying side effects.
                if (ev.Player != null && ev.Player.IsAlive)
                {
                    // Apply an exhaustion effect to simulate fatigue using the Exhausted effect with the configured exhaustion duration.
                    ev.Player.EnableEffect<CustomPlayerEffects.Exhausted>(_config.ExhaustionDuration);
                    // Show a hint message indicating the exhaustion effect to the player.
                    ev.Player.ShowHint(_config.ExhaustionMessage, _config.HintDuration);
                }
            });
        }
    }
}
