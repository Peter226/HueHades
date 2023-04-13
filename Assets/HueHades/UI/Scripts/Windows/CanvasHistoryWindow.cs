using HueHades.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HueHades.UI
{
    public class CanvasHistoryWindow : DockableWindow
    {
        public CanvasHistoryWindow(HueHadesWindow window) : base(window)
        {
            WindowName = "History";
        }

        public override Vector2 GetDefaultSize()
        {
            return new Vector2(200, 200);
        }

    }
}