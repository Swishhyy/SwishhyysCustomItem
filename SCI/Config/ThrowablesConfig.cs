using System.ComponentModel;

namespace SCI.Config
{
    #region SmokeGrenadeConfig
    public class SmokeGrenadeConfig
    {
        [Description("Whether this item is enabled or not")]
        public bool IsEnabled { get; set; } = true;

        [Description("Whether to remove the smoke effect after a delay")]
        public bool RemoveSmoke { get; set; } = true;

        [Description("How long the smoke cloud remains before being removed (seconds)")]
        public float SmokeTime { get; set; } = 10f;

        [Description("Scale factor for the smoke effect (0.01 is small, 1.0 is large)")]
        public float SmokeScale { get; set; } = 0.01f;

        [Description("Maximum diameter the smoke cloud can expand to")]
        public float SmokeDiameter { get; set; } = 0.0f;
    }
    #endregion

    #region ImpactGrenadeConfig
    public class ImpactGrenadeConfig
    {
        [Description("Whether this item is enabled or not")]
        public bool IsEnabled { get; set; } = true;

        [Description("Maximum damage dealt at the center of explosion")]
        public float MaximumDamage { get; set; } = 115f;

        [Description("Minimum damage dealt at the edge of explosion radius")]
        public float MinimumDamage { get; set; } = 35f;

        [Description("Radius of the explosion damage")]
        public float DamageRadius { get; set; } = 7f;
    }
    #endregion

    #region ClusterGrenadeConfig
    public class ClusterGrenadeConfig
    {
        [Description("Whether this item is enabled or not")]
        public bool IsEnabled { get; set; } = true;

        [Description("Number of child grenades to spawn after the initial explosion")]
        public int ChildGrenadeCount { get; set; } = 3;

        [Description("Fuse time for child grenades in seconds")]
        public float ChildGrenadeFuseTime { get; set; } = 1.5f;

        [Description("Maximum radius for random spread of child grenades")]
        public float SpreadRadius { get; set; } = 3f;

        [Description("Maximum damage from child grenades (decreases with distance)")]
        public float ChildGrenadeDamage { get; set; } = 40f;

        [Description("Minimum radius for random spread of child grenades")]
        public float MinSpreadRadius { get; set; } = 1f;

        [Description("Damage falloff multiplier for child grenades")]
        public float DamageFalloffMultiplier { get; set; } = 1f;
    }
    #endregion

    #region BioGrenadeConfig
    public class BioGrenadeConfig
    {
        [Description("Whether this item is enabled or not")]
        public bool IsEnabled { get; set; } = true;

        [Description("Whether to remove the smoke effect after a delay")]
        public bool RemoveSmoke { get; set; } = true;

        [Description("How long the smoke cloud remains before being removed (seconds)")]
        public float SmokeTime { get; set; } = 60f;

        [Description("Scale factor for the smoke effect (0.01 is small, 1.0 is large)")]
        public float SmokeScale { get; set; } = 0.01f;

        [Description("Maximum diameter the smoke cloud can expand to")]
        public float SmokeDiameter { get; set; } = 0.0f;

        [Description("Delay before applying decontamination effect (seconds)")]
        public float DecontaminationDelay { get; set; } = 5f;

        [Description("Duration of the decontamination effect (seconds)")]
        public float DecontaminationDuration { get; set; } = 30f;

        [Description("Radius within which players receive the decontamination effect")]
        public float EffectRadius { get; set; } = 5f;

        [Description("AHP amount to grant to players")]
        public float AhpAmount { get; set; } = 35f;

        [Description("Amount of health to heal for players in range")]
        public float HealAmount { get; set; } = 25f;

        [Description("Continuous healing - heal every few seconds while in the cloud")]
        public bool ContinuousHealing { get; set; } = true;

        [Description("Interval between healing pulses (seconds)")]
        public float HealingInterval { get; set; } = 5f;
    }
    #endregion
}
