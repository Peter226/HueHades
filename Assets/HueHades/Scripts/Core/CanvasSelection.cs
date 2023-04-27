using HueHades.Core;
using HueHades.Utilities;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CanvasSelection
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
        _selectionTexture = RenderTextureUtilities.GetTemporary(_dimensions.x, _dimensions.y, _format);
        RenderTextureUtilities.ClearTexture(_selectionTexture, new Color(1,1,1,0));
    }

    public void ApplySelection(ReusableTexture target) {
        if (!_hasSelection) return;
        RenderTextureUtilities.ApplyChannelMask(target, _selectionTexture);
    }


}
