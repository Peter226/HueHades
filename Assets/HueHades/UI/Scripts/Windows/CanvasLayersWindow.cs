using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HueHades.UI
{
    public class CanvasLayersWindow : DockableWindow
    {
        public CanvasLayersWindow(HueHadesWindow window) : base(window)
        {
            WindowName = "Layers";
        }

        public override Vector2 GetDefaultSize()
        {
            return new Vector2(200, 200);
        }

    }
}