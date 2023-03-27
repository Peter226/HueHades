using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CategoryButton : HueHadesButton
{

    private const string ussCategoryButton = "category-button";

    public CategoryButton(HueHadesWindow window) : base(window)
    {
        this.RegisterCallback<FocusOutEvent>(FocusOutCallback);
        AddToClassList(ussCategoryButton);
    }

    public EventHandler HideCategoryEvent;

    private void FocusOutCallback(FocusOutEvent e)
    {
        HideCategoryEvent?.Invoke(this, new EventArgs());
    }

}
