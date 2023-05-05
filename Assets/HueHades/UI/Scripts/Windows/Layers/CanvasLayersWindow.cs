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

            hierarchy.Add(new LayersFooter(window, this));



        }

        public override Vector2 DefaultSize
        {
            get { return new Vector2(200, 200); }
        }

    }
}