using HueHades.Core;
using HueHades.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace HueHades.Core
{
    public class CanvasSelection : IDisposable
    {
        ReusableTexture _selectionTexture;
        private bool _hasSelection;
        private int2 _dimensions;
        private RenderTextureFormat _format;
        public ReusableTexture SelectionTexture { get { return _selectionTexture; } }

        public CanvasSelection(int2 dimensions, RenderTextureFormat format)
        {
            _format = format;
            _dimensions = dimensions;
            var rt = new RenderTexture(_dimensions.x, _dimensions.y, 0, _format);
            rt.enableRandomWrite = true;
            rt.wrapMode = TextureWrapMode.Repeat;
            rt.filterMode = FilterMode.Point;
            rt.Create();
            _selectionTexture = new ReusableTexture(rt, _dimensions.x, _dimensions.y);
            RenderTextureUtilities.ClearTexture(_selectionTexture, new Color(1, 1, 1, 0));
        }

        public void ApplySelection(ReusableTexture target)
        {
            if (!_hasSelection) return;
            RenderTextureUtilities.ApplyChannelMask(target, _selectionTexture);
        }

        public void Dispose()
        {
            _selectionTexture.Dispose();
        }
    }
}