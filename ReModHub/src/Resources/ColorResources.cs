using System.Windows.Media;

namespace ReModHub.Resources
{
    internal class ColorResources
    {
        private static readonly Dictionary<ColorResourceType, Brush> ResourceTypeToBrush = [];

        private static readonly Dictionary<ColorResourceType, Color> ResourceTypeToColor = [];

        static ColorResources()
        {
            InitaializeColors();
            InitializeBrushes();
        }

        private static void InitaializeColors()
        {
            ResourceTypeToColor[ColorResourceType.Background] = Color.FromArgb(0xFF, 0xF3, 0xF3, 0xF3);
            ResourceTypeToColor[ColorResourceType.Foreground] = Color.FromArgb(0xFF, 0x2A, 0x2A, 0x2A);
            ResourceTypeToColor[ColorResourceType.Border] = Color.FromArgb(0xFF, 0xD1, 0xD1, 0xD1);
            ResourceTypeToColor[ColorResourceType.PrimaryText] = Color.FromArgb(0xFF, 0xB3, 0xB3, 0xB3);
        }

        private static void InitializeBrushes()
        {
            ResourceTypeToBrush[ColorResourceType.Background] = CreateBrushFromResourceType(ColorResourceType.Background);
            ResourceTypeToBrush[ColorResourceType.Foreground] = CreateBrushFromResourceType(ColorResourceType.Foreground);
            ResourceTypeToBrush[ColorResourceType.Border] = CreateBrushFromResourceType(ColorResourceType.Border);
            ResourceTypeToBrush[ColorResourceType.PrimaryText] = CreateBrushFromResourceType(ColorResourceType.PrimaryText);
        }

        private static Brush CreateBrushFromResourceType(ColorResourceType type)
        {
            return new SolidColorBrush(ResourceTypeToColor[type]);
        }

        public static Brush GetBrush(ColorResourceType type)
        {
            if (!ResourceTypeToBrush.ContainsKey(type))
            {
                throw new ArgumentException($"No brush found for ColorResourceType: {type}");
            }

            return ResourceTypeToBrush[type];
        }

        public static Color GetColor(ColorResourceType type)
        {
            if (!ResourceTypeToColor.ContainsKey(type))
            {
                throw new ArgumentException($"No color found for ColorResourceType: {type}");
            }

            return ResourceTypeToColor[type];
        }
    }
}
