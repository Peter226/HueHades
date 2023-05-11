using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuBarItemButton : HueHadesButton, IMenuBarElement
{
    private Type _menuBarFunction;
    private const string ussItemButton = "item-button";
    private const string ussItemButtonImage = "item-button-image";
    private const string ussItemButtonLabel = "item-button-label";

    public MenuBarItemButton(HueHadesWindow window, string name, MenuBarItemAttribute attribute, Type menuBarFunction) : base(window)
    {
        _menuBarFunction = menuBarFunction;
        
        Label label = new Label();
        label.AddToClassList(ussItemButtonLabel);
        label.text = name;
        Image image = new Image();
        image.AddToClassList(ussItemButtonImage);
        if(attribute.iconPath.Length > 0) image.image = Icons.GetIcon(attribute.iconPath);

        Add(image);
        Add(label);

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

    public VisualElement Element => this;

    private void OnLoseFocusCallback(FocusOutEvent focusOutEvent)
    {
        var focusedElement = focusOutEvent.relatedTarget;
        LoseMouse?.Invoke(focusedElement);
    }

    private void OnMouseEnterCallback(MouseEnterEvent mouseEnterEvent)
    {
        Focus();
    }

    public void InitializeMenu() { }
}