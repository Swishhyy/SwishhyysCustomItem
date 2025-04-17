//using Exiled.API.Enums;
//using Exiled.API.Features;
//using Exiled.API.Features.Spawn;
//using Exiled.CustomItems.API.Features;
//using Exiled.Events.EventArgs.Player;
//using MEC;
//using PlayerRoles;
//using SCI.Config;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;

//namespace SCI.Custom.Misc
//{
//    public class ReinforcementCall(ReinforcementCallConfig config) : CustomItem
//    {
//        private readonly ReinforcementCallConfig _config = config;
//        public override string Name { get; set; } = "<color=#FF0000>Reinforcement Call (Unusable As Of Now)</color>";
//        public override string Description { get; set; } = "A Radio to call for reinforcements";
//        public override float Weight { get; set; } = 0.5f;
//        public override ItemType Type { get; set; } = ItemType.Radio;
//        public override uint Id { get; set; } = 106;

//        // Dictionary to track cooldowns for each player by their UserId
//        private readonly Dictionary<string, DateTime> cooldowns = [];

//        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties
//        {
//            Limit = 2,
//            DynamicSpawnPoints =
//            [
//                new ()
//                {
//                    Chance = 15,
//                    Location = SpawnLocationType.InsideLczCafe,
//                },
//                new ()
//                {
//                    Chance = 15,
//                    Location = SpawnLocationType.InsideLczWc,
//                },
//                new()
//                {
//                    Chance = 15,
//                    Location = SpawnLocationType.Inside914,
//                },
//                new()
//                {
//                    Chance = 15,
//                    Location = SpawnLocationType.InsideGr18Glass,
//                },
//                new()
//                {
//                    Chance = 15,
//                    Location = SpawnLocationType.Inside096,
//                },
//            ],
//        };

//        protected override void SubscribeEvents()
//        {
//            Plugin.Instance?.DebugLog("ReinforcementCall.SubscribeEvents called");
//            // Using UsingItem event since InteractingRadio is not available
//            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
//            base.SubscribeEvents();
//            Plugin.Instance?.DebugLog("ReinforcementCall event subscriptions completed");
//        }

//        protected override void UnsubscribeEvents()
//        {
//            Plugin.Instance?.DebugLog("ReinforcementCall.UnsubscribeEvents called");
//            // Using UsingItem event since InteractingRadio is not available
//            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
//            base.UnsubscribeEvents();
//            Plugin.Instance?.DebugLog("ReinforcementCall event unsubscriptions completed");
//        }

//        // Using UsingItemEventArgs since InteractingRadioEventArgs is not available
//        private void OnUsingItem(UsingItemEventArgs ev)
//        {
//            Plugin.Instance?.DebugLog($"ReinforcementCall.OnUsingItem called for player: {ev.Player?.Nickname ?? "unknown"}");

//            try
//            {
//                // Verify that the item the player is using is this custom item
//                if (!Check(ev.Player.CurrentItem))
//                {
//                    Plugin.Instance?.DebugLog("OnUsingItem: Item check failed, not our custom item");
//                    return;
//                }

//                Plugin.Instance?.DebugLog("OnUsingItem: Item check passed, processing reinforcement call");

//                // Check if player exists
//                if (ev.Player == null)
//                {
//                    Log.Error("ReinforcementCall: Player is null in OnUsingItem");
//                    return;
//                }

//                // Get player's unique ID for cooldown tracking
//                string userId = ev.Player.UserId;
//                Plugin.Instance?.DebugLog($"OnUsingItem: Retrieved player UserId: {userId}");

//                // Check if the player is on cooldown
//                if (cooldowns.TryGetValue(userId, out DateTime lastUsed))
//                {
//                    double secondsSinceLastUse = (DateTime.UtcNow - lastUsed).TotalSeconds;
//                    Plugin.Instance?.DebugLog($"OnUsingItem: Player has used this item before, last use was {secondsSinceLastUse:F1} seconds ago (cooldown: {_config.Cooldown} seconds)");

//                    if (secondsSinceLastUse < _config.Cooldown)
//                    {
//                        int remainingSeconds = (int)(_config.Cooldown - secondsSinceLastUse);
//                        Plugin.Instance?.DebugLog($"OnUsingItem: Player is on cooldown, {remainingSeconds} seconds remaining");
//                        ev.Player.ShowHint(string.Format(_config.CooldownMessage, remainingSeconds), 5f);
//                        return;
//                    }
//                }

//                // Check the player's team
//                bool isChaosTeam = false;

//                // Using Role.Team to identify player's team
//                switch (ev.Player.Role.Team)
//                {
//                    case Team.ChaosInsurgency:
//                        isChaosTeam = true;
//                        Plugin.Instance?.DebugLog($"OnUsingItem: Player {ev.Player.Nickname} is Chaos Insurgency");
//                        break;
//                    case Team.FoundationForces:
//                    case Team.Scientists:
//                        Plugin.Instance?.DebugLog($"OnUsingItem: Player {ev.Player.Nickname} is MTF/Facility Personnel");
//                        break;
//                    default:
//                        // Player is not on a team that can call reinforcements
//                        Plugin.Instance?.DebugLog($"OnUsingItem: Player {ev.Player.Nickname} has no team to call reinforcements for");
//                        ev.Player.ShowHint(_config.NoTeamMessage, 5f);
//                        return;
//                }

//                // Update cooldown for player
//                cooldowns[userId] = DateTime.UtcNow;
//                Plugin.Instance?.DebugLog($"OnUsingItem: Updated cooldown timestamp for player {ev.Player.Nickname}");

//                // Remove the item from inventory
//                Plugin.Instance?.DebugLog($"OnUsingItem: Removing item from player {ev.Player.Nickname}'s inventory");
//                ev.Player.RemoveItem(ev.Player.CurrentItem);

//                // Display message and schedule reinforcements
//                string message;
//                // For Chaos Insurgency
//                if (isChaosTeam)
//                {
//                    message = string.Format(_config.ChaosCallMessage, (int)_config.ArrivalTime);
//                    ev.Player.ShowHint(message, 5f);

//                    // Schedule Chaos reinforcements
//                    Plugin.Instance?.DebugLog($"OnUsingItem: Scheduling Chaos reinforcements in {_config.ArrivalTime} seconds");
//                    Map.Broadcast(6, "<color=#00AA00>CI reinforcements are arriving momentarily</color>");
//                    Timing.CallDelayed(_config.ArrivalTime, () =>
//                    {
//                        Plugin.Instance?.DebugLog($"Spawning {_config.ChaosUnitCount} Chaos reinforcements");
//                        // Use the correct method signature
//                        Respawn.ForceWave(Faction.FoundationEnemy);

//                        // Broadcast to all players that reinforcements have arrived
//                        Map.Broadcast(10, "<color=#00AA00>Chaos Insurgency reinforcements have arrived!</color>");
//                    });
//                }
//                else // MTF team
//                {
//                    message = string.Format(_config.MtfCallMessage, (int)_config.ArrivalTime);
//                    ev.Player.ShowHint(message, 5f);
//                    Map.Broadcast(6, "<color=#0000FF>MTF reinforcements are arriving momentarily</color>");
//                    // Schedule MTF reinforcements
//                    Plugin.Instance?.DebugLog($"OnUsingItem: Scheduling MTF reinforcements in {_config.ArrivalTime} seconds");
//                    Timing.CallDelayed(_config.ArrivalTime, () =>
//                    {
//                        Plugin.Instance?.DebugLog($"Spawning {_config.MtfUnitCount} MTF reinforcements");
//                        // Use the correct method signature
//                        Respawn.ForceWave(Faction.FoundationStaff);

//                        // Broadcast to all players that reinforcements have arrived
//                        Map.Broadcast(10, "<color=#0000FF>MTF reinforcements have arrived!</color>");
//                    });
//                }

//                Plugin.Instance?.DebugLog($"OnUsingItem: Reinforcement call processing complete for player {ev.Player.Nickname}");
//            }
//            catch (Exception ex)
//            {
//                Log.Error($"ReinforcementCall: Error in OnUsingItem: {ex.Message}\n{ex.StackTrace}");
//                Plugin.Instance?.DebugLog($"OnUsingItem: Exception caught: {ex.Message}\n{ex.StackTrace}");
//            }
//        }
//    }
//}
