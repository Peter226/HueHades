using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBarItemAttribute : Attribute
{
    public string categoryPath;
    public int orderInCategory;

    public MenuBarItemAttribute(string categoryPath, int orderInCategory = 0)
    {
        this.categoryPath = categoryPath;
        this.orderInCategory = orderInCategory;
    }

}
