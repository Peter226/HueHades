using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CategoryButton : HueHadesButton
{

    private const string ussCategoryButton = "category-button";

    public CategoryButton(HueHadesWindow window, bool isMainCategory = false) : base(window)
    {
        RegisterCallback<FocusOutEvent>(FocusOutCallback);
        if (!isMainCategory)
        {
            RegisterCallback<MouseEnterEvent>(MouseEnterCallback);
        }
        else
        {
            clicked += () =>
            {
                Focus();
                GetMouse?.Invoke();
            };
        }

        AddToClassList(ussCategoryButton);
    }

    private void MouseEnterCallback(MouseEnterEvent evt)
    {
        Focus();
        GetMouse?.Invoke();
    }


    public Action<IEventHandler> LoseMouse;
    public Action GetMouse;

    private void FocusOutCallback(FocusOutEvent e)
    {
        var focusedElement = e.relatedTarget;
        LoseMouse?.Invoke(focusedElement);
    }

}
