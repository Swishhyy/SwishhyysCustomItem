using CommandSystem;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace SCI.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class GrantItemCommand : ICommand
    {
        public string Command { get; } = "give";

        public string[] Aliases { get; } = { "gitem" };

        public string Description { get; } = "Grants a specified custom item to yourself.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var player = Player.Get(sender);

            if (player == null)
            {
                response = "This command can only be used by a player.";
                return false;
            }

            // Check if the sender has the required permission
            if (!sender.CheckPermission("sci.admin"))
            {
                response = "You do not have permission to use this command.";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "Usage: give <itemid>";
                return false;
            }

            if (!uint.TryParse(arguments.At(0), out uint itemId))
            {
                response = "Invalid item ID.";
                return false;
            }

            // Give the specified custom item
            if (CustomItem.TryGet(itemId, out var item))
            {
                item.Give(player);
                response = $"You have been given the item with ID {itemId}.";
                return true;
            }
            else
            {
                response = $"Failed to retrieve the item with ID {itemId}.";
                return false;
            }
        }
    }
}
