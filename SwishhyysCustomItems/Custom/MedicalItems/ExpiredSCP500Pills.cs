using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using System;
using System.Collections.Generic;

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

        private void OnUsingItem(UsingItemEventArgs ev)
        {
            // Check if the used item is this custom item
            if (!Check(ev.Player.CurrentItem))
                return;

            // Remove the item from the player's inventory
            ev.Player.RemoveItem(ev.Player.CurrentItem);

            // Decide the effect category
            string category = GetRandomCategory();

            // Select a random effect from the chosen category
            Type selectedEffectType = GetRandomEffectFromCategory(category);

            // Apply the effect if one was selected
            if (selectedEffectType != null)
            {
                var selectedEffect = (CustomPlayerEffects.StatusEffectBase)Activator.CreateInstance(selectedEffectType);
                ev.Player.EnableEffect(selectedEffect, EffectDuration);

                // Provide feedback to the player
                ev.Player.ShowHint($"<color=yellow>You feel strange after consuming the expired pill...</color>", 5f);
            }
            else
            {
                // If no effect was applied
                ev.Player.ShowHint("<color=gray>Nothing seems to have happened...</color>", 5f);
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
