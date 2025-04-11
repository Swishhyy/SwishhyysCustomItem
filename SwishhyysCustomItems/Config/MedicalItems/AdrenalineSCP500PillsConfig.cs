using System.ComponentModel;

namespace SCI.Custom.Config
{
    public class AdrenalineSCP500PillsConfig
    {
        [Description("Speed multiplier for the player when using the adrenaline pills")]
        public float SpeedMultiplier { get; set; } = 1.8f;

        [Description("Duration of the adrenaline effect in seconds")]
        public float EffectDuration { get; set; } = 25f;

        [Description("Cooldown between using adrenaline pills in seconds")]
        public float Cooldown { get; set; } = 5f;

        [Description("Amount of stamina to restore when using the pills (0-100)")]
        public float StaminaRestoreAmount { get; set; } = 100f;

        [Description("Duration of the hint message in seconds")]
        public float HintDuration { get; set; } = 5f;

        [Description("Duration of the exhaustion effect after adrenaline wears off")]
        public float ExhaustionDuration { get; set; } = 5f;

        [Description("Message shown when adrenaline effect begins")]
        public string ActivationMessage { get; set; } = "<color=yellow>You feel a rush of adrenaline!</color>";

        [Description("Message shown when adrenaline effect ends")]
        public string ExhaustionMessage { get; set; } = "<color=red>You feel exhausted after the adrenaline rush...</color>";

        [Description("Message shown when trying to use during cooldown")]
        public string CooldownMessage { get; set; } = "You must wait before using another pill!";
    }
}
