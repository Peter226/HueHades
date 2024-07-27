using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using HueHades.Utilities;
using HueHades.Common;
using System;
using static HueHades.Core.ImageLayer;
using System.Linq;

namespace HueHades.Core {
    public class ImageCanvas : ILayerContainer, IDisposable
    {
        
        /// <summary>
        /// The layers of the canvas (without the contents of the group canvases)
        /// </summary>
        public List<LayerBase> Layers => _layers;
        private List<LayerBase> _layers = new List<LayerBase>();
        
        /// <summary>
        /// All of the layers of the canvas, including the contents of group canvases
        /// </summary>
        internal List<LayerBase> GlobalLayerCollection { get => _globalLayerCollection; }
        private List<LayerBase> _globalLayerCollection = new List<LayerBase>();

        /// <summary>
        /// Called when the hierarchy of the canvas changes
        /// </summary>
        public Action HierarchyUpdated { get; set; }
        
        /// <summary>
        /// All of the layers rendered into a single image, usable for displaying the texture, or saving it
        /// </summary>
        public ReusableTexture PreviewTexture { get { if (IsDirty) this.RenderPreview(); return _previewTexture; } }
        private ReusableTexture _previewTexture;

        /// <summary>
        /// When set to true, the canvas will update it's preview render before returning it
        /// </summary>
        public bool IsDirty { get => _isDirty; set { _isDirty = value; } }
        private bool _isDirty;

        /// <summary>
        /// Returns the size of the canvas in pixels
        /// </summary>
        public int2 Dimensions { get { return _dimensions; } }
        private int2 _dimensions;

        /// <summary>
        /// Called when the dimensions of the canvas are changed
        /// </summary>
        public Action<int2> CanvasDimensionsChanged;

        private RenderTextureFormat _format;
        /// <summary>
        /// The format of the canvas indicating channels and channel size
        /// </summary>
        public RenderTextureFormat Format { get { return _format; } }

        /// <summary>
        /// Holds information about the selected area of the canvas
        /// </summary>
        public CanvasSelection Selection { get; private set; }

        /// <summary>
        /// Holds all the data about the canvas's history, also used for recording history entries
        /// (this is done manually when a change is made)
        /// </summary>
        public CanvasHistory History { get { return _canvasHistory; } }
        private CanvasHistory _canvasHistory;

        /// <summary>
        /// The tile mode of the canvas, indicating tiling for image operations. Currently this is also tied to TileDisplayMode
        /// </summary>
        public CanvasTileMode TileMode { get { return _tileMode; } set { bool changed = _tileMode != value; _tileMode = value; if (changed) { UpdateTileMode(); TileModeChanged?.Invoke(_tileMode); } } }
        private CanvasTileMode _tileMode;

        /// <summary>
        /// The tile mode of the canvas, indicating tiling for displaying the canvas on screen. Currently this is also tied to TileMode
        /// </summary>
        public CanvasTileMode TileDisplayMode { get { return _tileDisplayMode; } set { bool changed = _tileDisplayMode != value; _tileDisplayMode = value; if (changed) TileDisplayModeChanged?.Invoke(_tileMode); } }
        private CanvasTileMode _tileDisplayMode;

        /// <summary>
        /// Called when the tile mode of the canvas is changed by user or script
        /// </summary>
        public Action<CanvasTileMode> TileModeChanged;

        /// <summary>
        /// Called when the tile display mode of the canvas is changed by user or script
        /// </summary>
        public Action<CanvasTileMode> TileDisplayModeChanged;

        /// <summary>
        /// Called when the filtering display mode of the canvas is changed by user or script (wether the canvas's display is pixelated or uses bilinear interpolation)
        /// </summary>
        public Action<FilterMode> PreviewFilterModeChanged;

        /// <summary>
        /// The filter mode of the display of the canvas (pixelated or interpolated using bilinear interpolation)
        /// </summary>
        public FilterMode PreviewFilterMode { get { return _previewFilterMode; } set { bool changed = _previewFilterMode != value; _previewFilterMode = value; if(changed) UpdateFilterMode(); } }
        private FilterMode _previewFilterMode = FilterMode.Bilinear;

        /// <summary>
        /// Called when the preview of the canvas is re-rendered
        /// </summary>
        public Action PreviewChanged { get { return _previewChanged; } set { _previewChanged = value; } }
        private Action _previewChanged;

        /// <summary>
        /// The currently active layer used for image operations, selected by the user.
        /// </summary>
        public LayerBase ActiveLayer { get => _activeLayer; }
        private LayerBase _activeLayer;

        private List<LayerBase> _selectedLayers;

        /// <summary>
        /// Selected layers in canvas
        /// </summary>
        public IEnumerable<LayerBase> SelectedLayers { get => _selectedLayers; }

        /// <summary>
        /// Remove all layers from selection
        /// </summary>
        /// <param name="callNotifyEvents">Send events about change? Default is true</param>
        public void ClearSelectedLayers(bool callNotifyEvents = true)
        {
            if (_selectedLayers.Count > 0)
            {
                _selectedLayers.Clear();
                if(callNotifyEvents) LayerSelectionChanged?.Invoke();
                _activeLayer = null;
            }
        }

        /// <summary>
        /// Select layer in canvas
        /// </summary>
        /// <param name="layer">Layer to select</param>
        /// <param name="keepOtherSelections">Keep existing layers in selection</param>
        /// <param name="callNotifyEvents">Send events about change? Default is true</param>
        public void SelectLayer(LayerBase layer, bool keepOtherSelections = false, bool callNotifyEvents = true)
        {
            if (layer == null)
            {
                if(!keepOtherSelections) ClearSelectedLayers(callNotifyEvents);
                return;
            }
            if (!_layers.Contains(layer)) {
                var container = layer.ContainerIn;
                int maxDepth = short.MaxValue;
                int depth = 0;
                while (container != null && (container is GroupLayer))
                {
                    depth++;
                    if (depth > maxDepth)
                    {
                        Debug.Log($"Recursive layer group detected, ignoring selection request of layer {layer} for canvas {this}");
                        return;
                    }
                    if (container == null)
                    {
                        Debug.LogError($"Error adding layer to selection, Layer {layer} does not exist in canvas {this}");
                        return;
                    }
                    if (container is ImageCanvas)
                    {
                        if (container == this)
                        {
                            break;
                        }
                        else
                        {
                            Debug.LogError($"Error adding layer to selection, Layer {layer} does not exist in canvas {this}");
                            return;
                        }
                    }
                    if (container is GroupLayer)
                    {
                        container = ((GroupLayer)container).ContainerIn;
                    }
                }
            }
            if (_selectedLayers.Contains(layer) && keepOtherSelections) return;
            if (_selectedLayers.Count == 1 && _selectedLayers[0] == layer) return;

            if (!keepOtherSelections)
            {
                _selectedLayers.Clear();
            }
            _selectedLayers.Add(layer);

            _activeLayer = layer;
            if(callNotifyEvents) LayerSelectionChanged?.Invoke();
        }


        /// <summary>
        /// Select a range of layers, given the first and last layers in the layer range
        /// </summary>
        /// <param name="firstLayer">First layer in the range</param>
        /// <param name="lastLayer">Last layer in the range, this will become the new active layer</param>
        /// <param name="keepOtherSelections">Keep all previous layers in the selections?</param>
        /// <param name="callNotifyEvents">Send events about change? Default is true</param>
        public void SelectRange(LayerBase firstLayer, LayerBase lastLayer, bool keepOtherSelections = false, bool callNotifyEvents = true)
        {
            var firstIndex = _layers.IndexOf(firstLayer);
            var lastIndex = _layers.IndexOf(lastLayer);
            if (firstIndex < 0 || lastIndex < 0)
            {
                if (!keepOtherSelections) ClearSelectedLayers(callNotifyEvents);
                return;
            }

            if (!keepOtherSelections) _selectedLayers.Clear();


            if (lastIndex >= firstIndex)
            {
                for (int i = firstIndex;i <= lastIndex;i++)
                {
                    SelectLayer(_layers[i], true, false);
                }
            }
            else
            {
                for (int i = firstIndex; i >= lastIndex; i--)
                {
                    SelectLayer(_layers[i], true, false);
                }
            }

            _activeLayer = lastLayer;
            if (callNotifyEvents) LayerSelectionChanged?.Invoke();
        }


        /// <summary>
        /// Deselect a specific layer
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="callNotifyEvents"></param>
        public void DeselectLayer(LayerBase layer, bool callNotifyEvents = true)
        {
            if (_selectedLayers.Contains(layer))
            {
                if (_activeLayer == layer)
                {
                    _activeLayer = null;
                }
                _selectedLayers.Remove(layer);
                if(callNotifyEvents) LayerSelectionChanged?.Invoke();
            }
        }



        /// <summary>
        /// Called when layer selection is changed
        /// </summary>
        public Action LayerSelectionChanged;

        /// <summary>
        /// The save path of the image file, default is empty if unsaved
        /// </summary>
        public string FilePath { get { return _filePath; } set { _filePath = value; } }
        private string _filePath;

        /// <summary>
        /// The name of the image file, default is Image.png if unsaved
        /// </summary>
        public string FileName { get { return _fileName; } set { _fileName = value; FileNameChanged?.Invoke(_fileName); } }
        private string _fileName = "Image.png";

        /// <summary>
        /// Called when the file name of the image changes, used for window headers mostly
        /// </summary>
        public Action<string> FileNameChanged;

        /// <summary>
        /// Called when the canvas is disposed and is no longer in use. Used for undocking the window that hosts the preview of the canvas
        /// </summary>
        public Action DestroyedCanvas;

        /// <summary>
        /// Set the dimensions of the canvas, while being able to inject code into the layer's resizing process, allowing for custom resize logic. Used for rotating and scaling images.
        /// </summary>
        /// <param name="dimensions">The new size of the canvas</param>
        /// <param name="onLayerResizedMethod">The action called when a layer is resized</param>
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

        /// <summary>
        /// Updates the tile mode of the preview texture, to avoid artifacts when not tiling and using a bilinear filter
        /// (the wrapped pixels would show on the other end of the image). 
        /// </summary>
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

        /// <summary>
        /// Updates the filter mode of the image to reflect the filter mode supplied in the canvas
        /// </summary>
        void UpdateFilterMode()
        {
            _previewTexture.texture.filterMode = _previewFilterMode;
            PreviewFilterModeChanged?.Invoke(_previewFilterMode);
            this.RenderPreview();
        }

        /// <summary>
        /// Creates a new canvas using the suppied dimensions, format, and initial layer color
        /// </summary>
        /// <param name="dimensions">The size of the texture in pixels</param>
        /// <param name="format">Format of the texture</param>
        /// <param name="initialColor">Color of the first layer</param>
        public ImageCanvas(int2 dimensions, RenderTextureFormat format, Color initialColor)
        {
            _selectedLayers = new List<LayerBase>();
            _canvasHistory = new CanvasHistory(this);
            _dimensions = dimensions;
            _format = format;
            var initialLayer = this.AddLayer(0, 0, initialColor);
            SelectLayer(initialLayer);
            History.AddRecord(new NewLayerHistoryRecord(0, 0, ActiveLayer.GlobalIndex, initialColor));
            _previewTexture = new ReusableTexture(_dimensions.x, _dimensions.y,_format,0);
            Selection = new CanvasSelection(dimensions, format);
            this.RenderPreview();
        }

        /// <summary>
        /// Free resources used by the canvas
        /// </summary>
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
    }
}