using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HueHadesButton : Button
{
    protected HueHadesWindow window;
    public HueHadesButton(HueHadesWindow window)
    {
        this.window = window;
    }
}
