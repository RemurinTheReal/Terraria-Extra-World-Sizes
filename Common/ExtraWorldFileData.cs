using ExtraWorldSizes.Core;
using Terraria;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExtraWorldSizes.Common;

public static class ExtraWorldFileData
{
    public static void OnLoad() => On_WorldFileData.SetWorldSize += On_SetWorldSize;
    public static void OnUnload() =>  On_WorldFileData.SetWorldSize -= On_SetWorldSize;
    
    private static void On_SetWorldSize(On_WorldFileData.orig_SetWorldSize orig, WorldFileData self, int x, int y)
    {
        var mod = ModContent.GetInstance<ExtraMain>();
        
        switch (x) {
            case ExtraWorldGen.WorldSizeTinyX:
                self._worldSizeName = Language.GetOrRegister(mod.GetLocalizationKey("UI.WorldSizeTiny"), () => "Tiny");
                break;
            case WorldGen.WorldSizeSmallX:
                self._worldSizeName = Language.GetText("UI.WorldSizeSmall");
                break;
            case WorldGen.WorldSizeMediumX:
                self._worldSizeName = Language.GetText("UI.WorldSizeMedium");
                break;
            case WorldGen.WorldSizeLargeX:
                self. _worldSizeName = Language.GetText("UI.WorldSizeLarge");
                break;
            case ExtraWorldGen.WorldSizeHugeX:
                self._worldSizeName = Language.GetOrRegister(mod.GetLocalizationKey("UI.WorldSizeHuge"), () => "Huge");
                break;
            default:
                self._worldSizeName = Language.GetText("UI.WorldSizeUnknown");
                break;
        }
    }
}