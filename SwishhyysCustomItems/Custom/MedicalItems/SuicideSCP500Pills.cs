using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using SCI.Custom.Config;
using System;
using UnityEngine;

namespace SCI.Custom.MedicalItems
{
    public class SuicideSCP500Pills : CustomItem
    {
        public override uint Id { get; set; } = 103;

        public override ItemType Type { get; set; } = ItemType.SCP500;

        public override string Name { get; set; } = "Suicide Pills";

        public override string Description { get; set; } = "Pills that cause the user to explode. There's a small chance of survival.";

        public override float Weight { get; set; } = 0.5f;

        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties();

        // Reference to the configuration
        private readonly SuicideSCP500PillsConfig _config;

        // Constructor that takes the config
        public SuicideSCP500Pills(SuicideSCP500PillsConfig config)
        {
            _config = config;
        }

        // Default constructor for compatibility
        public SuicideSCP500Pills()
        {
            // Create a default config if none was provided
            _config = new SuicideSCP500PillsConfig();
        }

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
            try
            {
                // Check if the used item is this custom item
                if (!Check(ev.Player.CurrentItem))
                    return;

                // Check if player is null before proceeding
                if (ev.Player == null)
                {
                    Log.Error("SuicidePills: Player is null in OnUsingItem");
                    return;
                }

                // Remove the item from the player's inventory
                ev.Player.RemoveItem(ev.Player.CurrentItem);

                // Determine if player survives based on config chance
                bool survives = UnityEngine.Random.Range(0f, 100f) <= _config.SurvivalChance;

                // Show message to player
                if (survives)
                {
                    ev.Player.ShowHint(_config.SurvivalMessage, _config.HintDuration);
                }
                else
                {
                    ev.Player.ShowHint(_config.DeathMessage, _config.HintDuration);
                }

                // Execute explosion effect
                ExplodePlayer(ev.Player, survives);

                Log.Debug($"Player {ev.Player.Nickname} used suicide pills. Survival: {survives}");
            }
            catch (Exception ex)
            {
                Log.Error($"SuicidePills: Error in OnUsingItem: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ExplodePlayer(Player player, bool survives)
        {
            try
            {
                // Kill the player if they don't survive
                if (!survives)
                {
                    // Give a small delay to show the hint before death
                    Timing.CallDelayed(0.2f, () =>
                    {
                        // Kill the player with explosion damage type
                        player.Kill(DamageType.Explosion);
                    });
                }
                else
                {
                    // Just damage the player if they survive
                    player.Hurt(70f, DamageType.Explosion);

                    // Set minimum health after survival
                    Timing.CallDelayed(0.5f, () =>
                    {
                        if (player.IsAlive && player.Health < _config.SurvivalHealthAmount)
                        {
                            player.Health = _config.SurvivalHealthAmount;
                        }
                    });
                }

                // Damage nearby players (explosion radius effect)
                foreach (Player target in Player.List)
                {
                    if (target == player || !target.IsAlive)
                        continue;

                    // Calculate distance
                    float distance = Vector3.Distance(player.Position, target.Position);

                    // Apply damage based on distance (closer = more damage) using config values
                    if (distance < _config.ExplosionRadius)
                    {
                        float damage = Mathf.Lerp(_config.MaxNearbyPlayerDamage, 10f, distance / _config.ExplosionRadius);
                        target.Hurt(damage, DamageType.Explosion);

                        // Show hint to nearby players
                        target.ShowHint($"You were hit by an explosion!", 3f);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error in ExplodePlayer: {ex.Message}");
            }
        }
    }
}