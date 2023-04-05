using HueHades.Tools;
using HueHades.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HueHades.UI
{
    public class ToolSettingsWindow : DockableWindow
    {
        private BrushPreset brushPreset;

        public ToolSettingsWindow(HueHadesWindow window) : base(window)
        {
            brushPreset = new BrushPreset();
        }

        public BrushPreset GetActiveBrushPreset()
        {
            brushPreset.color = window.ColorSelector.GetPrimaryColor();
            return brushPreset;
        }


        public override string GetWindowName()
        {
            return "Tool Settings";
        }

        public override Vector2 GetDefaultSize()
        {
            return new Vector2(200,200);
        }



    }
}