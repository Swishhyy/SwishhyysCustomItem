using System.ComponentModel;

namespace SCI.Custom.Config
{
    public class SuicideSCP500PillsConfig
    {
        [Description("Chance that the player survives the explosion (0-100)")]
        public float SurvivalChance { get; set; } = 5f;

        [Description("Amount of health to give the player if they survive")]
        public float SurvivalHealthAmount { get; set; } = 5f;

        [Description("Maximum explosion damage to the user")]
        public float UserDamage { get; set; } = 1000f;

        [Description("Maximum damage to nearby players")]
        public float MaxNearbyPlayerDamage { get; set; } = 70f;

        [Description("Explosion radius (in meters)")]
        public float ExplosionRadius { get; set; } = 10f;

        [Description("Duration of the hint message (in seconds)")]
        public float HintDuration { get; set; } = 5f;

        [Description("Message shown to player when they survive")]
        public string SurvivalMessage { get; set; } = "You consumed the suicide pills, but somehow survived the explosion!";

        [Description("Message shown to player when they won't survive")]
        public string DeathMessage { get; set; } = "You consumed the suicide pills...";
    }
}
