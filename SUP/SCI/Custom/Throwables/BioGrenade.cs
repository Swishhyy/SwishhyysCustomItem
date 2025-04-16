using Exiled.API.Enums;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace SCI.Custom.Throwables
{
    public class BioGrenade : CustomGrenade
    {
        [YamlIgnore]
        public override ItemType Type { get; set; } = ItemType.GrenadeHE;
        public override uint Id { get; set; } = 104;
        public override string Name { get; set; } = "<color=#FF0000>Cluster Grenade</color>";
        public override string Description { get; set; } = "When this grenade explodes, it spawns extra grenades near by";
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
    }
}
