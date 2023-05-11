using HueHades.Core;
using HueHades.Effects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightToNormalEffect : ImageEffect
{
    public float NormalStrength { get; set; }

    private ImageCanvas canvas;
    private ReusableTexture backupSnapshot;
    private ReusableTexture targetBuffer;
    private ImageLayer selectedLayer;
    private ImageLayer.CopyHandle copyHandle;

    public override void ApplyEffect()
    {
        
    }

    public override void BeginEffect(ImageCanvas canvas)
    {
        
    }

    public override void CancelEffect()
    {
        
    }

    public override void RenderEffect()
    {
        
    }
}
