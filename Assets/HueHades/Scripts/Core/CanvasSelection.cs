using HueHades.Core;
using HueHades.Utilities;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CanvasSelection : MonoBehaviour
{

    RenderTexture _selectionTexture;
    private bool _hasSelection;
    private int2 _dimensions;
    private RenderTextureFormat _format;
    public RenderTexture SelectionTexture { get { return _selectionTexture; } }
    
    public CanvasSelection(int2 dimensions, RenderTextureFormat format)
    {
        _format = format;
        _dimensions = dimensions;
        _selectionTexture = new RenderTexture(_dimensions.x, _dimensions.y, 0, _format, 4);
        _selectionTexture.wrapMode = TextureWrapMode.Repeat;
        _selectionTexture.enableRandomWrite = true;
        _selectionTexture.Create();
        RenderTextureUtilities.ClearTexture(_selectionTexture, new Color(1,1,1,0));
    }

    public void ApplySelection(RenderTexture target) {
        if (!_hasSelection) return;
        RenderTextureUtilities.ApplyChannelMask(target, _selectionTexture);
    }


}
