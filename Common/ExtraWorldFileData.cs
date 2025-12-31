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
    
    #region Hooks
    private static void On_SetWorldSize(On_WorldFileData.orig_SetWorldSize _, WorldFileData self, int x, int __)
    {
        var mod = ModContent.GetInstance<ExtraMain>();
        if (mod == null) return;
        
        self._worldSizeName = x switch
        {
            ExtraWorldGen.WorldSizeTinyX => Language.GetOrRegister(mod.GetLocalizationKey("UI.WorldSizeTiny"), () => "Tiny"),
            WorldGen.WorldSizeSmallX => Language.GetText("UI.WorldSizeSmall"),
            WorldGen.WorldSizeMediumX => Language.GetText("UI.WorldSizeMedium"),
            WorldGen.WorldSizeLargeX => Language.GetText("UI.WorldSizeLarge"),
            ExtraWorldGen.WorldSizeHugeX => Language.GetOrRegister(mod.GetLocalizationKey("UI.WorldSizeHuge"), () => "Huge"),
            
            _ => Language.GetText("UI.WorldSizeUnknown")
        };
    }
    #endregion
}