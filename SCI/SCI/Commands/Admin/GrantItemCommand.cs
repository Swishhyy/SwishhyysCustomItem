// Using directives bring necessary namespaces into scope for commands, logging, custom item API, permissions, and basic system functionalities.
using CommandSystem;                                      // Provides command handling interfaces and classes.
using Exiled.API.Features;                                // Provides access to core features of the Exiled API such as Player management.
using Exiled.CustomItems.API.Features;                    // Provides classes for managing and giving custom items.
using Exiled.Permissions.Extensions;                      // Extension methods for checking permissions.
using System;                                             // Base system functionalities.

namespace SCI.Commands
{
    // The CommandHandler attribute tells the system that this command is handled by the RemoteAdminCommandHandler.
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    // GrantItemCommand implements the ICommand interface to allow custom item granting through a command.
    public class GrantItemCommand : ICommand
    {
        // The main command name that is used in the Remote Admin console.
        public string Command { get; } = "give";

        // Command aliases that can be used interchangeably with the main command.
        public string[] Aliases { get; } = ["gitem"];

        // A short description of what the command does.
        public string Description { get; } = "Grants a specified custom item to yourself.";

        // Execute method is called when the command is executed.
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Plugin.Instance?.DebugLog($"GrantItemCommand.Execute called by sender: {(sender is CommandSender cs ? cs.LogName : "unknown")}");
            Plugin.Instance?.DebugLog($"Arguments count: {arguments.Count}");

            try
            {
                // Retrieve the player object from the command sender.
                var player = Player.Get(sender);
                Plugin.Instance?.DebugLog($"Player retrieved: {player?.Nickname ?? "null"}");

                string userName = player?.Nickname ?? (sender is CommandSender cs1 ? cs1.LogName : "Console");
                string argString = arguments.Count > 0 ? string.Join(" ", arguments) : "";

                // Check whether the sender is a valid player. If not, return an error response.
                if (player == null)
                {
                    Plugin.Instance?.DebugLog("Player is null, command can only be used by a player");
                    response = "This command can only be used by a player.";

                    // Send webhook notification for failed command
                    Plugin.Instance?.WebhookService?.SendCommandUsageAsync(Command, userName, argString, false).GetAwaiter().GetResult();

                    return false;
                }

                // Verify that the sender has the required permission ("sci.admin").
                bool hasPermission = sender.CheckPermission("sci.admin");
                Plugin.Instance?.DebugLog($"Permission check (sci.admin): {hasPermission}");

                if (!hasPermission)
                {
                    Plugin.Instance?.DebugLog("Permission denied, returning error message");
                    response = "You do not have permission to use this command.";

                    // Send webhook notification for failed command
                    Plugin.Instance?.WebhookService?.SendCommandUsageAsync(Command, userName, argString, false).GetAwaiter().GetResult();

                    return false;
                }

                // Ensure at least one argument (the item ID) is provided.
                if (arguments.Count < 1)
                {
                    Plugin.Instance?.DebugLog("No arguments provided, showing usage");
                    response = "Usage: give <itemid>";

                    // Send webhook notification for failed command
                    Plugin.Instance?.WebhookService?.SendCommandUsageAsync(Command, userName, argString, false).GetAwaiter().GetResult();

                    return false;
                }

                // Try to parse the first argument as an unsigned integer to represent the item ID.
                string itemIdArg = arguments.At(0);
                Plugin.Instance?.DebugLog($"Attempting to parse item ID: {itemIdArg}");

                if (!uint.TryParse(itemIdArg, out uint itemId))
                {
                    Plugin.Instance?.DebugLog($"Failed to parse item ID: {itemIdArg} is not a valid unsigned integer");
                    response = "Invalid item ID.";

                    // Send webhook notification for failed command
                    Plugin.Instance?.WebhookService?.SendCommandUsageAsync(Command, userName, argString, false).GetAwaiter().GetResult();

                    return false;
                }

                Plugin.Instance?.DebugLog($"Item ID parsed successfully: {itemId}");

                // Try to retrieve the custom item using the parsed itemId.
                Plugin.Instance?.DebugLog($"Attempting to get custom item with ID: {itemId}");
                bool itemFound = CustomItem.TryGet(itemId, out var item);
                Plugin.Instance?.DebugLog($"Item found: {itemFound}, Item type: {item?.GetType().Name ?? "null"}");

                if (itemFound)
                {
                    // If the item is found, grant it to the player.
                    Plugin.Instance?.DebugLog($"Giving {item.GetType().Name} to player {player.Nickname}");
                    item.Give(player);
                    response = $"You have been given the item with ID {itemId}.";
                    Plugin.Instance?.DebugLog($"Item given successfully, response: {response}");

                    // Send webhook notification for successful command
                    Plugin.Instance?.WebhookService?.SendCommandUsageAsync(Command, userName, argString, true).GetAwaiter().GetResult();

                    return true;
                }
                else
                {
                    // If the item could not be found, return an error response.
                    response = $"Failed to retrieve the item with ID {itemId}.";
                    Plugin.Instance?.DebugLog($"Item not found, response: {response}");

                    // Send webhook notification for failed command
                    Plugin.Instance?.WebhookService?.SendCommandUsageAsync(Command, userName, argString, false).GetAwaiter().GetResult();

                    return false;
                }
            }
            catch (Exception ex)
            {
                // Log any unhandled exceptions
                Log.Error($"GrantItemCommand: Error in Execute: {ex.Message}\n{ex.StackTrace}");
                Plugin.Instance?.DebugLog($"Exception in GrantItemCommand.Execute: {ex.Message}\n{ex.StackTrace}");
                response = $"An error occurred while executing the command: {ex.Message}";

                // Send webhook notification for failed command
                try
                {
                    string userName = (sender is CommandSender cs2) ? cs2.LogName : "unknown";
                    string argString = arguments.Count > 0 ? string.Join(" ", arguments) : "";
                    Plugin.Instance?.WebhookService?.SendCommandUsageAsync(Command, userName, argString, false).GetAwaiter().GetResult();
                }
                catch { /* Ignore errors in webhook during exception handling */ }

                return false;
            }
        }
    }
}