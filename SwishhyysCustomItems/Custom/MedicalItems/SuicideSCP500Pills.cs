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
using Exiled.API.Features.Items;

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
        public override string Name { get; set; } = "<color=#00FF00>Suicide Pills</color>";

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
            Plugin.Instance?.DebugLog($"SuicideSCP500Pills constructor with config called");
            _config = config;
            Plugin.Instance?.DebugLog($"SuicideSCP500Pills initialized with config: SurvivalChance={_config.SurvivalChance}, ExplosionRadius={_config.ExplosionRadius}");
        }

        // Default constructor for compatibility, creates a default configuration if none is provided.
        public SuicideSCP500Pills()
        {
            Plugin.Instance?.DebugLog("SuicideSCP500Pills default constructor called");
            _config = new SuicideSCP500PillsConfig();
            Plugin.Instance?.DebugLog("SuicideSCP500Pills initialized with default config");
        }
        protected override void SubscribeEvents()
        {
            Plugin.Instance?.DebugLog("SuicideSCP500Pills.SubscribeEvents called");
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
            base.SubscribeEvents();
            Plugin.Instance?.DebugLog("SuicideSCP500Pills event subscriptions completed");
        }
        protected override void UnsubscribeEvents()
        {
            Plugin.Instance?.DebugLog("SuicideSCP500Pills.UnsubscribeEvents called");
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
            base.UnsubscribeEvents();
            Plugin.Instance?.DebugLog("SuicideSCP500Pills event unsubscriptions completed");
        }

        // Event handler for when a player uses an item.
        public void OnUsingItem(UsingItemEventArgs ev)
        {
            Plugin.Instance?.DebugLog($"SuicideSCP500Pills.OnUsingItem called for player: {ev.Player?.Nickname ?? "unknown"}");

            try
            {
                // Check if the used item is this custom item
                if (!Check(ev.Player.CurrentItem))
                {
                    Plugin.Instance?.DebugLog("OnUsingItem: Item check failed, not our custom item");
                    return;
                }

                Plugin.Instance?.DebugLog("OnUsingItem: Item check passed, processing suicide pills usage");

                // Check if player is null before proceeding
                if (ev.Player == null)
                {
                    Log.Error("SuicidePills: Player is null in OnUsingItem");
                    Plugin.Instance?.DebugLog("OnUsingItem: Player is null, aborting");
                    return;
                }

                // Remove the item from the player's inventory
                Plugin.Instance?.DebugLog($"OnUsingItem: Removing item from player {ev.Player.Nickname}'s inventory");
                ev.Player.RemoveItem(ev.Player.CurrentItem);

                // Determine if player survives (5% chance)
                bool survives = UnityEngine.Random.Range(0f, 100f) <= _config.SurvivalChance;
                Plugin.Instance?.DebugLog($"OnUsingItem: Survival determined: {survives} (rolled against {_config.SurvivalChance}% chance)");

                // Show message to player
                if (survives)
                {
                    Plugin.Instance?.DebugLog($"OnUsingItem: Showing survival message to player: {_config.SurvivalMessage}");
                    ev.Player.ShowHint(_config.SurvivalMessage, _config.HintDuration);
                }
                else
                {
                    Plugin.Instance?.DebugLog($"OnUsingItem: Showing death message to player: {_config.DeathMessage}");
                    ev.Player.ShowHint(_config.DeathMessage, _config.HintDuration);
                }

                // Execute explosion effect
                Plugin.Instance?.DebugLog("OnUsingItem: Calling ExplodePlayer method");
                ExplodePlayer(ev.Player, survives);

                Log.Debug($"Player {ev.Player.Nickname} used suicide pills. Survival: {survives}");
                Plugin.Instance?.DebugLog("OnUsingItem: Method completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error($"SuicidePills: Error in OnUsingItem: {ex.Message}\n{ex.StackTrace}");
                Plugin.Instance?.DebugLog($"OnUsingItem: Exception caught: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // Handles the explosion effect for the player and nearby players.
        // Handles the explosion effect for the player and nearby players.
        private void ExplodePlayer(Player player, bool survives)
        {
            Plugin.Instance?.DebugLog($"ExplodePlayer called for {player.Nickname}, survives={survives}");

            try
            {
                // Create explosion effect at player's position
                Plugin.Instance?.DebugLog("ExplodePlayer: Creating explosion effect");

                // Create an explosive grenade at the player's position
                ExplosiveGrenade grenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
                grenade.FuseTime = 0.1f; // Very short fuse
                grenade.SpawnActive(player.Position + new Vector3(0, 0.1f, 0)); // Slightly above player to ensure visibility

                // If the player does not survive, schedule a delayed kill
                if (!survives)
                {
                    Plugin.Instance?.DebugLog("ExplodePlayer: Player will not survive, scheduling delayed kill");
                    Timing.CallDelayed(0.2f, () =>
                    {
                        Plugin.Instance?.DebugLog($"ExplodePlayer: Killing player {player.Nickname} with explosion damage");
                        player.Kill(DamageType.Explosion);
                    });
                }
                else
                {
                    Plugin.Instance?.DebugLog($"ExplodePlayer: Player survives, applying 70 damage instead of killing");
                    player.Hurt(70f, DamageType.Explosion);

                    Plugin.Instance?.DebugLog("ExplodePlayer: Scheduling health check after explosion");
                    Timing.CallDelayed(0.5f, () =>
                    {
                        if (player.IsAlive)
                        {
                            Plugin.Instance?.DebugLog($"ExplodePlayer: Player is alive, current health: {player.Health}");
                            if (player.Health < _config.SurvivalHealthAmount)
                            {
                                Plugin.Instance?.DebugLog($"ExplodePlayer: Setting health to {_config.SurvivalHealthAmount}");
                                player.Health = _config.SurvivalHealthAmount;
                            }
                        }
                        else
                        {
                            Plugin.Instance?.DebugLog("ExplodePlayer: Player died despite survival flag");
                        }
                    });
                }

                // Process damage to nearby players
                Plugin.Instance?.DebugLog("ExplodePlayer: Processing damage to nearby players");
                int affectedPlayerCount = 0;

                foreach (Player target in Player.List)
                {
                    if (target == player || !target.IsAlive)
                    {
                        Plugin.Instance?.DebugLog($"ExplodePlayer: Skipping player {target.Nickname} (self or not alive)");
                        continue;
                    }

                    // Calculate distance
                    float distance = Vector3.Distance(player.Position, target.Position);
                    Plugin.Instance?.DebugLog($"ExplodePlayer: Player {target.Nickname} is {distance}m away");

                    // Apply damage based on distance
                    if (distance < _config.ExplosionRadius)
                    {
                        float damage = Mathf.Lerp(_config.MaxNearbyPlayerDamage, 10f, distance / _config.ExplosionRadius);
                        Plugin.Instance?.DebugLog($"ExplodePlayer: Applying {damage} explosion damage to {target.Nickname}");
                        target.Hurt(damage, DamageType.Explosion);

                        target.ShowHint($"You were hit by an explosion!", 3f);
                        affectedPlayerCount++;
                    }
                }

                Plugin.Instance?.DebugLog($"ExplodePlayer: Affected {affectedPlayerCount} nearby players");
                Plugin.Instance?.DebugLog("ExplodePlayer: Method completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error($"Error in ExplodePlayer: {ex.Message}");
                Plugin.Instance?.DebugLog($"ExplodePlayer: Exception caught: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
