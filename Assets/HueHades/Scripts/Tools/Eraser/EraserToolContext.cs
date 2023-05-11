using HueHades.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EraserToolContext : IToolContext
{
    public BrushPreset BrushPreset { get; private set; }

    public EraserToolContext(BrushPreset brushPreset)
    {
        BrushPreset = brushPreset;
    }
}
