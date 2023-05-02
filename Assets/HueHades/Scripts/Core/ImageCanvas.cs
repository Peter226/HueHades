using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using HueHades.Utilities;
using HueHades.Common;
using System;

namespace HueHades.Core {
    public class ImageCanvas
    {
        private List<ImageLayer> _imageLayers = new List<ImageLayer>();
        private int2 _dimensions;
        private RenderTextureFormat _format;

        private ReusableTexture _previewTexture;
        private CanvasHistory _canvasHistory;
        public CanvasSelection Selection { get; private set; }
        public CanvasHistory History { get { return _canvasHistory; } }

        private CanvasTileMode _tileMode;
        private CanvasTileMode _tileDisplayMode;

        public CanvasTileMode TileMode { get { return _tileMode; } set { bool changed = _tileMode != value; _tileMode = value; if (changed) { UpdateTileMode(); TileModeChanged?.Invoke(_tileMode); } } }
        public CanvasTileMode TileDisplayMode { get { return _tileDisplayMode; } set { bool changed = _tileDisplayMode != value; _tileDisplayMode = value; if (changed) TileDisplayModeChanged?.Invoke(_tileMode); } }

        public Action<CanvasTileMode> TileModeChanged;
        public Action<CanvasTileMode> TileDisplayModeChanged;
        public Action<FilterMode> PreviewFilterModeChanged;

        private FilterMode _previewFilterMode = FilterMode.Bilinear;
        public FilterMode PreviewFilterMode { get { return _previewFilterMode; } set { bool changed = _previewFilterMode != value; _previewFilterMode = value; if(changed) UpdateFilterMode(); } }

        public int SelectedLayer { get; set; }

        void UpdateTileMode()
        {
            if (TileMode == CanvasTileMode.TileX || TileMode == CanvasTileMode.TileXY)
            {
                _previewTexture.texture.wrapModeU = TextureWrapMode.Repeat;
            }
            else
            {
                _previewTexture.texture.wrapModeU = TextureWrapMode.Clamp;
            }
            if (TileMode == CanvasTileMode.TileY || TileMode == CanvasTileMode.TileXY)
            {
                _previewTexture.texture.wrapModeV = TextureWrapMode.Repeat;
            }
            else
            {
                _previewTexture.texture.wrapModeV = TextureWrapMode.Clamp;
            }
        }

        void UpdateFilterMode()
        {
            _previewTexture.texture.filterMode = _previewFilterMode;
            PreviewFilterModeChanged?.Invoke(_previewFilterMode);
            RenderPreview();
        }


        public ImageCanvas(int2 dimensions, RenderTextureFormat format)
        {
            _dimensions = dimensions;
            _format = format;
            AddLayer(0);
            _previewTexture = new ReusableTexture(_dimensions.x, _dimensions.y,_format,0);
            Selection = new CanvasSelection(dimensions, format);
            RenderPreview();
        }

        public void AddLayer(int index)
        {
            var layer = new ImageLayer(_dimensions, _format);
            _imageLayers.Insert(index, layer);
            layer.LayerChanged += RenderPreview;
        }

        public void RenderPreview()
        {
            RenderTextureUtilities.CopyTexture(_imageLayers[0].Texture, _previewTexture);
        }


        public ReusableTexture PreviewTexture { get { return _previewTexture; } }
        public int2 Dimensions { get { return _dimensions; } }
        public RenderTextureFormat Format { get { return _format; } }
        public int LayerCount { get { return _imageLayers.Count; } }
        public ImageLayer GetLayer(int index)
        {
            return _imageLayers[index];
        }
    }
}