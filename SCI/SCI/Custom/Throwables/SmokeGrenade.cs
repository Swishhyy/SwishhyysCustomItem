using System.Collections.Generic;
using Exiled.API.Enums;
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
using SCI.Config;

namespace SCI.Custom.Throwables
{
    [CustomItem(ItemType.GrenadeFlash)]
    public class SmokeGrenade(SmokeGrenadeConfig config) : CustomGrenade
    {
        private readonly SmokeGrenadeConfig _config = config;

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
            DynamicSpawnPoints =
            [
                new()
                {
                    Chance = 25,
                    Location = SpawnLocationType.InsideHczArmory,
                },
                new()
                {
                    Chance = 25,
                    Location = SpawnLocationType.InsideGr18,
                },
                new()
                {
                    Chance = 25,
                    Location = SpawnLocationType.InsideSurfaceNuke,
                },
                new()
                {
                    Chance = 25,
                    Location = SpawnLocationType.InsideLczArmory,
                },
            ],
        };

        public override void Init()
        {
            base.Init();
            Plugin.Instance?.DebugLog($"SmokeGrenade initialized with smoke time: {_config.SmokeTime}");
        }

        protected override void OnExploding(ExplodingGrenadeEventArgs ev)
        {
            Plugin.Instance?.DebugLog($"SmokeGrenade.OnExploding called at position {ev.Position}");

            ev.IsAllowed = false;
            Vector3 savedGrenadePosition = ev.Position;

            Plugin.Instance?.DebugLog("SmokeGrenade: Creating SCP-244 smoke effect");

            Scp244 scp244 = (Scp244)Item.Create(ItemType.SCP244a);
            Pickup pickup = null;

            // Configure appearance and attributes
            scp244.Scale = new Vector3(_config.SmokeScale, _config.SmokeScale, _config.SmokeScale);
            scp244.Primed = true;
            scp244.MaxDiameter = _config.SmokeDiameter;

            Plugin.Instance?.DebugLog($"SmokeGrenade: Creating pickup at {savedGrenadePosition}");

            pickup = scp244.CreatePickup(savedGrenadePosition);

            if (_config.RemoveSmoke)
            {
                Plugin.Instance?.DebugLog($"SmokeGrenade: Scheduled smoke removal in {_config.SmokeTime} seconds");

                Timing.CallDelayed(_config.SmokeTime, () =>
                {
                    Plugin.Instance?.DebugLog("SmokeGrenade: Removing smoke by moving it down");

                    pickup.Position += Vector3.down * 10;

                    Timing.CallDelayed(10, () =>
                    {
                        Plugin.Instance?.DebugLog("SmokeGrenade: Destroying smoke pickup");

                        pickup.Destroy();
                    });
                });
            }
            Plugin.Instance?.DebugLog("SmokeGrenade.OnExploding completed successfully");
        }
    }
}