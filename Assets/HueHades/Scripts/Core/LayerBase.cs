using HueHades.Common;
using HueHades.Utilities;
using System;
using Unity.Mathematics;
using UnityEngine;
using static HueHades.Core.ImageLayer;

namespace HueHades.Core
{
    public abstract class LayerBase : IDisposable
    {
        protected ReusableTexture renderTexture;
        public abstract ReusableTexture Texture { get; }

        public Action LayerChanged;

        private ColorBlendMode _layerBlendMode = ColorBlendMode.Default;
        private float _layerOpacity;
        public float LayerOpacity { get { return _layerOpacity; } set { if (_layerOpacity == value) return; _layerOpacity = value; LayerChanged?.Invoke(); } }
        public ColorBlendMode LayerBlendMode { get { return _layerBlendMode; } set { if (_layerBlendMode == value) return; _layerBlendMode = value; LayerChanged?.Invoke(); } }

        protected int2 _dimensions;
        private RenderTextureFormat _format;

        public RenderTextureFormat Format => _format;
        public int2 Dimensions => _dimensions;

        private string _name = "Layer";
        public string Name { get { return _name; } set { _name = value; } }


        private ILayerContainer _containerIn;
        private ImageCanvas _canvasIn;
        public ILayerContainer ContainerIn { get => _containerIn; internal set { _containerIn = value; } }
        public ImageCanvas CanvasIn { get => _canvasIn; internal set { _canvasIn = value; } }

        private LayerSettings _layerSettings;
        public LayerSettings LayerSettings => _layerSettings;
        
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


        internal abstract void SetDimensions(int2 dimensions, Action<ResizeLayerEventArgs> onResizeMethod);

        public LayerBase(int2 dimensions, RenderTextureFormat format)
        {
            _dimensions = dimensions;
            _format = format;
            renderTexture = new ReusableTexture(_dimensions.x, _dimensions.y, _format, 0);
        }

        protected virtual void OnDispose() { }

        public void Dispose()
        {
            renderTexture.Dispose();
            OnDispose();
        }

    }

    public struct LayerSettings
    {
        public bool inheritAlpha;
        public bool invisible;
    }
}