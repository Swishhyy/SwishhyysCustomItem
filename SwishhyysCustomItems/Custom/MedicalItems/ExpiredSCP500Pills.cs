using CustomPlayerEffects;
using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCI.Custom.MedicalItems
{
    public class ExpiredSCP500Pills : CustomItem
    {
        public override uint Id { get; set; } = 102;

        public override ItemType Type { get; set; } = ItemType.SCP500;

        public override string Name { get; set; } = "Expired SCP-500 Pills";

        public override string Description { get; set; } = "An expired bottle of SCP-500 pills with unpredictable effects.";

        public override float Weight { get; set; } = 0.5f;

        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties();

        // Configuration properties
        public float EffectDuration { get; set; } = 10f; // Duration in seconds

        // Positive effects with their chances
        private readonly Dictionary<Type, float> positiveEffects = new Dictionary<Type, float>
           {
               { typeof(CustomPlayerEffects.MovementBoost), 25f },   // 25% chance
               { typeof(CustomPlayerEffects.Scp207), 25f },          // 25% chance
           };

        // Negative effects with their chances
        private readonly Dictionary<Type, float> negativeEffects = new Dictionary<Type, float>
           {
               { typeof(CustomPlayerEffects.Concussed), 15f },       // 15% chance
               { typeof(CustomPlayerEffects.Bleeding), 15f },        // 15% chance
           };

        // SCP effects with their chances
        private readonly Dictionary<Type, float> scpEffects = new Dictionary<Type, float>
           {
               { typeof(CustomPlayerEffects.Poisoned), 10f },        // 10% chance
               { typeof(CustomPlayerEffects.Asphyxiated), 10f },     // 10% chance
           };

        // Overall category probabilities
        private readonly Dictionary<string, float> effectCategoryChances = new Dictionary<string, float>
           {
               { "Positive", 50f },  // 50% chance
               { "Negative", 30f },  // 30% chance
               { "SCP", 20f }        // 20% chance
           };

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;

            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;

            base.UnsubscribeEvents();
        }

        public void OnUsingItem(UsingItemEventArgs ev)
        {
            if (ev.Item.Type == ItemType.SCP500)
            {
                // Check if player is null before proceeding
                if (ev.Player == null)
                {
                    Log.Error("ExpiredSCP500Pills: Player is null in OnUsingItem");
                    return;
                }

                // Make sure we track if any effect was applied to determine random effects
                bool effectApplied = false;

                // Random chance for different effects
                if (UnityEngine.Random.Range(0, 100) <= 25) // Fixed: Added UnityEngine
                {
                    // Only apply effects if the player has a valid status effect manager
                    if (ev.Player.ReferenceHub?.playerEffectsController != null)
                    {
                        StatusEffectBase effectType = ev.Player.GetEffect<Poisoned>();
                        if (effectType != null)
                        {
                            ev.Player.EnableEffect(effectType, 10, 30f);
                            effectApplied = true;
                        }
                    }
                }

                if (UnityEngine.Random.Range(0, 100) <= 25 && !effectApplied) // Fixed: Added UnityEngine
                {
                    if (ev.Player.ReferenceHub?.playerEffectsController != null)
                    {
                        StatusEffectBase effectType = ev.Player.GetEffect<Bleeding>();
                        if (effectType != null)
                        {
                            ev.Player.EnableEffect(effectType, 10, 30f);
                            effectApplied = true;
                        }
                    }
                }

                // Add similar checks for other effects...

                // If no negative effects were applied, partially heal as a fallback
                if (!effectApplied)
                {
                    ev.Player.Health += UnityEngine.Random.Range(15f, 35f); // Fixed: Added UnityEngine
                    ev.Player.ShowHint("You consumed an expired SCP-500 pill. It partially healed you.");
                }
            }
        }

        private string GetRandomCategory()
        {
            float totalChance = 0;
            float randomValue = UnityEngine.Random.Range(0f, 100f);

            foreach (var category in effectCategoryChances)
            {
                totalChance += category.Value;
                if (randomValue <= totalChance)
                    return category.Key;
            }

            // Fallback to Positive if something goes wrong
            return "Positive";
        }

        private Type GetRandomEffectFromCategory(string category)
        {
            Dictionary<Type, float> effects = null;

            switch (category)
            {
                case "Positive":
                    effects = positiveEffects;
                    break;
                case "Negative":
                    effects = negativeEffects;
                    break;
                case "SCP":
                    effects = scpEffects;
                    break;
            }

            if (effects == null || effects.Count == 0)
                return null;

            float totalChance = 0;
            float randomValue = UnityEngine.Random.Range(0f, 100f);

            foreach (var effect in effects)
            {
                totalChance += effect.Value;
                if (randomValue <= totalChance)
                    return effect.Key;
            }

            return null;
        }
    }
}