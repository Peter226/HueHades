using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBarItemButton : HueHadesButton
{
    private Type _menuBarFunction;
    private const string ussItemButton = "item-button";

    public MenuBarItemButton(HueHadesWindow window, string name, Type menuBarFunction) : base(window)
    {
        _menuBarFunction = menuBarFunction;
        text = name;
        AddToClassList(ussItemButton);
        clicked += () => {
            Debug.Log("clicked button");
            (menuBarFunction as IMenuBarFunction)?.Execute(window);
        };
    }
}