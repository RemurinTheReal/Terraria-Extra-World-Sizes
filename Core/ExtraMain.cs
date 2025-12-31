using System.Reflection;
using ExtraWorldSizes.Common;
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
            const int width = ((ExtraWorldGen.WorldSizeHugeX - 1) / Main.sectionWidth + 1) * Main.sectionWidth;
            const int height = ((ExtraWorldGen.WorldSizeHugeY - 1) / Main.sectionHeight + 1) * Main.sectionHeight;

            SetWorldLimit(width, height);
        }

        public override void Unload()
        {
            const int width = ((WorldGen.WorldSizeLargeX - 1) / Main.sectionWidth + 1) * Main.sectionWidth;
            const int height = ((WorldGen.WorldSizeLargeY - 1) / Main.sectionHeight + 1) * Main.sectionHeight;

            SetWorldLimit(width, height);
        }
        
        private static void SetWorldLimit(int width, int height)
        {
            if (_tilemapConstructor == null) return;
            
            Main.Map = new WorldMap(width, height);
            Main.tile = (Tilemap)_tilemapConstructor.Invoke(new object[]
            {
                (ushort)width, 
                (ushort)height
            });
        }
    }
}
