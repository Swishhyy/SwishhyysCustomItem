namespace SCI
{
    using Exiled.API.Features;
    using Exiled.CustomItems.API;
    using SCI.Custom.MedicalItems;
    using System;

    public class Plugin : Plugin<Config>
    {
        public override string Name => "SCI";
        public override string Author => "Swishhyy";
        public override Version Version => new Version(2, 0, 0);

        // Properties to store the custom items
        private ExpiredSCP500Pills _expiredSCP500Pills;
        private AdrenalineSCP500Pills _adrenalineSCP500Pills;
        private SuicideSCP500Pills _suicideSCP500Pills;

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

            // Create the instances with their respective configs from the main config
            _expiredSCP500Pills = new ExpiredSCP500Pills(Config.ExpiredSCP500);
            _adrenalineSCP500Pills = new AdrenalineSCP500Pills(Config.AdrenalineSCP500);
            _suicideSCP500Pills = new SuicideSCP500Pills(Config.SuicideSCP500);

            // Register all custom items
            _expiredSCP500Pills.Register();
            _adrenalineSCP500Pills.Register();
            _suicideSCP500Pills.Register();

            Log.Debug($"Registered {Name} custom items: Expired SCP-500 Pills, Adrenaline Pills, Suicide Pills");
        }

        public override void OnDisabled()
        {
            // Unregister and clean up all custom items
            _expiredSCP500Pills?.Unregister();
            _adrenalineSCP500Pills?.Unregister();
            _suicideSCP500Pills?.Unregister();

            _expiredSCP500Pills = null;
            _adrenalineSCP500Pills = null;
            _suicideSCP500Pills = null;

            Log.Info($"{Name} has been disabled!");
        }
    }
}
