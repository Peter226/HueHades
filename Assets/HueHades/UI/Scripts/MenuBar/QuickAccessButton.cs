using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;

public class QuickAccessButton : HueHadesButton
{
    private const string ussQuickAccessButton = "quick-access-button";

    public QuickAccessButton(HueHadesWindow window, MenuBarItemAttribute attribute, Type menuBarFunction) : base(window)
    {
        Image icon = new Image();
        icon.image = Icons.GetIcon(attribute.iconPath);
        Add(icon);

        AddToClassList(ussQuickAccessButton);
        clicked += () => {
            IMenuBarFunction instance = (IMenuBarFunction)Activator.CreateInstance(menuBarFunction);
            instance?.Execute(window);
        };
    }


    public VisualElement Element => this;
}