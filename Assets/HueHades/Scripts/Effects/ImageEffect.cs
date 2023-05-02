using HueHades.Core;

namespace HueHades.Effects
{
    public abstract class ImageEffect
    {
        public abstract void BeginEffect(ImageCanvas canvas);
        public abstract void RenderEffect();
        public abstract void ApplyEffect();
        public abstract void CancelEffect();
    }
}