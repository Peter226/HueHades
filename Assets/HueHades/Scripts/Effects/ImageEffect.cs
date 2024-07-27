using HueHades.Core;

namespace HueHades.Effects
{
    public abstract class ImageEffect
    {
        public virtual bool CanExecute(ImageCanvas canvas)
        {
            return canvas.ActiveLayer is ImageLayer;
        }
        public abstract void BeginEffect(ImageCanvas canvas);
        public abstract void RenderEffect();
        public abstract void ApplyEffect();
        public abstract void CancelEffect();
    }
}