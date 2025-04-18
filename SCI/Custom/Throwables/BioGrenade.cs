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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;

namespace SCI.Custom.Throwables
{
    public class BioGrenade(BioGrenadeConfig config) : CustomGrenade
    {
        #region Configuration
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
                new() { Chance = 15, Location = SpawnLocationType.InsideLczCafe },
                new() { Chance = 15, Location = SpawnLocationType.InsideLczWc },
                new() { Chance = 15, Location = SpawnLocationType.Inside914 },
                new() { Chance = 15, Location = SpawnLocationType.InsideGr18Glass },
                new() { Chance = 15, Location = SpawnLocationType.Inside096 },
            ],
        };

        private readonly BioGrenadeConfig _config = config;
        private Pickup _smokeEffect = null;
        private CoroutineHandle _effectCoroutine;
        private readonly Dictionary<Player, float> _exposedPlayers = [];
        #endregion

        #region Event Management
        protected override void OnExploding(ExplodingGrenadeEventArgs ev)
        {
            // Prevent the default explosion
            ev.IsAllowed = false;
            Vector3 savedGrenadePosition = ev.Position;

            // Create the smoke effect using SCP-244
            Scp244 scp244 = (Scp244)Item.Create(ItemType.SCP244a);

            // Configure smoke appearance
            scp244.Scale = new Vector3(_config.SmokeScale, _config.SmokeScale, _config.SmokeScale);
            scp244.Primed = true;
            scp244.MaxDiameter = _config.SmokeDiameter;

            // Create the pickup at the grenade position
            _smokeEffect = scp244.CreatePickup(savedGrenadePosition);

            // Reset exposed players dictionary
            _exposedPlayers.Clear();

            // Start tracking player exposure to the smoke
            _effectCoroutine = Timing.RunCoroutine(TrackPlayerExposureCoroutine(savedGrenadePosition));

            // Remove the smoke after the configured time
            if (_config.RemoveSmoke)
            {
                Timing.CallDelayed(_config.SmokeTime, () =>
                {
                    if (_smokeEffect != null)
                    {
                        _smokeEffect.Position += Vector3.down * 10;

                        // Stop the exposure tracking coroutine
                        Timing.KillCoroutines(_effectCoroutine);
                        _exposedPlayers.Clear();

                        Timing.CallDelayed(10, () =>
                        {
                            _smokeEffect.Destroy();
                            _smokeEffect = null;
                        });
                    }
                });
            }
        }

        private IEnumerator<float> TrackPlayerExposureCoroutine(Vector3 position)
        {
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
                        if (_exposedPlayers.TryGetValue(player, out float value))
                        {
                            _exposedPlayers[player] = value + 1f; // Add 1 second of exposure

                            // If player has been exposed for 10 seconds
                            if (_exposedPlayers[player] >= 10f)
                            {
                                // Apply the Decontaminating effect
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
                        }
                    }
                    else if (_exposedPlayers.ContainsKey(player))
                    {
                        // Player has left the smoke area, reset their exposure
                        _exposedPlayers.Remove(player);
                    }
                }
            }
        }
        #endregion
    }
}
