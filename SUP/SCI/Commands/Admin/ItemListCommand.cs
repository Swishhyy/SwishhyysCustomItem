using System;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;

namespace SCI.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class ItemListCommand : ICommand
    {
        public string Command { get; } = "itemlist";
        public string[] Aliases { get; } = ["items"];
        public string Description { get; } = "Displays all custom item IDs in the project.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            // Dynamically retrieve all custom items
            var items = CustomItem.Registered;

            if (!items.Any())
            {
                response = "No custom items found in the project.";
                return true;
            }

            // Build the response string with items sorted by ID (ascending order)
            response = "Custom Items in the Project:\n" +
                       string.Join("\n", items.OrderBy(item => item.Id)
                                              .Select(item => $"{item.Id}: {item.Name}"));

            return true;
        }
    }
}
