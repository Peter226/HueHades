using HueHades.Common;
using HueHades.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class LayerSettingsWindow : PopupWindow
    {
        private LayerBase _layer;
        private LayerSettings _startLayerSettings;

        private ToggleButton _visibilityToggle;
        private ToggleButton _alphaInheritToggle;
        private Slider _opacitySlider;
        private DropDownInput<ColorBlendMode> _blendModeDropdown;

        private bool _applied;

        public LayerSettingsWindow(HueHadesWindow window, LayerBase layer) : base(window)
        {
            _layer = layer;
            _startLayerSettings = _layer.LayerSettings;


            var _toggles = new VisualElement();
            _toggles.AddToClassList(Layouts.Horizontal);

            _visibilityToggle = new ToggleButton("Visibility",icon: "Icons/VisibleIcon", toggledIcon: "Icons/InvisibleIcon");
            _visibilityToggle.Toggled = _startLayerSettings.invisible;
            _visibilityToggle.ValueChanged += (v) => UpdateSettings();
            _toggles.Add(_visibilityToggle);
            
            _alphaInheritToggle = new ToggleButton("Inherit alpha", icon: "Icons/AlphaUnlockedIcon", toggledIcon: "Icons/AlphaLockedIcon");
            _alphaInheritToggle.Toggled = _startLayerSettings.inheritAlpha;
            _alphaInheritToggle.ValueChanged += (v) => UpdateSettings();
            _toggles.Add(_alphaInheritToggle);

            container.Add(_toggles);

            _opacitySlider = new Slider();
            _opacitySlider.label = "Opacity          ";
            _opacitySlider.lowValue = 0;
            _opacitySlider.highValue = 1;

            _opacitySlider.value = _startLayerSettings.opacity;
            
            _opacitySlider.RegisterValueChangedCallback(OnOpacityChange);
            container.Add(_opacitySlider);

            _blendModeDropdown = new DropDownInput<ColorBlendMode>(window);
            _blendModeDropdown.SetDataSource(new List<ColorBlendMode>() { ColorBlendMode.Default, ColorBlendMode.Multiply, ColorBlendMode.Add, ColorBlendMode.Subtract }, (bm) => Enum.GetName(typeof(ColorBlendMode), bm));
            _blendModeDropdown.label = "Blend mode  ";
            _blendModeDropdown.value = _startLayerSettings.blendMode;
            _blendModeDropdown.ValueChanged += (v) => UpdateSettings();
            container.Add(_blendModeDropdown);

            var _bottomButtos = new VisualElement();
            _bottomButtos.AddToClassList(Layouts.Horizontal);
            _bottomButtos.AddToClassList(Layouts.SpaceAround);

            var _applyButton = new Button();
            _applyButton.text = "Apply";
            _bottomButtos.Add(_applyButton);
            _applyButton.clicked += Apply;


            var _cancelButton = new Button();
            _cancelButton.text = "Cancel";
            _bottomButtos.Add(_cancelButton);
            _bottomButtos.style.flexGrow = 1;
            _bottomButtos.style.alignContent = Align.FlexEnd;
            _bottomButtos.style.alignItems = Align.FlexEnd;
            _cancelButton.clicked += Close;

            hierarchy.Add(_bottomButtos);
        }

        private void OnOpacityChange(ChangeEvent<float> evt)
        {
            UpdateSettings();
        }

        private void UpdateSettings()
        {
            var newSettings = new LayerSettings()
            {
                opacity = _opacitySlider.value,
                blendMode = _blendModeDropdown.value,
                inheritAlpha = _alphaInheritToggle.Toggled,
                invisible = _visibilityToggle.Toggled,
            };

            _layer.SetLayerSettings(newSettings, false);
        }

        protected override Vector2 GetDefaultSize()
        {
            return base.GetDefaultSize() * new Vector2(1,0.6f);
        }



        protected override void OnClose()
        {
            if (!_applied)
            {
                _layer.SetLayerSettings(_startLayerSettings, false);
            }
        }

        private void Apply()
        {
            _applied = true;


            var newSettings = new LayerSettings()
            {
                opacity = _opacitySlider.value,
                blendMode = _blendModeDropdown.value,
                inheritAlpha = _alphaInheritToggle.Toggled,
                invisible = _visibilityToggle.Toggled,
            };

            _layer.SetLayerSettings(newSettings, false);
            _layer.CanvasIn.History.AddRecord(new ModifyLayerSettingsHistoryRecord(_layer.GlobalIndex, _startLayerSettings, newSettings));
            Close();
        }

        protected override string GetWindowName()
        {
            return "Layer Settings";
        }
    }
}