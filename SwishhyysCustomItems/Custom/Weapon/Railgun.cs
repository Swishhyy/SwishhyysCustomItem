using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features.Items;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using UnityEngine;
using YamlDotNet.Serialization;
using MEC;
using Player = Exiled.API.Features.Player;
using Log = Exiled.API.Features.Log;
using JetBrains.Annotations;
using AdminToys;
using Exiled.API.Features.Toys;


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
        public override ItemType Type { get; set; } = ItemType.GunE11SR;
        public override uint Id { get; set; } = 107;
        public override string Name { get; set; } = "<color=#0066FF>Railgun</color>";
        public override string Description { get; set; } = "A powerful railgun created by combining a Micro HID and a Particle Disruptor";
        public override float Weight { get; set; } = 3.2f;
        [CanBeNull]
        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties
        {
            Limit = 1,
            DynamicSpawnPoints = new List<DynamicSpawnPoint>
           {
               new DynamicSpawnPoint
               {
                   Chance = 10,
                   Location = SpawnLocationType.InsideHczArmory,
               },
               new DynamicSpawnPoint
               {
                   Chance = 10,
                   Location = SpawnLocationType.InsideSurfaceNuke,
               }
           }
        };

        // Subscribe to events when item is registered
        protected override void SubscribeEvents()
        {
            Plugin.Instance?.DebugLog("Railgun.SubscribeEvents called");
            Exiled.Events.Handlers.Player.Shot += OnPlayerShot;
            base.SubscribeEvents();
            Plugin.Instance?.DebugLog("Railgun event subscriptions completed");
        }

        // Unsubscribe from events when item is unregistered
        protected override void UnsubscribeEvents()
        {
            Plugin.Instance?.DebugLog("Railgun.UnsubscribeEvents called");
            Exiled.Events.Handlers.Player.Shot -= OnPlayerShot;
            base.UnsubscribeEvents();
            Plugin.Instance?.DebugLog("Railgun event unsubscriptions completed");
        }

        // Handle when player fires the railgun
        private void OnPlayerShot(ShotEventArgs ev)
        {
            try
            {
                if (!Check(ev.Player.CurrentItem))
                {
                    return;
                }

                Plugin.Instance?.DebugLog($"Railgun shot by {ev.Player.Nickname}");

                // Since we can't cancel the shot directly, we'll have to let it through
                // but handle our own damage and effects

                // Get the current item as a Firearm
                if (!(ev.Player.CurrentItem is Firearm firearm))
                {
                    Plugin.Instance?.DebugLog("Failed to cast item to Firearm");
                    return;
                }

                // Check ammo using a compatible approach
                int currentAmmo = GetAmmoFromFirearm(firearm);
                Plugin.Instance?.DebugLog($"Current ammo in firearm: {currentAmmo}");

                if (currentAmmo <= 0)
                {
                    ev.Player.ShowHint("<color=#FF0000>Railgun: No charge!</color>", 2f);
                    return;
                }

                // Store previous ammo count for logging
                int previousAmmo = currentAmmo;

                // Set ammo to 0 to simulate using all ammo for the shot
                SetAmmoInFirearm(firearm, 0);

                Plugin.Instance?.DebugLog($"Railgun fired, consuming all ammo. Was: {previousAmmo}, Now: {GetAmmoFromFirearm(firearm)}");

                // Fire logic executed when the item is used
                FireRailgun(ev.Player);

                // Show charging notification
                Timing.CallDelayed(0.5f, () => {
                    if (ev.Player.IsAlive && ev.Player.CurrentItem != null && Check(ev.Player.CurrentItem))
                    {
                        ev.Player.ShowHint("<color=#0066FF>Railgun charging...</color>", 3f);
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error($"Railgun: Error in OnShot: {ex.Message}");
                Plugin.Instance?.DebugLog($"OnShot: Exception: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // Helper method to get ammo from a Firearm based on available properties
        private int GetAmmoFromFirearm(Firearm firearm)
        {
            // Try different approaches depending on what's available in your API version
            try
            {
                // Some APIs expose Ammo property
                var ammoProperty = firearm.GetType().GetProperty("Ammo");
                if (ammoProperty != null)
                    return (int)ammoProperty.GetValue(firearm);

                // Some expose CurrentAmmo property
                var currentAmmoProperty = firearm.GetType().GetProperty("CurrentAmmo");
                if (currentAmmoProperty != null)
                    return (int)currentAmmoProperty.GetValue(firearm);

                // Some expose Ammunitions field or property
                var ammoField = firearm.GetType().GetField("Ammunitions");
                if (ammoField != null)
                    return (int)ammoField.GetValue(firearm);

                var ammoProperty2 = firearm.GetType().GetProperty("Ammunitions");
                if (ammoProperty2 != null)
                    return (int)ammoProperty2.GetValue(firearm);

                // Try to access base object, see if it has attributes
                if (firearm.Base != null)
                {
                    var baseAmmoProperty = firearm.Base.GetType().GetProperty("Ammo") ??
                                         firearm.Base.GetType().GetProperty("CurrentAmmo") ??
                                         firearm.Base.GetType().GetProperty("Ammunitions");

                    if (baseAmmoProperty != null)
                        return (int)baseAmmoProperty.GetValue(firearm.Base);
                }

                // If we can't find it, just return a default value
                Plugin.Instance?.DebugLog("Could not find ammo property on firearm");
                return 1; // Assume there's at least one bullet
            }
            catch (Exception ex)
            {
                Plugin.Instance?.DebugLog($"Error accessing ammo: {ex.Message}");
                return 1; // Default value on error
            }
        }

        // Helper method to set ammo in a Firearm
        private void SetAmmoInFirearm(Firearm firearm, int value)
        {
            try
            {
                // Try different approaches depending on what's available in your API version
                var ammoProperty = firearm.GetType().GetProperty("Ammo");
                if (ammoProperty != null && ammoProperty.CanWrite)
                {
                    ammoProperty.SetValue(firearm, value);
                    return;
                }

                var currentAmmoProperty = firearm.GetType().GetProperty("CurrentAmmo");
                if (currentAmmoProperty != null && currentAmmoProperty.CanWrite)
                {
                    currentAmmoProperty.SetValue(firearm, value);
                    return;
                }

                var ammoField = firearm.GetType().GetField("Ammunitions");
                if (ammoField != null)
                {
                    ammoField.SetValue(firearm, value);
                    return;
                }

                var ammoProperty2 = firearm.GetType().GetProperty("Ammunitions");
                if (ammoProperty2 != null && ammoProperty2.CanWrite)
                {
                    ammoProperty2.SetValue(firearm, value);
                    return;
                }

                // Try to access base object, see if it has attributes
                if (firearm.Base != null)
                {
                    var baseAmmoProperty = firearm.Base.GetType().GetProperty("Ammo");
                    if (baseAmmoProperty != null && baseAmmoProperty.CanWrite)
                    {
                        baseAmmoProperty.SetValue(firearm.Base, value);
                        return;
                    }

                    baseAmmoProperty = firearm.Base.GetType().GetProperty("CurrentAmmo");
                    if (baseAmmoProperty != null && baseAmmoProperty.CanWrite)
                    {
                        baseAmmoProperty.SetValue(firearm.Base, value);
                        return;
                    }

                    baseAmmoProperty = firearm.Base.GetType().GetProperty("Ammunitions");
                    if (baseAmmoProperty != null && baseAmmoProperty.CanWrite)
                    {
                        baseAmmoProperty.SetValue(firearm.Base, value);
                        return;
                    }
                }

                // If we couldn't set it, log the error
                Plugin.Instance?.DebugLog("Could not find ammo property to set on firearm");
            }
            catch (Exception ex)
            {
                Plugin.Instance?.DebugLog($"Error setting ammo: {ex.Message}");
            }
        }
        private readonly Queue<Primitive> _spawnedPrimitives = new Queue<Primitive>();

        private void FireRailgun(Player player)
        {
            try
            {
                Plugin.Instance?.DebugLog("Firing railgun");

                // Create the beam effect and calculate hit point
                bool hitSomething = Physics.Raycast(player.CameraTransform.position, player.CameraTransform.forward, out RaycastHit hit, _config.Range);

                // Determine end position based on hit
                Vector3 impactPoint = hitSomething
                    ? hit.point
                    : player.CameraTransform.position + player.CameraTransform.forward * _config.Range;

                if (!hitSomething)
                {
                    Plugin.Instance?.DebugLog($"Railgun fired but hit nothing in range, using max range point {impactPoint}");
                }
                else
                {
                    Plugin.Instance?.DebugLog($"Railgun hit at position {impactPoint}");
                }

                // Create a beam effect
                SpawnBeam(player.CameraTransform.position, impactPoint);

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
                        target.Hurt(_config.Damage, DamageType.Explosion);
                        target.ShowHint("<color=#0066FF>You've been hit by a Railgun blast!</color>", 3f);
                    }
                }

                // Create explosion effect at impact point
                if (_config.SpawnExplosive)
                {
                    Plugin.Instance?.DebugLog($"Creating explosion at impact point: {impactPoint}");

                    // Create the main impact explosion directly at the impact point
                    ExplosiveGrenade grenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
                    grenade.FuseTime = 0.01f; // Near-instant detonation
                    grenade.SpawnActive(impactPoint);
                    Plugin.Instance?.DebugLog($"Spawned impact explosion at: {impactPoint}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Railgun: Error in FireRailgun: {ex.Message}");
                Plugin.Instance?.DebugLog($"FireRailgun: Exception: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // Method to spawn a beam effect between two points
        private void SpawnBeam(Vector3 start, Vector3 end)
        {
            try
            {
                Vector3 direction = end - start;
                float distance = direction.magnitude;
                Vector3 midPoint = start + direction * 0.5f;
                Quaternion rotation = Quaternion.LookRotation(direction);

                // Create beam primitive
                Primitive beam = Primitive.Create(PrimitiveType.Cylinder);
                beam.Flags = PrimitiveFlags.Visible;

                // Set bright red color with slight transparency
                beam.Color = new Color(10f, 0f, 0f, 0.9f);

                // Position and orient the beam
                beam.Position = midPoint;
                beam.Rotation = rotation * Quaternion.Euler(90f, 0f, 0f);
                beam.Scale = new Vector3(0.03f, distance / 2f, 0.03f);

                // Add to queue for cleanup
                _spawnedPrimitives.Enqueue(beam);

                // Start fade out and destroy coroutine
                Timing.RunCoroutine(FadeOutAndDestroy(beam, 2f));

                Plugin.Instance?.DebugLog("Railgun beam effect created");
            }
            catch (Exception ex)
            {
                Log.Error($"Error creating beam effect: {ex.Message}");
                Plugin.Instance?.DebugLog($"SpawnBeam: Exception: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // Coroutine to fade out and destroy the beam
        private IEnumerator<float> FadeOutAndDestroy(Primitive primitive, float duration)
        {
            if (primitive == null)
                yield break;

            Color initialColor = primitive.Color;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                if (primitive == null)
                    break;

                elapsedTime += 0.1f;
                float alpha = Mathf.Lerp(initialColor.a, 0f, elapsedTime / duration);
                primitive.Color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);

                yield return Timing.WaitForSeconds(0.1f);
            }

            if (primitive != null)
            {
                primitive.Destroy(); 
                if (_spawnedPrimitives.Contains(primitive))
                {
                    _spawnedPrimitives.Dequeue();
                }
            }
        }

        // Method to clean up all beams when the weapon is unloaded
        private void CleanupBeams()
        {
            while (_spawnedPrimitives.Count > 0)
            {
                Primitive beam = _spawnedPrimitives.Dequeue();
                beam?.Destroy(); 
            }
        }

        // Override OnDestroy to clean up any remaining beams
        public override void Destroy()
        {
            CleanupBeams();
            base.Destroy();
        }
    }
}

          