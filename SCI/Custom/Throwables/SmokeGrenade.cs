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
        #region Configuration
        [YamlIgnore]
        public override ItemType Type { get; set; } = ItemType.GrenadeFlash;
        public override uint Id { get; set; } = 110;
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
        private readonly SmokeGrenadeConfig _config = config;
        #endregion

        public override void Init()
        {
            base.Init();
        }

        #region Grenade Functionality
        protected override void OnExploding(ExplodingGrenadeEventArgs ev)
        {
            // Prevent default explosion
            ev.IsAllowed = false;

            // Save position for smoke creation
            Vector3 savedGrenadePosition = ev.Position;

            // Create SCP-244 item to use as smoke source
            Scp244 scp244 = (Scp244)Item.Create(ItemType.SCP244a);

            // Configure smoke appearance from config
            scp244.Scale = new Vector3(_config.SmokeScale, _config.SmokeScale, _config.SmokeScale);
            scp244.Primed = true;
            scp244.MaxDiameter = _config.SmokeDiameter;

            // Create the smoke pickup at explosion position
            Pickup pickup = scp244.CreatePickup(savedGrenadePosition);

            // Set up smoke removal if enabled in config
            if (_config.RemoveSmoke)
            {
                Timing.CallDelayed(_config.SmokeTime, () =>
                {
                    // Move smoke below the map
                    pickup.Position += Vector3.down * 10;

                    // Clean up after a delay
                    Timing.CallDelayed(10, () =>
                    {
                        pickup.Destroy();
                    });
                });
            }
        }
        #endregion
    }
}
