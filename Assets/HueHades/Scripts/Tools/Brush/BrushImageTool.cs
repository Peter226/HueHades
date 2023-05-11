using HueHades.Common;
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
        private int _globalLayerIndex;
        private Vector2 _lastPoint;
        private Vector2 _lastUniquePoint;
        private Vector2 _lastDirection;
        private float _directionSmoothing = 0.025f;
        private float _lastPressure;
        private float _lastTilt;
        private float _leftOverLength;
        private float _paintInterval = 0.1f;
        private float _lastRadius = 1.0f;
        private List<PaintPoint> paintPoints = new List<PaintPoint>();
        private BrushToolContext _toolContext;
        private bool _firstMovement;

        private ReusableTexture _layerOperatingCopyBuffer;
        private ReusableTexture _layerCopyBuffer;
        private ReusableTexture _paintBuffer;
        private ImageLayer.CopyHandle _layerOperatingCopy;

        private static float SqrtTwoReciprocal = 1.0f / Mathf.Sqrt(2);
        private struct PaintPoint
        {
            public Vector2 position;
            public float rotation;
            public float radius;
            public float paintRadius;
            public float pressure;
        }


        protected override void OnBeginUse(IToolContext toolContext, ImageCanvas canvas, int globalLayerIndex, Vector2 startPoint, float startPressure, float startTilt)
        {
            _paintCanvas = canvas;
            _paintLayer = _paintCanvas.GetLayerByGlobalID(globalLayerIndex) as ImageLayer;
            _lastPoint = startPoint;
            _lastUniquePoint = startPoint;
            _lastPressure = startPressure;
            _lastTilt = startTilt;
            _lastDirection = Vector2.up;
            _leftOverLength = 0.0f;
            _toolContext = (BrushToolContext)toolContext;
            _globalLayerIndex = globalLayerIndex;
            _paintInterval = Mathf.Max(0.001f,_toolContext.BrushPreset.spacing);

            var canvasDimensions = _paintCanvas.Dimensions;
            _layerOperatingCopyBuffer = RenderTextureUtilities.GetTemporary(canvasDimensions.x, canvasDimensions.y, _paintCanvas.Format);
            _layerCopyBuffer = RenderTextureUtilities.GetTemporary(canvasDimensions.x, canvasDimensions.y, _paintCanvas.Format);
            _paintBuffer = RenderTextureUtilities.GetTemporary(canvasDimensions.x, canvasDimensions.y, _paintCanvas.Format);
            RenderTextureUtilities.ClearTexture(_paintBuffer, Color.clear);
            _layerOperatingCopy = _paintLayer.GetOperatingCopy(_layerOperatingCopyBuffer);
            RenderTextureUtilities.CopyTexture(_layerOperatingCopyBuffer, _layerCopyBuffer);
            _firstMovement = true;
            _lastRadius = 0;
        }

        protected override void OnUseUpdate(Vector2 currentPoint, float currentPressure, float currentTilt)
        {
            float distance = Vector2.Distance(currentPoint, _lastPoint);
            if (currentPoint != _lastPoint) _lastUniquePoint = _lastPoint;

            float travelAmount = distance + _leftOverLength;
            float pathDistance = -_leftOverLength;

            if (_firstMovement)
            {
                _lastDirection = (currentPoint - _lastUniquePoint).normalized;
                _firstMovement = false;
                return;
            }


            Vector2 direction = Vector2.Lerp(_lastDirection, (currentPoint - _lastUniquePoint).normalized, _directionSmoothing * travelAmount);

            Rect bounds = new Rect(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);

            ReusableTexture tempGradient = RenderTextureUtilities.GetTemporaryGradient(512, RenderTextureFormat.ARGBFloat);

            bool autoSpacing = _toolContext.BrushPreset.autoSpacing;

            while (travelAmount >= (autoSpacing ? _paintInterval * _lastRadius : _paintInterval))
            {
                pathDistance += (autoSpacing ? _paintInterval * _lastRadius : _paintInterval);
                float pathTime = pathDistance / distance;
                var point = Vector2.Lerp(_lastPoint, currentPoint, pathTime);
                
                var pressure = Mathf.Lerp(_lastPressure, currentPressure, pathTime);

                var rotation = _toolContext.BrushPreset.rotation;
                if (_toolContext.BrushPreset.autoRotation) rotation -= Vector2.SignedAngle(Vector2.up, Vector2.Lerp(_lastDirection, direction, pathTime)); ;
                
                var radius = Mathf.Max(0.5f, _toolContext.BrushPreset.size * pressure * pressure);
                _lastRadius = radius;
                var rad = Mathf.Deg2Rad * (rotation + 45);
                var maxRotationMargin = (Mathf.Max(Mathf.Abs(Mathf.Cos(rad)),Mathf.Abs(Mathf.Sin(rad))) / SqrtTwoReciprocal);
                var marginAppliedRadius = radius * maxRotationMargin;
                var paintPoint = new PaintPoint()
                {
                    position = point,
                    radius = radius,
                    pressure = pressure,
                    rotation = rotation,
                    paintRadius = marginAppliedRadius
                };

                bounds.min = Vector2.Min(bounds.min, paintPoint.position - new Vector2(marginAppliedRadius, marginAppliedRadius));
                bounds.max = Vector2.Max(bounds.max, paintPoint.position + new Vector2(marginAppliedRadius, marginAppliedRadius));

                paintPoints.Add(paintPoint);

                travelAmount -= (autoSpacing ? _paintInterval * _lastRadius : _paintInterval);
            }

            if (paintPoints.Count > 0)
            {
                int bufferWidth = Mathf.CeilToInt(bounds.width);
                int bufferHeight = Mathf.CeilToInt(bounds.height);
                int bufferStartX = Mathf.FloorToInt(bounds.min.x);
                int bufferStartY = Mathf.FloorToInt(bounds.min.y);

                for (int i = 0; i < paintPoints.Count; i++)
                {
                    if (i == paintPoints.Count - 1) TextureDebugger.BeginSession("Brush Image Tool Paint"); ;
                    var point = paintPoints[i];
                    int pointWidth = Mathf.CeilToInt(point.paintRadius * 2);
                    int pointHeight = Mathf.CeilToInt(point.paintRadius * 2);
                    int pointStartX = Mathf.FloorToInt(point.position.x - point.paintRadius);
                    int pointStartY = Mathf.FloorToInt(point.position.y - point.paintRadius);
                    if (pointWidth <= 0 || pointHeight <= 0) continue;
                    var pointBuffer = RenderTextureUtilities.GetTemporary(pointWidth, pointHeight, _paintCanvas.Format);
                    var paintCopyBuffer = RenderTextureUtilities.GetTemporary(pointWidth, pointHeight, _paintCanvas.Format);
                    TextureDebugger.DebugRenderTexture(paintCopyBuffer, "paint copy buffer fresh");
                    RenderTextureUtilities.CopyTexture(_paintBuffer, pointStartX, pointStartY, pointWidth, pointHeight, paintCopyBuffer, 0, 0, CanvasTileMode.None, _paintCanvas.TileMode);
                    TextureDebugger.DebugRenderTexture(paintCopyBuffer, "paint copy buffer copied");
                    var color = _toolContext.BrushPreset.color;
                    color.a = _toolContext.BrushPreset.opacity;
                    RenderTextureUtilities.Brushes.DrawBrush(pointBuffer, new Vector2(point.position.x - pointStartX, point.position.y - pointStartY), new Vector2(point.radius, point.radius * _toolContext.BrushPreset.sizeHeightRatio), point.rotation, _toolContext.BrushPreset.shape, color, tempGradient, _toolContext.BrushPreset.softness);
                    TextureDebugger.DebugRenderTexture(pointBuffer, "point buffer drawn brush");
                    RenderTextureUtilities.LayerImageArea(paintCopyBuffer, _paintBuffer, 0, 0, pointWidth, pointHeight, pointBuffer, Common.ColorBlendMode.Default, pointStartX, pointStartY, _paintCanvas.TileMode);
                    TextureDebugger.DebugRenderTexture(_paintBuffer, "paint buffer layered point buffer on top");
                    RenderTextureUtilities.ReleaseTemporary(pointBuffer);
                    RenderTextureUtilities.ReleaseTemporary(paintCopyBuffer);
                }

                RenderTextureUtilities.LayerImageArea(_layerCopyBuffer, _layerOperatingCopyBuffer, bufferStartX, bufferStartY, bufferWidth, bufferHeight, _paintBuffer, Common.ColorBlendMode.Default, bufferStartX, bufferStartY, _paintCanvas.TileMode, _paintCanvas.TileMode, _toolContext.BrushPreset.color.a);
                TextureDebugger.DebugRenderTexture(_layerOperatingCopyBuffer, "painting done, layering onto original copy");
                _paintLayer.ApplyBufferArea(_layerOperatingCopyBuffer, bufferStartX, bufferStartY, bufferStartX, bufferStartY, bufferWidth, bufferHeight, _paintCanvas.TileMode);
            }
            paintPoints.Clear();


            RenderTextureUtilities.ReleaseTemporaryGradient(tempGradient);

            _leftOverLength = travelAmount;
            _lastPoint = currentPoint;
            _lastPressure = currentPressure;
            _lastTilt = currentTilt;
            _lastDirection = direction;

            TextureDebugger.EndSession("Brush Image Tool Paint");
        }

        protected override void OnEndUse(Vector2 endPoint, float endPressure, float endTilt)
        {
            ModifyLayerHistoryRecord modifyLayerHistoryRecord = new ModifyLayerHistoryRecord(_globalLayerIndex, _layerCopyBuffer, _paintLayer.Texture, "Brush Stroke");
            _paintCanvas.History.AddRecord(modifyLayerHistoryRecord);

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