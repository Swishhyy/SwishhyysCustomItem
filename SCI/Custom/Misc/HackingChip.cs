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
    public class HackingChip : CustomItem
    {
        public override string Name { get; set; } = "<color=#FF0000>Hacking Chip</color>";
        public override string Description { get; set; } = "This is a hacking chip, it can be used to mess with the facility";
        public override float Weight { get; set; } = 0.5f;
        public override ItemType Type { get; set; } = ItemType.KeycardChaosInsurgency;
        public override uint Id { get; set; } = 105;
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
