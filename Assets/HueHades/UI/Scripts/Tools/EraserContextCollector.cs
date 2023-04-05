using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EraserContextCollector : ToolContextCollector
{
    public override IToolContext CollectContext(HueHadesWindow window)
    {
        return new EraserToolContext();
    }
}
