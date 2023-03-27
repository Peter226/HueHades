using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using HueHades.Utilities;

namespace HueHades.Core {
    public class ImageCanvas
    {
        private List<ImageLayer> _imageLayers = new List<ImageLayer>();
        private int2 _dimensions;
        private RenderTextureFormat _format;

        private RenderTexture _previewTexture;
        private CanvasHistory _canvasHistory;
        public CanvasSelection Selection { get; private set; }
        public CanvasHistory History { get { return _canvasHistory; } }

        public ImageCanvas(int2 dimensions, RenderTextureFormat format)
        {
            _dimensions = dimensions;
            _format = format;
            AddLayer(0);
            _previewTexture = new RenderTexture(_dimensions.x, _dimensions.y, 0, _format, 4);
            _previewTexture.wrapMode = TextureWrapMode.Repeat;
            _previewTexture.enableRandomWrite = true;
            _previewTexture.Create();
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


        public RenderTexture PreviewTexture { get { return _previewTexture; } }
        public int2 Dimensions { get { return _dimensions; } }
        public RenderTextureFormat Format { get { return _format; } }
        public int LayerCount { get { return _imageLayers.Count; } }
        public ImageLayer GetLayer(int index)
        {
            return _imageLayers[index];
        }
    }
}