using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp939;
using MEC;
using SCI.Config;
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
        private readonly HashSet<Player> _invisiblePlayers = [];

        #endregion

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
            Exiled.Events.Handlers.Player.Dying += OnPlayerDying;
            Exiled.Events.Handlers.Player.InteractingDoor += OnInteractingDoor;
            Exiled.Events.Handlers.Player.InteractingElevator += OnInteractingElevator;
            Exiled.Events.Handlers.Player.InteractingLocker += OnInteractingLocker;
            Exiled.Events.Handlers.Scp939.ValidatingVisibility += OnValidatingVisibility;
            Exiled.Events.Handlers.Scp939.PlayingFootstep += OnPlayingFootstep;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
            Exiled.Events.Handlers.Player.Dying -= OnPlayerDying;
            Exiled.Events.Handlers.Player.InteractingDoor -= OnInteractingDoor;
            Exiled.Events.Handlers.Player.InteractingElevator -= OnInteractingElevator;
            Exiled.Events.Handlers.Player.InteractingLocker -= OnInteractingLocker;
            Exiled.Events.Handlers.Scp939.ValidatingVisibility -= OnValidatingVisibility;
            Exiled.Events.Handlers.Scp939.PlayingFootstep -= OnPlayingFootstep;
            base.UnsubscribeEvents();
        }

        private void OnPlayerDying(DyingEventArgs ev)
        {
            if (_invisiblePlayers.Contains(ev.Player))
                _invisiblePlayers.Remove(ev.Player);
        }

        private void OnValidatingVisibility(ValidatingVisibilityEventArgs ev)
        {
            if (_invisiblePlayers.Contains(ev.Target))
                ev.IsAllowed = false;
        }

        private void OnPlayingFootstep(PlayingFootstepEventArgs ev)
        {
            if (_invisiblePlayers.Contains(ev.Player))
                ev.IsAllowed = false;
        }

        private void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (_invisiblePlayers.Contains(ev.Player))
                Timing.CallDelayed(0.1f, () => ReapplyInvisibility(ev.Player));
        }

        private void OnInteractingElevator(InteractingElevatorEventArgs ev)
        {
            if (_invisiblePlayers.Contains(ev.Player))
                Timing.CallDelayed(0.1f, () => ReapplyInvisibility(ev.Player));
        }

        private void OnInteractingLocker(InteractingLockerEventArgs ev)
        {
            if (_invisiblePlayers.Contains(ev.Player))
                Timing.CallDelayed(0.1f, () => ReapplyInvisibility(ev.Player));
        }

        private void ReapplyInvisibility(Player player)
        {
            if (player is { IsAlive: true } && _invisiblePlayers.Contains(player))
                player.EnableEffect(EffectType.Invisible, 1);
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
                _invisiblePlayers.Add(ev.Player);
                ev.Player.EnableEffect(EffectType.Invisible, 1, _config.Duration);
                ev.Player.ShowHint(ActivationMessage, 5f);

                // Remove the item as it's consumed
                ev.Player.RemoveItem(ev.Player.CurrentItem);

                // Track usage to prevent repeated uses
                _usedByPlayers.Add(userId);

                // Schedule end of invisibility
                Timing.CallDelayed(_config.Duration, () =>
                {
                    if (ev.Player is { IsAlive: true })
                    {
                        _invisiblePlayers.Remove(ev.Player);
                        ev.Player.ShowHint(DeactivationMessage, 5f);
                    }
                    else if (ev.Player != null)
                    {
                        _invisiblePlayers.Remove(ev.Player);
                    }
                });
            }
            catch (System.Exception ex)
            {
                Log.Error($"SCP500D: Error in OnUsingItem: {ex.Message}");
            }
        }
    }
}
