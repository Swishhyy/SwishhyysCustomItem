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
using Exiled.API.Features.Toys;
using Exiled.Events.EventArgs.Item;
using SCI.Config;
using AdminToys;

namespace SCI.Custom.Weapon
{
    public class Railgun(RailgunConfig config) : CustomWeapon
    {
        #region Configuration
        [YamlIgnore]
        public override ItemType Type { get; set; } = ItemType.GunE11SR;
        public override uint Id { get; set; } = 112;
        public override float Damage { get; set; } = 150f;
        public override string Name { get; set; } = "<color=#0066FF>Railgun</color>";
        public override string Description { get; set; } = "A powerful railgun created by combining a Micro HID and a Particle Disruptor";
        public override float Weight { get; set; } = 3.2f;
        public override byte ClipSize { get; set; } = 1;
        private const bool SpawnExplosive = true;
        private const float BeamWidth = 0.75f;

        [YamlIgnore]
        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties
        {
            Limit = 1,
            DynamicSpawnPoints =
            [
               new()
               {
                   Chance = 10,
                   Location = SpawnLocationType.InsideHczArmory,
               },
               new()
               {
                   Chance = 10,
                   Location = SpawnLocationType.InsideSurfaceNuke,
               }
            ]
        };
        private readonly RailgunConfig _config = config;
        private readonly Queue<Primitive> _spawnedPrimitives = new(4);
        private readonly System.Random _random = new();
        #endregion

        #region Constructor and Event Management
        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shot += OnPlayerShot;
            Exiled.Events.Handlers.Player.ActivatingWorkstation += OnModify;
            Exiled.Events.Handlers.Item.ChangingAttachments += OnAttachmentChange;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shot -= OnPlayerShot;
            Exiled.Events.Handlers.Player.ActivatingWorkstation -= OnModify;
            Exiled.Events.Handlers.Item.ChangingAttachments -= OnAttachmentChange;
            base.UnsubscribeEvents();
        }
        #endregion

        #region Event Handlers
        private void OnModify(ActivatingWorkstationEventArgs ev)
        {
            try
            {
                if (ev.Player?.CurrentItem != null && Check(ev.Player.CurrentItem))
                {
                    ev.IsAllowed = false;
                    ev.Player.ShowHint("<color=#FF0000>This Railgun cannot be modified!</color>", 3f);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Railgun: Error in OnModify: {ex.Message}");
            }
        }

        private void OnAttachmentChange(ChangingAttachmentsEventArgs ev)
        {
            try
            {
                if (ev.Item != null && Check(ev.Item))
                {
                    ev.IsAllowed = false;
                    ev.Player?.ShowHint("<color=#FF0000>This Railgun cannot be modified!</color>", 3f);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Railgun: Error in OnAttachmentChange: {ex.Message}");
            }
        }
        private void OnPlayerShot(ShotEventArgs ev)
        {
            try
            {
                if (!Check(ev.Player.CurrentItem))
                    return;

                // Get the current item as a Firearm
                if (ev.Player.CurrentItem is not Firearm firearm)
                    return;

                // Check ammo
                int currentAmmo = GetAmmoFromFirearm(firearm);
                if (currentAmmo <= 0)
                {
                    ev.Player.ShowHint("<color=#FF0000>Railgun: No charge!</color>", 2f);
                    return;
                }

                // Set ammo to 0 to simulate using all ammo for the shot
                SetAmmoInFirearm(firearm, 0);

                // Fire the railgun
                FireRailgun(ev.Player);

                // Show charging notification
                Timing.CallDelayed(0.5f, () =>
                {
                    if (ev.Player.IsAlive && ev.Player.CurrentItem != null && Check(ev.Player.CurrentItem))
                        ev.Player.ShowHint("<color=#0066FF>Railgun charging...</color>", 3f);
                });
            }
            catch (Exception ex)
            {
                Log.Error($"Railgun: Error in OnShot: {ex.Message}");
            }
        }
        #endregion

        #region Weapon Functionality
        private void FireRailgun(Player player)
        {
            try
            {
                // Create the beam effect and calculate hit point
                bool hitSomething = Physics.Raycast(player.CameraTransform.position, player.CameraTransform.forward, out RaycastHit hit, _config.Range);

                // Determine end position based on hit
                Vector3 impactPoint = hitSomething
                    ? hit.point
                    : player.CameraTransform.position + player.CameraTransform.forward * _config.Range;

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
                    if (distanceToBeam <= BeamWidth)
                    {
                        target.Hurt(_config.Damage, DamageType.Explosion);
                        target.ShowHint("<color=#0066FF>You've been hit by a Railgun blast!</color>", 3f);
                    }
                }

                // Create explosion effect at impact point
                if (SpawnExplosive && hitSomething)
                {
                    ExplosiveGrenade grenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
                    grenade.FuseTime = 0.01f; // Near-instant detonation
                    grenade.SpawnActive(impactPoint);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Railgun: Error in FireRailgun: {ex.Message}");
            }
        }
        #endregion

        #region Visual Effects
        private void SpawnBeam(Vector3 start, Vector3 end)
        {
            try
            {
                Vector3 direction = end - start;
                float distance = direction.magnitude;
                Vector3 midPoint = start + direction * 0.5f;
                Quaternion rotation = Quaternion.LookRotation(direction);

                // Calculate rotation once
                Quaternion beamRotation = rotation * Quaternion.Euler(90f, 0f, 0f);

                // Generate a base hue for the beam (randomize between blue-cyan)
                float baseHue = 0.5f + (_random.Next(-10, 10) / 100f); // Randomize around 0.5 (cyan)

                // Create main beam
                Primitive mainBeam = Primitive.Create(PrimitiveType.Cylinder);
                mainBeam.Flags = PrimitiveFlags.Visible;
                Color beamColor = Color.HSVToRGB(baseHue, 1f, 10f);
                beamColor.a = 0.9f;
                mainBeam.Color = beamColor;
                mainBeam.Position = midPoint;
                mainBeam.Rotation = beamRotation;
                mainBeam.Scale = new Vector3(0.05f, distance / 2f, 0.05f);
                _spawnedPrimitives.Enqueue(mainBeam);

                // Create outer beam
                Primitive outerBeam = Primitive.Create(PrimitiveType.Cylinder);
                outerBeam.Flags = PrimitiveFlags.Visible;
                float outerHue = baseHue + (_random.Next(-5, 5) / 100f);
                Color outerColor = Color.HSVToRGB(outerHue, 0.8f, 5f);
                outerColor.a = 0.5f;
                outerBeam.Color = outerColor;
                outerBeam.Position = midPoint;
                outerBeam.Rotation = beamRotation;
                outerBeam.Scale = new Vector3(0.08f, distance / 2f, 0.08f);
                _spawnedPrimitives.Enqueue(outerBeam);

                // Start animation
                Timing.RunCoroutine(AnimateBeam(mainBeam, outerBeam, 3.5f, baseHue));
            }
            catch (Exception ex)
            {
                Log.Error($"Error creating beam effect: {ex.Message}");
            }
        }
        private IEnumerator<float> AnimateBeam(Primitive mainBeam, Primitive outerBeam, float duration, float baseHue)
        {
            if (mainBeam == null || outerBeam == null)
                yield break;

            Color initialMainColor = mainBeam.Color;
            Color initialOuterColor = outerBeam.Color;
            Vector3 mainBeamInitialScale = mainBeam.Scale;
            Vector3 outerBeamInitialScale = outerBeam.Scale;

            float elapsedTime = 0f;
            const float pulseSpeed = 8f;
            const float colorShiftSpeed = 3f;
            const float updateInterval = 0.05f;

            while (elapsedTime < duration)
            {
                if (mainBeam == null || outerBeam == null)
                    break;

                // Calculate animation progress (0 to 1)
                float fadeProgress = elapsedTime / duration;

                // Pulse effect for beam thickness
                float pulseFactor = 1f + (0.15f * Mathf.Sin(elapsedTime * pulseSpeed));

                // Color shifting effect
                float mainHueShift = baseHue + (0.05f * Mathf.Sin(elapsedTime * colorShiftSpeed));
                float outerHueShift = baseHue + (0.08f * Mathf.Sin(elapsedTime * colorShiftSpeed + 1f));

                // Update main beam
                float mainScaleXZ = 0.05f * pulseFactor;
                mainBeam.Scale = new Vector3(mainScaleXZ, mainBeamInitialScale.y, mainScaleXZ);

                Color mainColor = Color.HSVToRGB(mainHueShift, 1f, 10f * (1f - fadeProgress * 0.5f));
                mainColor.a = Mathf.Lerp(initialMainColor.a, 0f, fadeProgress);
                mainBeam.Color = mainColor;

                // Update outer beam
                float outerScaleXZ = 0.08f * pulseFactor;
                outerBeam.Scale = new Vector3(outerScaleXZ, outerBeamInitialScale.y, outerScaleXZ);

                Color outerColor = Color.HSVToRGB(outerHueShift, 0.8f, 5f * (1f - fadeProgress * 0.5f));
                outerColor.a = Mathf.Lerp(initialOuterColor.a, 0f, fadeProgress);
                outerBeam.Color = outerColor;

                elapsedTime += updateInterval;
                yield return Timing.WaitForSeconds(updateInterval);
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

        private void CleanupBeams()
        {
            while (_spawnedPrimitives.Count > 0)
            {
                Primitive beam = _spawnedPrimitives.Dequeue();
                beam?.Destroy();
            }
        }
        #endregion

        #region Utility Methods
        private int GetAmmoFromFirearm(Firearm firearm)
        {
            try
            {
                // Try different approaches to get ammo count
                var ammoProperty = firearm.GetType().GetProperty("Ammo");
                if (ammoProperty != null)
                    return (int)ammoProperty.GetValue(firearm);

                var currentAmmoProperty = firearm.GetType().GetProperty("CurrentAmmo");
                if (currentAmmoProperty != null)
                    return (int)currentAmmoProperty.GetValue(firearm);

                var ammoField = firearm.GetType().GetField("Ammunitions");
                if (ammoField != null)
                    return (int)ammoField.GetValue(firearm);

                var ammoProperty2 = firearm.GetType().GetProperty("Ammunitions");
                if (ammoProperty2 != null)
                    return (int)ammoProperty2.GetValue(firearm);

                // Try base object
                if (firearm.Base != null)
                {
                    var baseAmmoProperty = firearm.Base.GetType().GetProperty("Ammo") ??
                                         firearm.Base.GetType().GetProperty("CurrentAmmo") ??
                                         firearm.Base.GetType().GetProperty("Ammunitions");

                    if (baseAmmoProperty != null)
                        return (int)baseAmmoProperty.GetValue(firearm.Base);
                }

                return 1; // Default assumption
            }
            catch
            {
                return 1; // Default on error
            }
        }

        private void SetAmmoInFirearm(Firearm firearm, int value)
        {
            try
            {
                // Try different approaches to set ammo
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

                // Try base object
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
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error setting ammo: {ex.Message}");
            }
        }
        #endregion

        #region Lifecycle Methods
        public override void Destroy()
        {
            CleanupBeams();
            base.Destroy();
        }
        #endregion
    }
}
