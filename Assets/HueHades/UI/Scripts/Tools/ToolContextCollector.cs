using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ToolContextCollector
{
    public abstract IToolContext CollectContext(HueHadesWindow window);
}
