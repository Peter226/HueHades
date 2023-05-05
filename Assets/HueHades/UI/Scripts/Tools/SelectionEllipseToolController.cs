using HueHades.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionEllipseToolController : ToolController
{
    public override IToolContext CollectContext(HueHadesWindow window)
    {
        throw new System.NotImplementedException();
    }

    public override Texture GetIcon()
    {
        if (Icon == null)
        {
            Icon = Resources.Load<Texture2D>("Icons/SelectionEllipseIcon");
        }
        return Icon;
    }

    protected override ImageTool InitializeTool()
    {
        throw new System.NotImplementedException();
    }
}
