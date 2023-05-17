
using HueHades.Core;
using HueHades.Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class ColorAdjustmentsEffectWindow : EffectWindow
    {
        private ColorAdjustmentsEffect _effect;

        private ColorPickerGradient _hueGradient;
        private ColorPickerGradient _saturationGradient;
        private ColorPickerGradient _brightnessGradient;
        private Slider _contrastSlider;

        public ColorAdjustmentsEffectWindow(HueHadesWindow window) : base(window)
        {
            _hueGradient = new ColorPickerGradient(window);
            _hueGradient.Mode = ColorPickerGradient.GradientMode.Hue;
            _hueGradient.PickerPosition = 0.5f;
            _hueGradient.ValueChanged += (v) => {
                _effect.Hue = v;
                _saturationGradient.ColorB = Color.HSVToRGB(v, 1, 1);
                dataDirty = true;
            };
            _hueGradient.label = "Hue";
            _hueGradient.showInputField = true;

            _saturationGradient = new ColorPickerGradient(window);
            _saturationGradient.PickerPosition = 0.5f;
            _saturationGradient.ColorA = Color.white;
            _saturationGradient.ColorB = Color.HSVToRGB(_hueGradient.PickerPosition,1,1);
            _saturationGradient.ValueChanged += (v) => { _effect.Saturation = v; dataDirty = true; };
            _saturationGradient.label = "Saturation";
            _saturationGradient.showInputField = true;

            _brightnessGradient = new ColorPickerGradient(window);
            _brightnessGradient.ColorA = Color.black;
            _brightnessGradient.ColorB = Color.white;
            _brightnessGradient.PickerPosition = 0.5f;
            _brightnessGradient.ValueChanged += (v) => { _effect.Brightness = v; dataDirty = true; };
            _brightnessGradient.label = "Brightness";
            _brightnessGradient.showInputField = true;

            _contrastSlider = new Slider();
            _contrastSlider.lowValue = 0;
            _contrastSlider.highValue = 1;
            _contrastSlider.value = 0.5f;
            _contrastSlider.label = "Contrast";
            _contrastSlider.showInputField = true;
            _contrastSlider.RegisterCallback<FocusOutEvent>((f) => { OnContrastChanged(null); });
            _contrastSlider.RegisterValueChangedCallback(OnContrastChanged);

            container.Add(_hueGradient);
            container.Add(_saturationGradient);
            container.Add(_brightnessGradient);
            container.Add(_contrastSlider);
        }

        private void OnContrastChanged(ChangeEvent<float> evt)
        {
            _effect.Contrast = _contrastSlider.value;
            dataDirty = true;
        }

        protected override string GetWindowName()
        {
            return "Color Adjustments";
        }


        protected override void OnApplyEffect()
        {
            _effect.ApplyEffect();
        }

        protected override void OnBeginEffect(ImageCanvas canvas)
        {
            _effect = new ColorAdjustmentsEffect();
            _effect.Hue = _hueGradient.PickerPosition;
            _effect.Saturation = _saturationGradient.PickerPosition;
            _effect.Brightness = _brightnessGradient.PickerPosition;
            _effect.Contrast = _contrastSlider.value;

            if (!_effect.CanExecute(canvas))
            {
                Close();
                return;
            }

            _effect.BeginEffect(canvas);
        }

        protected override void OnCancelEffect()
        {
            _effect.CancelEffect();
        }

        protected override void OnRenderEffect()
        {
            _effect.RenderEffect();
        }
    }
}