using HueHades.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class ColorSelectorWindow : DockableWindow
    {
        private ColorPickerRectangle _colorPickerRectangle;
        private ColorPickerGradient _colorPickerHue;
        private ColorPickerGradient _colorPickerAlpha;

        public ColorSelectorWindow(HueHadesWindow window) : base(window)
        {
            style.flexDirection = FlexDirection.Column;
            _colorPickerRectangle = new ColorPickerRectangle(window);
            _colorPickerHue = new ColorPickerGradient(window);
            _colorPickerHue.Mode = ColorPickerGradient.GradientMode.Hue;
            _colorPickerAlpha = new ColorPickerGradient(window);

            _colorPickerHue.PickerPosition = 0.5f;
            _colorPickerAlpha.PickerPosition = 1.0f;
            _colorPickerRectangle.PickerPosition = new Vector2(1,0);

            hierarchy.Add(_colorPickerRectangle);
            hierarchy.Add(_colorPickerHue);
            hierarchy.Add(_colorPickerAlpha);
            _colorPickerRectangle.style.height = 60;

            _colorPickerHue.style.height = 16;
            _colorPickerAlpha.style.height = 16;

            _colorPickerHue.OnValueChanged += OnHueChanged;
        }

        private void OnHueChanged(float hue)
        {
            _colorPickerRectangle.HueColor = Color.HSVToRGB(hue, 1, 1);
        }

        public Color GetPrimaryColor()
        {
            var pickerPos = _colorPickerRectangle.PickerPosition;
            var huePos = _colorPickerHue.PickerPosition;

            var blendedColor = Color.Lerp(Color.black,Color.Lerp(Color.white, Color.HSVToRGB(huePos, 1, 1), pickerPos.x), 1 - pickerPos.y);
            blendedColor.a = _colorPickerAlpha.PickerPosition;
            return blendedColor.linear;
        }


        public override Vector2 GetDefaultSize()
        {
            return new Vector2(200, 200);
        }


        public override string GetWindowName()
        {
            return "Colors";
        }
    }
}
