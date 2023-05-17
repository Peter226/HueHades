using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HueHades.Tools
{
    public class SelectionBrushToolContext : IToolContext
    {
        public BrushPreset BrushPreset { get; private set; }
        public SelectMode SelectMode { get; private set; }

        public SelectionBrushToolContext(BrushPreset brushPreset, SelectMode selectMode)
        {
            BrushPreset = brushPreset;
            SelectMode = selectMode;
        }
    }
}
