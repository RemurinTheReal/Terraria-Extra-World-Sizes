using Terraria;
using static Terraria.WorldGen;

namespace ExtraWorldSizes.Common;

public static class ExtraWorldGen
{
    #region Properties
    public const int WorldSizeHugeX = 10500;
    public const int WorldSizeHugeY = 3000;

    public const int WorldSizeTinyX = 2000;
    public const int WorldSizeTinyY = 900;
    #endregion

    public static void OnLoad()
    {
        On_WorldGen.GetWorldSize += On_GetWorldSize;
    }

    public static void OnUnload()
    {
        On_WorldGen.GetWorldSize -= On_GetWorldSize;
    }
    
    #region Hooks
    private static int On_GetWorldSize(On_WorldGen.orig_GetWorldSize orig)
    {
        return Main.maxTilesX switch
        {
            <= WorldSizeTinyX => -1,
            <= WorldSizeSmallX => 0,
            <= WorldSizeMediumX => 1,
            <= WorldSizeLargeX => 2,
            
            _ => 3
        };
    }
    #endregion
}