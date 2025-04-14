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

namespace SCI.Custom.Weapon
{
    public class GrenadeLauncher : CustomWeapon
    {
        private readonly GrenadeLauncherConfig _config;

        public GrenadeLauncher(GrenadeLauncherConfig config)
        {
            Plugin.Instance?.DebugLog("GrenadeLauncher constructor with config called");
            _config = config;
            Plugin.Instance?.DebugLog($"GrenadeLauncher initialized with config: LaunchForce={_config.LaunchForce}, FuseTime={FuseTime}");
        }

        [YamlIgnore]
        public override ItemType Type { get; set; } = ItemType.GunLogicer;
        public override uint Id { get; set; } = 108;
        public override float Damage { get; set; } = 0.1f;
        public override string Name { get; set; } = "<color=#FFA500>Grenade Launcher</color>";
        public override string Description { get; set; } = "A weapon that fires grenades at a distance";
        public override byte ClipSize { get; set; } = 3;
        public override float Weight { get; set; } = 3.0f;
        private float FuseTime { get; set; } = 1.0f;
        private float SpawnDistance { get; set; } = 1.0f;

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

        protected override void SubscribeEvents()
        {
            Plugin.Instance?.DebugLog("GrenadeLauncher.SubscribeEvents called");
            Exiled.Events.Handlers.Player.Shot += OnShotDD;
            base.SubscribeEvents();
            Plugin.Instance?.DebugLog("GrenadeLauncher event subscriptions completed");
        }

        protected override void UnsubscribeEvents()
        {
            Plugin.Instance?.DebugLog("GrenadeLauncher.UnsubscribeEvents called");
            Exiled.Events.Handlers.Player.Shot -= OnShotDD;
            base.UnsubscribeEvents();
            Plugin.Instance?.DebugLog("GrenadeLauncher event unsubscriptions completed");
        }

        private void OnShotDD(ShotEventArgs ev)
        {
            try
            {
                if (!Check(ev.Player.CurrentItem))
                    return;

                Plugin.Instance?.DebugLog($"GrenadeLauncher fired by {ev.Player.Nickname}");

                ev.CanHurt = false;

                // Use configuration for spawn position distance
                Vector3 spawnPos = ev.Player.CameraTransform.position + ev.Player.CameraTransform.forward * SpawnDistance;
                Quaternion rotation = Quaternion.identity;

                // Create and spawn the grenade
                Pickup grenade = Pickup.CreateAndSpawn(ItemType.GrenadeHE, spawnPos, rotation);

                if (grenade.Rigidbody is Rigidbody rb)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;

                    // Add configurable upward arc
                    Vector3 launchDirection = ev.Player.CameraTransform.forward + Vector3.up * _config.UpwardArc;
                    launchDirection.Normalize();

                    // Use configurable launch force
                    rb.velocity = launchDirection * _config.LaunchForce;

                    Plugin.Instance?.DebugLog($"Grenade launched with force: {_config.LaunchForce}");
                }
                else
                {
                    Log.Warn("Grenade doesn't have a rigidbody.");
                    Plugin.Instance?.DebugLog("Failed to access grenade rigidbody");
                }

                // Use configurable fuse time
                Timing.CallDelayed(_config.ExplosionDelay, () =>
                {
                    if (grenade != null)
                    {
                        var grenadePickup = grenade.As<GrenadePickup>();
                        if (grenadePickup != null)
                        {
                            grenadePickup.FuseTime = FuseTime;
                            grenadePickup.Explode();
                            Plugin.Instance?.DebugLog("Grenade explosion triggered");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error($"GrenadeLauncher: Error in OnShot: {ex.Message}");
                Plugin.Instance?.DebugLog($"OnShot: Exception: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
