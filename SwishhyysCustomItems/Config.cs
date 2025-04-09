namespace SCI
{
    using Exiled.API.Interfaces;
    using System.ComponentModel;
    public class Config : IConfig
    {
        [Description("Whether the plugin is enabled.")]
        public bool IsEnabled { get; set; } = true;
        [Description("Whether debug messages should be shown in the console.")]
        public bool Debug { get; set; } = false;

    }
}
