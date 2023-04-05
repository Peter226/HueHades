using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HueHades.Tools
{
    public class BrushPreset
    {
        private static List<BrushPreset> _presets = new List<BrushPreset>();
        public static List<BrushPreset> Presets { get { return _presets; } set { _presets = value; PresetsChanged?.Invoke(); } }

        public static Action PresetsChanged;


        public string name { get; set; }
        public Texture icon { get { if (_icon == null) _icon = Resources.Load<Texture2D>(iconPath);  return _icon; } }
        private Texture _icon;
        private string _iconPath;
        public string iconPath { get { return _iconPath; } set { _iconPath = value; if (_icon != null) _icon = Resources.Load<Texture2D>(value); } }
        public Color color { get; set; }
        public float opacity { get; set; }
        public float rotation { get; set; }
        public float baseSize { get; set; }
        public float softness { get; set; }
        public AnimationCurve softnessCurve { get; set; }
        public float spacing { get; set; }
        public Texture texture { get; set; }

        public enum BrushShape
        {
            Ellipse,
            Rectangle,
            Texture,
            ColoredTexture
        }
    }
}