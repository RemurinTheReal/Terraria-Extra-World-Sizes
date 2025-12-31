using System;
using System.Reflection;
using ExtraWorldSizes.Common;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Map;
using Terraria.ModLoader;

namespace ExtraWorldSizes.Core
{
	public sealed class ExtraMain : Mod
	{
        private static readonly ConstructorInfo _tilemapConstructor = typeof(Tilemap).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, new[] { typeof(ushort), typeof(ushort) });
        
        public override void Load()
        {
 
            const int sectionWidth = ((ExtraWorldGen.WorldSizeHugeX - 1) / Main.sectionWidth + 1) * Main.sectionWidth;
            const int sectionHeight = ((ExtraWorldGen.WorldSizeHugeY - 1) / Main.sectionHeight + 1) * Main.sectionHeight;

            var mapWidth = ExtraWorldGen.WorldSizeHugeX  / Main.textureMaxWidth + 2;
            var mapHeight = ExtraWorldGen.WorldSizeHugeY / Main.textureMaxHeight + 2;

            if (SetWorldLimit(sectionWidth, sectionHeight))
            {
                SetMapLimit(mapWidth, mapHeight);
                
                ExtraUIWorldCreation.OnLoad();
                ExtraWorldFileData.OnLoad();
                ExtraWorldGen.OnLoad();
            }
        }

        public override void Unload()
        {
            const int sectionWidth = ((WorldGen.WorldSizeLargeX - 1) / Main.sectionWidth + 1) * Main.sectionWidth;
            const int sectionHeight = ((WorldGen.WorldSizeLargeY - 1) / Main.sectionHeight + 1) * Main.sectionHeight;
            
            var mapWidth = WorldGen.WorldSizeLargeX / Main.textureMaxWidth + 2;
            var mapHeight = WorldGen.WorldSizeLargeY / Main.textureMaxHeight + 2;

            if (SetWorldLimit(sectionWidth, sectionHeight))
            {
                SetMapLimit(mapWidth, mapHeight);
                
                ExtraUIWorldCreation.OnUnload();
                ExtraWorldFileData.OnUnload();
                ExtraWorldGen.OnUnload();
            }
        }
        
        private static bool SetWorldLimit(int width, int height)
        {
            if (_tilemapConstructor == null) return false;
            
            Main.Map = new WorldMap(width, height);
            Main.tile = (Tilemap)_tilemapConstructor.Invoke(new object[]
            {
                (ushort)width, 
                (ushort)height
            });

            return true;
        }
        
        private static void SetMapLimit(int width, int height)
        {
            Main.mapTargetX = Math.Max(5, width);
            Main.mapTargetY = Math.Max(3, height);
            
            Main.instance.mapTarget = new RenderTarget2D[Main.mapTargetX, Main.mapTargetY];
            Main.initMap = new bool[Main.mapTargetX, Main.mapTargetY];
            
            Main.mapWasContentLost = new bool[Main.mapTargetX, Main.mapTargetY];
        }
    }
}
