using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ExtraWorldSizes.Core;

public sealed class ExtraConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;
    
    [DefaultValue(true)] public bool EnableTinyWorldSize { get; set; }
    [DefaultValue(true)] public bool EnableHugeWorldSize { get; set; }
}