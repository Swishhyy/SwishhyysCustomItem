using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using SCI.Config;
using System;
using UnityEngine;
using Exiled.API.Features.Items;
using JetBrains.Annotations;
using Exiled.API.Features.Roles;
using PlayerRoles;
using System.Collections.Generic;

namespace SCI.Custom.MedicalItems
{
    public class Anti096SCP500Pills(Anti096SCP500pPillsConfig config) : CustomItem
    {
        #region Configuration
        public override uint Id { get; set; } = 103;
        public override ItemType Type { get; set; } = ItemType.SCP500;
        public override string Name { get; set; } = "<color=#00FF00>Anti-096 Pills</color>";
        public override string Description { get; set; } = "Pills that cause the user to no longer be a target of SCP-096";
        public override float Weight { get; set; } = 0.5f;

        private const string NotTargetedMessage = "You are not on SCP-096's target list. The pills have no effect.";
        private const string TargetRemovedMessage = "You have been removed from SCP-096's target list.";
        private const string ImmunityProvidedMessage = "You are now immune to SCP-096 targeting for {0} seconds.";

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

        private readonly Anti096SCP500pPillsConfig _config = config;
        private readonly Dictionary<string, DateTime> _immunityExpiration = [];

        #endregion

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
            Exiled.Events.Handlers.Scp096.AddingTarget += OnAddingTarget;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
            Exiled.Events.Handlers.Scp096.AddingTarget -= OnAddingTarget;
            base.UnsubscribeEvents();
        }

        private void OnAddingTarget(Exiled.Events.EventArgs.Scp096.AddingTargetEventArgs ev)
        {
            if (ev.Target == null || !_config.ProvideTemporaryImmunity) return;

            // Check if player has immunity
            if (_immunityExpiration.TryGetValue(ev.Target.UserId, out DateTime expiryTime))
            {
                if (DateTime.UtcNow < expiryTime)
                {
                    // Player has immunity, prevent targeting
                    ev.IsAllowed = false;
                    Log.Debug($"Player {ev.Target.Nickname} has immunity to SCP-096 targeting. Target acquisition prevented.");
                }
                else
                {
                    // Immunity expired, remove from dictionary
                    _immunityExpiration.Remove(ev.Target.UserId);
                }
            }
        }

        public void OnUsingItem(UsingItemEventArgs ev)
        {
            try
            {
                // Check if this is our item
                if (!Check(ev.Player?.CurrentItem))
                    return;

                if (ev.Player == null)
                {
                    Log.Error("Anti096Pills: Player is null in OnUsingItem");
                    return;
                }

                // Check if player is on SCP-096's target list by finding SCP-096 players
                bool isTarget = false;
                foreach (Player player in Player.List)
                {
                    if (player.Role.Type == RoleTypeId.Scp096)
                    {
                        // Check if our player is in 096's targets
                        Scp096Role scp096 = player.Role as Scp096Role;
                        if (scp096 != null && scp096.HasTarget(ev.Player))
                        {
                            isTarget = true;
                            // Remove player from SCP-096's targets
                            scp096.RemoveTarget(ev.Player);
                        }
                    }
                }

                // Handle based on target status
                if (isTarget)
                {
                    // Show target removed message
                    ev.Player.ShowHint(TargetRemovedMessage, _config.MessageDuration);

                    // Provide temporary immunity if configured
                    if (_config.ProvideTemporaryImmunity && _config.ImmunityDuration > 0)
                    {
                        _immunityExpiration[ev.Player.UserId] = DateTime.UtcNow.AddSeconds(_config.ImmunityDuration);
                        ev.Player.ShowHint(string.Format(ImmunityProvidedMessage, _config.ImmunityDuration), _config.MessageDuration);
                        Log.Debug($"Player {ev.Player.Nickname} gained immunity to SCP-096 targeting for {_config.ImmunityDuration} seconds.");
                    }

                    // Remove the item from inventory as it was consumed
                    ev.Player.RemoveItem(ev.Player.CurrentItem);

                    Log.Debug($"Player {ev.Player.Nickname} used Anti-096 Pills and was removed from SCP-096's target list.");
                }
                else
                {
                    // Not on target list, do not consume the item
                    ev.Player.ShowHint(NotTargetedMessage, _config.MessageDuration);
                    ev.IsAllowed = false; // Prevents item from being consumed

                    Log.Debug($"Player {ev.Player.Nickname} attempted to use Anti-096 Pills but was not on SCP-096's target list.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Anti096Pills: Error in OnUsingItem: {ex.Message}");
            }
        }
    }
}
