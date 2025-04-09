// Import the CommandSystem namespace which provides functionalities for handling commands.
using CommandSystem;
// Import the System namespace to use basic system types.
using System;
// Import Exiled.API.Features for access to Log
using Exiled.API.Features;

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
            Plugin.Instance?.DebugLog("HeadCommand constructor called");
            LoadGeneratedCommands();
            Plugin.Instance?.DebugLog("HeadCommand constructor completed - subcommands loaded");
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
            Plugin.Instance?.DebugLog("HeadCommand.LoadGeneratedCommands called");

            // Register the GrantItemCommand as a subcommand.
            Plugin.Instance?.DebugLog("Registering GrantItemCommand as subcommand");
            RegisterCommand(new GrantItemCommand());
            Plugin.Instance?.DebugLog("GrantItemCommand registered successfully");

            // Additional subcommands can be registered in a similar fashion.
            Plugin.Instance?.DebugLog("HeadCommand.LoadGeneratedCommands completed");
        }

        // This method is executed when the parent command is called without any subcommands.
        // It provides usage information to the user.
        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Plugin.Instance?.DebugLog($"HeadCommand.ExecuteParent called by sender: {(sender is CommandSender cmdSender ? cmdSender.LogName : "unknown")}");
            Plugin.Instance?.DebugLog($"Arguments count: {arguments.Count}");

            // Set the response message to instruct the user how to use the command properly.
            response = "Usage: sci <subcommand> [arguments...]\nAvailable subcommands: give";
            Plugin.Instance?.DebugLog($"ExecuteParent response: {response}");

            // Return false to indicate that the command execution did not complete a specific action.
            Plugin.Instance?.DebugLog("HeadCommand.ExecuteParent returning false");
            return false;
        }
    }
}
