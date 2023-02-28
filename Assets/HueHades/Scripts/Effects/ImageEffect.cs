using HueHades.Core;

namespace HueHades.Effects
{
    public abstract class ImageEffect
    {
        public abstract void StartEffect(ImageCanvas canvas, ImageEffectContext context);
    }

    public class ImageEffectContext
    {
        public readonly int selectedLayer;

        public ImageEffectContext(int selectedLayer)
        {
            this.selectedLayer = selectedLayer;
        }
    }
}