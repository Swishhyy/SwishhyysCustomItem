using System.ComponentModel;

namespace SCI.Config
{
    public class RailgunConfig
    {
        [Description("Unique ID for the railgun (must be unique across all custom items)")]
        public uint Id { get; set; } = 107;

        [Description("Maximum damage dealt by the railgun")]
        public float Damage { get; set; } = 75f;

        [Description("Maximum range of the railgun beam in meters")]
        public float Range { get; set; } = 50f;

        [Description("Width of the beam/hit detection in meters")]
        public float BeamWidth { get; set; } = 0.5f;

        [Description("Whether the railgun creates an explosion at the impact point")]
        public bool SpawnExplosive { get; set; } = true;

        [Description("Maximum number of railguns that can spawn in a round")]
        public int SpawnLimit { get; set; } = 1;

        [Description("Cooldown between shots in seconds")]
        public float Cooldown { get; set; } = 10f;
    }
}
