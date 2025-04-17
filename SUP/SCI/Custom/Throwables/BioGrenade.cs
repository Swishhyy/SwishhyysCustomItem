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
        private CoroutineHandle _healingCoroutine;

        [YamlIgnore]
        public override ItemType Type { get; set; } = ItemType.GrenadeFlash;
        public override uint Id { get; set; } = 107;
        public override string Name { get; set; } = "<color=#00FF00>Bio Grenade</color>";
        public override string Description { get; set; } = "When this grenade explodes, it emits a smoke cloud that grants decontamination effects";
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

            // Apply initial decontamination effect after delay
            Timing.CallDelayed(_config.DecontaminationDelay, () =>
            {
                ApplyDecontaminationEffect(savedGrenadePosition);
            });

            // Start continuous healing if enabled
            if (_config.ContinuousHealing)
            {
                Plugin.Instance?.DebugLog($"BioGrenade: Starting continuous healing every {_config.HealingInterval} seconds");
                _healingCoroutine = Timing.RunCoroutine(ContinuousHealingCoroutine(savedGrenadePosition));
            }

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

                        // Also stop the healing coroutine if it's running
                        if (_config.ContinuousHealing)
                        {
                            Plugin.Instance?.DebugLog("BioGrenade: Stopping continuous healing");
                            Timing.KillCoroutines(_healingCoroutine);
                        }

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

        private void ApplyDecontaminationEffect(Vector3 position)
        {
            Plugin.Instance?.DebugLog($"BioGrenade: Applying decontamination effect to players within {_config.EffectRadius}m");

            // Get all players within the effect radius
            foreach (Player player in Player.List.Where(p =>
                p.IsAlive &&
                Vector3.Distance(p.Position, position) <= _config.EffectRadius))
            {
                Plugin.Instance?.DebugLog($"BioGrenade: Applying decontamination to {player.Nickname}");

                try
                {
                    // Apply AHP
                    float previousAhp = player.ArtificialHealth;
                    player.ArtificialHealth += _config.AhpAmount;

                    // Apply healing
                    float previousHealth = player.Health;
                    player.Health += _config.HealAmount;

                    Plugin.Instance?.DebugLog($"BioGrenade: Applied effects to {player.Nickname}, AHP: {previousAhp} -> {player.ArtificialHealth}, Health: {previousHealth} -> {player.Health}");

                    // Show a hint to the player
                    player.ShowHint("<color=green>You feel the decontamination chemicals cleansing your body!</color>", 5f);
                }
                catch (Exception ex)
                {
                    Plugin.Instance?.DebugLog($"BioGrenade: Error applying effect to {player.Nickname}: {ex.Message}");
                    Log.Error($"BioGrenade: Error applying effect: {ex.Message}");
                }
            }
        }

        private IEnumerator<float> ContinuousHealingCoroutine(Vector3 position)
        {
            // Continue healing pulses for the duration of the smoke
            float elapsed = 0f;
            while (elapsed < _config.SmokeTime && _smokeEffect != null)
            {
                // Wait for the next interval
                yield return Timing.WaitForSeconds(_config.HealingInterval);
                elapsed += _config.HealingInterval;

                // Apply healing to players in range
                ApplyDecontaminationEffect(position);

                Plugin.Instance?.DebugLog($"BioGrenade: Continuous healing pulse applied at {elapsed} seconds");
            }

            Plugin.Instance?.DebugLog("BioGrenade: Continuous healing coroutine ended");
        }
    }
}
