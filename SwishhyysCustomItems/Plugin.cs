namespace SCI
{
    using Exiled.API.Features;
    using Exiled.CustomItems;
    using Exiled.CustomItems.API;
    using SCI.Custom.MedicalItems;
    using SCI.Custom.Config;
    using System;

    public class Plugin : Plugin<Config>
    {
        public override string Name => "SCI";
        public override string Author => "Swishhyy";
        public override Version Version => new Version(2, 0, 0);

        // Property to store the config
        public ExpiredSCP500PillsConfig ExpiredSCP500Configuration { get; private set; }

        private ExpiredSCP500Pills _expiredSCP500Pills;
        private AdrenalineSCP500Pills _adrenalineSCP500Pills;

        private readonly Version requiredExiledVersion = new Version(9, 5, 1);

        public override void OnEnabled()
        {
            if (Exiled.Loader.Loader.Version < requiredExiledVersion)
            {
                Log.Error($"{Name} requires Exiled version {requiredExiledVersion} or higher. Current version: {Exiled.Loader.Loader.Version}");
                return;
            }
            Log.Info($"{Name} has been enabled!");

            base.OnEnabled();

            // Initialize the specific config
            ExpiredSCP500Configuration = new ExpiredSCP500PillsConfig();

            // Create the instances with the config
            _adrenalineSCP500Pills = new AdrenalineSCP500Pills();
            _expiredSCP500Pills = new ExpiredSCP500Pills(ExpiredSCP500Configuration);

            // Register custom items
            _adrenalineSCP500Pills.Register();
            _expiredSCP500Pills.Register();
        }

        public override void OnDisabled()
        {
            // Simplified null checks using null-conditional operator (?.)
            _adrenalineSCP500Pills?.Unregister();
            _expiredSCP500Pills?.Unregister();

            _adrenalineSCP500Pills = null;
            _expiredSCP500Pills = null;
            ExpiredSCP500Configuration = null;

            Log.Info($"{Name} has been disabled!");
        }
    }
}