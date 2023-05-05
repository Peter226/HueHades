using HueHades.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EraserToolController : ToolController
{

    public override IToolContext CollectContext(HueHadesWindow window)
    {
        return new EraserToolContext();
    }

    public override Texture GetIcon()
    {
        if (Icon == null)
        {
            Icon = Resources.Load<Texture2D>("Icons/EraserIcon");
        }
        return Icon;
    }

    protected override ImageTool InitializeTool()
    {
        return new EraserImageTool();
    }
}
