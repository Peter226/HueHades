using HueHades.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SelectionEllipseToolController : ToolController
{
    public override IToolContext CollectContext(HueHadesWindow window, PointerDownEvent pointerDownEvent)
    {
        var selectMode = SelectMode.Fresh;
        if (pointerDownEvent.shiftKey)
        {
            selectMode = SelectMode.Subtract;
        }
        else
        {
            if (pointerDownEvent.ctrlKey)
            {
                selectMode = SelectMode.Add;
            }
        }

        var context = new SelectionEllipseToolContext(selectMode);
        return context;
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
        return new SelectionEllipseImageTool();
    }
}
