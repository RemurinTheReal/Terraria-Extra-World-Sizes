using System.ComponentModel;
using ExtraWorldSizes.Common;
using Terraria.ModLoader.Config;

namespace ExtraWorldSizes.Core;

public sealed class ExtraConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;
    
    [DefaultValue(true)] public bool EnableTinyWorldSize { get; set; }
    [DefaultValue(true)] public bool EnableHugeWorldSize { get; set; }
    
    [DefaultValue(WorldSizeID.Medium)] public WorldSizeID DefaultWorldSize { get; set; }
}