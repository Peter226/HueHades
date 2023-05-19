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


        public static void ResizeCanvas(ImageCanvas canvas, int2 newDimensions, bool addHistoryRecord = true)
        {

            int2 canvasDimensions = canvas.Dimensions;
            float2 size = newDimensions / (float2)canvasDimensions;
            float2 pivot = (float2)(canvasDimensions - 1) * 0.5f;
            float2 newpivot = (float2)(newDimensions - 1) * 0.5f;

            if (addHistoryRecord)
            {
                canvas.History.AddRecord(new ResizeCanvasHistoryRecord(canvas, newDimensions));
            }

            canvas.SetDimensions(newDimensions, (resizeEventArgs) => {
                RenderTextureUtilities.Sampling.Resample(resizeEventArgs.oldTexture, resizeEventArgs.newTexture, size, 0, pivot, newpivot, SamplerMode.Linear);
            });
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


        private class ResizeCanvasHistoryRecord : HistoryRecord
        {
            public override int MemoryConsumption => recordElements.Count * _oldDimensions.x * _oldDimensions.y * sizeof(float) * 4;
            public override string name => "Resize Image";

            private List<ReusableTexture> recordElements = new List<ReusableTexture>();

            int2 _oldDimensions;
            int2 _newDimensions;

            public ResizeCanvasHistoryRecord(ImageCanvas canvas, int2 newDimensions)
            {
                _oldDimensions = canvas.Dimensions;
                _newDimensions = newDimensions;


                foreach (var layer in canvas.GetGlobalLayers())
                {
                    var copy = RenderTextureUtilities.GetTemporary(_oldDimensions.x, _oldDimensions.y, canvas.Format);
                    RenderTextureUtilities.CopyTexture(layer.Texture, copy);
                    recordElements.Add(copy);
                }
            }

            public override void Dispose()
            {
                foreach (var element in recordElements)
                {
                    element.Dispose();
                }
            }

            protected override void OnRedo(ImageCanvas canvas)
            {
                ResizeCanvas(canvas, _newDimensions, false);
            }

            protected override void OnUndo(ImageCanvas canvas)
            {
                int counter = 1;
                ResizeCanvas(canvas, _oldDimensions, false);
                foreach (var copy in recordElements)
                {
                    var layer = canvas.GetLayerByGlobalID(counter);
                    RenderTextureUtilities.CopyTexture(copy, layer.Texture);
                    layer.LayerChanged?.Invoke();
                    counter++;
                }
            }
        }

    }
}