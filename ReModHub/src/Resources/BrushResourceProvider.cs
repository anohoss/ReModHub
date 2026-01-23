using System.Runtime.CompilerServices;
using System.Windows.Markup;
using System.Windows.Media;

namespace ReModHub.Resources
{
    [MarkupExtensionReturnType(typeof(Brush))]
    internal class BrushResourceProvider : MarkupExtension
    {
        private ColorResourceType ResourceType { get; init; }

        public BrushResourceProvider(ColorResourceType resourceType) 
        { 
            ResourceType = resourceType;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return ColorResources.GetBrush(ResourceType);
        }
    }
}
