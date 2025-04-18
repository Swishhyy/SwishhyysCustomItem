using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCI.Config
{
    public class RailgunConfig
    {
        [Description("Whether this item is enabled or not")]
        public bool IsEnabled { get; set; } = true;

        [Description("Maximum damage dealt by the railgun")]
        public float Damage { get; set; } = 85f;

        [Description("Maximum range of the railgun beam in meters")]
        public float Range { get; set; } = 50f;
    }

    public class GrenadeLauncherConfig
    {
        [Description("Whether this item is enabled or not")]
        public bool IsEnabled { get; set; } = true;

        [Description("Force applied to launched grenades")]
        public float LaunchForce { get; set; } = 20f;

        [Description("Amount of upward arc for the grenade (higher values = steeper arc)")]
        public float UpwardArc { get; set; } = 0.2f;

        [Description("Delay before the grenade explodes after being fired (seconds)")]
        public float ExplosionDelay { get; set; } = 5.0f;

    }
}
