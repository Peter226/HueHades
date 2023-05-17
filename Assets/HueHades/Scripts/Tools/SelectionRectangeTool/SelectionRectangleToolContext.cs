using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HueHades.Tools
{
    public class SelectionRectangleToolContext : IToolContext
    {
        public SelectMode SelectMode { get; private set; }
        public SelectionRectangleToolContext(SelectMode selectMode)
        {
            SelectMode = selectMode;
        }
    }
}