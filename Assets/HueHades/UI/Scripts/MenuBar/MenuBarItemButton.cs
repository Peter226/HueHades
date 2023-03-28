using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
            IMenuBarFunction instance = (IMenuBarFunction)Activator.CreateInstance(menuBarFunction);
            instance?.Execute(window);
            LoseMouse?.Invoke(null);
        };
        RegisterCallback<MouseEnterEvent>(OnMouseEnterCallback);
        RegisterCallback<FocusOutEvent>(OnLoseFocusCallback);
    }

    public Action<IEventHandler> LoseMouse;

    private void OnLoseFocusCallback(FocusOutEvent focusOutEvent)
    {
        var focusedElement = focusOutEvent.relatedTarget;
        LoseMouse?.Invoke(focusedElement);
    }

    private void OnMouseEnterCallback(MouseEnterEvent mouseEnterEvent)
    {
        Focus();
    }
}