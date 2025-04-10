// Using directives: import the necessary namespaces for spawning items, custom item functionality, event arguments, timing, configuration, and collections.
using Exiled.API.Features.Spawn;                   // Provides functionality related to item spawn properties.
using Exiled.CustomItems.API.Features;             // Contains base classes and features for custom items.
using Exiled.Events.EventArgs.Player;              // Provides event argument types, such as those used in the UsingItem event.
using MEC;                                         // Allows for calling delayed actions using Timing.CallDelayed.
using SCI.Custom.Config;                           // Provides access to custom configuration classes.
using System;                                      // Provides basic system functionality.
using System.Collections.Generic;                  // Provides generic collection types like Dictionary.
using Exiled.API.Features;
using Exiled.API.Enums;
using JetBrains.Annotations;                         // For accessing Log and other API features

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
        public override string Name { get; set; } = "<color=#8A2BE2>Adrenaline Pills</color>";

        // A short description of what the custom item does.
        public override string Description { get; set; } = "A small bottle of pills that gives you a boost of energy.";

        // The weight of the item which might affect in-game mechanics like inventory management.
        public override float Weight { get; set; } = 0.5f;

        // Spawn properties that define how and where the item spawns in the game.
        [CanBeNull]
        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties
        {
            Limit = 2,
            DynamicSpawnPoints = new List<DynamicSpawnPoint>
            {
                new DynamicSpawnPoint
                {
                    Chance = 15,
                    Location = SpawnLocationType.InsideLczCafe,
                },

                new DynamicSpawnPoint
                {
                    Chance = 15,
                    Location = SpawnLocationType.InsideLczWc,
                },

                new DynamicSpawnPoint
                {
                    Chance = 15,
                    Location = SpawnLocationType.Inside914,
                },

                new DynamicSpawnPoint
                {
                    Chance = 15,
                    Location = SpawnLocationType.InsideGr18Glass,
                },
                new DynamicSpawnPoint
                {
                    Chance = 15,
                    Location = SpawnLocationType.Inside096,
                },
            },
        };

        // Readonly field to store the custom configuration for this item.
        private readonly AdrenalineSCP500PillsConfig _config;

        // Dictionary to track cooldowns for each player by their UserId.
        private readonly Dictionary<string, DateTime> cooldowns = new Dictionary<string, DateTime>();

        // Constructor that takes a custom configuration object.
        public AdrenalineSCP500Pills(AdrenalineSCP500PillsConfig config)
        {
            Plugin.Instance?.DebugLog($"AdrenalineSCP500Pills constructor with config called");
            _config = config;
            Plugin.Instance?.DebugLog($"AdrenalineSCP500Pills initialized with config: SpeedMultiplier={_config.SpeedMultiplier}, EffectDuration={_config.EffectDuration}, Cooldown={_config.Cooldown}");
        }

        // Default constructor for compatibility; initializes the custom configuration with default values.
        public AdrenalineSCP500Pills()
        {
            Plugin.Instance?.DebugLog("AdrenalineSCP500Pills default constructor called");
            _config = new AdrenalineSCP500PillsConfig();
            Plugin.Instance?.DebugLog("AdrenalineSCP500Pills initialized with default config");
        }

        // Method called when the item is registered. Subscribes to the UsingItem event.
        protected override void SubscribeEvents()
        {
            Plugin.Instance?.DebugLog("AdrenalineSCP500Pills.SubscribeEvents called");
            // Subscribe to the event when a player uses an item.
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
            // Ensure base event subscriptions are executed.
            base.SubscribeEvents();
            Plugin.Instance?.DebugLog("AdrenalineSCP500Pills event subscriptions completed");
        }

        // Method called when the item is unregistered. Unsubscribes from the UsingItem event.
        protected override void UnsubscribeEvents()
        {
            Plugin.Instance?.DebugLog("AdrenalineSCP500Pills.UnsubscribeEvents called");
            // Unsubscribe from the UsingItem event to prevent memory leaks.
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
            // Ensure base unsubscription processes are executed.
            base.UnsubscribeEvents();
            Plugin.Instance?.DebugLog("AdrenalineSCP500Pills event unsubscriptions completed");
        }

        // Event handler invoked when a player uses an item.
        private void OnUsingItem(UsingItemEventArgs ev)
        {
            Plugin.Instance?.DebugLog($"AdrenalineSCP500Pills.OnUsingItem called for player: {ev.Player?.Nickname ?? "unknown"}");

            try
            {
                // Verify that the item the player is using is this custom item.
                if (!Check(ev.Player.CurrentItem))
                {
                    Plugin.Instance?.DebugLog("OnUsingItem: Item check failed, not our custom item");
                    return;
                }

                Plugin.Instance?.DebugLog("OnUsingItem: Item check passed, processing adrenaline pills usage");

                // Check if player is null before proceeding
                if (ev.Player == null)
                {
                    Log.Error("AdrenalineSCP500Pills: Player is null in OnUsingItem");
                    Plugin.Instance?.DebugLog("OnUsingItem: Player is null, aborting");
                    return;
                }

                // Retrieve the player's unique identifier.
                string userId = ev.Player.UserId;
                Plugin.Instance?.DebugLog($"OnUsingItem: Retrieved player UserId: {userId}");

                // Check if the player is on cooldown for using this item.
                if (cooldowns.TryGetValue(userId, out DateTime lastUsed))
                {
                    double secondsSinceLastUse = (DateTime.UtcNow - lastUsed).TotalSeconds;
                    Plugin.Instance?.DebugLog($"OnUsingItem: Player has used this item before, last use was {secondsSinceLastUse:F1} seconds ago (cooldown: {_config.Cooldown} seconds)");

                    // If the cooldown period has not elapsed, show a message and exit.
                    if (secondsSinceLastUse < _config.Cooldown)
                    {
                        Plugin.Instance?.DebugLog($"OnUsingItem: Player is on cooldown, {_config.Cooldown - secondsSinceLastUse:F1} seconds remaining");
                        ev.Player.ShowHint(_config.CooldownMessage, _config.HintDuration);
                        Plugin.Instance?.DebugLog($"OnUsingItem: Showed cooldown message to player: {_config.CooldownMessage}");
                        return;
                    }
                    else
                    {
                        Plugin.Instance?.DebugLog("OnUsingItem: Cooldown has expired, player can use the item");
                    }
                }
                else
                {
                    Plugin.Instance?.DebugLog("OnUsingItem: Player has not used this item before (no cooldown)");
                }

                // Apply a movement speed boost effect (here using the Scp207 effect as a substitute) for the duration defined in the config.
                Plugin.Instance?.DebugLog($"OnUsingItem: Applying Scp207 effect with duration {_config.EffectDuration} seconds");
                ev.Player.EnableEffect<CustomPlayerEffects.Scp207>(_config.EffectDuration);

                // Retrieve the applied Scp207 effect to adjust its intensity.
                var scp207 = ev.Player.GetEffect<CustomPlayerEffects.Scp207>();
                if (scp207 != null)
                {
                    // Calculate the intensity based on the configured speed multiplier, ensuring it stays within the valid byte range (0 to 255).
                    byte intensity = (byte)Math.Max(0, Math.Min(_config.SpeedMultiplier * 10, 255));
                    Plugin.Instance?.DebugLog($"OnUsingItem: Setting Scp207 effect intensity to {intensity} (from multiplier {_config.SpeedMultiplier})");
                    // Set the intensity of the effect.
                    scp207.Intensity = intensity;
                }
                else
                {
                    Plugin.Instance?.DebugLog("OnUsingItem: WARNING - Failed to retrieve Scp207 effect to adjust intensity");
                }

                // Restore the player's stamina to full or to the specified amount defined in the configuration.
                Plugin.Instance?.DebugLog($"OnUsingItem: Player stamina before: {ev.Player.Stamina:F1}");
                ev.Player.Stamina = _config.StaminaRestoreAmount;
                Plugin.Instance?.DebugLog($"OnUsingItem: Player stamina after: {ev.Player.Stamina:F1} (set to {_config.StaminaRestoreAmount})");

                // Display an activation message hint to the player for a duration specified by the config.
                Plugin.Instance?.DebugLog($"OnUsingItem: Showing activation message to player: {_config.ActivationMessage}");
                ev.Player.ShowHint(_config.ActivationMessage, _config.HintDuration);

                // Remove the item from the player's inventory as it is consumed on use.
                Plugin.Instance?.DebugLog($"OnUsingItem: Removing item from player {ev.Player.Nickname}'s inventory");
                ev.Player.RemoveItem(ev.Player.CurrentItem);

                // Update the cooldown timer for this player by setting the current UTC time.
                cooldowns[userId] = DateTime.UtcNow;
                Plugin.Instance?.DebugLog($"OnUsingItem: Updated cooldown timestamp for player {ev.Player.Nickname}");

                // Schedule a delayed call to apply side effects after the main effect duration finishes.
                Plugin.Instance?.DebugLog($"OnUsingItem: Scheduling exhaustion effect in {_config.EffectDuration} seconds");
                Timing.CallDelayed(_config.EffectDuration, () =>
                {
                    Plugin.Instance?.DebugLog($"Delayed callback: Effect duration ended for player {ev.Player?.Nickname ?? "unknown"}");
                    // Verify that the player is still valid and alive before applying side effects.
                    if (ev.Player != null && ev.Player.IsAlive)
                    {
                        Plugin.Instance?.DebugLog($"Delayed callback: Player is still valid and alive, applying exhaustion effect for {_config.ExhaustionDuration} seconds");
                        // Apply an exhaustion effect to simulate fatigue using the Exhausted effect with the configured exhaustion duration.
                        ev.Player.EnableEffect<CustomPlayerEffects.Exhausted>(_config.ExhaustionDuration);
                        // Show a hint message indicating the exhaustion effect to the player.
                        ev.Player.ShowHint(_config.ExhaustionMessage, _config.HintDuration);
                        Plugin.Instance?.DebugLog($"Delayed callback: Showed exhaustion message to player: {_config.ExhaustionMessage}");
                    }
                    else
                    {
                        Plugin.Instance?.DebugLog("Delayed callback: Player is no longer valid or alive, not applying exhaustion effect");
                    }
                });

                Plugin.Instance?.DebugLog("OnUsingItem: Method completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error($"AdrenalineSCP500Pills: Error in OnUsingItem: {ex.Message}\n{ex.StackTrace}");
                Plugin.Instance?.DebugLog($"OnUsingItem: Exception caught: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
