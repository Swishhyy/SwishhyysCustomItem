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
        public string[] Aliases { get; } = { "gitem" };

        // A short description of what the command does.
        public string Description { get; } = "Grants a specified custom item to yourself.";

        // Execute method is called when the command is executed.
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            // Retrieve the player object from the command sender.
            var player = Player.Get(sender);

            // Check whether the sender is a valid player. If not, return an error response.
            if (player == null)
            {
                response = "This command can only be used by a player.";
                return false;
            }

            // Verify that the sender has the required permission ("sci.admin").
            if (!sender.CheckPermission("sci.admin"))
            {
                response = "You do not have permission to use this command.";
                return false;
            }

            // Ensure at least one argument (the item ID) is provided.
            if (arguments.Count < 1)
            {
                response = "Usage: give <itemid>";
                return false;
            }

            // Try to parse the first argument as an unsigned integer to represent the item ID.
            if (!uint.TryParse(arguments.At(0), out uint itemId))
            {
                response = "Invalid item ID.";
                return false;
            }

            // Try to retrieve the custom item using the parsed itemId.
            if (CustomItem.TryGet(itemId, out var item))
            {
                // If the item is found, grant it to the player.
                item.Give(player);
                response = $"You have been given the item with ID {itemId}.";
                return true;
            }
            else
            {
                // If the item could not be found, return an error response.
                response = $"Failed to retrieve the item with ID {itemId}.";
                return false;
            }
        }
    }
}
