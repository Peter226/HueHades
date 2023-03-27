using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MenuBarCategory
{
    public readonly int order;
    public readonly string name;

    public MenuBarCategory(string name, int order)
    {
        this.name = name;
        this.order = order;
    }
}
