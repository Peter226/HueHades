using HueHades.Core;
using HueHades.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;

namespace HueHades.Tools
{
    public class SelectionEllipseImageTool : ImageTool
    {

        private int _lastX;
        private int _lastY;
        private int _lastWidth;
        private int _lastHeight;
        private float _startX;
        private float _startY;

        private ReusableTexture _copyBuffer;
        private ReusableTexture _targetBuffer;

        private ImageCanvas _canvas;
        SelectionEllipseToolContext _toolContext;

        protected override void OnBeginUse(IToolContext toolContext, ImageCanvas canvas, int layer, Vector2 startPoint, float startPressure, float startTilt)
        {
            _copyBuffer = RenderTextureUtilities.GetTemporary(canvas.Dimensions.x, canvas.Dimensions.y, canvas.Format);
            _targetBuffer = RenderTextureUtilities.GetTemporary(canvas.Dimensions.x, canvas.Dimensions.y, canvas.Format);
            RenderTextureUtilities.ClearTexture(_targetBuffer, Color.clear);
            
            _toolContext = (SelectionEllipseToolContext)toolContext;

            if (_toolContext.SelectMode == SelectMode.Fresh)
            {
                RenderTextureUtilities.ClearTexture(_copyBuffer, Color.clear);
                RenderTextureUtilities.ClearTexture(canvas.Selection.SelectionTexture, Color.clear);
            }
            else
            {
                RenderTextureUtilities.CopyTexture(canvas.Selection.SelectionTexture, _copyBuffer);
            }

            _startX = startPoint.x;
            _startY = startPoint.y;
            _lastX = Mathf.RoundToInt(startPoint.x);
            _lastY = Mathf.RoundToInt(startPoint.y);
            _lastWidth = 1;
            _lastHeight = 1;
            _canvas = canvas;
        }
        protected override void OnEndUse(Vector2 endPoint, float endPressure, float endTilt)
        {
            RenderTextureUtilities.ReleaseTemporary(_copyBuffer);
            RenderTextureUtilities.ReleaseTemporary(_targetBuffer);
            _canvas.Selection.SetDirty();
        }
        protected override void OnUseUpdate(Vector2 currentPoint, float currentPressure, float currentTilt)
        {
            int endX = Mathf.RoundToInt(currentPoint.x);
            int endY = Mathf.RoundToInt(currentPoint.y);

            int minX = Mathf.Min(Mathf.RoundToInt(_startX - 1), endX - 1);
            int minY = Mathf.Min(Mathf.RoundToInt(_startY - 1), endY - 1);

            int maxX = Mathf.Max(Mathf.RoundToInt(_startX + 1), endX + 1);
            int maxY = Mathf.Max(Mathf.RoundToInt(_startY + 1), endY + 1);

            int width = maxX - minX;
            int height = maxY - minY;


            Vector2 center = new Vector2((minX + maxX) * 0.5f - minX, (minY + maxY) * 0.5f - minY);
            Vector2 size = new Vector2(width * 0.5f, height * 0.5f);

            int minBoundsX = Mathf.Min(minX, _lastX);
            int minBoundsY = Mathf.Min(minY, _lastY);

            int maxBoundsX = Mathf.Max(maxX, _lastX + _lastWidth);
            int maxBoundsY = Mathf.Max(maxY, _lastY + _lastHeight);

            int maxWidth = maxBoundsX - minBoundsX;
            int maxheight = maxBoundsY - minBoundsY;

            if (width > 0 && height > 0) {
                TextureDebugger.BeginSession("Ellipse Selection");
                RenderTextureUtilities.ClearTexture(_targetBuffer, Color.clear);
                TextureDebugger.DebugRenderTexture(_targetBuffer, "Target");
                var shapeBuffer = RenderTextureUtilities.GetTemporary(width, height, _canvas.Format);
                var layeredBuffer = RenderTextureUtilities.GetTemporary(width, height, _canvas.Format);

                RenderTextureUtilities.CopyTexture(_copyBuffer, minX, minY, width, height, layeredBuffer, 0, 0, Common.CanvasTileMode.None, _canvas.TileMode);

                RenderTextureUtilities.Selection.DrawEllipse(shapeBuffer, center, size);
                TextureDebugger.DebugRenderTexture(shapeBuffer, "Shape buffer");
                RenderTextureUtilities.Selection.LayerSelectionArea(new Vector2(minX, minY), layeredBuffer, _targetBuffer, 0, 0, width, height, shapeBuffer, _toolContext.SelectMode, minX, minY, _canvas.TileMode);

                RenderTextureUtilities.ReleaseTemporary(layeredBuffer);
                
                TextureDebugger.DebugRenderTexture(_targetBuffer, "Layered onto target");
                RenderTextureUtilities.ReleaseTemporary(shapeBuffer);

                RenderTextureUtilities.CopyTexture(_copyBuffer, minBoundsX, minBoundsY, maxWidth, maxheight, _canvas.Selection.SelectionTexture, minBoundsX, minBoundsY,_canvas.TileMode, _canvas.TileMode);
                RenderTextureUtilities.CopyTexture(_targetBuffer, minX, minY, width, height, _canvas.Selection.SelectionTexture, minX,minY,_canvas.TileMode, _canvas.TileMode);
                _canvas.Selection.SetDirty();
                TextureDebugger.DebugRenderTexture(_canvas.Selection.SelectionTexture, "Canvas");
                TextureDebugger.EndSession("Ellipse Selection");
            }

            _lastX = minX;
            _lastY = minY;
            _lastWidth = width;
            _lastHeight = height;
        }
        protected override void OnDeselected()
        {

        }
        protected override void OnSelected()
        {
            
        }

    }
}