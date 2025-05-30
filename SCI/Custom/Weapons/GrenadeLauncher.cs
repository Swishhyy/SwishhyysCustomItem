using System;
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
        public override string Description { get; set; } = "A weapon that fires grenades at a distance";
        public override byte ClipSize { get; set; } = 3;
        public override float Weight { get; set; } = 3.0f;

        private const float FuseTime = 1.0f;
        private const float SpawnDistance = 1.0f;

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
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shooting -= OnShooting;
            base.UnsubscribeEvents();
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
            }
            catch (Exception ex)
            {
                Log.Error($"GrenadeLauncher: Error in OnShooting: {ex.Message}");
            }
        }
        #endregion
    }
}
