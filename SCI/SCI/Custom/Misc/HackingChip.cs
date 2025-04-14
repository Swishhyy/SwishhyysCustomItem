using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCI.Custom.Misc
{
    public class HackingChip : CustomItem
    {
        public override string Name { get; set; } = "<color=#FF0000>Hacking Chip</color>";
        public override string Description { get; set; } = "This is a hacking chip, it can be used to mess with the facility";
        public override float Weight { get; set; } = 0.5f;
        public override ItemType Type { get; set; } = ItemType.KeycardChaosInsurgency;
        public override uint Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override SpawnProperties SpawnProperties { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
