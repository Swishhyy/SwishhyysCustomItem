using System.ComponentModel;

namespace SCI.Custom.Config
{
    public class ClusterGrenadeConfig
    {
        [Description("Number of child grenades to spawn after the initial explosion")]
        public int ChildGrenadeCount { get; set; } = 3;

        [Description("Fuse time for child grenades in seconds")]
        public float ChildGrenadeFuseTime { get; set; } = 1.5f;

        [Description("Delay between spawning each child grenade in seconds")]
        public float ChildGrenadeDelay { get; set; } = 0.1f;

        [Description("Maximum radius for random spread of child grenades")]
        public float SpreadRadius { get; set; } = 3f;

        [Description("Maximum damage radius for child grenades")]
        public float ChildGrenadeRadius { get; set; } = 5f;

        [Description("Maximum damage from child grenades (decreases with distance)")]
        public float ChildGrenadeDamage { get; set; } = 40f;

        [Description("Enables a scatter grenade for visual effect before child grenades")]
        public bool UseScatterEffect { get; set; } = true;

        [Description("Minimum radius for random spread of child grenades")]
        public float MinSpreadRadius { get; set; } = 1f;

        [Description("Damage falloff multiplier for child grenades")]
        public float DamageFalloffMultiplier { get; set; } = 1f;

        [Description("Enables logging for debugging purposes")]
        public bool EnableDebugLogging { get; set; } = false;
    }
}