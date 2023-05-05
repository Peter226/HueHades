using HueHades.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionRectangleToolController : ToolController
{
    public override IToolContext CollectContext(HueHadesWindow window)
    {
        throw new System.NotImplementedException();
    }

    public override Texture GetIcon()
    {
        if (Icon == null)
        {
            Icon = Resources.Load<Texture2D>("Icons/SelectionRectangleIcon");
        }
        return Icon;
    }

    protected override ImageTool InitializeTool()
    {
        throw new System.NotImplementedException();
    }
}
