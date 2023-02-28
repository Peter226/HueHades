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

        private RenderTexture _previewTexture;
        private CanvasHistory _canvasHistory;

        public CanvasHistory History { get { return _canvasHistory; } }


        public ImageCanvas(int2 dimensions, RenderTextureFormat format)
        {
            _dimensions = dimensions;
            AddLayer(0);
            _previewTexture = new RenderTexture(_dimensions.x, _dimensions.y, 0, format, 0);
            _previewTexture.enableRandomWrite = true;
            _previewTexture.Create();
            RenderTextureUtilities.ClearTexture(_previewTexture, Color.white);
            Debug.Log("preview generated");
        }

        public void AddLayer(int index)
        {
            var layer = new ImageLayer();
            _imageLayers.Insert(index, layer);
        }

        public RenderTexture RenderPreview()
        {



            return _previewTexture;
        }


        public RenderTexture PreviewTexture { get { return _previewTexture; } }
        public int2 Dimensions { get { return _dimensions; } }
        public int LayerCount { get { return _imageLayers.Count; } }
        public ImageLayer GetLayer(int index)
        {
            return _imageLayers[index];
        }
    }
}