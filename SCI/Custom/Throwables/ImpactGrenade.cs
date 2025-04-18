using System;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using JetBrains.Annotations;
using MEC;
using SCI.Config;
using UnityEngine;
using YamlDotNet.Serialization;

namespace SCI.Custom.Throwables
{
    [CustomItem(ItemType.GrenadeHE)]
    public class ImpactGrenade(ImpactGrenadeConfig config) : CustomGrenade
    {
        #region Configuration

        [YamlIgnore]
        public override ItemType Type { get; set; } = ItemType.GrenadeHE;
        public override uint Id { get; set; } = 109;
        public override string Name { get; set; } = "<color=#00FFFF>Impact Grenade</color>";
        public override string Description { get; set; } = "Explodes immediately upon impact with any surface.";
        public override float Weight { get; set; } = 0.75f;
        public override bool ExplodeOnCollision { get; set; } = true;
        public override float FuseTime { get; set; } = 10f;

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
        private readonly ImpactGrenadeConfig _config = config;
        #endregion

        public override void Init()
        {
            base.Init();
        }

        #region Grenade Functionality
        protected override void OnExploding(ExplodingGrenadeEventArgs ev)
        {
            // Enhanced damage calculation for nearby players
            foreach (Player target in Player.List)
            {
                if (target == null || !target.IsAlive)
                    continue;

                float distance = Vector3.Distance(ev.Position, target.Position);
                if (distance <= _config.DamageRadius)
                {
                    // Calculate damage based on distance (more damage when closer)
                    float damage = Mathf.Lerp(_config.MaximumDamage, _config.MinimumDamage,
                        distance / _config.DamageRadius);

                    // Apply damage after a tiny delay to ensure explosion effect shows first
                    Timing.CallDelayed(0.1f, () =>
                    {
                        target.Hurt(damage, DamageType.Explosion);
                    });
                }
            }
        }
        #endregion
    }
}
