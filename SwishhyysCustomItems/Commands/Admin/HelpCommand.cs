using CommandSystem;
using System;
using System.Text;

namespace SCI.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class HelpCommand : ICommand
    {
        public string Command { get; } = "help";
        public string[] Aliases { get; } = ["h"];
        public string Description { get; } = "Shows help information for all available commands.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            StringBuilder helpText = new StringBuilder();
            helpText.AppendLine("Swishhyys Custom Items Commands:");
            helpText.AppendLine();

            // Main command help
            helpText.AppendLine("Main Command:");
            helpText.AppendLine("- sci: Root command for all custom item interactions");
            helpText.AppendLine();

            // Subcommands
            helpText.AppendLine("Available Subcommands:");
            helpText.AppendLine("- sci give <itemId>: Gives a custom item with the specified ID to yourself");
            helpText.AppendLine("- sci itemlist: Displays all available custom item IDs in the project");
            helpText.AppendLine("- sci help: Shows this help information");
            helpText.AppendLine();

            // Usage examples
            helpText.AppendLine("Examples:");
            helpText.AppendLine("- sci give 36");
            helpText.AppendLine("- sci itemlist");

            response = helpText.ToString();
            return true;
        }
    }
}
