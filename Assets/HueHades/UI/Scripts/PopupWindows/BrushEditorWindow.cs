using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrushEditorWindow : PopupWindow
{
    public BrushEditorWindow(HueHadesWindow window) : base(window)
    {
    }

    protected override void OnOpen()
    {
        
    }

    protected override string GetWindowName()
    {
        return "Brush Editor";
    }

}
