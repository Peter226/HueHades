using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HueHades.Tools
{
    public class BrushToolContext : IToolContext
    {
        public BrushPreset BrushPreset { get; private set; }

        public BrushToolContext(BrushPreset brushPreset) {
            BrushPreset = brushPreset;
        }
    }
}