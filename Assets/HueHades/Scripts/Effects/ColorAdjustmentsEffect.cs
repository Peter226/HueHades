using HueHades.Core;
using HueHades.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HueHades.Effects
{
    public class ColorAdjustmentsEffect : ImageEffect
    {
        public float Hue { get; set; }
        public float Saturation { get; set; }
        public float Brightness { get; set; }
        public float Contrast { get; set; }


        private ImageCanvas canvas;
        private ReusableTexture backupSnapshot;
        private ImageLayer selectedLayer;

        public override void BeginEffect(ImageCanvas canvas)
        {
            this.canvas = canvas;
            //backupSnapshot = RenderTextureUtilities.GetTemporary(canvas.Dimensions.x,canvas.Dimensions.y, canvas.Format);
            selectedLayer = canvas.GetLayer(canvas.SelectedLayer);
            //RenderTextureUtilities.CopyTexture(selectedLayer.get);
        }
        public override void ApplyEffect()
        {
            
        }

        public override void RenderEffect()
        {
            
        }

        public override void CancelEffect()
        {

            //RenderTextureUtilities.ReleaseTemporary(backupSnapshot);
        }
    }
}