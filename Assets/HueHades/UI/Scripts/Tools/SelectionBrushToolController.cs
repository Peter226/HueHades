using HueHades.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SelectionBrushToolController : ToolController
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

        var context = new SelectionBrushToolContext(window.ToolSettings.GetActiveBrushPreset(), selectMode);
        return context;
    }

    public override Texture GetIcon()
    {
        if (Icon == null)
        {
            Icon = Resources.Load<Texture2D>("Icons/SelectionBrushIcon");
        }
        return Icon;
    }

    protected override ImageTool InitializeTool()
    {
        return new SelectionBrushImageTool();
    }
}
