using CommandSystem;
using System;

namespace SCI.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class HeadCommand : ParentCommand
    {
        public HeadCommand()
        {
            LoadGeneratedCommands();
        }

        public override string Command { get; } = "sci";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Root command for Swishhyys Custom Items.";

        public override void LoadGeneratedCommands()
        {
            // Register subcommands here
            RegisterCommand(new GrantItemCommand());
            // You can register more subcommands similarly
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            // This method is called if no subcommand is specified
            response = "Usage: sci <subcommand> [arguments...]\nAvailable subcommands: give";
            return false;
        }
    }
}
