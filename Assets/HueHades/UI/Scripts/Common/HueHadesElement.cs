using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HueHadesElement : VisualElement
{
    protected HueHadesWindow window;
    public HueHadesWindow HueHadesWindowIn { get { return window; } }

    public HueHadesElement(HueHadesWindow window)
    {
        this.window = window;
    }
}
