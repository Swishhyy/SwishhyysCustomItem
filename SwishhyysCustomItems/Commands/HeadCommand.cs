// Import the CommandSystem namespace which provides functionalities for handling commands.
using CommandSystem;
// Import the System namespace to use basic system types.
using System;

namespace SCI.Commands
{
    // Use the CommandHandler attribute to indicate that this command is handled by the RemoteAdminCommandHandler.
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    // Define the HeadCommand class that inherits from ParentCommand.
    // This serves as the root command for your custom items plugin.
    public class HeadCommand : ParentCommand
    {
        // Constructor for the HeadCommand class.
        // It calls LoadGeneratedCommands to register subcommands.
        public HeadCommand()
        {
            LoadGeneratedCommands();
        }

        // The main command string that players/admins will use.
        public override string Command { get; } = "sci";

        // Aliases for the command; an empty array is provided since there are no aliases.
        public override string[] Aliases { get; } = new string[] { };

        // A description of what the command does.
        public override string Description { get; } = "Root command for Swishhyys Custom Items.";

        // Method to load and register all subcommands under the HeadCommand.
        public override void LoadGeneratedCommands()
        {
            // Register the GrantItemCommand as a subcommand.
            RegisterCommand(new GrantItemCommand());
            // Additional subcommands can be registered in a similar fashion.
        }

        // This method is executed when the parent command is called without any subcommands.
        // It provides usage information to the user.
        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            // Set the response message to instruct the user how to use the command properly.
            response = "Usage: sci <subcommand> [arguments...]\nAvailable subcommands: give";
            // Return false to indicate that the command execution did not complete a specific action.
            return false;
        }
    }
}
