using HueHades.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasSelection : MonoBehaviour
{

    RenderTexture _selection;
    private bool _hasSelection;
    public RenderTexture Selection { get { return _selection; } }
    
    public void ApplySelection(RenderTexture target) {
        if (!_hasSelection) return;
        RenderTextureUtilities.ApplyChannelMask(target, _selection);
    }


}
