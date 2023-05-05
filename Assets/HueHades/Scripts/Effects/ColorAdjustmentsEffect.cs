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
        private ReusableTexture targetBuffer;
        private ImageLayer selectedLayer;
        private ImageLayer.CopyHandle copyHandle;

        public override void BeginEffect(ImageCanvas canvas)
        {
            this.canvas = canvas;
            backupSnapshot = RenderTextureUtilities.GetTemporary(canvas.Dimensions.x,canvas.Dimensions.y, canvas.Format);
            targetBuffer = RenderTextureUtilities.GetTemporary(canvas.Dimensions.x,canvas.Dimensions.y, canvas.Format);
            selectedLayer = canvas.GetLayer(canvas.SelectedLayer);
            copyHandle = selectedLayer.GetOperatingCopy(backupSnapshot);
        }
        public override void ApplyEffect()
        {
            RenderTextureUtilities.Effects.ColorAdjustments(backupSnapshot, targetBuffer, Hue, Saturation, Brightness, Contrast);
            selectedLayer.PasteOperatingCopy(targetBuffer,copyHandle);

            RenderTextureUtilities.ReleaseTemporary(backupSnapshot);
            RenderTextureUtilities.ReleaseTemporary(targetBuffer);
        }
    

        public override void RenderEffect()
        {
            RenderTextureUtilities.Effects.ColorAdjustments(backupSnapshot, targetBuffer, Hue, Saturation, Brightness, Contrast);
            selectedLayer.PasteOperatingCopy(targetBuffer, copyHandle);
        }

        public override void CancelEffect()
        {
            selectedLayer.PasteOperatingCopy(backupSnapshot, copyHandle);
            RenderTextureUtilities.ReleaseTemporary(backupSnapshot);
            RenderTextureUtilities.ReleaseTemporary(targetBuffer);
            Debug.Log("cancelled");
        }
    }
}