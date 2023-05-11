using HueHades.Utilities;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace HueHades.Core.Utilities
{
    public static class CanvasUtilities
    {
        public static void MirrorCanvas(ImageCanvas canvas, MirrorMode mirrorMode, bool addHistoryRecord = true)
        {
            var mirrorBuffer = RenderTextureUtilities.GetTemporary(canvas.Dimensions.x, canvas.Dimensions.y, canvas.Format);

            foreach (var layer in canvas.GetGlobalLayers())
            {
                RenderTextureUtilities.Sampling.Mirror(layer.Texture, mirrorBuffer, mirrorMode);
                RenderTextureUtilities.CopyTexture(mirrorBuffer, layer.Texture);
            }
            RenderTextureUtilities.ReleaseTemporary(mirrorBuffer);

            canvas.RenderPreview();

            if (addHistoryRecord)
            {
                canvas.History.AddRecord(new MirrorCanvasHistoryRecord(mirrorMode));
            }
        }

        public static void RotateCanvas(ImageCanvas canvas, RotateMode rotateMode, bool addHistoryRecord = true)
        {
           
            int2 canvasDimensions = canvas.Dimensions;
            int2 newCanvasDimensions = rotateMode == RotateMode.OneEighty ? canvasDimensions : new int2(canvasDimensions.y, canvasDimensions.x);

            if (math.all(canvasDimensions == newCanvasDimensions)) {

                var rotateBuffer = RenderTextureUtilities.GetTemporary(canvas.Dimensions.x, canvas.Dimensions.y, canvas.Format);
                foreach (var layer in canvas.GetGlobalLayers())
                {
                    RenderTextureUtilities.Sampling.Rotate(layer.Texture, rotateBuffer, rotateMode);
                    RenderTextureUtilities.CopyTexture(rotateBuffer, layer.Texture);
                    layer.LayerChanged?.Invoke();
                }
                RenderTextureUtilities.ReleaseTemporary(rotateBuffer);

                canvas.RenderPreview();
            }
            else
            {
                canvas.SetDimensions(newCanvasDimensions, (resizeEventArgs) => {
                    RenderTextureUtilities.Sampling.Rotate(resizeEventArgs.oldTexture, resizeEventArgs.newTexture, rotateMode);
                });
            }

            if (addHistoryRecord)
            {
                canvas.History.AddRecord(new RotateCanvasHistoryRecord(rotateMode));
            }
        }

        private class MirrorCanvasHistoryRecord : HistoryRecord
        {
            public override int MemoryConsumption => 1;
            private MirrorMode _mirrorMode;
            private const string nameHorizontal = "Mirror Horizontal";
            private const string nameVertical = "Mirror Vertical";
            public override string name => _mirrorMode == MirrorMode.Horizontal ? nameHorizontal : nameVertical;

            public MirrorCanvasHistoryRecord(MirrorMode mirrorMode)
            {
                _mirrorMode = mirrorMode;
            }

            public override void Dispose()
            {
                
            }

            protected override void OnRedo(ImageCanvas canvas)
            {
                MirrorCanvas(canvas,_mirrorMode, false);
            }

            protected override void OnUndo(ImageCanvas canvas)
            {
                MirrorCanvas(canvas, _mirrorMode, false);
            }
        }


        private class RotateCanvasHistoryRecord : HistoryRecord
        {
            public override int MemoryConsumption => 1;
            private RotateMode _rotateMode;

            private const string nameClockwise = "Rotate 90 Clockwise";
            private const string nameCounterClockwise = "Rotate 90 Counter Clockwise";
            private const string nameOneEighty = "Rotate 180";
            public override string name => _rotateMode == RotateMode.Clockwise ? nameClockwise : (_rotateMode == RotateMode.CounterClockwise ? nameCounterClockwise : nameOneEighty);

            public RotateCanvasHistoryRecord(RotateMode rotateMode)
            {
                _rotateMode = rotateMode;
            }

            public override void Dispose()
            {

            }

            protected override void OnRedo(ImageCanvas canvas)
            {
                RotateCanvas(canvas, _rotateMode, false);
            }

            protected override void OnUndo(ImageCanvas canvas)
            {
                var rotateMode = _rotateMode;
                switch (rotateMode)
                {
                    case RotateMode.Clockwise:
                        rotateMode = RotateMode.CounterClockwise;
                        break;
                    case RotateMode.CounterClockwise:
                        rotateMode = RotateMode.Clockwise;
                        break;
                    default:
                        rotateMode = RotateMode.OneEighty;
                        break;
                }
                RotateCanvas(canvas, rotateMode, false);
            }
        }


    }
}