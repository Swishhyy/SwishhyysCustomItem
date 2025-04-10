using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using JetBrains.Annotations;
using MEC;
using UnityEngine;
using YamlDotNet.Serialization;
using Log = Exiled.API.Features.Log;

namespace SCI.Custom.Throwables
{
    [CustomItem(ItemType.GrenadeFlash)]
    public class SmokeGrenade : CustomGrenade
    {
        private readonly SmokeGrenadeConfig _config;

        public SmokeGrenade(SmokeGrenadeConfig config)
        {
            _config = config;
        }

        [YamlIgnore]
        public override ItemType Type { get; set; } = ItemType.GrenadeFlash;
        public override uint Id { get; set; } = 106;
        public override string Name { get; set; } = "<color=#6600CC>Smoke Grenade</color>";
        public override string Description { get; set; } = "This is a smoke grenade, when detonated, a smoke cloud will be deployed";
        public override float Weight { get; set; } = 1.15f;

        public override bool ExplodeOnCollision { get; set; } = false;
        public override float FuseTime { get; set; } = 3f;

        [CanBeNull]
        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties
        {
            Limit = 5,
            DynamicSpawnPoints = new List<DynamicSpawnPoint>
            {
                new DynamicSpawnPoint
                {
                    Chance = 25,
                    Location = SpawnLocationType.InsideHczArmory,
                },
                new DynamicSpawnPoint
                {
                    Chance = 25,
                    Location = SpawnLocationType.InsideGr18,
                },
                new DynamicSpawnPoint
                {
                    Chance = 25,
                    Location = SpawnLocationType.InsideSurfaceNuke,
                },
                new DynamicSpawnPoint
                {
                    Chance = 25,
                    Location = SpawnLocationType.InsideLczArmory,
                },
            },
        };

        public override void Init()
        {
            base.Init();
            if (_config.EnableDebugLogging)
                Plugin.Instance?.DebugLog($"SmokeGrenade initialized with smoke time: {_config.SmokeTime}");
        }

        protected override void OnExploding(ExplodingGrenadeEventArgs ev)
        {
            if (_config.EnableDebugLogging)
                Plugin.Instance?.DebugLog($"SmokeGrenade.OnExploding called at position {ev.Position}");

            ev.IsAllowed = false;
            Vector3 savedGrenadePosition = ev.Position;

            if (_config.EnableDebugLogging)
                Plugin.Instance?.DebugLog("SmokeGrenade: Creating SCP-244 smoke effect");

            Scp244 scp244 = (Scp244)Item.Create(ItemType.SCP244a);
            Pickup pickup = null;

            // Configure appearance and attributes
            scp244.Scale = new Vector3(_config.SmokeScale, _config.SmokeScale, _config.SmokeScale);
            scp244.Primed = true;
            scp244.MaxDiameter = _config.SmokeDiameter;

            if (_config.EnableDebugLogging)
                Plugin.Instance?.DebugLog($"SmokeGrenade: Creating pickup at {savedGrenadePosition}");

            pickup = scp244.CreatePickup(savedGrenadePosition);

            if (_config.RemoveSmoke)
            {
                if (_config.EnableDebugLogging)
                    Plugin.Instance?.DebugLog($"SmokeGrenade: Scheduled smoke removal in {_config.SmokeTime} seconds");

                Timing.CallDelayed(_config.SmokeTime, () =>
                {
                    if (_config.EnableDebugLogging)
                        Plugin.Instance?.DebugLog("SmokeGrenade: Removing smoke by moving it down");

                    pickup.Position += Vector3.down * 10;

                    Timing.CallDelayed(10, () =>
                    {
                        if (_config.EnableDebugLogging)
                            Plugin.Instance?.DebugLog("SmokeGrenade: Destroying smoke pickup");

                        pickup.Destroy();
                    });
                });
            }

            if (_config.EnableDebugLogging)
                Plugin.Instance?.DebugLog("SmokeGrenade.OnExploding completed successfully");
        }
    }
}