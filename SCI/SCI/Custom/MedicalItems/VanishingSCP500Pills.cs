using Exiled.API.Enums;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using SCI.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCI.Custom.MedicalItems
{
    public class VanishingSCP500Pills(VanishingSCP500PillsConfig config) : CustomItem
    {
        public override string Name { get; set; } = "<color=#FF0000>Vanishing SCP-500 Pills</color>";
        public override string Description { get; set; } = "These pills make people vanish for a short amount of time.";
        public override float Weight { get; set; } = 0.5f;
        public override uint Id { get; set; } = 110;
        public override ItemType Type { get; set; } = ItemType.SCP500;
        
        private readonly VanishingSCP500PillsConfig _config = config;

        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties
        {
            Limit = 2,
            DynamicSpawnPoints =
           [
               new()
                {
                    Chance = 15,
                    Location = SpawnLocationType.InsideLczCafe,
                },

                new()
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
