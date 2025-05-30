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
using System.Linq;
using Exiled.API.Extensions;

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
        private readonly Queue<Primitive> _spawnedPrimitives = new(12); // Increased size for charging effects
        private readonly System.Random _random = new();
        private readonly Dictionary<Player, CoroutineHandle> _chargingPlayers = new Dictionary<Player, CoroutineHandle>();
        private readonly Dictionary<Player, List<Primitive>> _chargingEffects = new Dictionary<Player, List<Primitive>>();
        #endregion

        #region Constructor and Event Management
        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shooting += OnShooting;
            Exiled.Events.Handlers.Player.ActivatingWorkstation += OnModify;
            Exiled.Events.Handlers.Item.ChangingAttachments += OnAttachmentChange;
            Exiled.Events.Handlers.Player.Dying += OnPlayerDying;
            Exiled.Events.Handlers.Player.DroppingItem += OnItemDropped;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shooting -= OnShooting;
            Exiled.Events.Handlers.Player.ActivatingWorkstation -= OnModify;
            Exiled.Events.Handlers.Item.ChangingAttachments -= OnAttachmentChange;
            Exiled.Events.Handlers.Player.Dying -= OnPlayerDying;
            Exiled.Events.Handlers.Player.DroppingItem -= OnItemDropped;
            base.UnsubscribeEvents();
        }
        #endregion

        #region Event Handlers
        protected override void OnShooting(ShootingEventArgs ev)
        {
            try
            {
                if (ev.Player == null || ev.Player.CurrentItem == null || !Check(ev.Player.CurrentItem))
                    return;

                // IMPORTANT: Cancel vanilla shooting
                ev.IsAllowed = false;

                // Get the current item as a Firearm
                if (ev.Player.CurrentItem is not Firearm firearm)
                    return;

                // Check ammo
                int currentAmmo = Railgun.GetAmmoFromFirearm(firearm);
                if (currentAmmo <= 0)
                {
                    ev.Player.ShowHint("<color=#FF0000>Railgun: No charge!</color>", 2f);
                    return;
                }

                // Check if already charging
                if (_chargingPlayers.ContainsKey(ev.Player))
                {
                    ev.Player.ShowHint("<color=#FF9900>Railgun is already charging!</color>", 2f);
                    return;
                }

                // Set ammo to 0 to simulate using all ammo for the shot
                Railgun.SetAmmoInFirearm(firearm, 0);

                // Play firing sound to give feedback that action was registered
                try
                {
                    ev.Player.PlayGunSound(firearm.Type, 0, 0);
                }
                catch (Exception soundEx)
                {
                    Log.Error($"Railgun: Error playing gun sound: {soundEx.Message}");
                    // Continue even if sound fails
                }

                // Start charging
                StartChargingRailgun(ev.Player);

                // Log for debugging
                Log.Debug($"Railgun: Starting charging sequence for {ev.Player.Nickname}");
            }
            catch (Exception ex)
            {
                Log.Error($"Railgun: Error in OnShooting: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

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

        private void OnPlayerDying(DyingEventArgs ev)
        {
            if (ev.Player != null)
                CancelCharging(ev.Player);
        }

        private void OnItemDropped(DroppingItemEventArgs ev)
        {
            if (ev.Player != null && ev.Item != null && Check(ev.Item))
            {
                CancelCharging(ev.Player);
            }
        }
        #endregion

        #region Charging Mechanics
        private void StartChargingRailgun(Player player)
        {
            try
            {
                // Sanity checks
                if (player == null)
                {
                    Log.Error("Railgun: Cannot start charging - player is null");
                    return;
                }

                if (_config == null)
                {
                    Log.Error("Railgun: Cannot start charging - config is null");
                    return;
                }

                // Initial notification
                player.ShowHint("<color=#0066FF>Railgun charging initiated...</color>", 3f);

                // Make sure the player has an entry in the charging effects dictionary
                if (!_chargingEffects.ContainsKey(player))
                {
                    _chargingEffects[player] = new List<Primitive>();
                }

                // Start charging visual effect - track the handle to ensure it runs
                CoroutineHandle visualHandle = Timing.RunCoroutine(CreateChargingEffectCoroutine(player));

                // Log confirmation that visual effect coroutine has started - WITHOUT including the handle in the string
                Log.Debug($"Railgun: Visual effect coroutine started for {player.Nickname}");

                // Start charging coroutine
                CoroutineHandle handle = Timing.RunCoroutine(ChargeRailgunCoroutine(player));

                // Store the handle in the dictionary
                _chargingPlayers[player] = handle;

                // Log confirmation of charging start - WITHOUT including the handle in the string
                Log.Debug($"Railgun: Charging coroutine started for {player.Nickname}");
            }
            catch (Exception ex)
            {
                Log.Error($"Railgun: Error starting charge: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

        private IEnumerator<float> ChargeRailgunCoroutine(Player player)
        {
            // Early check to prevent null reference exceptions
            if (player == null || _config == null)
            {
                Log.Error("Railgun: Cannot run charging coroutine - player or config is null");
                yield break;
            }

            float chargeTime = _config.ChargeTime;
            float elapsedTime = 0f;
            float updateInterval = 0.5f;
            bool fired = false;

            Log.Debug($"Railgun: Charge sequence started for {player.Nickname} with charge time {chargeTime}s");

            // Charging loop
            while (elapsedTime < chargeTime)
            {
                bool shouldContinue = true;

                try
                {
                    // Check if player still exists and has railgun
                    if (player == null || !player.IsAlive || player.CurrentItem == null || !Check(player.CurrentItem))
                    {
                        Log.Debug($"Railgun: Player conditions not met, breaking charge loop");
                        shouldContinue = false;
                    }
                    else
                    {
                        // Show progress if enabled
                        if (_config.ShowChargeProgress)
                        {
                            float progress = elapsedTime / chargeTime * 100f;
                            player.ShowHint($"<color=#0066FF>Railgun charging: {progress:F0}%</color>", 1f);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Railgun: Error in charging coroutine loop: {ex.Message}");
                    shouldContinue = false;
                }

                if (!shouldContinue)
                    break;

                // Wait for interval - outside try block
                yield return Timing.WaitForSeconds(updateInterval);
                elapsedTime += updateInterval;
            }

            Log.Debug($"Railgun: Charge loop completed for {player?.Nickname ?? "null"}, attempting to fire");

            try
            {
                // Check if player still exists and has railgun before firing
                if (player != null && player.IsAlive && player.CurrentItem != null && Check(player.CurrentItem))
                {
                    // Final notification
                    player.ShowHint("<color=#00FFFF>Railgun FIRING!</color>", 2f);

                    // Clean up charging effects before firing
                    CleanupChargingEffect(player);

                    // Fire the railgun
                    FireRailgun(player);
                    fired = true;

                    Log.Debug($"Railgun: Successfully fired for {player.Nickname}");
                }
                else
                {
                    Log.Debug($"Railgun: Conditions not met for firing, player is alive: {player?.IsAlive ?? false}, has item: {player?.CurrentItem != null}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Railgun: Error in firing sequence: {ex.Message}");
            }
            finally
            {
                // Safe cleanup
                if (player != null && _chargingPlayers != null)
                {
                    _chargingPlayers.Remove(player);
                }

                // Clean up any remaining charging effects
                if (player != null)
                {
                    CleanupChargingEffect(player);
                }

                // If we didn't fire (interrupted), we might want to restore ammo
                if (!fired && player != null && player.IsAlive && player.CurrentItem != null && Check(player.CurrentItem) && player.CurrentItem is Firearm firearm)
                {
                    Railgun.SetAmmoInFirearm(firearm, 1);
                    player.ShowHint("<color=#FF9900>Railgun charging interrupted. Charge restored.</color>", 3f);
                    Log.Debug($"Railgun: Charge interrupted, ammo restored for {player.Nickname}");
                }
            }
        }

        private IEnumerator<float> CreateChargingEffectCoroutine(Player player)
        {
            // Early safety check
            if (player == null || _config == null || _chargingEffects == null || _spawnedPrimitives == null)
            {
                Log.Error("Railgun: Cannot create charging effect - required objects are null");
                yield break;
            }

            // Initialize a list to store the charging effect primitives for this player
            if (!_chargingEffects.ContainsKey(player))
            {
                _chargingEffects[player] = new List<Primitive>();
            }

            // Variables for the ring effect
            const int numRings = 3;
            const float baseRingRadius = 0.2f;
            const float ringThickness = 0.02f;
            const float ringSpacing = 0.08f;
            const float spinSpeed = 180f; // degrees per second
            const float updateInterval = 0.05f;

            // Store initial sizes for scaling effects
            float[] initialRadii = new float[numRings];
            for (int i = 0; i < numRings; i++)
            {
                initialRadii[i] = baseRingRadius + (i * ringSpacing);
            }

            Log.Debug($"Railgun: Creating visual effect for {player.Nickname}");

            // Create initial rings
            for (int i = 0; i < numRings; i++)
            {
                // Create a ring (using a thin torus approximated by a bunch of small spheres)
                const int segments = 8; // Number of segments in each ring
                for (int j = 0; j < segments; j++)
                {
                    try
                    {
                        float angle = j * (360f / segments);
                        float radius = baseRingRadius + (i * ringSpacing);

                        // Calculate initial position (will be updated in the animation loop)
                        float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
                        float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

                        // Create a sphere primitive for this segment
                        Primitive segment = Primitive.Create(PrimitiveType.Sphere);
                        if (segment == null)
                        {
                            Log.Error($"Railgun: Failed to create primitive segment");
                            continue;
                        }

                        // Set color based on ring index
                        Color color;
                        float alpha = 0.7f;

                        if (i == 0) // Inner ring (blue)
                            color = new Color(0f, 0.5f, 1f, alpha);
                        else if (i == 1) // Middle ring (cyan)
                            color = new Color(0f, 0.8f, 1f, alpha);
                        else // Outer ring (white-blue)
                            color = new Color(0.5f, 0.8f, 1f, alpha);

                        segment.Color = color;
                        segment.Scale = new Vector3(ringThickness, ringThickness, ringThickness);
                        segment.Flags = PrimitiveFlags.Visible;

                        // Add to tracking lists
                        _chargingEffects[player].Add(segment);
                        _spawnedPrimitives.Enqueue(segment);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error creating segment {j} for ring {i}: {ex.Message}");
                    }
                }
            }

            Log.Debug($"Railgun: Created {_chargingEffects[player].Count} visual elements for {player.Nickname}");

            // Animation loop
            float elapsedTime = 0f;
            float chargeTime = _config.ChargeTime;

            while (player != null && player.IsAlive && player.CurrentItem != null && Check(player.CurrentItem) &&
                   _chargingPlayers != null && _chargingPlayers.ContainsKey(player))
            {
                bool updateSuccessful = true;

                try
                {
                    if (_chargingEffects == null || !_chargingEffects.ContainsKey(player) || _chargingEffects[player] == null)
                    {
                        Log.Error("Railgun: Charging effects collection invalid during animation");
                        break;
                    }

                    // Calculate charge progress
                    float chargeProgress = Mathf.Clamp01(elapsedTime / chargeTime);

                    // Calculate pulse and expansion effects
                    float globalPulse = 1f + (0.1f * Mathf.Sin(elapsedTime * 4f));
                    float finalExpansion = 1f + (chargeProgress * 0.3f); // Rings expand slightly as charge increases

                    // Update the position of all ring segments
                    int segmentIndex = 0;
                    for (int i = 0; i < numRings; i++)
                    {
                        // Apply different effects to each ring
                        float individualPulse = 1f + (0.15f * Mathf.Sin((elapsedTime + i * 0.5f) * 3f));
                        float ringPulse = globalPulse * individualPulse;

                        // Calculate dynamic radius with pulse and expansion effects
                        float radius = initialRadii[i] * ringPulse * finalExpansion;

                        float rotationOffset = i * 30f; // Offset each ring's rotation
                        float direction = (i % 2 == 0) ? 1f : -1f; // Alternate ring rotation direction
                        float speedFactor = 1f + (chargeProgress * 1.5f); // Speed up rotation as charging completes

                        // Calculate muzzle position (forward from player's camera)
                        Vector3 muzzlePosition = player.CameraTransform.position +
                                                player.CameraTransform.forward * 1.0f;

                        const int segments = 8;
                        for (int j = 0; j < segments; j++)
                        {
                            if (segmentIndex >= _chargingEffects[player].Count)
                                continue;

                            Primitive segment = _chargingEffects[player][segmentIndex++];
                            if (segment == null)
                                continue;

                            // Calculate rotation angle with time - speed increases with charge
                            float currentAngle = j * (360f / segments) +
                                                (elapsedTime * spinSpeed * direction * speedFactor) +
                                                rotationOffset;

                            // Calculate position in local space
                            float x = Mathf.Sin(Mathf.Deg2Rad * currentAngle) * radius;
                            float y = Mathf.Cos(Mathf.Deg2Rad * currentAngle) * radius;

                            // Convert to world space based on player's orientation
                            Vector3 localOffset = new Vector3(x, y, 0);
                            Vector3 worldPosition = muzzlePosition +
                                                  (player.CameraTransform.right * localOffset.x) +
                                                  (player.CameraTransform.up * localOffset.y);

                            // Update primitive position
                            segment.Position = worldPosition;

                            // Dynamically adjust particle size based on charge
                            float sizeFactor = 1f + (chargeProgress * 0.5f); // Grow slightly with charge
                            segment.Scale = new Vector3(
                                ringThickness * sizeFactor,
                                ringThickness * sizeFactor,
                                ringThickness * sizeFactor
                            );

                            // Pulse the brightness based on charge progress
                            float pulseFactor = 1f + 0.5f * Mathf.Sin(elapsedTime * 5f);

                            // More intense glow as charging completes
                            float brightness = Mathf.Lerp(0.5f, 2.5f, chargeProgress) * pulseFactor;

                            // Get the base color for this segment (based on ring index)
                            Color baseColor = segment.Color;

                            // Final color gets increasingly bright and saturated as charge completes
                            segment.Color = new Color(
                                baseColor.r * brightness,
                                baseColor.g * brightness,
                                baseColor.b * brightness,
                                baseColor.a
                            );
                        }
                    }

                    // Final charge effect - add electrical arcs between rings at high charge levels
                    if (chargeProgress > 0.7f && _chargingEffects[player].Count >= 16)
                    {
                        // Random electrical arcs between particles
                        if (UnityEngine.Random.value < 0.2f) // 20% chance each frame
                        {
                            int randomIdx1 = UnityEngine.Random.Range(0, 8);
                            int randomIdx2 = UnityEngine.Random.Range(8, 16);

                            // Safely access particles
                            if (randomIdx1 < _chargingEffects[player].Count && randomIdx2 < _chargingEffects[player].Count)
                            {
                                var particle1 = _chargingEffects[player][randomIdx1];
                                var particle2 = _chargingEffects[player][randomIdx2];

                                if (particle1 != null && particle2 != null)
                                {
                                    // Store original colors to restore
                                    Color color1 = particle1.Color;
                                    Color color2 = particle2.Color;

                                    // Flash to bright white
                                    particle1.Color = new Color(1f, 1f, 1f, 0.8f);
                                    particle2.Color = new Color(1f, 1f, 1f, 0.8f);

                                    // Schedule restoration of original colors
                                    Timing.CallDelayed(0.05f, () => {
                                        try
                                        {
                                            if (particle1 != null) particle1.Color = color1;
                                            if (particle2 != null) particle2.Color = color2;
                                        }
                                        catch { /* Ignore errors during restoration */ }
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error updating charging effect: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    updateSuccessful = false;
                }

                if (!updateSuccessful)
                    break;

                // Update time and wait
                elapsedTime += updateInterval;
                yield return Timing.WaitForSeconds(updateInterval);
            }

            // Final cleanup
            if (player != null)
            {
                CleanupChargingEffect(player);
                Log.Debug($"Railgun: Visual effect completed for {player.Nickname}");
            }
        }

        private void CleanupChargingEffect(Player player)
        {
            try
            {
                if (player == null || _chargingEffects == null)
                {
                    Log.Error("Railgun: Cannot clean up charging effects - objects are null");
                    return;
                }

                if (_chargingEffects.TryGetValue(player, out var effects))
                {
                    Log.Debug($"Railgun: Cleaning up {effects?.Count ?? 0} visual effects for {player.Nickname}");

                    if (effects != null)
                    {
                        foreach (var primitive in effects)
                        {
                            try
                            {
                                primitive?.Destroy();
                            }
                            catch { /* Ignore errors during cleanup */ }
                        }

                        effects.Clear();
                    }

                    _chargingEffects.Remove(player);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error cleaning up charging effect: {ex.Message}");
            }
        }

        private void CancelCharging(Player player)
        {
            try
            {
                if (player == null || _chargingPlayers == null)
                {
                    Log.Error("Railgun: Cannot cancel charging - required objects are null");
                    return;
                }

                if (_chargingPlayers.TryGetValue(player, out CoroutineHandle handle))
                {
                    Log.Debug($"Railgun: Cancelling charging for {player.Nickname}");

                    Timing.KillCoroutines(handle);
                    _chargingPlayers.Remove(player);

                    // Clean up charging visual effects
                    CleanupChargingEffect(player);

                    // Optionally restore ammo if interrupted
                    if (player.IsAlive && player.CurrentItem != null && Check(player.CurrentItem) && player.CurrentItem is Firearm firearm)
                    {
                        Railgun.SetAmmoInFirearm(firearm, 1);
                        player.ShowHint("<color=#FF9900>Railgun charging interrupted. Charge restored.</color>", 3f);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Railgun: Error cancelling charge: {ex.Message}");
            }
        }
        #endregion

        #region Weapon Functionality
        private void FireRailgun(Player player)
        {
            try
            {
                if (player == null || _config == null)
                {
                    Log.Error("Railgun: Cannot fire - player or config is null");
                    return;
                }

                Log.Debug($"Railgun: Firing sequence initiated for {player.Nickname}");

                // Play a firing sound
                try
                {
                    if (player.CurrentItem is Firearm firearm)
                        player.PlayGunSound(firearm.Type, 1, 0);
                }
                catch (Exception soundEx)
                {
                    Log.Error($"Railgun: Error playing firing sound: {soundEx.Message}");
                    // Continue even if sound fails
                }

                // Give player feedback that the railgun is about to fire
                player.ShowHint("<color=#00FFFF>Railgun capacitors discharging...</color>", 1.0f);

                // Start a coroutine to delay the actual firing effect
                Timing.RunCoroutine(DelayedFireEffect(player));
            }
            catch (Exception ex)
            {
                Log.Error($"Railgun: Error in FireRailgun: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

        private IEnumerator<float> DelayedFireEffect(Player player)
        {
            // Wait a short time before firing (simulating capacitor discharge)
            yield return Timing.WaitForSeconds(0.75f);

            try
            {
                // Make sure player is still valid
                if (player == null || !player.IsAlive || player.CurrentItem == null || !Check(player.CurrentItem))
                {
                    Log.Debug("Railgun: Player no longer valid during delayed fire");
                    yield break;
                }

                // Create the beam effect and calculate hit point
                bool hitSomething = Physics.Raycast(player.CameraTransform.position, player.CameraTransform.forward, out RaycastHit hit, _config.Range);

                // Log the raycast result
                Log.Debug($"Railgun: Raycast hit something: {hitSomething}, range: {_config.Range}m");

                // Determine end position based on hit
                Vector3 impactPoint = hitSomething
                    ? hit.point
                    : player.CameraTransform.position + player.CameraTransform.forward * _config.Range;

                // Give the player a recoil effect
                player.ShowHint("<color=#00DDFF>*RECOIL*</color>", 0.3f);

                // Create a beam effect
                SpawnBeam(player.CameraTransform.position, impactPoint);
                Log.Debug($"Railgun: Beam spawned from {player.CameraTransform.position} to {impactPoint}");

                // Find and damage players in the beam's path
                foreach (Player target in Player.List)
                {
                    if (target == null || !target.IsAlive || target == player)
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
                        Log.Debug($"Railgun: Hit player {target.Nickname} for {_config.Damage} damage");
                    }
                }

                // Create explosion effect at impact point
                if (SpawnExplosive && hitSomething)
                {
                    try
                    {
                        ExplosiveGrenade grenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
                        if (grenade != null)
                        {
                            grenade.FuseTime = 0.01f; // Near-instant detonation
                            grenade.SpawnActive(impactPoint);
                            Log.Debug($"Railgun: Explosion spawned at impact point {impactPoint}");
                        }
                    }
                    catch (Exception grenadeEx)
                    {
                        Log.Error($"Railgun: Error spawning explosion: {grenadeEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Railgun: Error in delayed fire effect: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

        private static int GetAmmoFromFirearm(Firearm firearm)
        {
            try
            {
                if (firearm == null)
                {
                    Log.Error("Railgun: Cannot get ammo - firearm is null");
                    return 0;
                }

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

        private static void SetAmmoInFirearm(Firearm firearm, int value)
        {
            try
            {
                if (firearm == null)
                {
                    Log.Error("Railgun: Cannot set ammo - firearm is null");
                    return;
                }

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

        private void SpawnBeam(Vector3 start, Vector3 end)
        {
            try
            {
                if (_spawnedPrimitives == null || _random == null)
                {
                    Log.Error("Railgun: Cannot spawn beam - required objects are null");
                    return;
                }

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
                if (mainBeam == null)
                {
                    Log.Error("Railgun: Failed to create main beam primitive");
                    return;
                }

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
                if (outerBeam == null)
                {
                    Log.Error("Railgun: Failed to create outer beam primitive");
                    return;
                }

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

                Log.Debug($"Railgun: Beam effect created with length {distance}m");
            }
            catch (Exception ex)
            {
                Log.Error($"Error creating beam effect: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

        private IEnumerator<float> AnimateBeam(Primitive mainBeam, Primitive outerBeam, float duration, float baseHue)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                try
                {
                    float progress = elapsedTime / duration;

                    // Fade out the beams
                    if (mainBeam != null)
                    {
                        Color mainColor = mainBeam.Color;
                        mainColor.a = Mathf.Lerp(0.9f, 0f, progress);
                        mainBeam.Color = mainColor;
                    }

                    if (outerBeam != null)
                    {
                        Color outerColor = outerBeam.Color;
                        outerColor.a = Mathf.Lerp(0.5f, 0f, progress);
                        outerBeam.Color = outerColor;
                    }

                    // Add a flicker effect
                    if (UnityEngine.Random.value < 0.1f)
                    {
                        float flickerHue = baseHue + UnityEngine.Random.Range(-0.1f, 0.1f);
                        if (mainBeam != null)
                            mainBeam.Color = Color.HSVToRGB(flickerHue, 1f, 1f);
                        if (outerBeam != null)
                            outerBeam.Color = Color.HSVToRGB(flickerHue, 0.8f, 0.8f);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error animating beam: {ex.Message}");
                }

                elapsedTime += Time.deltaTime;
                yield return Timing.WaitForOneFrame;
            }

            // Cleanup
            try
            {
                mainBeam?.Destroy();
                outerBeam?.Destroy();
            }
            catch (Exception ex)
            {
                Log.Error($"Error in CleanupBeams: {ex.Message}");
            }
        }
        #endregion
    }
}
