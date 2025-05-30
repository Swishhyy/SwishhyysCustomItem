using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp939;
using MEC;
using SCI.Config;
using System;
using System.Collections.Generic;

namespace SCI.Custom.MedicalItems
{
    public class SCP500D(SCP500D_Config config) : CustomItem
    {
        #region Configuration
        public override string Name { get; set; } = "<color=#FF0000>SCP500-D</color>";
        public override string Description { get; set; } = "These pills make people vanish for a short amount of time. They can only be used once.";
        public override float Weight { get; set; } = 0.5f;
        public override uint Id { get; set; } = 104;
        public override ItemType Type { get; set; } = ItemType.SCP500;

        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties
        {
            Limit = 2,
            DynamicSpawnPoints =
            [
                new() { Chance = 15, Location = SpawnLocationType.InsideLczCafe },
                new() { Chance = 15, Location = SpawnLocationType.InsideLczWc },
                new() { Chance = 15, Location = SpawnLocationType.Inside914 },
                new() { Chance = 15, Location = SpawnLocationType.InsideGr18Glass },
                new() { Chance = 15, Location = SpawnLocationType.Inside096 },
            ],
        };

        private readonly SCP500D_Config _config = config;
        private const string ActivationMessage = "<color=#FF0000>You have become invisible for 7 seconds!</color>";
        private const string DeactivationMessage = "<color=red>You are visible again!</color>";
        private readonly HashSet<string> _usedByPlayers = [];
        private readonly Dictionary<Player, CoroutineHandle> _invisibilityCoroutines = new Dictionary<Player, CoroutineHandle>();
        #endregion

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
            Exiled.Events.Handlers.Player.Dying += OnPlayerDying;
            Exiled.Events.Handlers.Scp939.ValidatingVisibility += OnValidatingVisibility;
            Exiled.Events.Handlers.Scp939.PlayingFootstep += OnPlayingFootstep;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
            Exiled.Events.Handlers.Player.Dying -= OnPlayerDying;
            Exiled.Events.Handlers.Scp939.ValidatingVisibility -= OnValidatingVisibility;
            Exiled.Events.Handlers.Scp939.PlayingFootstep -= OnPlayingFootstep;
            base.UnsubscribeEvents();
        }

        private void OnPlayerDying(DyingEventArgs ev)
        {
            RemoveInvisibility(ev.Player);
        }

        private void OnValidatingVisibility(ValidatingVisibilityEventArgs ev)
        {
            if (_invisibilityCoroutines.ContainsKey(ev.Target))
                ev.IsAllowed = false;
        }

        private void OnPlayingFootstep(PlayingFootstepEventArgs ev)
        {
            if (_invisibilityCoroutines.ContainsKey(ev.Player))
                ev.IsAllowed = false;
        }

        private void OnUsingItem(UsingItemEventArgs ev)
        {
            try
            {
                // Verify this is our custom item
                if (!Check(ev.Player.CurrentItem))
                    return;

                if (ev.Player == null)
                {
                    Log.Error("SCP500D: Player is null in OnUsingItem");
                    return;
                }

                // Check if the player has already used this type of pill
                string userId = ev.Player.UserId;
                if (_usedByPlayers.Contains(userId))
                {
                    ev.Player.ShowHint("You've already used these pills before. They have no effect.", 5f);
                    return;
                }

                // Apply invisibility
                ApplyInvisibility(ev.Player);

                // Remove the item as it's consumed
                ev.Player.RemoveItem(ev.Player.CurrentItem);

                // Track usage to prevent repeated uses
                _usedByPlayers.Add(userId);
            }
            catch (Exception ex)
            {
                Log.Error($"SCP500D: Error in OnUsingItem: {ex.Message}");
            }
        }

        private void ApplyInvisibility(Player player)
        {
            try
            {
                // If there's already an invisibility coroutine running, stop it
                RemoveInvisibility(player);

                // Show activation message
                player.ShowHint(ActivationMessage, 5f);

                // Create a new coroutine to handle the invisibility
                CoroutineHandle handle = Timing.RunCoroutine(InvisibilityCoroutine(player));
                _invisibilityCoroutines[player] = handle;

                Log.Debug($"SCP500D: Applied invisibility to {player.Nickname} for {_config.Duration} seconds");
            }
            catch (Exception ex)
            {
                Log.Error($"SCP500D: Error applying invisibility: {ex.Message}");
            }
        }

        private void RemoveInvisibility(Player player)
        {
            try
            {
                if (player == null)
                    return;

                if (_invisibilityCoroutines.TryGetValue(player, out CoroutineHandle handle))
                {
                    // Kill the coroutine if it's running
                    Timing.KillCoroutines(handle);
                    _invisibilityCoroutines.Remove(player);
                    
                    // Disable the invisibility effect if the player is still alive
                    if (player.IsAlive)
                    {
                        player.DisableEffect(EffectType.Invisible);
                        Log.Debug($"SCP500D: Removed invisibility from {player.Nickname}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"SCP500D: Error removing invisibility: {ex.Message}");
            }
        }

        private IEnumerator<float> InvisibilityCoroutine(Player player)
        {
            if (player == null || !player.IsAlive)
                yield break;

            float remainingTime = _config.Duration;
            float updateInterval = 0.25f; // Check and refresh invisibility every 0.25 seconds

            // Initial application of invisibility
            ApplyInvisibilityEffect(player);
            
            // Main loop - keep refreshing invisibility until time runs out
            while (remainingTime > 0 && player != null && player.IsAlive)
            {
                // Wait for the update interval
                yield return Timing.WaitForSeconds(updateInterval);
                remainingTime -= updateInterval;
                
                // Refresh invisibility effect
                ApplyInvisibilityEffect(player);
            }
            
            // End of invisibility
            if (player != null && player.IsAlive)
            {
                try
                {
                    player.DisableEffect(EffectType.Invisible);
                    player.ShowHint(DeactivationMessage, 5f);
                    Log.Debug($"SCP500D: Invisibility expired for {player.Nickname}");
                }
                catch (Exception ex)
                {
                    Log.Error($"SCP500D: Error ending invisibility: {ex.Message}");
                }
            }
            
            // Clean up
            if (player != null && _invisibilityCoroutines.ContainsKey(player))
                _invisibilityCoroutines.Remove(player);
        }
        
        private void ApplyInvisibilityEffect(Player player)
        {
            try
            {
                if (player != null && player.IsAlive)
                {
                    player.EnableEffect(EffectType.Invisible);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"SCP500D: Error applying invisibility effect: {ex.Message}");
            }
        }
    }
}
