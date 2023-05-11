using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBarItemAttribute : Attribute
{
    public string categoryPath;
    public string iconPath;
    public bool quickAccess;

    public MenuBarItemAttribute(string categoryPath, string iconPath = "", bool quickAccess = false)
    {
        this.categoryPath = categoryPath;
        this.iconPath = iconPath;
        this.quickAccess = quickAccess;
    }

}
