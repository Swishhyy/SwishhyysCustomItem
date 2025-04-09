// Using directives: bring in external libraries and APIs needed for this class.
using Exiled.API.Enums;                              // Provides enumerations such as DamageType.
using Exiled.API.Features;                           // Provides core game features such as Player, Log, etc.
using Exiled.API.Features.Spawn;                     // Offers functionality related to item spawning.
using Exiled.CustomItems.API.Features;               // Contains custom item base classes and features.
using Exiled.Events.EventArgs.Player;                // Contains event argument classes, such as for UsingItem events.
using MEC;                                           // Used for managing delayed operations (Timing.CallDelayed).
using SCI.Custom.Config;                             // Access to custom configuration classes for plugin features.
using System;                                        // Provides basic system functionalities.
using UnityEngine;                                   // Provides access to Unity engine features, such as Vector3 and Mathf.

namespace SCI.Custom.MedicalItems
{
    // This class defines a custom item named "SuicideSCP500Pills" which extends the base CustomItem.
    public class SuicideSCP500Pills : CustomItem
    {
        // Unique identifier for the custom item.
        public override uint Id { get; set; } = 103;

        // Set the item type to SCP500 (as defined in the Exiled API).
        public override ItemType Type { get; set; } = ItemType.SCP500;

        // Name of the custom item.
        public override string Name { get; set; } = "Suicide Pills";

        // Description provides information about what the item does.
        public override string Description { get; set; } = "Pills that cause the user to explode. There's a small chance of survival.";

        // Weight property which may affect in-game item mechanics.
        public override float Weight { get; set; } = 0.5f;

        // Spawn properties determine how and when this item spawns in the game.
        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties();

        // Private field storing the configuration for this custom item.
        private readonly SuicideSCP500PillsConfig _config;

        // Constructor that initializes the custom item with a provided configuration.
        public SuicideSCP500Pills(SuicideSCP500PillsConfig config)
        {
            _config = config;
        }

        // Default constructor for compatibility, creates a default configuration if none is provided.
        public SuicideSCP500Pills()
        {
            _config = new SuicideSCP500PillsConfig();
        }

        // SubscribeEvents is called when the item is registered.
        // It subscribes to relevant player events to handle item usage.
        protected override void SubscribeEvents()
        {
            // Subscribe to the UsingItem event so that OnUsingItem is called when a player uses an item.
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
            // Call the base subscription to ensure any additional inherited functionality is executed.
            base.SubscribeEvents();
        }

        // UnsubscribeEvents is called when the item is unregistered.
        // It removes the event subscription.
        protected override void UnsubscribeEvents()
        {
            // Unsubscribe from the UsingItem event to avoid potential memory leaks or unintended behavior.
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
            // Call the base unsubscription to ensure cleanup routines of the base class are executed.
            base.UnsubscribeEvents();
        }

        // Event handler for when a player uses an item.
        public void OnUsingItem(UsingItemEventArgs ev)
        {
            try
            {
                // Verify that the current item the player is using is this custom item.
                if (!Check(ev.Player.CurrentItem))
                    return;

                // Ensure the player object is not null before proceeding.
                if (ev.Player == null)
                {
                    Log.Error("SuicidePills: Player is null in OnUsingItem");
                    return;
                }

                // Remove this custom item from the player's inventory as it is consumed upon use.
                ev.Player.RemoveItem(ev.Player.CurrentItem);

                // Determine if the player survives the explosion.
                // The survival is based on comparing a randomly generated number to the survival chance defined in the config.
                bool survives = UnityEngine.Random.Range(0f, 100f) <= _config.SurvivalChance;

                // Provide feedback to the player via a hint based on the survival outcome.
                if (survives)
                {
                    ev.Player.ShowHint(_config.SurvivalMessage, _config.HintDuration);
                }
                else
                {
                    ev.Player.ShowHint(_config.DeathMessage, _config.HintDuration);
                }

                // Trigger the explosion effect, handling both the user's experience and effects on nearby players.
                ExplodePlayer(ev.Player, survives);

                // Log a debug message including the player's nickname and the survival status.
                Log.Debug($"Player {ev.Player.Nickname} used suicide pills. Survival: {survives}");
            }
            catch (Exception ex)
            {
                // Log errors if an exception occurs during the event handling.
                Log.Error($"SuicidePills: Error in OnUsingItem: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // Handles the explosion effect for the player and nearby players.
        private void ExplodePlayer(Player player, bool survives)
        {
            try
            {
                // If the player does not survive, schedule a delayed kill to allow the hint to be displayed.
                if (!survives)
                {
                    Timing.CallDelayed(0.2f, () =>
                    {
                        // Kill the player using explosion damage type.
                        player.Kill(DamageType.Explosion);
                    });
                }
                else
                {
                    // If the player survives, hurt them with a fixed amount of damage.
                    player.Hurt(70f, DamageType.Explosion);

                    // After a short delay, adjust the player's health if it falls below a minimum survival threshold.
                    Timing.CallDelayed(0.5f, () =>
                    {
                        if (player.IsAlive && player.Health < _config.SurvivalHealthAmount)
                        {
                            player.Health = _config.SurvivalHealthAmount;
                        }
                    });
                }

                // Iterate over all players in the game to apply explosion damage to those near the affected player.
                foreach (Player target in Player.List)
                {
                    // Skip the player who used the item or any players that are not alive.
                    if (target == player || !target.IsAlive)
                        continue;

                    // Calculate the distance between the explosion origin and the target.
                    float distance = Vector3.Distance(player.Position, target.Position);

                    // If the target is within the explosion radius, calculate and apply damage.
                    if (distance < _config.ExplosionRadius)
                    {
                        // Damage is calculated using linear interpolation based on proximity (closer targets take more damage).
                        float damage = Mathf.Lerp(_config.MaxNearbyPlayerDamage, 10f, distance / _config.ExplosionRadius);
                        // Apply damage to the nearby player with explosion as the damage type.
                        target.Hurt(damage, DamageType.Explosion);

                        // Show a hint to the nearby player informing them they were hit by an explosion.
                        target.ShowHint($"You were hit by an explosion!", 3f);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any exception that occurs during the explosion effect execution.
                Log.Error($"Error in ExplodePlayer: {ex.Message}");
            }
        }
    }
}
