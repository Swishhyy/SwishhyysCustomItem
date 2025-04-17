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
        
        [Description("Duration of the exhaustion effect after adrenaline wears off")]
        public float ExhaustionDuration { get; set; } = 5f;

        [Description("Cooldown between using adrenaline pills in seconds")]
        public float Cooldown { get; set; } = 5f;

        [Description("Amount of stamina to restore when using the pills (0-100)")]
        public float StaminaRestoreAmount { get; set; } = 100f;
    }

    public class VanishingSCP500PillsConfig
    {
        [Description("Duration of invisibility in seconds")]
        public float Duration { get; set; } = 7f;
    }

    public class Anti096SCP500pPillsConfig
    {
        [Description("Duration to show success/failure message hints (in seconds)")]
        public float MessageDuration { get; set; } = 5f;

        [Description("Whether to give the player immunity from being targeted by SCP-096 again for a period")]
        public bool ProvideTemporaryImmunity { get; set; } = false;

        [Description("Duration of immunity from SCP-096 targeting (in seconds)")]
        public float ImmunityDuration { get; set; } = 60f;
    }

    public class ExpiredSCP500PillsConfig
    {
        [Description("Default duration for applied effects (in seconds)")]
        public float DefaultEffectDuration { get; set; } = 10f;

        [Description("Category probabilities (must sum to 100)")]
        public Dictionary<string, float> CategoryChances { get; set; } = new() { { "Positive", 50f }, { "Negative", 30f }, { "SCP", 20f } };

        [Description("Positive effects with their individual chances and intensities")]
        public Dictionary<EffectType, EffectSettings> PositiveEffects { get; set; } = CreateEffects(
            (EffectType.Scp207, 10f, 1, 3), (EffectType.Invigorated, 15f, 1, 5),
            (EffectType.BodyshotReduction, 10f, 1, 3), (EffectType.Invisible, 5f, 1, 2),
            (EffectType.Vitality, 15f, 1, 4), (EffectType.DamageReduction, 15f, 1, 3),
            (EffectType.AntiScp207, 10f, 1, 1));

        [Description("Negative effects with their individual chances and intensities")]
        public Dictionary<EffectType, EffectSettings> NegativeEffects { get; set; } = CreateEffects(
            (EffectType.Concussed, 10f, 1, 8), (EffectType.Bleeding, 12f, 1, 10),
            (EffectType.Burned, 8f, 1, 6), (EffectType.Deafened, 10f, 1, 8),
            (EffectType.Exhausted, 12f, 1, 10), (EffectType.Flashed, 10f, 1, 6),
            (EffectType.Disabled, 5f, 1, 6), (EffectType.Ensnared, 8f, 1, 8),
            (EffectType.Hemorrhage, 5f, 1, 10), (EffectType.Poisoned, 10f, 1, 10));

        [Description("SCP-like effects with their individual chances and intensities")]
        public Dictionary<EffectType, EffectSettings> SCPEffects { get; set; } = CreateEffects(
            (EffectType.Asphyxiated, 15f, 1, 10), (EffectType.CardiacArrest, 10f, 1, 10),
            (EffectType.Decontaminating, 15f, 1, 8), (EffectType.SeveredHands, 10f, 1, 1),
            (EffectType.Stained, 10f, 1, 8), (EffectType.AmnesiaVision, 15f, 1, 1),
            (EffectType.Corroding, 10f, 1, 8));

        [Description("Healing fallback settings if no effects are applied")]
        public HealSettings HealFallback { get; set; } = new() { MinHeal = 15f, MaxHeal = 35f };

        // Helper method to create effects dictionary inline
        private static Dictionary<EffectType, EffectSettings> CreateEffects(params (EffectType type, float chance, byte min, byte max)[] effects) =>
            effects.ToDictionary(e => e.type, e => new EffectSettings { Chance = e.chance, MinIntensity = e.min, MaxIntensity = e.max });

        // Define nested classes for settings to keep everything in one class
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
}
