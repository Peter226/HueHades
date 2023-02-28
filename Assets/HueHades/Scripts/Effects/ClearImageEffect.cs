using HueHades.Core;

namespace HueHades.Effects
{
    public class ClearImageEffect : ImageEffect
    {
        public override void StartEffect(ImageCanvas canvas, ImageEffectContext context)
        {
            var selectedLayer = canvas.GetLayer(context.selectedLayer);

        }
    }
}