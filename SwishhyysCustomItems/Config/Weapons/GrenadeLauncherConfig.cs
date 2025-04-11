using System.ComponentModel;

namespace SCI.Custom.Config
{
    public class GrenadeLauncherConfig
    {
        [Description("Unique ID for the Grenade Launcher")]
        public uint Id { get; set; } = 108;

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
