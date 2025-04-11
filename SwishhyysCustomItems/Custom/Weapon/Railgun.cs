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
using Exiled.Events.EventArgs.Item;


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
            Exiled.Events.Handlers.Player.ActivatingWorkstation += OnModify;
            Exiled.Events.Handlers.Item.ChangingAttachments += OnAttachmentChange;
            base.SubscribeEvents();
            Plugin.Instance?.DebugLog("Railgun event subscriptions completed");
        }

        // Unsubscribe from events when item is unregistered
        protected override void UnsubscribeEvents()
        {
            Plugin.Instance?.DebugLog("Railgun.UnsubscribeEvents called");
            Exiled.Events.Handlers.Player.Shot -= OnPlayerShot;
            Exiled.Events.Handlers.Player.ActivatingWorkstation -= OnModify;
            Exiled.Events.Handlers.Item.ChangingAttachments -= OnAttachmentChange;
            base.UnsubscribeEvents();
            Plugin.Instance?.DebugLog("Railgun event unsubscriptions completed");
        }

        // Handle workstation activation to prevent modifications
        private void OnModify(ActivatingWorkstationEventArgs ev)
        {
            try
            {
                // Check if the player is trying to modify a railgun
                if (ev.Player?.CurrentItem != null && Check(ev.Player.CurrentItem))
                {
                    // Prevent workstation activation
                    ev.IsAllowed = false;
                    ev.Player.ShowHint("<color=#FF0000>This Railgun cannot be modified!</color>", 3f);
                    Plugin.Instance?.DebugLog($"Prevented {ev.Player.Nickname} from modifying Railgun at workstation");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Railgun: Error in OnModify: {ex.Message}");
                Plugin.Instance?.DebugLog($"OnModify: Exception: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // Handle attachment changes to prevent modifications
        private void OnAttachmentChange(ChangingAttachmentsEventArgs ev)
        {
            try
            {
                // Check if someone is trying to modify railgun attachments
                if (ev.Item != null && Check(ev.Item))
                {
                    // Prevent attachment changes
                    ev.IsAllowed = false;
                    ev.Player?.ShowHint("<color=#FF0000>This Railgun cannot be modified!</color>", 3f);
                    Plugin.Instance?.DebugLog("Prevented Railgun attachment modification");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Railgun: Error in OnAttachmentChange: {ex.Message}");
                Plugin.Instance?.DebugLog($"OnAttachmentChange: Exception: {ex.Message}\n{ex.StackTrace}");
            }
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
                Timing.CallDelayed(0.5f, () =>
                {
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
        // Add this field to store randomized beam color information
        private readonly System.Random _random = new System.Random();

        // Method to spawn an animated beam effect with randomized hues
        private void SpawnBeam(Vector3 start, Vector3 end)
        {
            try
            {
                Vector3 direction = end - start;
                float distance = direction.magnitude;
                Vector3 midPoint = start + direction * 0.5f;
                Quaternion rotation = Quaternion.LookRotation(direction);

                // Create main beam
                Primitive mainBeam = Primitive.Create(PrimitiveType.Cylinder);
                mainBeam.Flags = PrimitiveFlags.Visible;

                // Generate a base hue for the beam (randomize between blue-cyan)
                float baseHue = 0.5f + (_random.Next(-10, 10) / 100f); // Randomize around 0.5 (cyan)
                Color beamColor = Color.HSVToRGB(baseHue, 1f, 10f);
                beamColor.a = 0.9f;

                mainBeam.Color = beamColor;
                mainBeam.Position = midPoint;
                mainBeam.Rotation = rotation * Quaternion.Euler(90f, 0f, 0f);
                mainBeam.Scale = new Vector3(0.05f, distance / 2f, 0.05f);
                _spawnedPrimitives.Enqueue(mainBeam);

                // Create a slightly larger, more transparent outer beam with a complementary hue
                Primitive outerBeam = Primitive.Create(PrimitiveType.Cylinder);
                outerBeam.Flags = PrimitiveFlags.Visible;

                // Complementary hue for outer glow
                float outerHue = baseHue + (_random.Next(-5, 5) / 100f);
                Color outerColor = Color.HSVToRGB(outerHue, 0.8f, 5f);
                outerColor.a = 0.5f;

                outerBeam.Color = outerColor;
                outerBeam.Position = midPoint;
                outerBeam.Rotation = rotation * Quaternion.Euler(90f, 0f, 0f);
                outerBeam.Scale = new Vector3(0.08f, distance / 2f, 0.08f);
                _spawnedPrimitives.Enqueue(outerBeam);

                // Start coroutines for beam effects
                Timing.RunCoroutine(AnimateBeam(mainBeam, outerBeam, 3.5f, baseHue));

                Plugin.Instance?.DebugLog("Railgun animated beam effect created");
            }
            catch (Exception ex)
            {
                Log.Error($"Error creating beam effect: {ex.Message}");
                Plugin.Instance?.DebugLog($"SpawnBeam: Exception: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // Coroutine to animate and destroy the beam
        private IEnumerator<float> AnimateBeam(Primitive mainBeam, Primitive outerBeam, float duration, float baseHue)
        {
            if (mainBeam == null || outerBeam == null)
                yield break;

            Color initialMainColor = mainBeam.Color;
            Color initialOuterColor = outerBeam.Color;

            float elapsedTime = 0f;
            float pulseSpeed = 8f;
            float colorShiftSpeed = 3f;

            while (elapsedTime < duration)
            {
                if (mainBeam == null || outerBeam == null)
                    break;

                // Calculate animation progress (0 to 1)
                float fadeProgress = elapsedTime / duration;

                // Pulse effect - sin wave for beam thickness
                float pulseFactor = 1f + (0.15f * Mathf.Sin(elapsedTime * pulseSpeed));

                // Color shifting effect - slowly shift hue over time
                float mainHueShift = baseHue + (0.05f * Mathf.Sin(elapsedTime * colorShiftSpeed));
                float outerHueShift = baseHue + (0.08f * Mathf.Sin(elapsedTime * colorShiftSpeed + 1f));

                // Update main beam
                mainBeam.Scale = new Vector3(
                    0.05f * pulseFactor,
                    mainBeam.Scale.y,
                    0.05f * pulseFactor);

                Color mainColor = Color.HSVToRGB(mainHueShift, 1f, 10f * (1f - fadeProgress * 0.5f));
                mainColor.a = Mathf.Lerp(initialMainColor.a, 0f, fadeProgress);
                mainBeam.Color = mainColor;

                // Update outer beam
                outerBeam.Scale = new Vector3(
                    0.08f * pulseFactor,
                    outerBeam.Scale.y,
                    0.08f * pulseFactor);

                Color outerColor = Color.HSVToRGB(outerHueShift, 0.8f, 5f * (1f - fadeProgress * 0.5f));
                outerColor.a = Mathf.Lerp(initialOuterColor.a, 0f, fadeProgress);
                outerBeam.Color = outerColor;

                elapsedTime += 0.05f;
                yield return Timing.WaitForSeconds(0.05f);
            }

            // Cleanup
            mainBeam?.Destroy();
            outerBeam?.Destroy();

            // Remove from queue
            if (_spawnedPrimitives.Contains(mainBeam))
                _spawnedPrimitives.Dequeue();
            if (_spawnedPrimitives.Contains(outerBeam))
                _spawnedPrimitives.Dequeue();
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

          