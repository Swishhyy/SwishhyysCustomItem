using System.ComponentModel;

namespace SCI.Config
{
    #region SmokeGrenadeConfig
    public class SmokeGrenadeConfig
    {
        [Description("Whether to remove the smoke effect after a delay")]
        public bool RemoveSmoke { get; set; } = true;

        [Description("How long the smoke cloud remains before being removed (seconds)")]
        public float SmokeTime { get; set; } = 10f;

        [Description("Scale factor for the smoke effect (0.01 is small, 1.0 is large)")]
        public float SmokeScale { get; set; } = 0.01f;

        [Description("Maximum diameter the smoke cloud can expand to")]
        public float SmokeDiameter { get; set; } = 0.0f;

        [Description("Show hint message to players caught in smoke")]
        public bool ShowSmokeMessage { get; set; } = false;

        [Description("Message to show to players caught in smoke")]
        public string SmokeMessage { get; set; } = "You're in a smoke cloud!";

        [Description("Duration to show smoke message (seconds)")]
        public float MessageDuration { get; set; } = 3f;

        [Description("Enable debug logging")]
        public bool EnableDebugLogging { get; set; } = false;
    }
    #endregion

    #region ImpactGrenadeConfig
    public class ImpactGrenadeConfig
    {
        [Description("Maximum damage dealt at the center of explosion")]
        public float MaximumDamage { get; set; } = 115f;

        [Description("Minimum damage dealt at the edge of explosion radius")]
        public float MinimumDamage { get; set; } = 35f;

        [Description("Radius of the explosion damage")]
        public float DamageRadius { get; set; } = 7f;

        [Description("Message shown to players hit by the grenade")]
        public string EffectMessage { get; set; } = "You were hit by an impact grenade!";

        [Description("Duration to show the effect message in seconds")]
        public float MessageDuration { get; set; } = 3f;

        [Description("Whether to show hit message to affected players")]
        public bool ShowEffectMessage { get; set; } = true;

        [Description("Enable debug logging")]
        public bool EnableDebugLogging { get; set; } = false;
    }
    #endregion

    #region ClusterGrenadeConfig
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
    #endregion
}
