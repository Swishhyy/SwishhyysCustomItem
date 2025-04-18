using Exiled.API.Enums;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using JetBrains.Annotations;
using MEC;
using UnityEngine;
using YamlDotNet.Serialization;
using Item = Exiled.API.Features.Items.Item;
using Random = System.Random;
using SCI.Config;

namespace SCI.Custom.Items.Grenades
{
    [CustomItem(ItemType.GrenadeHE)]
    public class ClusterGrenade(ClusterGrenadeConfig config) : CustomGrenade
    {
        #region Configuration

        [YamlIgnore]
        public override ItemType Type { get; set; } = ItemType.GrenadeHE;
        public override uint Id { get; set; } = 108;
        public override string Name { get; set; } = "<color=#FF0000>Cluster Grenade</color>";
        public override string Description { get; set; } = "When this grenade explodes, it spawns extra grenades near by";
        public override float Weight { get; set; } = 1.75f;
        public override bool ExplodeOnCollision { get; set; } = false;
        public override float FuseTime { get; set; } = 5f;

        [CanBeNull]
        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties
        {
            Limit = 2,
            DynamicSpawnPoints =
            [
                new() { Chance = 15, Location = SpawnLocationType.InsideLczArmory },
                new() { Chance = 15, Location = SpawnLocationType.InsideHczArmory },
                new() { Chance = 15, Location = SpawnLocationType.Inside049Armory },
                new() { Chance = 15, Location = SpawnLocationType.InsideSurfaceNuke },
                new() { Chance = 15, Location = SpawnLocationType.Inside079Armory },
            ],
        };

        private readonly ClusterGrenadeConfig _config = config;
        private readonly Random _random = new();

        #endregion

        #region Grenade Functionality

        protected override void OnExploding(ExplodingGrenadeEventArgs ev)
        {
            // Add slight delay to ensure main explosion happens first
            Timing.CallDelayed(0.1f, () =>
            {
                // Spawn initial scatter grenade
                ExplosiveGrenade scatterGrenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
                scatterGrenade.FuseTime = 0.25f;
                scatterGrenade.ScpDamageMultiplier = 0.5f;
                scatterGrenade.ChangeItemOwner(null, ev.Player);
                scatterGrenade.SpawnActive(ev.Position, ev.Player);

                // Configure child grenades
                ExplosiveGrenade childGrenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
                childGrenade.FuseTime = _config.ChildGrenadeFuseTime;
                childGrenade.ScpDamageMultiplier = 3f;

                // Spawn multiple child grenades
                for (int i = 0; i < _config.ChildGrenadeCount; i++)
                {
                    childGrenade.ChangeItemOwner(null, ev.Player);

                    // Determine spawn position based on config
                    Vector3 spawnPosition = _config.SpreadRadius > 0
                        ? CalculateRandomPosition(ev.Position)
                        : ev.Position;

                    childGrenade.SpawnActive(spawnPosition, ev.Player);
                }
            });
        }

        private Vector3 CalculateRandomPosition(Vector3 basePosition)
        {
            // Calculate random position within min and max spread radius
            float distance = _config.MinSpreadRadius + ((float)_random.NextDouble() * (_config.SpreadRadius - _config.MinSpreadRadius));
            float angle = (float)_random.NextDouble() * 360f;

            // Convert to radians
            float radians = angle * Mathf.Deg2Rad;

            // Calculate offset
            float xOffset = distance * Mathf.Cos(radians);
            float zOffset = distance * Mathf.Sin(radians);

            // Return new position with same y-coordinate
            return new Vector3(
                basePosition.x + xOffset,
                basePosition.y,
                basePosition.z + zOffset
            );
        }

        #endregion
    }
}
