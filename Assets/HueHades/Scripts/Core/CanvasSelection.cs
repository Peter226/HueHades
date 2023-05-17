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
        private int _selectedArea = 0;
        private int4 _selectedAreaBounds;

        public int SelectedArea
        {
            get
            {
                if (IsDirty)
                {
                    ProcessDirty();
                }
                return _selectedArea;
            }
        }

        public int4 SelectedAreaBounds
        {
            get
            {
                if (IsDirty)
                {
                    ProcessDirty();
                }
                return _selectedAreaBounds;
            }
        }
    


        public bool IsDirty { get; private set; }

        public void SetDirty()
        {
            IsDirty = true;
        }

        private void ProcessDirty()
        {
            RenderTextureUtilities.Selection.GetSelectionStats(SelectionTexture, out int area, out int minX, out int minY, out int maxX, out int maxY);
            _selectedArea = area;
            _selectedAreaBounds = new int4(minX, minY, maxX, maxY);
            IsDirty = false;
        }

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

        public void Dispose()
        {
            _selectionTexture.Dispose();
        }
    }
}