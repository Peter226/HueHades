using HueHades.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MoveSelectionToolController : ToolController
{
    public override IToolContext CollectContext(HueHadesWindow window, PointerDownEvent pointerDownEvent)
    {
        throw new System.NotImplementedException();
    }

    public override Texture GetIcon()
    {
        if (Icon == null)
        {
            Icon = Resources.Load<Texture2D>("Icons/MoveSelectionIcon");
        }
        return Icon;
    }

    protected override ImageTool InitializeTool()
    {
        throw new System.NotImplementedException();
    }
}
