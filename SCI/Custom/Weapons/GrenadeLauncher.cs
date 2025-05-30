using System;
using System.Collections.Generic;
using System.Reflection;
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

        // Dictionary to track remaining shots for each weapon instance (by serial number)
        private readonly Dictionary<ushort, int> _remainingShots = new Dictionary<ushort, int>();

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
        public override void Init()
        {
            base.Init();
            Log.Debug($"GrenadeLauncher: Initialized with MaxShots={MaxShots}, ClipSize={ClipSize}");
        }

        protected override void SubscribeEvents()
        {
            // Normal event handlers
            Exiled.Events.Handlers.Player.Shooting += OnShooting;
            Exiled.Events.Handlers.Player.DroppingItem += OnDroppingItem;
            Exiled.Events.Handlers.Player.Dying += OnPlayerDying;
            Exiled.Events.Handlers.Player.PickingUpItem += OnPickingUpItem;
            
            // Item added event for tracking when player gets a weapon
            Exiled.Events.Handlers.Player.ItemAdded += OnItemAdded;
            
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shooting -= OnShooting;
            Exiled.Events.Handlers.Player.DroppingItem -= OnDroppingItem;
            Exiled.Events.Handlers.Player.Dying -= OnPlayerDying;
            Exiled.Events.Handlers.Player.PickingUpItem -= OnPickingUpItem;
            Exiled.Events.Handlers.Player.ItemAdded -= OnItemAdded;
            base.UnsubscribeEvents();
        }
        #endregion

        #region Event Handlers
        private void OnItemAdded(ItemAddedEventArgs ev)
        {
            try
            {
                // Check if the item is our grenade launcher
                if (ev.Item is Firearm firearm && Check(firearm))
                {
                    // Get the serial number to track this specific weapon
                    ushort serial = firearm.Serial;
                    
                    // Reset or initialize ammunition count for this weapon instance
                    if (!_remainingShots.ContainsKey(serial) || _remainingShots[serial] <= 0)
                    {
                        _remainingShots[serial] = MaxShots;
                        Log.Debug($"GrenadeLauncher: New weapon with serial {serial} initialized with {MaxShots} shots");
                    }
                    
                    // Set the ammo directly on the firearm
                    SetAmmoInFirearm(firearm, _remainingShots[serial]);
                    
                    // Show a hint to the player
                    ev.Player.ShowHint($"<color=#FFA500>Grenade Launcher: {_remainingShots[serial]} shots loaded</color>", 3f);
                    
                    Log.Debug($"GrenadeLauncher: Item added to {ev.Player.Nickname}, serial {serial}, ammo {_remainingShots[serial]}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"GrenadeLauncher: Error in OnItemAdded: {ex.Message}");
            }
        }
        
        protected override void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (Check(ev.Item) && ev.Item is Firearm firearm)
            {
                ushort serial = firearm.Serial;
                
                if (_remainingShots.ContainsKey(serial))
                {
                    Log.Debug($"GrenadeLauncher: {ev.Player.Nickname} dropped launcher serial {serial} with {_remainingShots[serial]} shots left");
                }
            }
            
            base.OnDroppingItem(ev);
        }

        private void OnPlayerDying(DyingEventArgs ev)
        {
            // We don't remove weapons from tracking on death anymore
            // Instead, we track by serial number so the same weapon can be picked up later with its remaining ammo
        }

        private void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (Check(ev.Pickup))
            {
                // Give player feedback after a small delay to ensure the item is actually picked up
                Timing.CallDelayed(0.5f, () => 
                {
                    if (ev.Player.IsAlive && ev.Player.CurrentItem != null && Check(ev.Player.CurrentItem))
                    {
                        // Directly set ammo on the firearm
                        if (ev.Player.CurrentItem is Firearm firearm)
                        {
                            ushort serial = firearm.Serial;
                            
                            // If this is a new weapon or it's empty, initialize it
                            if (!_remainingShots.ContainsKey(serial) || _remainingShots[serial] <= 0)
                            {
                                _remainingShots[serial] = MaxShots;
                                Log.Debug($"GrenadeLauncher: Picked up new weapon with serial {serial}, setting ammo to {MaxShots}");
                            }
                            
                            // Set the ammo in the firearm to match our tracking
                            SetAmmoInFirearm(firearm, _remainingShots[serial]);
                            
                            // Show the ammo count to the player
                            ev.Player.ShowHint($"<color=#FFA500>Grenade Launcher: {_remainingShots[serial]} shots loaded</color>", 3f);
                        }
                    }
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

                // Get the current item as a Firearm for later use
                Firearm firearm = null;
                ushort serial = 0;
                
                if (ev.Player.CurrentItem is Firearm gun)
                {
                    firearm = gun;
                    serial = firearm.Serial;
                    
                    // Debug log the current ammo in the firearm
                    Log.Debug($"GrenadeLauncher: Serial {serial} - Current ammo in firearm for {ev.Player.Nickname}: {GetAmmoFromFirearm(firearm)}");
                }
                else
                {
                    return; // Not a firearm, can't proceed
                }

                // Check if weapon has shots remaining, initialize if not in dictionary
                if (!_remainingShots.TryGetValue(serial, out int shots))
                {
                    // If not tracked, initialize with max shots
                    _remainingShots[serial] = MaxShots;
                    shots = MaxShots;
                    
                    // Also set the ammo in the firearm
                    SetAmmoInFirearm(firearm, MaxShots);
                    
                    Log.Debug($"GrenadeLauncher: Initializing serial {serial} to {MaxShots} shots");
                }

                // Check if we have shots left
                if (shots <= 0)
                {
                    ev.Player.ShowHint("<color=#FF0000>Grenade Launcher: No ammunition remaining!</color>", 2f);
                    Log.Debug($"GrenadeLauncher: {ev.Player.Nickname} attempted to fire serial {serial} with no ammo");
                    
                    // Remove the weapon if it's empty
                    Timing.CallDelayed(0.5f, () => 
                    {
                        if (ev.Player.IsAlive && Check(ev.Player.CurrentItem))
                        {
                            ev.Player.RemoveItem(ev.Player.CurrentItem);
                            ev.Player.ShowHint("<color=#FF0000>Grenade Launcher has been discarded.</color>", 3f);
                            Log.Debug($"GrenadeLauncher: Removed empty weapon serial {serial} from {ev.Player.Nickname}");
                        }
                    });
                    
                    return;
                }

                // Decrement shots by ONE and update tracking
                _remainingShots[serial] = shots - 1;
                
                // Update the firearm's ammo count to match our tracking
                SetAmmoInFirearm(firearm, _remainingShots[serial]);
                
                Log.Debug($"GrenadeLauncher: {ev.Player.Nickname} fired a shot from serial {serial}, {_remainingShots[serial]} remaining");
                ev.Player.ShowHint($"<color=#FFA500>Grenade Launcher: {_remainingShots[serial]} shots remaining</color>", 2f);

                // Spawn position calculation - moved to a separate method for clarity
                FireGrenade(ev.Player);

                // Check if this was the last shot
                if (_remainingShots[serial] <= 0)
                {
                    Timing.CallDelayed(0.5f, () =>
                    {
                        if (ev.Player.IsAlive && Check(ev.Player.CurrentItem))
                        {
                            ev.Player.RemoveItem(ev.Player.CurrentItem);
                            ev.Player.ShowHint("<color=#FF0000>Grenade Launcher is empty and has been discarded.</color>", 3f);
                            Log.Debug($"GrenadeLauncher: {ev.Player.Nickname} used last shot from serial {serial}, removing weapon");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error($"GrenadeLauncher: Error in OnShooting: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // This method handles the actual firing of a grenade to separate concerns
        private void FireGrenade(Player player)
        {
            // Spawn position calculation
            Vector3 spawnPos = player.CameraTransform.position + player.CameraTransform.forward * SpawnDistance;

            // Create and spawn the grenade - only spawning ONE grenade
            Pickup grenade = Pickup.CreateAndSpawn(ItemType.GrenadeHE, spawnPos, Quaternion.identity);
            Log.Debug($"GrenadeLauncher: Single grenade created for {player.Nickname}");

            // Apply physics to the grenade
            if (grenade.Rigidbody is Rigidbody rb)
            {
                rb.isKinematic = false;
                rb.useGravity = true;

                // Calculate launch direction with upward arc
                Vector3 launchDirection = player.CameraTransform.forward + Vector3.up * _config.UpwardArc;
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

        private static int GetAmmoFromFirearm(Firearm firearm)
        {
            try
            {
                if (firearm == null)
                {
                    Log.Error("GrenadeLauncher: Cannot get ammo - firearm is null");
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

                return 0; // Default to 0 if we can't determine
            }
            catch (Exception ex)
            {
                Log.Error($"GrenadeLauncher: Error getting ammo: {ex.Message}");
                return 0;
            }
        }
        
        private static void SetAmmoInFirearm(Firearm firearm, int value)
        {
            try
            {
                if (firearm == null)
                {
                    Log.Error("GrenadeLauncher: Cannot set ammo - firearm is null");
                    return;
                }

                // Try different approaches to set ammo
                var ammoProperty = firearm.GetType().GetProperty("Ammo");
                if (ammoProperty != null && ammoProperty.CanWrite)
                {
                    ammoProperty.SetValue(firearm, value);
                    Log.Debug($"GrenadeLauncher: Set ammo via Ammo property to {value}");
                    return;
                }

                var currentAmmoProperty = firearm.GetType().GetProperty("CurrentAmmo");
                if (currentAmmoProperty != null && currentAmmoProperty.CanWrite)
                {
                    currentAmmoProperty.SetValue(firearm, value);
                    Log.Debug($"GrenadeLauncher: Set ammo via CurrentAmmo property to {value}");
                    return;
                }

                var ammoField = firearm.GetType().GetField("Ammunitions");
                if (ammoField != null)
                {
                    ammoField.SetValue(firearm, value);
                    Log.Debug($"GrenadeLauncher: Set ammo via Ammunitions field to {value}");
                    return;
                }

                var ammoProperty2 = firearm.GetType().GetProperty("Ammunitions");
                if (ammoProperty2 != null && ammoProperty2.CanWrite)
                {
                    ammoProperty2.SetValue(firearm, value);
                    Log.Debug($"GrenadeLauncher: Set ammo via Ammunitions property to {value}");
                    return;
                }

                // Try base object
                if (firearm.Base != null)
                {
                    var baseAmmoProperty = firearm.Base.GetType().GetProperty("Ammo");
                    if (baseAmmoProperty != null && baseAmmoProperty.CanWrite)
                    {
                        baseAmmoProperty.SetValue(firearm.Base, value);
                        Log.Debug($"GrenadeLauncher: Set ammo via Base.Ammo property to {value}");
                        return;
                    }

                    baseAmmoProperty = firearm.Base.GetType().GetProperty("CurrentAmmo");
                    if (baseAmmoProperty != null && baseAmmoProperty.CanWrite)
                    {
                        baseAmmoProperty.SetValue(firearm.Base, value);
                        Log.Debug($"GrenadeLauncher: Set ammo via Base.CurrentAmmo property to {value}");
                        return;
                    }

                    baseAmmoProperty = firearm.Base.GetType().GetProperty("Ammunitions");
                    if (baseAmmoProperty != null && baseAmmoProperty.CanWrite)
                    {
                        baseAmmoProperty.SetValue(firearm.Base, value);
                        Log.Debug($"GrenadeLauncher: Set ammo via Base.Ammunitions property to {value}");
                        return;
                    }
                }

                // If nothing worked, try setting directly in the Base object using reflection
                if (firearm.Base != null)
                {
                    var props = firearm.Base.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var prop in props)
                    {
                        if (prop.PropertyType == typeof(int) && prop.CanWrite && 
                            (prop.Name.Contains("Ammo") || prop.Name.Contains("ammo") || 
                             prop.Name.Contains("Clip") || prop.Name.Contains("clip")))
                        {
                            try
                            {
                                prop.SetValue(firearm.Base, value);
                                Log.Debug($"GrenadeLauncher: Set ammo via Base.{prop.Name} property to {value}");
                            }
                            catch
                            {
                                // Ignore reflection errors, try next property
                            }
                        }
                    }
                }
                
                Log.Debug($"GrenadeLauncher: Failed to set ammo to {value} - no usable property found");
            }
            catch (Exception ex)
            {
                Log.Error($"GrenadeLauncher: Error setting ammo: {ex.Message}");
            }
        }
        #endregion
    }
}
