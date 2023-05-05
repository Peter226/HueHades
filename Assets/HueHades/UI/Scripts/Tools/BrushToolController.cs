using HueHades.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrushToolController : ToolController
{
    public override IToolContext CollectContext(HueHadesWindow window)
    {
        var context = new BrushToolContext(window.ToolSettings.GetActiveBrushPreset());
        return context;
    }

    public override Texture GetIcon()
    {
        if (Icon == null)
        {
            Icon = Resources.Load<Texture2D>("Icons/BrushIcon");
        }
        return Icon;
    }

    protected override ImageTool InitializeTool()
    {
        return new BrushImageTool();
    }
}
