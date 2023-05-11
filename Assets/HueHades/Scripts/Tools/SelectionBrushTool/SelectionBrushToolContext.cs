using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HueHades.Tools
{
    public class SelectionBrushToolContext : IToolContext
    {
        public BrushPreset BrushPreset { get; private set; }

        public SelectionBrushToolContext(BrushPreset brushPreset)
        {
            BrushPreset = brushPreset;
        }
    }
}
