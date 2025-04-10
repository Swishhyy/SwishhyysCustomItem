using Exiled.API.Enums;
using System.Collections.Generic;
using System.ComponentModel;

namespace SCI.Custom.Config
{
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
            { EffectType.MovementBoost, new EffectSettings { Chance = 15f, MinIntensity = 1, MaxIntensity = 5 } },
            { EffectType.Scp207, new EffectSettings { Chance = 10f, MinIntensity = 1, MaxIntensity = 3 } },
            { EffectType.Invigorated, new EffectSettings { Chance = 15f, MinIntensity = 1, MaxIntensity = 5 } },
            { EffectType.BodyshotReduction, new EffectSettings { Chance = 10f, MinIntensity = 1, MaxIntensity = 3 } },
            { EffectType.Invisible, new EffectSettings { Chance = 5f, MinIntensity = 1, MaxIntensity = 2 } },
            { EffectType.Vitality, new EffectSettings { Chance = 15f, MinIntensity = 1, MaxIntensity = 4 } },
            { EffectType.DamageReduction, new EffectSettings { Chance = 15f, MinIntensity = 1, MaxIntensity = 3 } },
            { EffectType.RainbowTaste, new EffectSettings { Chance = 5f, MinIntensity = 1, MaxIntensity = 1 } },
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
            { EffectType.AmnesiaItems, new EffectSettings { Chance = 15f, MinIntensity = 1, MaxIntensity = 1 } },
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