using HueHades.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrushContextCollector : ToolContextCollector
{
    public override IToolContext CollectContext(HueHadesWindow window)
    {
        var context = new BrushToolContext(window.ToolSettings.GetActiveBrushPreset());
        return context;
    }
}
