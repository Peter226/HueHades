using HueHades.Core;
using HueHades.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HueHades.Tools
{
    public class MoveSelectedImageTool : ImageTool
    {
        ReusableTexture dummyBuffer;
        ReusableTexture dummyBuffer2;
        ImageCanvas canvas;
        ImageLayer layer;
        Vector2 startPoint;
        protected override void OnBeginUse(IToolContext toolContext, ImageCanvas canvas, int layer, Vector2 startPoint, float startPressure, float startTilt)
        {
            this.startPoint = startPoint;
            this.canvas = canvas;
            this.layer = canvas.GetLayer(layer);
            dummyBuffer = RenderTextureUtilities.GetTemporary(canvas.Dimensions.x, canvas.Dimensions.y, canvas.Format);
            dummyBuffer2 = RenderTextureUtilities.GetTemporary(Mathf.CeilToInt(canvas.Dimensions.y * 0.5f), Mathf.CeilToInt(canvas.Dimensions.x * 0.5f), canvas.Format);

            RenderTextureUtilities.CopyTexture(this.layer.Texture, dummyBuffer);



        }

        protected override void OnUseUpdate(Vector2 currentPoint, float currentPressure, float currentTilt)
        {

            
            RenderTextureUtilities.Sampling.Resample(dummyBuffer, dummyBuffer2, new Vector2(-0.5f, 0.25f), Vector2.Distance(startPoint, currentPoint), new Vector2(canvas.Dimensions.x * 0.5f, canvas.Dimensions.y * 0.5f), new Vector2(canvas.Dimensions.y * 0.25f, canvas.Dimensions.x * 0.25f), SamplerMode.Point);

            RenderTextureUtilities.CopyTexture(dummyBuffer2, layer.Texture);

            canvas.RenderPreview();
        }


        protected override void OnDeselected()
        {
            
        }

        protected override void OnEndUse(Vector2 endPoint, float endPressure, float endTilt)
        {
            RenderTextureUtilities.ReleaseTemporary(dummyBuffer);
            RenderTextureUtilities.ReleaseTemporary(dummyBuffer2);
        }

        protected override void OnSelected()
        {
            
        }

        
    }
}