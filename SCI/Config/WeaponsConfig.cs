using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCI.Config
{
    public class RailgunConfig
    {

        [Description("Maximum damage dealt by the railgun")]
        public float Damage { get; set; } = 85f;

        [Description("Maximum range of the railgun beam in meters")]
        public float Range { get; set; } = 50f;

        [Description("Width of the beam/hit detection in meters")]
        public float BeamWidth { get; set; } = 0.75f;

        [Description("Whether the railgun creates an explosion at the impact point")]
        public bool SpawnExplosive { get; set; } = true;

        [Description("Maximum number of railguns that can spawn in a round")]
        public int SpawnLimit { get; set; } = 1;

        [Description("Cooldown between shots in seconds")]
        public float Cooldown { get; set; } = 8f;

        [Description("Enable debug logging")]
        public bool EnableDebugLogging { get; set; } = false;
    }

    public class GrenadeLauncherConfig
    {
        [Description("Force applied to launched grenades")]
        public float LaunchForce { get; set; } = 20f;

        [Description("Amount of upward arc for the grenade (higher values = steeper arc)")]
        public float UpwardArc { get; set; } = 0.2f;

        [Description("Distance in front of player to spawn the grenade")]
        public float SpawnDistance { get; set; } = 1.0f;

        [Description("Delay before the grenade explodes after being fired (seconds)")]
        public float ExplosionDelay { get; set; } = 5.0f;

        [Description("Fuse time for grenade when it explodes (seconds)")]
        public float FuseTime { get; set; } = 1.0f;
    }
}
