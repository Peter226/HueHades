using HueHades.Core;
using HueHades.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class HistogramDisplayElement : HueHadesElement
    {

        private ReusableTexture _displayTexture;
        private int2 _displaySize;
        private Image _displayImage;

        private ImageCanvas _sourceCanvas;
        public ImageCanvas SourceCanvas { get { return _sourceCanvas; } set { SwitchCanvas(_sourceCanvas, value ); _sourceCanvas = value; RegenerateTexture(); } }

        private ComputeBuffer _histogramBuffer;
        private bool _hasBuffer;

        private uint[] _emptyDataBuffer = new uint[256 * 4];

        public HistogramDisplayElement(HueHadesWindow window) : base(window)
        {

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);


            _displayImage = new Image();
            hierarchy.Add(_displayImage);
            style.display = DisplayStyle.Flex;
            style.flexGrow = 1;
            
        }


        private void SwitchCanvas(ImageCanvas oldCanvas, ImageCanvas newCanvas)
        {
            if(oldCanvas is not null) oldCanvas.PreviewChanged -= RegenerateTexture;
            if (newCanvas is not null) newCanvas.PreviewChanged += RegenerateTexture;
        }



        private void RegenerateTexture()
        {
            if (_displayTexture == null) return;
            if (!_hasBuffer) return;
            if (SourceCanvas == null) return;

            _histogramBuffer.SetData(_emptyDataBuffer);

            RenderTextureUtilities.Statistics.CalculateHistogram(SourceCanvas.PreviewTexture, _histogramBuffer);
            var stats = RenderTextureUtilities.Statistics.GetHistogramStatistics(_histogramBuffer);
            RenderTextureUtilities.Statistics.DisplayHistogram(_histogramBuffer, _displayTexture, Math.Max(stats.Rmax, Math.Max(stats.Gmax, stats.Bmax)));
        }

        private void ResizeTexture(int2 size)
        {
            if (_displayTexture != null)
            {
                RenderTextureUtilities.ReleaseTemporary(_displayTexture);

            }
            _displayTexture = RenderTextureUtilities.GetTemporary(size.x, size.y, RenderTextureFormat.ARGB32);
            _displayImage.image = _displayTexture.texture;
            _displayImage.uv = new Rect(0, 0, size.x / (float)_displayTexture.actualWidth, size.y / (float)_displayTexture.actualHeight);
            _displaySize = size;
            RegenerateTexture();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            var size = evt.newRect.size;
            ResizeTexture(new int2(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y)));
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            if (_displayTexture != null) RenderTextureUtilities.ReleaseTemporary(_displayTexture);
            _displayTexture = null;
            _hasBuffer = false;
            _histogramBuffer.Release();
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            _histogramBuffer = new ComputeBuffer(256 * 4, sizeof(uint));
            _histogramBuffer.SetData(_emptyDataBuffer);
            _hasBuffer = true;
            if (_displaySize.x > 0 && _displaySize.y > 0)
            {
                ResizeTexture(_displaySize);
            }
        }
    }
}
