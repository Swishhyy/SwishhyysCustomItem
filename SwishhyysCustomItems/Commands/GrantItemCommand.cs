using CommandSystem;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using Exiled.Permissions.Extensions;
using SCI.Custom.MedicalItems;
using System;

namespace SCI.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class GrantItemCommand : ICommand
    {
        public string Command { get; } = "giveadrenaline";

        public string[] Aliases { get; } = { "gad" };

        public string Description { get; } = "Grants the Adrenaline Pills item to yourself.";

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

            // Give the Adrenaline Pills item
            if (CustomItem.TryGet(101, out var item))
            {
                item.Give(player);
                response = "You have been given the Adrenaline Pills.";
                return true;
            }
            else
            {
                response = "Failed to retrieve the Adrenaline Pills item.";
                return false;
            }
        }
    }
}
