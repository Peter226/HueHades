using HueHades.Core;
using HueHades.Tools;
using HueHades.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HueHades.Tools
{

    public class EraserImageTool : ImageTool
    {
        private ImageCanvas _paintCanvas;
        private ImageLayer _paintLayer;
        private Vector2 _lastPoint;
        private float _lastPressure;
        private float _lastTilt;
        private float _leftOverLength;
        private float _paintInterval = 1.0f;
        private List<PaintPoint> paintPoints = new List<PaintPoint>();
        private EraserToolContext _toolContext;

        private static Texture Icon;


        private struct PaintPoint
        {
            public Vector2 position;
            public float radius;
            public float pressure;
        }

        public override Texture GetIcon()
        {
            if (Icon == null)
            {
                Icon = Resources.Load<Texture2D>("Icons/EraserIcon");
            }
            return Icon;
        }




        protected override void OnBeginUse(IToolContext toolContext, ImageCanvas canvas, int layer, Vector2 startPoint, float startPressure, float startTilt)
        {
            _paintCanvas = canvas;
            _paintLayer = _paintCanvas.GetLayer(layer);
            _lastPoint = startPoint;
            _lastPressure = startPressure;
            _lastTilt = startTilt;
            _leftOverLength = 0.0f;
            _toolContext = (EraserToolContext)toolContext;
        }

        protected override void OnUseUpdate(Vector2 currentPoint, float currentPressure, float currentTilt)
        {

            float distance = Vector2.Distance(currentPoint, _lastPoint);
            float travelAmount = distance + _leftOverLength;
            float pathDistance = -_leftOverLength;


            Rect bounds = new Rect(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);



            while (travelAmount >= _paintInterval)
            {
                pathDistance += _paintInterval;
                float pathTime = pathDistance / distance;
                var point = Vector2.Lerp(_lastPoint, currentPoint, pathTime);
                var pressure = Mathf.Lerp(_lastPressure, currentPressure, pathTime);

                var paintPoint = new PaintPoint()
                {
                    position = point,
                    radius = 100 * pressure * pressure + 1,
                    pressure = pressure
                };

                bounds.min = Vector2.Min(bounds.min, paintPoint.position - new Vector2(paintPoint.radius, paintPoint.radius) * 0.5f);
                bounds.max = Vector2.Max(bounds.max, paintPoint.position + new Vector2(paintPoint.radius, paintPoint.radius) * 0.5f);

                paintPoints.Add(paintPoint);

                travelAmount -= _paintInterval;
            }

            if (paintPoints.Count > 0)
            {
                int bufferWidth = Mathf.RoundToInt(bounds.width);
                int bufferHeight = Mathf.RoundToInt(bounds.height);
                int bufferStartX = Mathf.RoundToInt(bounds.min.x);
                int bufferStartY = Mathf.RoundToInt(bounds.min.y);

                var paintBufferA = RenderTextureUtilities.GetTemporary(bufferWidth, bufferHeight, _paintCanvas.Format, out int bufferSizeA);
                var paintBufferB = RenderTextureUtilities.GetTemporary(bufferWidth, bufferHeight, _paintCanvas.Format, out int bufferSizeB);
                var paintBufferC = RenderTextureUtilities.GetTemporary(bufferWidth, bufferHeight, _paintCanvas.Format, out int bufferSizeC);
                RenderTextureUtilities.ClearTexture(paintBufferA, Color.clear);
                RenderTextureUtilities.ClearTexture(paintBufferB, Color.clear);

                for (int i = 0; i < paintPoints.Count; i++)
                {
                    var point = paintPoints[i];
                    int pointWidth = Mathf.RoundToInt(point.radius);
                    int pointHeight = Mathf.RoundToInt(point.radius);
                    int pointStartX = Mathf.RoundToInt(point.position.x - point.radius * 0.5f) - bufferStartX;
                    int pointStartY = Mathf.RoundToInt(point.position.y - point.radius * 0.5f) - bufferStartY;


                    var pointBuffer = RenderTextureUtilities.GetTemporary(pointWidth, pointHeight, _paintCanvas.Format, out int pointBufferSize);
                    var color = Color.blue;
                    /*if (i == 0)
                    {
                        color = Color.green;
                    }
                    if (i == paintPoints.Count - 1)
                    {
                        color = Color.blue;
                    }*/
                    color.a *= point.pressure * point.pressure * 0.5f;
                    RenderTextureUtilities.ClearTexture(pointBuffer, color);
                    RenderTextureUtilities.CopyTexture(pointBuffer, 0, 0, pointWidth, pointHeight, paintBufferA, pointStartX, pointStartY);
                    RenderTextureUtilities.ReleaseTemporary(pointBuffer);
                    RenderTextureUtilities.LayerImage(paintBufferB, paintBufferA, paintBufferC, Common.ColorBlendMode.Add);
                    RenderTextureUtilities.ClearTexture(paintBufferA, Color.clear);
                    var oldBufferB = paintBufferB;
                    paintBufferB = paintBufferC;
                    paintBufferC = oldBufferB;
                }

                var sourceBuffer = RenderTextureUtilities.GetTemporary(bufferWidth, bufferHeight, _paintCanvas.Format, out int sourceBufferSize);
                var resultBuffer = RenderTextureUtilities.GetTemporary(bufferWidth, bufferHeight, _paintCanvas.Format, out int resultBufferSize);
                _paintLayer.GetOperatingCopy(sourceBuffer, bufferStartX, bufferStartY);
                RenderTextureUtilities.LayerImage(sourceBuffer, paintBufferB, resultBuffer, Common.ColorBlendMode.Default);
                _paintLayer.ApplyBufferArea(resultBuffer, bufferStartX, bufferStartY, 0, 0, bufferWidth, bufferHeight);
                RenderTextureUtilities.ReleaseTemporary(paintBufferA);
                RenderTextureUtilities.ReleaseTemporary(paintBufferB);
                RenderTextureUtilities.ReleaseTemporary(paintBufferC);
                RenderTextureUtilities.ReleaseTemporary(sourceBuffer);
                RenderTextureUtilities.ReleaseTemporary(resultBuffer);
            }


            _leftOverLength = travelAmount;
            _lastPoint = currentPoint;
            _lastPressure = currentPressure;
            _lastTilt = currentTilt;

            paintPoints.Clear();
        }

        protected override void OnEndUse(Vector2 endPoint, float endPressure, float endTilt)
        {

        }

        protected override void OnSelected()
        {

        }

        protected override void OnDeselected()
        {

        }
    }
}