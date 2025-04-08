﻿namespace SwishhyysCustomItems
{
    using Exiled.API.Features;
    using System;
    public class Plugin : Plugin<Config>
    {
        public override string Name => "SwishhyysCustomItems";
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
        }
        public override void OnDisabled()
        {
            Log.Info($"{Name} has been disabled!");
        }
    }
}
