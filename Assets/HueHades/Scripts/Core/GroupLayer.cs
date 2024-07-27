using HueHades.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static HueHades.Core.ImageLayer;

namespace HueHades.Core
{
    public class GroupLayer : LayerBase, ILayerContainer
    {

        private List<LayerBase> _layers = new List<LayerBase>();

        public GroupLayer(int2 dimensions, RenderTextureFormat format) : base(dimensions, format)
        {
        }

        public override ReusableTexture Texture => PreviewTexture;


        private bool _isDirty;
        public bool IsDirty { get => _isDirty; set { _isDirty = value; if (value) LayerChanged?.Invoke(); } }

        public List<LayerBase> Layers => _layers;
        public ReusableTexture PreviewTexture { get { if (IsDirty) this.RenderPreview(); return renderTexture; } }
        public Action PreviewChanged { get => _previewChanged; set { _previewChanged = value; } }

        private Action _previewChanged;



        internal override void SetDimensions(int2 dimensions, Action<ResizeLayerEventArgs> onResizeMethod)
        {
            _dimensions = dimensions;
            var oldTexture = renderTexture;
            renderTexture = new ReusableTexture(_dimensions.x, _dimensions.y, oldTexture.format, 0);
            

            //set dimensions on sublayers


            oldTexture.Dispose();
        }
    }
}