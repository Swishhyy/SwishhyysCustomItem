using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using System;
using System.Collections.Generic;

namespace SCI.Custom.MedicalItems
{
    public class AdrenalineSCP500Pills : CustomItem
    {
        public override uint Id { get; set; } = 101;

        public override ItemType Type { get; set; } = ItemType.SCP500;

        public override string Name { get; set; } = "Adrenaline Pills";

        public override string Description { get; set; } = "A small bottle of pills that gives you a boost of energy.";

        public override float Weight { get; set; } = 0.5f;

        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties();

        // Configuration properties
        public float SpeedMultiplier { get; set; } = 1.5f;

        public float EffectDuration { get; set; } = 10f; // Duration in seconds

        public float Cooldown { get; set; } = 30f; // Cooldown duration in seconds

        // Dictionary to track cooldowns per player
        private readonly Dictionary<string, DateTime> cooldowns = new Dictionary<string, DateTime>();

        protected override void SubscribeEvents()
        {
            // Subscribe to the UsingItem event
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;

            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            // Unsubscribe from the UsingItem event
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;

            base.UnsubscribeEvents();
        }

        private void OnUsingItem(UsingItemEventArgs ev)
        {
            // Check if the used item is this custom item
            if (!Check(ev.Player.CurrentItem))
                return;

            // Get the player's unique identifier
            string userId = ev.Player.UserId;

            // Check for cooldown
            if (cooldowns.TryGetValue(userId, out DateTime lastUsed))
            {
                if ((DateTime.UtcNow - lastUsed).TotalSeconds < Cooldown)
                {
                    ev.Player.ShowHint("You must wait before using another pill!", 5);
                    return;
                }
            }

            // Apply movement speed boost using the Scp207 effect
            ev.Player.EnableEffect<CustomPlayerEffects.Scp207>(EffectDuration);

            // Adjust the intensity of the effect to set the speed multiplier
            var scp207 = ev.Player.GetEffect<CustomPlayerEffects.Scp207>();
            if (scp207 != null)
            {
                // Ensure intensity is within byte range
                byte intensity = (byte)Math.Max(0, Math.Min(SpeedMultiplier * 10, 255)); // Adjust scaling factor as needed
                scp207.Intensity = intensity;
            }

            // Restore stamina to full
            ev.Player.Stamina = 100f;

            // Provide feedback to the player
            ev.Player.Broadcast(5, "<color=yellow>You feel a rush of adrenaline!</color>");

            // Remove the item from the player's inventory
            ev.Player.RemoveItem(ev.Player.CurrentItem);

            // Update the cooldown timer
            cooldowns[userId] = DateTime.UtcNow;

            // Apply side effects after the effect duration ends
            Timing.CallDelayed(EffectDuration, () =>
            {
                if (ev.Player != null && ev.Player.IsAlive)
                {
                    // Apply exhaustion effect
                    ev.Player.EnableEffect<CustomPlayerEffects.Exhausted>(5f);
                    ev.Player.Broadcast(5, "<color=red>You feel exhausted after the adrenaline rush...</color>");
                }
            });
        }
    }
}
