using HueHades.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionEllipseToolContext : IToolContext
{
    public SelectMode SelectMode { get; private set; }
    public SelectionEllipseToolContext(SelectMode selectMode)
    {
        SelectMode = selectMode;
    }
}
