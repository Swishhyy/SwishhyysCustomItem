using Exiled.API.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCI.Config
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

    public class ExpiredSCP500PillsConfig
    {
        [Description("Default duration for applied effects (in seconds)")]
        public float DefaultEffectDuration { get; set; } = 10f;

        [Description("Category probabilities (must sum to 100)")]
        public Dictionary<string, float> CategoryChances { get; set; } = new Dictionary<string, float>
        {
            { "Positive", 50f },
            { "Negative", 30f },
            { "SCP", 20f }
        };

        [Description("Positive effects with their individual chances and intensities")]
        public Dictionary<EffectType, EffectSettings> PositiveEffects { get; set; } = new Dictionary<EffectType, EffectSettings>
        {
            { EffectType.Scp207, new EffectSettings { Chance = 10f, MinIntensity = 1, MaxIntensity = 3 } },
            { EffectType.Invigorated, new EffectSettings { Chance = 15f, MinIntensity = 1, MaxIntensity = 5 } },
            { EffectType.BodyshotReduction, new EffectSettings { Chance = 10f, MinIntensity = 1, MaxIntensity = 3 } },
            { EffectType.Invisible, new EffectSettings { Chance = 5f, MinIntensity = 1, MaxIntensity = 2 } },
            { EffectType.Vitality, new EffectSettings { Chance = 15f, MinIntensity = 1, MaxIntensity = 4 } },
            { EffectType.DamageReduction, new EffectSettings { Chance = 15f, MinIntensity = 1, MaxIntensity = 3 } },
            { EffectType.AntiScp207, new EffectSettings { Chance = 10f, MinIntensity = 1, MaxIntensity = 1 } }
        };

        [Description("Negative effects with their individual chances and intensities")]
        public Dictionary<EffectType, EffectSettings> NegativeEffects { get; set; } = new Dictionary<EffectType, EffectSettings>
        {
            { EffectType.Concussed, new EffectSettings { Chance = 10f, MinIntensity = 1, MaxIntensity = 8 } },
            { EffectType.Bleeding, new EffectSettings { Chance = 12f, MinIntensity = 1, MaxIntensity = 10 } },
            { EffectType.Burned, new EffectSettings { Chance = 8f, MinIntensity = 1, MaxIntensity = 6 } },
            { EffectType.Deafened, new EffectSettings { Chance = 10f, MinIntensity = 1, MaxIntensity = 8 } },
            { EffectType.Exhausted, new EffectSettings { Chance = 12f, MinIntensity = 1, MaxIntensity = 10 } },
            { EffectType.Flashed, new EffectSettings { Chance = 10f, MinIntensity = 1, MaxIntensity = 6 } },
            { EffectType.Disabled, new EffectSettings { Chance = 5f, MinIntensity = 1, MaxIntensity = 6 } },
            { EffectType.Ensnared, new EffectSettings { Chance = 8f, MinIntensity = 1, MaxIntensity = 8 } },
            { EffectType.Hemorrhage, new EffectSettings { Chance = 5f, MinIntensity = 1, MaxIntensity = 10 } },
            { EffectType.Poisoned, new EffectSettings { Chance = 10f, MinIntensity = 1, MaxIntensity = 10 } }
        };

        [Description("SCP-like effects with their individual chances and intensities")]
        public Dictionary<EffectType, EffectSettings> SCPEffects { get; set; } = new Dictionary<EffectType, EffectSettings>
        {
            { EffectType.Asphyxiated, new EffectSettings { Chance = 15f, MinIntensity = 1, MaxIntensity = 10 } },
            { EffectType.CardiacArrest, new EffectSettings { Chance = 10f, MinIntensity = 1, MaxIntensity = 10 } },
            { EffectType.Decontaminating, new EffectSettings { Chance = 15f, MinIntensity = 1, MaxIntensity = 8 } },
            { EffectType.SeveredHands, new EffectSettings { Chance = 10f, MinIntensity = 1, MaxIntensity = 1 } },
            { EffectType.Stained, new EffectSettings { Chance = 10f, MinIntensity = 1, MaxIntensity = 8 } },
            { EffectType.AmnesiaVision, new EffectSettings { Chance = 15f, MinIntensity = 1, MaxIntensity = 1 } },
            { EffectType.Corroding, new EffectSettings { Chance = 10f, MinIntensity = 1, MaxIntensity = 8 } }
        };

        [Description("Healing fallback settings if no effects are applied")]
        public HealSettings HealFallback { get; set; } = new HealSettings
        {
            MinHeal = 15f,
            MaxHeal = 35f
        };
    }

    public class EffectSettings
    {
        [Description("Chance for this effect to be selected (within its category)")]
        public float Chance { get; set; } = 10f;

        [Description("Minimum intensity level for this effect")]
        public byte MinIntensity { get; set; } = 1;

        [Description("Maximum intensity level for this effect")]
        public byte MaxIntensity { get; set; } = 10;

        [Description("Custom duration for this effect (in seconds). Set to 0 to use default duration.")]
        public float CustomDuration { get; set; } = 0f;
    }

    public class HealSettings
    {
        [Description("Minimum healing amount when no effects are applied")]
        public float MinHeal { get; set; } = 15f;

        [Description("Maximum healing amount when no effects are applied")]
        public float MaxHeal { get; set; } = 35f;
    }
}
