using HueHades.UI;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class ColorSelectorWindow : DockableWindow
    {
        private ColorPickerRectangle _colorPickerRectangle;
        private ColorPickerGradient _colorPickerHue;
        private ColorPickerGradient _colorPickerAlpha;
        private ColorPickerGradient _colorPickerRed;
        private ColorPickerGradient _colorPickerGreen;
        private ColorPickerGradient _colorPickerBlue;
        private TextField _hexField;

        public ColorSelectorWindow(HueHadesWindow window) : base(window)
        {
            style.flexDirection = FlexDirection.Column;
            _colorPickerRectangle = new ColorPickerRectangle(window);

            _hexField = new TextField();
            _hexField.label = "Hex";
            _hexField.RegisterValueChangedCallback<string>(OnHexChanged);
            _hexField.RegisterCallback<FocusOutEvent>(OnHexLostFocus);

            _colorPickerHue = new ColorPickerGradient(window);
            _colorPickerHue.label = "H";
            _colorPickerHue.showInputField = true;

            _colorPickerHue.Mode = ColorPickerGradient.GradientMode.Hue;
            _colorPickerAlpha = new ColorPickerGradient(window);
            _colorPickerAlpha.label = "A";
            _colorPickerAlpha.showInputField = true;

            _colorPickerRed = new ColorPickerGradient(window);
            _colorPickerRed.label = "R";
            _colorPickerRed.showInputField = true;
            _colorPickerGreen = new ColorPickerGradient(window);
            _colorPickerGreen.label = "G";
            _colorPickerGreen.showInputField = true;
            _colorPickerBlue = new ColorPickerGradient(window);
            _colorPickerBlue.label = "B";
            _colorPickerBlue.showInputField = true;

            _colorPickerHue.PickerPosition = 0.5f;
            _colorPickerAlpha.PickerPosition = 1.0f;
            _colorPickerRectangle.PickerPosition = new Vector2(1,0);

            hierarchy.Add(_colorPickerRectangle);
            hierarchy.Add(_hexField);
            hierarchy.Add(_colorPickerHue);
            hierarchy.Add(_colorPickerAlpha);
            hierarchy.Add(_colorPickerRed);
            hierarchy.Add(_colorPickerGreen);
            hierarchy.Add(_colorPickerBlue);

            _colorPickerRectangle.style.height = 60;

            _colorPickerHue.ValueChangedByUser += OnHueChanged;
            _colorPickerRed.ValueChangedByUser += OnRedChanged;
            _colorPickerGreen.ValueChangedByUser += OnGreenChanged;
            _colorPickerBlue.ValueChangedByUser += OnBlueChanged;
            _colorPickerRectangle.OnValueChangedByUser += OnRectangleChanged;
            _colorPickerAlpha.ValueChangedByUser += OnAlphaChanged;

            WindowName = "Colors";
            UpdateColors(GetPrimaryColorDefault());
        }

        private void OnAlphaChanged(float obj)
        {
            UpdateColors(GetPrimaryColorDefault());
        }

        private void OnHexLostFocus(FocusOutEvent evt)
        {
            if (!ColorUtility.TryParseHtmlString("#" + _hexField.value, out Color color))
            {
                UpdateColors(GetPrimaryColorDefault(),true);
            }
        }

        private void OnHexChanged(ChangeEvent<string> evt)
        {
            if (evt.newValue == evt.previousValue) return;
            if (ColorUtility.TryParseHtmlString("#" + evt.newValue, out Color color))
            {
                UpdateColors(color, false);
            }
        }

        /// <summary>
        /// Used to update all color picker values
        /// </summary>
        /// <param name="color"></param>
        /// <param name="updateHex"></param>
        /// <param name="updateHue"></param>
        /// <param name="updateRectangle"></param>
        private void UpdateColors(Color color, bool updateHex = true, bool updateHue = true, bool updateRectangle = true)
        {

            _colorPickerAlpha.PickerPosition = color.a;
            color.a = 1;

            var rmin = color;
            rmin.r = 0;
            var rmax = rmin;
            rmax.r = 1;

            var gmin = color;
            gmin.g = 0;
            var gmax = gmin;
            gmax.g = 1;

            var bmin = color;
            bmin.b = 0;
            var bmax = bmin;
            bmax.b = 1;

            _colorPickerRed.ColorA = rmin;
            _colorPickerRed.ColorB = rmax;
            _colorPickerRed.PickerPosition = color.r;

            _colorPickerGreen.ColorA = gmin;
            _colorPickerGreen.ColorB = gmax;
            _colorPickerGreen.PickerPosition = color.g;

            _colorPickerBlue.ColorA = bmin;
            _colorPickerBlue.ColorB = bmax;
            _colorPickerBlue.PickerPosition = color.b;

            Color.RGBToHSV(color, out float h, out float s, out float v);

            if(updateHue) _colorPickerHue.PickerPosition = h;
            if (updateRectangle)
            {
                _colorPickerRectangle.HueColor = Color.HSVToRGB(_colorPickerHue.PickerPosition, 1, 1);
                _colorPickerRectangle.PickerPosition = new Vector2(s, 1 - v);
            }
            if (updateHex)
            {
                color = GetPrimaryColorDefault();
                _hexField.SetValueWithoutNotify(ColorUtility.ToHtmlStringRGBA(color));
            }
        }

        private void OnRectangleChanged(Vector2 obj)
        {
            UpdateColors(GetPrimaryColorDefault(), updateHue: false, updateRectangle: false);
        }

        private void OnBlueChanged(float pickerPosition)
        {
            var color = GetPrimaryColorDefault();
            color.b = pickerPosition;
            UpdateColors(color);
        }

        private void OnGreenChanged(float pickerPosition)
        {
            var color = GetPrimaryColorDefault();
            color.g = pickerPosition;
            UpdateColors(color);
        }

        private void OnRedChanged(float pickerPosition)
        {
            var color = GetPrimaryColorDefault();
            color.r = pickerPosition;
            UpdateColors(color);
        }

        private void OnHueChanged(float hue)
        {
            UpdateColors(GetPrimaryColorDefault(), updateHue: false);
        }


        public Color GetPrimaryColorDefault()
        {
            var pickerPos = _colorPickerRectangle.PickerPosition;
            var huePos = _colorPickerHue.PickerPosition;

            var blendedColor = Color.Lerp(Color.black, Color.Lerp(Color.white, Color.HSVToRGB(huePos, 1, 1), pickerPos.x), 1 - pickerPos.y);
            blendedColor.a = _colorPickerAlpha.PickerPosition;
            return blendedColor;
        }


        public Color GetPrimaryColor()
        {
            var pickerPos = _colorPickerRectangle.PickerPosition;
            var huePos = _colorPickerHue.PickerPosition;

            var blendedColor = Color.Lerp(Color.black,Color.Lerp(Color.white, Color.HSVToRGB(huePos, 1, 1), pickerPos.x), 1 - pickerPos.y);
            blendedColor.a = _colorPickerAlpha.PickerPosition;
            return blendedColor.linear;
        }


        public override Vector2 DefaultSize
        {
            get { return new Vector2(200, 200); }
        }

    }
}
