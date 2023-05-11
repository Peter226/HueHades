using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using HueHades.Utilities;
using HueHades.Common;
using System;
using static HueHades.Core.ImageLayer;

namespace HueHades.Core {
    public class ImageCanvas : ILayerContainer, IDisposable
    {
        private List<LayerBase> _layers = new List<LayerBase>();
        public List<LayerBase> Layers => _layers;


        private List<LayerBase> _globalLayerCollection = new List<LayerBase>();
        internal List<LayerBase> GlobalLayerCollection { get => _globalLayerCollection; }
        public Action HierarchyUpdated { get; set; }


        private int2 _dimensions;
        private RenderTextureFormat _format;
        private bool _isDirty;
        public bool IsDirty { get => _isDirty; set { _isDirty = value; } }

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

        private Action _previewChanged;
        public Action PreviewChanged { get { return _previewChanged; } set { _previewChanged = value; } }

        private LayerBase _selectedLayer;
        public LayerBase SelectedLayer { get => _selectedLayer; set { if (_selectedLayer == value) return; _selectedLayer = value; LayerSelected?.Invoke(value); } }

        public Action<LayerBase> LayerSelected;

        private string _fileName = "Image.png";
        private string _filePath;
        public string FilePath { get { return _filePath; } set { _filePath = value; } }
        public string FileName { get { return _fileName; } set { _fileName = value; FileNameChanged?.Invoke(_fileName); } }
        public Action<string> FileNameChanged;

        public Action DestroyedCanvas;


        public void SetDimensions(int2 dimensions, Action<ResizeLayerEventArgs> onLayerResizedMethod = null)
        {
            _dimensions = dimensions;
            foreach (var layer in Layers)
            {
                layer.SetDimensions(dimensions, onLayerResizedMethod);
            }

            _previewTexture?.Dispose();
            _previewTexture = new ReusableTexture(_dimensions.x, _dimensions.y, _format, 0);
            Selection?.Dispose();
            Selection = new CanvasSelection(dimensions, _format);
            CanvasDimensionsChanged?.Invoke(_dimensions);
            this.RenderPreview();
        }

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
            this.RenderPreview();
        }


        public ImageCanvas(int2 dimensions, RenderTextureFormat format, Color initialColor)
        {
            _canvasHistory = new CanvasHistory(this);
            _dimensions = dimensions;
            _format = format;
            SelectedLayer = this.AddLayer(0, 0, initialColor);
            History.AddRecord(new NewLayerHistoryRecord(0, 0, SelectedLayer.GlobalIndex, initialColor));
            _previewTexture = new ReusableTexture(_dimensions.x, _dimensions.y,_format,0);
            Selection = new CanvasSelection(dimensions, format);
            this.RenderPreview();
        }


        public void Dispose()
        {
            DestroyedCanvas?.Invoke();
            foreach (ImageLayer layer in Layers)
            {
                layer.Dispose();
            }
            Layers.Clear();
            History.Dispose();
            PreviewTexture.Dispose();
        }

        public ReusableTexture PreviewTexture { get { if (IsDirty) this.RenderPreview(); return _previewTexture; } }
        public int2 Dimensions { get { return _dimensions; } }

        public Action<int2> CanvasDimensionsChanged;

        public RenderTextureFormat Format { get { return _format; } }

    }
}