using Exiled.API.Enums;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCI.Custom.Misc
{
    public class ReinforcementCall : CustomItem
    {
        public override string Name { get; set; } = "<color=#FF0000>Reinforcement Call</color>";
        public override string Description { get; set; } = "A Radio to call for reinforcements";
        public override float Weight { get; set; } = 0.5f;
        public override ItemType Type { get; set; } = ItemType.Radio;
        public override uint Id { get; set; } = 111;
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
