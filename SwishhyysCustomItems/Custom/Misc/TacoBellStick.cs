using Exiled.API.Enums;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using JetBrains.Annotations;
using SCI.Config;

namespace SCI.Custom.Misc
{
    public class TacoBellStick(TacoBellStickConfig config) : CustomItem
    {
        // Change the type from TacoBellStick to TacoBellStickConfig
        private readonly TacoBellStickConfig _config = config;

        public override uint Id { get; set; } = 110;
        public override ItemType Type { get; set; } = ItemType.Jailbird;
        public override string Name { get; set; } = "<color=#8A2BE2>TacoBell Stick</color>";
        public override string Description { get; set; } = "A Stick That Makes You Experience Taco Bell";
        public override float Weight { get; set; } = TacoBellStickConfig.TacoBellStickWeight;
        
        [CanBeNull]
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
