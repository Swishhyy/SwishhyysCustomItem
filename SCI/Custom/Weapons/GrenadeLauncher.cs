using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;
using YamlDotNet.Serialization;
using JetBrains.Annotations;
using SCI.Config;
using Exiled.API.Features.Items;

namespace SCI.Custom.Weapon
{
    public class GrenadeLauncher(GrenadeLauncherConfig config) : CustomWeapon
    {
        #region Configuration
        [YamlIgnore]
        public override ItemType Type { get; set; } = ItemType.GunLogicer;
        public override uint Id { get; set; } = 111;
        public override float Damage { get; set; } = 0.1f;
        public override string Name { get; set; } = "<color=#FFA500>Grenade Launcher</color>";
        public override string Description { get; set; } = "A weapon that fires grenades at a distance. Has 8 shots total.";
        public override byte ClipSize { get; set; } = 8; // Setting clip size to 8
        public override float Weight { get; set; } = 3.0f;

        private const float FuseTime = 1.0f;
        private const float SpawnDistance = 1.0f;
        private const int MaxShots = 8; // Maximum number of shots

        // Dictionary to track remaining shots for each player
        private readonly Dictionary<Player, int> _remainingShots = new Dictionary<Player, int>();

        [CanBeNull]
        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties
        {
            Limit = 1,
            DynamicSpawnPoints =
            [
                new() {
                    Chance = 15,
                    Location = SpawnLocationType.InsideHczArmory,
                },
                new() {
                    Chance = 15,
                    Location = SpawnLocationType.InsideSurfaceNuke,
                }
            ]
        };
        private readonly GrenadeLauncherConfig _config = config;
        #endregion

        #region Constructor and Event Management
        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shooting += OnShooting;
            Exiled.Events.Handlers.Player.DroppingItem += OnDroppingItem;
            Exiled.Events.Handlers.Player.Dying += OnPlayerDying;
            Exiled.Events.Handlers.Player.PickingUpItem += OnPickingUpItem;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shooting -= OnShooting;
            Exiled.Events.Handlers.Player.DroppingItem -= OnDroppingItem;
            Exiled.Events.Handlers.Player.Dying -= OnPlayerDying;
            Exiled.Events.Handlers.Player.PickingUpItem -= OnPickingUpItem;
            base.UnsubscribeEvents();
        }
        #endregion

        #region Event Handlers
        protected override void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (Check(ev.Item) && _remainingShots.ContainsKey(ev.Player))
            {
                _remainingShots.Remove(ev.Player);
            }
            
            base.OnDroppingItem(ev);
        }

        private void OnPlayerDying(DyingEventArgs ev)
        {
            if (_remainingShots.ContainsKey(ev.Player))
            {
                _remainingShots.Remove(ev.Player);
            }
        }

        private void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (Check(ev.Pickup) && !_remainingShots.ContainsKey(ev.Player))
            {
                // Initialize ammunition count when picking up the item
                _remainingShots[ev.Player] = MaxShots;
                Timing.CallDelayed(0.5f, () => 
                {
                    ev.Player.ShowHint("<color=#FFA500>Grenade Launcher: 8 shots loaded</color>", 3f);
                });
            }
        }
        #endregion

        #region Weapon Functionality
        protected override void OnShooting(ShootingEventArgs ev)
        {
            try
            {
                if (!Check(ev.Player.CurrentItem))
                    return;

                // Prevent direct damage from the shot
                ev.IsAllowed = false;

                // Check if player has shots remaining, initialize if not in dictionary
                if (!_remainingShots.TryGetValue(ev.Player, out int shots))
                {
                    _remainingShots[ev.Player] = MaxShots;
                    shots = MaxShots;
                }

                // Check if we have shots left
                if (shots <= 0)
                {
                    ev.Player.ShowHint("<color=#FF0000>Grenade Launcher: No ammunition remaining!</color>", 2f);
                    
                    // Remove the weapon if it's empty
                    Timing.CallDelayed(0.5f, () => 
                    {
                        if (ev.Player.IsAlive && Check(ev.Player.CurrentItem))
                        {
                            ev.Player.RemoveItem(ev.Player.CurrentItem);
                            ev.Player.ShowHint("<color=#FF0000>Grenade Launcher has been discarded.</color>", 3f);
                        }
                    });
                    
                    return;
                }

                // Decrement shots and update player
                _remainingShots[ev.Player] = shots - 1;
                ev.Player.ShowHint($"<color=#FFA500>Grenade Launcher: {_remainingShots[ev.Player]} shots remaining</color>", 2f);

                // Spawn position calculation
                Vector3 spawnPos = ev.Player.CameraTransform.position + ev.Player.CameraTransform.forward * SpawnDistance;

                // Create and spawn the grenade
                Pickup grenade = Pickup.CreateAndSpawn(ItemType.GrenadeHE, spawnPos, Quaternion.identity);

                // Apply physics to the grenade
                if (grenade.Rigidbody is Rigidbody rb)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;

                    // Calculate launch direction with upward arc
                    Vector3 launchDirection = ev.Player.CameraTransform.forward + Vector3.up * _config.UpwardArc;
                    launchDirection.Normalize();

                    // Apply launch force
                    rb.velocity = launchDirection * _config.LaunchForce;
                }
                else
                {
                    Log.Warn("Grenade doesn't have a rigidbody.");
                }

                // Delayed explosion
                Timing.CallDelayed(_config.ExplosionDelay, () =>
                {
                    if (grenade != null)
                    {
                        var grenadePickup = grenade.As<GrenadePickup>();
                        if (grenadePickup != null)
                        {
                            grenadePickup.FuseTime = FuseTime;
                            grenadePickup.Explode();
                        }
                    }
                });

                // Check if this was the last shot
                if (_remainingShots[ev.Player] <= 0)
                {
                    Timing.CallDelayed(0.5f, () =>
                    {
                        if (ev.Player.IsAlive && Check(ev.Player.CurrentItem))
                        {
                            ev.Player.RemoveItem(ev.Player.CurrentItem);
                            ev.Player.ShowHint("<color=#FF0000>Grenade Launcher is empty and has been discarded.</color>", 3f);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error($"GrenadeLauncher: Error in OnShooting: {ex.Message}");
            }
        }
        #endregion
    }
}
