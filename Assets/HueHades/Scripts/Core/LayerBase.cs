using HueHades.Common;
using HueHades.Utilities;
using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using static HueHades.Core.ImageLayer;

namespace HueHades.Core
{
    public abstract class LayerBase : IDisposable
    {
        /// <summary>
        /// Texture used for drawing and layering the layer
        /// </summary>
        public abstract ReusableTexture Texture { get; }
        protected ReusableTexture renderTexture;

        /// <summary>
        /// Called when the layer's contents changed
        /// </summary>
        public Action LayerChanged;

        /// <summary>
        /// Width and height of the layer
        /// </summary>
        public int2 Dimensions => _dimensions;
        protected int2 _dimensions;

        /// <summary>
        /// Format of the layer's texture
        /// </summary>
        public RenderTextureFormat Format => _format;
        private RenderTextureFormat _format;

        /// <summary>
        /// Display name of the layer
        /// </summary>
        public string Name { get { return _name; } set { _name = value; } }
        private string _name = "Layer";

        /// <summary>
        /// The layer container the layer is in
        /// </summary>
        public ILayerContainer ContainerIn { get => _containerIn; internal set { _containerIn = value; } }
        private ILayerContainer _containerIn;
        
        /// <summary>
        /// The canvas (or root layer container) the layer is in
        /// </summary>
        public ImageCanvas CanvasIn { get => _canvasIn; internal set { _canvasIn = value; } }
        private ImageCanvas _canvasIn;

        /// <summary>
        /// Is the layer selected in the canvas
        /// </summary>
        public bool IsSelected { get => _canvasIn.SelectedLayers.Contains(this); }

        /// <summary>
        /// Is the layer the active layer in canvas
        /// </summary>
        public bool IsActive { get => _canvasIn.ActiveLayer == this; }

        /// <summary>
        /// Settings of the layer such as visibility and alpha inheritance
        /// </summary>
        public LayerSettings LayerSettings => _layerSettings;
        private LayerSettings _layerSettings;
        
        /// <summary>
        /// Set the layer's settings, and update the layer
        /// </summary>
        /// <param name="layerSettings"></param>
        /// <param name="createHistoryEntry"></param>
        public void SetLayerSettings(LayerSettings layerSettings, bool createHistoryEntry = true)
        {
            var oldLayerSettings = _layerSettings;
            _layerSettings = layerSettings;

            if (createHistoryEntry)
            {
                CanvasIn.History.AddRecord(new ModifyLayerSettingsHistoryRecord(GlobalIndex, oldLayerSettings, layerSettings));
            }
            LayerChanged?.Invoke();
        }


        /// <summary>
        /// Current index of the layer relative to it's container
        /// </summary>
        public int RelativeIndex { get; internal set; }

        /// <summary>
        /// Global index of the layer in the canvas hierarchy
        /// </summary>
        public int GlobalIndex { get; internal set; }

        /// <summary>
        /// Change the dimensions of the layer (only use in ImageCanvas!)
        /// </summary>
        /// <param name="dimensions"></param>
        /// <param name="onResizeMethod"></param>
        internal abstract void SetDimensions(int2 dimensions, Action<ResizeLayerEventArgs> onResizeMethod);

        public LayerBase(int2 dimensions, RenderTextureFormat format)
        {
            _dimensions = dimensions;
            _format = format;
            renderTexture = new ReusableTexture(_dimensions.x, _dimensions.y, _format, 0);
            SetLayerSettings(new LayerSettings { blendMode = ColorBlendMode.Default, inheritAlpha = false, invisible = false, opacity = 1.0f },false);
        }

        /// <summary>
        /// Called when the layer is disposed
        /// </summary>
        protected virtual void OnDispose() { }

        /// <summary>
        /// Release the resources used by the layer
        /// </summary>
        public void Dispose()
        {
            renderTexture.Dispose();
            OnDispose();
        }
    }

    /// <summary>
    /// Layer settings struct
    /// </summary>
    public struct LayerSettings
    {
        public bool inheritAlpha;
        public bool invisible;
        public ColorBlendMode blendMode;
        public float opacity;
    }
}