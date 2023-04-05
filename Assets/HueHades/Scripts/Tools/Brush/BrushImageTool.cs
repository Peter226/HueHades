using HueHades.Core;
using HueHades.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HueHades.Tools
{
    public class BrushImageTool : ImageTool
    {
        private ImageCanvas _paintCanvas;
        private ImageLayer _paintLayer;
        private Vector2 _lastPoint;
        private float _lastPressure;
        private float _lastTilt;
        private float _leftOverLength;
        private float _paintInterval = 1.0f;
        private List<PaintPoint> paintPoints = new List<PaintPoint>();
        private BrushToolContext _toolContext;

        private static Texture Icon;

        private RenderTexture _layerOperatingCopyBuffer;
        private RenderTexture _layerCopyBuffer;
        private RenderTexture _paintBuffer;
        private ImageLayer.CopyHandle _layerOperatingCopy;

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
                Icon = Resources.Load<Texture2D>("Icons/BrushIcon");
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
            _toolContext = (BrushToolContext)toolContext;

            var canvasDimensions = _paintCanvas.Dimensions;
            _layerOperatingCopyBuffer = RenderTextureUtilities.GetTemporary(canvasDimensions.x, canvasDimensions.y, _paintCanvas.Format, out int availableSize);
            _layerCopyBuffer = RenderTextureUtilities.GetTemporary(canvasDimensions.x, canvasDimensions.y, _paintCanvas.Format, out availableSize);
            _paintBuffer = RenderTextureUtilities.GetTemporary(canvasDimensions.x, canvasDimensions.y, _paintCanvas.Format, out availableSize);
            RenderTextureUtilities.ClearTexture(_paintBuffer, Color.clear);
            _layerOperatingCopy = _paintLayer.GetOperatingCopy(_layerOperatingCopyBuffer);
            RenderTextureUtilities.CopyTexture(_layerOperatingCopyBuffer, _layerCopyBuffer);
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

                for (int i = 0;i < paintPoints.Count;i++)
                {
                    var point = paintPoints[i];
                    int pointWidth = Mathf.RoundToInt(point.radius);
                    int pointHeight = Mathf.RoundToInt(point.radius);
                    int pointStartX = Mathf.RoundToInt(point.position.x - point.radius * 0.5f);
                    int pointStartY = Mathf.RoundToInt(point.position.y - point.radius * 0.5f);

                    var pointBuffer = RenderTextureUtilities.GetTemporary(pointWidth, pointHeight, _paintCanvas.Format, out int pointBufferSize);
                    var paintCopyBuffer = RenderTextureUtilities.GetTemporary(pointWidth, pointHeight, _paintCanvas.Format, out pointBufferSize);
                    RenderTextureUtilities.CopyTexture(_paintBuffer, pointStartX, pointStartY, pointWidth, pointHeight, paintCopyBuffer);
                    var color = _toolContext.BrushPreset.color;
                    RenderTextureUtilities.ClearTexture(pointBuffer, color);
                    RenderTextureUtilities.LayerImageArea(paintCopyBuffer, _paintBuffer, 0, 0, pointWidth, pointHeight, pointBuffer, Common.ColorBlendMode.Default, pointStartX, pointStartY);
                    RenderTextureUtilities.ReleaseTemporary(pointBuffer);
                    RenderTextureUtilities.ReleaseTemporary(paintCopyBuffer);
                }

                RenderTextureUtilities.LayerImageArea(_layerCopyBuffer, _layerOperatingCopyBuffer, bufferStartX, bufferStartY, bufferWidth, bufferHeight, _paintBuffer, Common.ColorBlendMode.Default, bufferStartX, bufferStartY);
                _paintLayer.ApplyBufferArea(_layerOperatingCopyBuffer, bufferStartX, bufferStartY, bufferStartX, bufferStartY, bufferWidth, bufferHeight);
            }


            _leftOverLength = travelAmount;
            _lastPoint = currentPoint;
            _lastPressure = currentPressure;
            _lastTilt = currentTilt;

            paintPoints.Clear();
        }

        protected override void OnEndUse(Vector2 endPoint, float endPressure, float endTilt)
        {
            RenderTextureUtilities.ReleaseTemporary(_layerOperatingCopyBuffer);
            RenderTextureUtilities.ReleaseTemporary(_paintBuffer);
            RenderTextureUtilities.ReleaseTemporary(_layerCopyBuffer);
        }

        protected override void OnSelected()
        {
            
        }

        protected override void OnDeselected()
        {
            
        }
    }
}