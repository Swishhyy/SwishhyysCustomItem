namespace SCI
{
    using Exiled.API.Features;
    using SCI.Custom.MedicalItems;
    using System;

    public class Plugin : Plugin<Config>
    {
        public override string Name => "SCI";
        public override string Author => "Swishhyy";
        public override Version Version => new Version(1, 0, 0);

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
            // Register the AdrenalinePills custom item
            new AdrenalinePills().Register();
        }
        public override void OnDisabled()
        {
            Log.Info($"{Name} has been disabled!");
        }
    }
}
