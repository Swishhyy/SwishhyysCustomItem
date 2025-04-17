using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using JetBrains.Annotations;
using MEC;
using SCI.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;

namespace SCI.Custom.Throwables
{
    public class BioGrenade(BioGrenadeConfig config) : CustomGrenade
    {
        private readonly BioGrenadeConfig _config = config;
        private Pickup _smokeEffect = null;
        private CoroutineHandle _effectCoroutine;
        // Dictionary to track player exposure time
        private readonly Dictionary<Player, float> _exposedPlayers = [];

        [YamlIgnore]
        public override ItemType Type { get; set; } = ItemType.GrenadeFlash;
        public override uint Id { get; set; } = 107;
        public override string Name { get; set; } = "<color=#00FF00>Bio Grenade</color>";
        public override string Description { get; set; } = "When this grenade explodes, it emits a toxic cloud that applies decontamination effects";
        public override float Weight { get; set; } = 1.75f;
        public override bool ExplodeOnCollision { get; set; } = false;
        public override float FuseTime { get; set; } = 3f;

        [CanBeNull]
        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties
        {
            Limit = 2,
            DynamicSpawnPoints =
            [
                new ()
                {
                    Chance = 15,
                    Location = SpawnLocationType.InsideLczCafe,
                },
                new ()
                {
                    Chance = 15,
                    Location = SpawnLocationType.InsideLczWc,
                },
                new()
                {
                    Chance = 15,
                    Location = SpawnLocationType.Inside914,
                },
                new()
                {
                    Chance = 15,
                    Location = SpawnLocationType.InsideGr18Glass,
                },
                new()
                {
                    Chance = 15,
                    Location = SpawnLocationType.Inside096,
                },
            ],
        };

        public override void Init()
        {
            base.Init();
            Plugin.Instance?.DebugLog($"BioGrenade initialized with smoke time: {_config.SmokeTime}");
        }

        protected override void OnExploding(ExplodingGrenadeEventArgs ev)
        {
            Plugin.Instance?.DebugLog($"BioGrenade.OnExploding called at position {ev.Position}");

            // Prevent the default explosion
            ev.IsAllowed = false;
            Vector3 savedGrenadePosition = ev.Position;

            // Create the smoke effect using SCP-244
            Plugin.Instance?.DebugLog("BioGrenade: Creating smoke effect");
            Scp244 scp244 = (Scp244)Item.Create(ItemType.SCP244a);

            // Configure smoke appearance
            scp244.Scale = new Vector3(_config.SmokeScale, _config.SmokeScale, _config.SmokeScale);
            scp244.Primed = true;
            scp244.MaxDiameter = _config.SmokeDiameter;

            // Create the pickup at the grenade position
            Plugin.Instance?.DebugLog($"BioGrenade: Creating smoke pickup at {savedGrenadePosition}");
            _smokeEffect = scp244.CreatePickup(savedGrenadePosition);

            // Reset exposed players dictionary
            _exposedPlayers.Clear();

            // Start tracking player exposure to the smoke
            _effectCoroutine = Timing.RunCoroutine(TrackPlayerExposureCoroutine(savedGrenadePosition));

            // Remove the smoke after the configured time
            if (_config.RemoveSmoke)
            {
                Plugin.Instance?.DebugLog($"BioGrenade: Scheduled smoke removal in {_config.SmokeTime} seconds");
                Timing.CallDelayed(_config.SmokeTime, () =>
                {
                    if (_smokeEffect != null)
                    {
                        Plugin.Instance?.DebugLog("BioGrenade: Removing smoke by moving it down");
                        _smokeEffect.Position += Vector3.down * 10;

                        // Stop the exposure tracking coroutine
                        Plugin.Instance?.DebugLog("BioGrenade: Stopping effect tracking");
                        Timing.KillCoroutines(_effectCoroutine);
                        _exposedPlayers.Clear();

                        Timing.CallDelayed(10, () =>
                        {
                            Plugin.Instance?.DebugLog("BioGrenade: Destroying smoke pickup");
                            _smokeEffect.Destroy();
                            _smokeEffect = null;
                        });
                    }
                });
            }

            Plugin.Instance?.DebugLog("BioGrenade.OnExploding completed successfully");
        }

        private IEnumerator<float> TrackPlayerExposureCoroutine(Vector3 position)
        {
            Plugin.Instance?.DebugLog("BioGrenade: Starting player exposure tracking");

            // Continue tracking for the duration of the smoke
            float elapsed = 0f;
            while (elapsed < _config.SmokeTime && _smokeEffect != null)
            {
                // Wait for a short interval (checking every second)
                yield return Timing.WaitForSeconds(1f);
                elapsed += 1f;

                // Check all alive players
                foreach (Player player in Player.List.Where(p => p.IsAlive))
                {
                    float distance = Vector3.Distance(player.Position, position);

                    // If player is within effect radius
                    if (distance <= _config.EffectRadius)
                    {
                        // Add or update exposure time for this player
                        if (_exposedPlayers.ContainsKey(player))
                        {
                            _exposedPlayers[player] += 1f; // Add 1 second of exposure

                            // If player has been exposed for 5 seconds
                            if (_exposedPlayers[player] >= 10f)
                            {
                                // Apply the Decontaminating effect
                                Plugin.Instance?.DebugLog($"BioGrenade: Applying Decontaminating effect to {player.Nickname} after 5 seconds exposure");

                                // Apply the effect using the correct method
                                player.EnableEffect(EffectType.Corroding, _config.DecontaminationDuration);

                                // Show a hint to the player
                                player.ShowHint("<color=red>You've been exposed to decontamination chemicals!</color>", 5f);

                                // Remove from tracking to avoid multiple effect applications
                                _exposedPlayers.Remove(player);
                            }
                        }
                        else
                        {
                            // First time player is detected in the smoke
                            _exposedPlayers.Add(player, 1f);
                            Plugin.Instance?.DebugLog($"BioGrenade: Player {player.Nickname} entered smoke cloud");
                        }
                    }
                    else if (_exposedPlayers.ContainsKey(player))
                    {
                        // Player has left the smoke area, reset their exposure
                        Plugin.Instance?.DebugLog($"BioGrenade: Player {player.Nickname} left smoke cloud, resetting exposure time");
                        _exposedPlayers.Remove(player);
                    }
                }
            }

            Plugin.Instance?.DebugLog("BioGrenade: Player exposure tracking ended");
        }
    }
}
