using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using JetBrains.Annotations;
using SCI.Config;
using UnityEngine;
using YamlDotNet.Serialization;
using MEC;
using Player = Exiled.API.Features.Player;
using Log = Exiled.API.Features.Log;
using Exiled.API.Enums;

namespace SCI.Custom.Weapon
{
    public class Railgun : CustomItem
    {
        // Readonly field to hold configuration for this weapon
        private readonly RailgunConfig _config;

        // Constructor that takes a configuration object
        public Railgun(RailgunConfig config)
        {
            Plugin.Instance?.DebugLog("Railgun constructor with config called");
            _config = config;
            Id = config.Id;
            Plugin.Instance?.DebugLog($"Railgun initialized with config: Damage={_config.Damage}, Range={_config.Range}, BeamWidth={_config.BeamWidth}");
        }

        // Define item properties
        [YamlIgnore]
        public override ItemType Type { get; set; } = ItemType.ParticleDisruptor;
        public override uint Id { get; set; } = 107;
        public override string Name { get; set; } = "<color=#0066FF>Railgun</color>";
        public override string Description { get; set; } = "A powerful railgun created by combining a Micro HID and a Particle Disruptor";
        public override float Weight { get; set; } = 3.2f;

        // Define spawn properties
        [CanBeNull]
        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties
        {
            Limit = 1,
            DynamicSpawnPoints = new List<DynamicSpawnPoint>()
        };

        // Subscribe to events when item is registered
        protected override void SubscribeEvents()
        {
            Plugin.Instance?.DebugLog("Railgun.SubscribeEvents called");
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
            base.SubscribeEvents();
            Plugin.Instance?.DebugLog("Railgun event subscriptions completed");
        }

        // Unsubscribe from events when item is unregistered
        protected override void UnsubscribeEvents()
        {
            Plugin.Instance?.DebugLog("Railgun.UnsubscribeEvents called");
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
            base.UnsubscribeEvents();
            Plugin.Instance?.DebugLog("Railgun event unsubscriptions completed");
        }

        // Handle when player uses the railgun
        private void OnUsingItem(UsingItemEventArgs ev)
        {
            try
            {
                if (!Check(ev.Item))
                {
                    Plugin.Instance?.DebugLog("OnUsingItem: Item check failed, not our railgun");
                    return;
                }

                Plugin.Instance?.DebugLog($"Railgun being used by {ev.Player.Nickname}");

                // Fire logic executed when the item is used
                Task.Run(() => FireRailgun(ev.Player));
            }
            catch (Exception ex)
            {
                Log.Error($"Railgun: Error in OnUsingItem: {ex.Message}");
                Plugin.Instance?.DebugLog($"OnUsingItem: Exception: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // Railgun firing logic
        private async Task FireRailgun(Player player)
        {
            try
            {
                Plugin.Instance?.DebugLog("Firing railgun");

                // Create the beam effect and calculate hit point
                if (!Physics.Raycast(player.CameraTransform.position, player.CameraTransform.forward, out RaycastHit hit, _config.Range))
                {
                    Plugin.Instance?.DebugLog("Railgun fired but hit nothing");
                    return;
                }

                Vector3 endPosition = hit.point;

                // Create visual beam effect
                Plugin.Instance?.DebugLog($"Creating visual effect from {player.CameraTransform.position} to {endPosition}");

                // Use multiple small visual effects instead of TeslaGate.Zap
                for (int i = 0; i < 3; i++)
                {
                    int index = i; // Capture the iteration variable for the closure
                    Timing.CallDelayed(i * 0.1f, () =>
                    {
                        try
                        {
                            // Create particle effect by spawning a small explosion and removing it quickly
                            ExplosiveGrenade visualEffect = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
                            visualEffect.FuseTime = 0.01f;

                            // Calculate position along the beam for this visual effect
                            float t = (float)index / 2f; // 0, 0.5, or 1
                            Vector3 effectPosition = Vector3.Lerp(player.CameraTransform.position, endPosition, t);

                            visualEffect.SpawnActive(effectPosition);

                            Plugin.Instance?.DebugLog($"Created visual effect {index} at {effectPosition}");
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Failed to create beam visual effect: {ex.Message}");
                        }
                    });
                }

                // Find and damage players in the beam's path
                foreach (Player target in Player.List)
                {
                    if (!target.IsAlive || target == player)
                        continue;

                    // Check if the player is in the beam path
                    Vector3 directionToTarget = target.Position - player.CameraTransform.position;
                    float distanceToPlayer = directionToTarget.magnitude;

                    if (distanceToPlayer > _config.Range)
                        continue;

                    // Calculate how close the player is to the beam's path
                    Vector3 beamDirection = player.CameraTransform.forward.normalized;
                    Vector3 perpendicularDirection = directionToTarget - Vector3.Dot(directionToTarget, beamDirection) * beamDirection;
                    float distanceToBeam = perpendicularDirection.magnitude;

                    // If the player is close enough to the beam path, damage them
                    if (distanceToBeam <= _config.BeamWidth)
                    {
                        Plugin.Instance?.DebugLog($"Damaging player {target.Nickname} for {_config.Damage} damage");
                        target.Hurt(_config.Damage, DamageType.Explosion); // Using Explosion instead of Tesla
                    }
                }

                // Create explosion effect at impact point
                if (_config.SpawnExplosive)
                {
                    Plugin.Instance?.DebugLog($"Creating explosion at {endPosition}");
                    ExplosiveGrenade grenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
                    grenade.FuseTime = 0.1f; // Near-instant detonation
                    grenade.SpawnActive(hit.point);
                }

                // Small delay for effect
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                Log.Error($"Railgun: Error in FireRailgun: {ex.Message}");
                Plugin.Instance?.DebugLog($"FireRailgun: Exception: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
