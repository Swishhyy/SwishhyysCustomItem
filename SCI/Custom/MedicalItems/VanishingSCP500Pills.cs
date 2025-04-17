using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp939;
using JetBrains.Annotations;
using MEC;
using SCI.Config;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCI.Custom.MedicalItems
{
    public class VanishingSCP500Pills : CustomItem
    {
        public override string Name { get; set; } = "<color=#FF0000>Vanishing Pills</color>";
        public override string Description { get; set; } = "These pills make people vanish for a short amount of time. They can only be used once.";
        public override float Weight { get; set; } = 0.5f;
        public override uint Id { get; set; } = 104;
        public override ItemType Type { get; set; } = ItemType.SCP500;

        private readonly VanishingSCP500PillsConfig _config;
        private readonly string ActivationMessage = "<color=#FF0000>You have become invisible for 7 seconds!</color>";
        private readonly string DeactivationMessage = "<color=red>You are visible again!</color>";

        // Track players who have used these pills to prevent multiple uses
        private readonly HashSet<string> usedByPlayers = [];

        // Track players who are currently invisible
        private readonly HashSet<Player> invisiblePlayers = [];

        public VanishingSCP500Pills(VanishingSCP500PillsConfig config)
        {
            Plugin.Instance?.DebugLog("VanishingSCP500Pills constructor with config called");
            _config = config;
            Plugin.Instance?.DebugLog($"VanishingSCP500Pills initialized with config: Duration={_config.Duration}");
        }

        public VanishingSCP500Pills()
        {
            Plugin.Instance?.DebugLog("VanishingSCP500Pills default constructor called");
            _config = new VanishingSCP500PillsConfig();
            Plugin.Instance?.DebugLog("VanishingSCP500Pills initialized with default config");
        }

        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties
        {
            Limit = 2,
            DynamicSpawnPoints =
            [
                new()
                {
                    Chance = 15,
                    Location = SpawnLocationType.InsideLczCafe,
                },

                new()
                {
                    Chance = 15,
                    Location = SpawnLocationType.InsideLczWc,
                },

                new()
                {
                    Chance = 15,
                    Location = SpawnLocationType.Inside914,
                },

                new()
                {
                    Chance = 15,
                    Location = SpawnLocationType.InsideGr18Glass,
                },
                new()
                {
                    Chance = 15,
                    Location = SpawnLocationType.Inside096,
                },
            ],
        };

        protected override void SubscribeEvents()
        {
            Plugin.Instance?.DebugLog("VanishingSCP500Pills.SubscribeEvents called");
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
            Exiled.Events.Handlers.Player.Dying += OnPlayerDying;

            // Subscribe to events that would normally break invisibility to maintain it
            Exiled.Events.Handlers.Player.InteractingDoor += OnInteractingDoor;
            Exiled.Events.Handlers.Player.InteractingElevator += OnInteractingElevator;
            Exiled.Events.Handlers.Player.InteractingLocker += OnInteractingLocker;

            // Use ValidatingVisibility event to prevent SCP-939 from seeing invisible players
            Exiled.Events.Handlers.Scp939.ValidatingVisibility += OnValidatingVisibility;

            // Prevent footstep detection by SCP-939
            Exiled.Events.Handlers.Scp939.PlayingFootstep += OnPlayingFootstep;

            base.SubscribeEvents();
            Plugin.Instance?.DebugLog("VanishingSCP500Pills event subscriptions completed");
        }

        protected override void UnsubscribeEvents()
        {
            Plugin.Instance?.DebugLog("VanishingSCP500Pills.UnsubscribeEvents called");
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
            Exiled.Events.Handlers.Player.Dying -= OnPlayerDying;

            // Unsubscribe from the events we added
            Exiled.Events.Handlers.Player.InteractingDoor -= OnInteractingDoor;
            Exiled.Events.Handlers.Player.InteractingElevator -= OnInteractingElevator;
            Exiled.Events.Handlers.Player.InteractingLocker -= OnInteractingLocker;

            // Unsubscribe from SCP-939 events
            Exiled.Events.Handlers.Scp939.ValidatingVisibility -= OnValidatingVisibility;
            Exiled.Events.Handlers.Scp939.PlayingFootstep -= OnPlayingFootstep;

            base.UnsubscribeEvents();
            Plugin.Instance?.DebugLog("VanishingSCP500Pills event unsubscriptions completed");
        }

        // Handle player death while invisible
        private void OnPlayerDying(DyingEventArgs ev)
        {
            if (invisiblePlayers.Contains(ev.Player))
            {
                Plugin.Instance?.DebugLog($"OnPlayerDying: Player {ev.Player.Nickname} died while invisible, removing from tracking");
                invisiblePlayers.Remove(ev.Player);
            }
        }

        // Prevent SCP-939 from seeing invisible players
        private void OnValidatingVisibility(ValidatingVisibilityEventArgs ev)
        {
            if (invisiblePlayers.Contains(ev.Target))
            {
                Plugin.Instance?.DebugLog($"OnValidatingVisibility: Preventing SCP-939 from seeing invisible player {ev.Target.Nickname}");
                ev.IsAllowed = false;
            }
        }

        // Prevent SCP-939 from hearing footsteps of invisible players
        private void OnPlayingFootstep(PlayingFootstepEventArgs ev)
        {
            if (invisiblePlayers.Contains(ev.Player))
            {
                Plugin.Instance?.DebugLog($"OnPlayingFootstep: Preventing SCP-939 from hearing footsteps of invisible player {ev.Player.Nickname}");
                ev.IsAllowed = false;
            }
        }

        // Preserving invisibility when interacting with doors
        private void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (invisiblePlayers.Contains(ev.Player))
            {
                Plugin.Instance?.DebugLog($"OnInteractingDoor: Player {ev.Player.Nickname} is invisible, preserving invisibility");
                Timing.CallDelayed(0.1f, () => ReapplyInvisibility(ev.Player));
            }
        }

        // Preserving invisibility when interacting with elevators
        private void OnInteractingElevator(InteractingElevatorEventArgs ev)
        {
            if (invisiblePlayers.Contains(ev.Player))
            {
                Plugin.Instance?.DebugLog($"OnInteractingElevator: Player {ev.Player.Nickname} is invisible, preserving invisibility");
                Timing.CallDelayed(0.1f, () => ReapplyInvisibility(ev.Player));
            }
        }

        // Preserving invisibility when interacting with lockers
        private void OnInteractingLocker(InteractingLockerEventArgs ev)
        {
            if (invisiblePlayers.Contains(ev.Player))
            {
                Plugin.Instance?.DebugLog($"OnInteractingLocker: Player {ev.Player.Nickname} is invisible, preserving invisibility");
                Timing.CallDelayed(0.1f, () => ReapplyInvisibility(ev.Player));
            }
        }

        // Helper method to reapply invisibility effect
        private void ReapplyInvisibility(Player player)
        {
            if (player != null && player.IsAlive && invisiblePlayers.Contains(player))
            {
                Plugin.Instance?.DebugLog($"ReapplyInvisibility: Reapplying invisibility for player {player.Nickname}");
                player.EnableEffect(EffectType.Invisible, 1);
            }
        }

        private void OnUsingItem(UsingItemEventArgs ev)
        {
            Plugin.Instance?.DebugLog($"VanishingSCP500Pills.OnUsingItem called for player: {ev.Player?.Nickname ?? "unknown"}");

            try
            {
                // Verify this is our custom item
                if (!Check(ev.Player.CurrentItem))
                {
                    Plugin.Instance?.DebugLog("OnUsingItem: Item check failed, not our custom item");
                    return;
                }

                Plugin.Instance?.DebugLog("OnUsingItem: Item check passed, processing vanishing pills usage");

                // Check if player is null before proceeding
                if (ev.Player == null)
                {
                    Log.Error("VanishingSCP500Pills: Player is null in OnUsingItem");
                    Plugin.Instance?.DebugLog("OnUsingItem: Player is null, aborting");
                    return;
                }

                // Get the player's unique ID
                string userId = ev.Player.UserId;

                // Check if the player has already used this type of pill
                if (usedByPlayers.Contains(userId))
                {
                    Plugin.Instance?.DebugLog($"OnUsingItem: Player {ev.Player.Nickname} has already used vanishing pills");
                    ev.Player.ShowHint("You've already used these pills before. They have no effect.", 5f);
                    return;
                }

                // Add player to the list of invisible players
                invisiblePlayers.Add(ev.Player);
                Plugin.Instance?.DebugLog($"OnUsingItem: Added player {ev.Player.Nickname} to invisible players list");

                // Apply invisibility effect (7 seconds)
                Plugin.Instance?.DebugLog($"OnUsingItem: Applying invisibility effect for {_config.Duration} seconds");
                ev.Player.EnableEffect(EffectType.Invisible, 1, _config.Duration);

                // Show activation message to player
                Plugin.Instance?.DebugLog($"OnUsingItem: Showing activation message to player: {ActivationMessage}");
                ev.Player.ShowHint(ActivationMessage, 5f);

                // Remove the item from the player's inventory as it's consumed
                Plugin.Instance?.DebugLog($"OnUsingItem: Removing item from player {ev.Player.Nickname}'s inventory");
                ev.Player.RemoveItem(ev.Player.CurrentItem);

                // Add player to the used list so they can't use it again
                usedByPlayers.Add(userId);
                Plugin.Instance?.DebugLog($"OnUsingItem: Added player {ev.Player.Nickname} to used list");

                // Schedule a callback for when invisibility ends
                Timing.CallDelayed(_config.Duration, () =>
                {
                    if (ev.Player != null && ev.Player.IsAlive)
                    {
                        Plugin.Instance?.DebugLog($"Delayed callback: Invisibility ended for player {ev.Player.Nickname}");

                        // Remove from invisible players list
                        invisiblePlayers.Remove(ev.Player);
                        Plugin.Instance?.DebugLog($"Delayed callback: Removed player {ev.Player.Nickname} from invisible players list");

                        // Notify the player that they are visible again
                        ev.Player.ShowHint(DeactivationMessage, 5f);
                        Plugin.Instance?.DebugLog($"Delayed callback: Showed deactivation message to player: {DeactivationMessage}");
                    }
                    else
                    {
                        // If player is no longer valid, still remove them from the list
                        if (ev.Player != null)
                        {
                            invisiblePlayers.Remove(ev.Player);
                            Plugin.Instance?.DebugLog($"Delayed callback: Removed invalid player from invisible players list");
                        }
                    }
                });

                Plugin.Instance?.DebugLog("OnUsingItem: Method completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error($"VanishingSCP500Pills: Error in OnUsingItem: {ex.Message}\n{ex.StackTrace}");
                Plugin.Instance?.DebugLog($"OnUsingItem: Exception caught: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
