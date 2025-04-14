using System;
using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
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
        private readonly ImpactGrenadeConfig _config = config;

        [YamlIgnore]
        public override ItemType Type { get; set; } = ItemType.GrenadeHE;
        public override uint Id { get; set; } = 105;
        public override string Name { get; set; } = "<color=#00FFFF>Impact Grenade</color>";
        public override string Description { get; set; } = "Explodes immediately upon impact with any surface.";
        public override float Weight { get; set; } = 0.75f;
        [CanBeNull]
        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties
        {
            Limit = 2,
            DynamicSpawnPoints = [new() { Chance = 15, Location = SpawnLocationType.InsideLczArmory }, new() { Chance = 15, Location = SpawnLocationType.InsideHczArmory }, new() { Chance = 15, Location = SpawnLocationType.Inside049Armory }, new() { Chance = 15, Location = SpawnLocationType.InsideSurfaceNuke }, new() { Chance = 15, Location = SpawnLocationType.Inside079Armory },],
        };

        // Key settings for impact grenade
        public override bool ExplodeOnCollision { get; set; } = true;
        public override float FuseTime { get; set; } = 10f; // Backup fuse time if collision doesn't trigger

        public override void Init()
        {
            base.Init();
            Log.Debug($"Impact Grenade initialized with damage radius: {_config.DamageRadius}");
        }

        protected override void OnExploding(ExplodingGrenadeEventArgs ev)
        {

            Log.Debug($"Impact Grenade exploding at position {ev.Position}");

            // Calculate enhanced damage for nearby players
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

                     Log.Debug($"Applying {damage} damage to {target.Nickname} at distance {distance}");

                    // Apply damage after a tiny delay to ensure explosion effect shows first
                    Timing.CallDelayed(0.1f, () =>
                    {
                        target.Hurt(damage, DamageType.Explosion);
                    });
                }
            }
        }
    }
}