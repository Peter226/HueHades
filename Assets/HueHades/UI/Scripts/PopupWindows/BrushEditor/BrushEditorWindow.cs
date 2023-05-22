using HueHades.Common;
using HueHades.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class BrushEditorWindow : PopupWindow
    {
        private BrushPresetSelector _presetSelector;

        private VisualElement _mainLayout;
        private VisualElement _rightLayout;
        private ScrollView _rightContent;
        private const string _ussBrushEditorWindowMain = "brush-editor-main-layout";
        private const string _ussBrushEditorWindowRight = "brush-editor-right-layout";
        private const string _ussBrushEditorWindowContent = "brush-editor-content-layout";
        private BrushPreset _selectedPreset;
        public BrushPreset selectedPreset { get { return _selectedPreset; } private set { if (_selectedPreset != value) _selectedPreset = value; } }

        private TextField _nameField;
        private DropDownInput<BrushShape> _brushShapeDropDown;
        private DropDownInput<ColorBlendMode> _blendModeDropDown;
        private Slider _softnessSlider;
        private Slider _baseOpacitySlider;
        private Slider _rotationSlider;
        private Toggle _autoRotationToggle;
        private Slider _baseSizeSlider;
        private Slider _sizeHeightRatioSlider;
        private Slider _spacingSlider;
        private Toggle _autoSpacingToggle;
        public BrushEditorWindow(HueHadesWindow window) : base(window)
        {
            _mainLayout = new VisualElement();
            container.Add(_mainLayout);

            _rightLayout = new VisualElement();
            
            _rightLayout.AddToClassList(_ussBrushEditorWindowRight);
            _rightLayout.AddToClassList(Layouts.Grow);

            _rightContent = new ScrollView();
            _rightContent.AddToClassList(Layouts.Vertical);
            _rightContent.AddToClassList(_ussBrushEditorWindowContent);
            _rightContent.horizontalScrollerVisibility = ScrollerVisibility.Auto;
            _rightContent.verticalScrollerVisibility = ScrollerVisibility.Auto;
            _rightLayout.Add(_rightContent);

            _presetSelector = new BrushPresetSelector(window);
            _presetSelector.PresetSelected += OnPresetSelected;

            _nameField = new TextField();
            _nameField.label = "Preset name";
            _rightContent.Add(_nameField);

            _brushShapeDropDown = new DropDownInput<BrushShape>(window);
            _brushShapeDropDown.SetDataSource(
                new List<BrushShape>() {
                    BrushShape.Rectangle,
                    BrushShape.Ellipse,
                },
                (shape) => { return Enum.GetName(typeof(BrushShape), shape); }
            );
            _brushShapeDropDown.label = "Shape";
            _rightContent.Add(_brushShapeDropDown);



            _blendModeDropDown = new DropDownInput<ColorBlendMode>(window);
            _blendModeDropDown.SetDataSource(
                new List<ColorBlendMode>() {
                    ColorBlendMode.Default,
                    ColorBlendMode.Multiply,
                    ColorBlendMode.Add,
                    ColorBlendMode.Subtract,
                },
                (blend) => { return Enum.GetName(typeof(ColorBlendMode), blend); }
            );
            _blendModeDropDown.label = "Blend mode";
            _rightContent.Add(_blendModeDropDown);


            _baseSizeSlider = new Slider();
            _baseSizeSlider.label = "Base size";
            _baseSizeSlider.showInputField = true;
            _baseSizeSlider.lowValue = 1;
            _baseSizeSlider.highValue = 512;
            _rightContent.Add(_baseSizeSlider);

            _sizeHeightRatioSlider = new Slider();
            _sizeHeightRatioSlider.label = "Size Height Ratio";
            _sizeHeightRatioSlider.showInputField = true;
            _sizeHeightRatioSlider.lowValue = 0.0001f;
            _sizeHeightRatioSlider.highValue = 1.0f;
            _rightContent.Add(_sizeHeightRatioSlider);

            _spacingSlider = new Slider();
            _spacingSlider.label = "Spacing";
            _spacingSlider.showInputField = true;
            _spacingSlider.lowValue = 0.001f;
            _spacingSlider.highValue = 100;
            _rightContent.Add(_spacingSlider);

            _autoSpacingToggle = new Toggle();
            _autoSpacingToggle.label = "Auto Spacing";
            _autoSpacingToggle.RegisterValueChangedCallback<bool>(OnAutoSpacingChanged);
            _rightContent.Add(_autoSpacingToggle);

            _rotationSlider = new Slider();
            _rotationSlider.label = "Rotation";
            _rotationSlider.showInputField = true;
            _rotationSlider.lowValue = 0;
            _rotationSlider.highValue = 360;
            _rightContent.Add(_rotationSlider);

            _autoRotationToggle = new Toggle();
            _autoRotationToggle.label = "Auto Rotation";
            _rightContent.Add(_autoRotationToggle);

            _softnessSlider = new Slider();
            _softnessSlider.label = "Softness";
            _softnessSlider.showInputField = true;
            _softnessSlider.lowValue = 0;
            _softnessSlider.highValue = 1;
            _rightContent.Add(_softnessSlider);

            _baseOpacitySlider = new Slider();
            _baseOpacitySlider.label = "Base Opacity";
            _baseOpacitySlider.showInputField = true;
            _baseOpacitySlider.lowValue = 0.001f;
            _baseOpacitySlider.highValue = 1;
            _rightContent.Add(_baseOpacitySlider);

            var _bottomButtos = new VisualElement();
            _bottomButtos.AddToClassList(Layouts.Horizontal);
            _bottomButtos.AddToClassList(Layouts.SpaceAround);
            _rightLayout.Add(_bottomButtos);

            var _overrideButton = new Button();
            _overrideButton.text = "Override preset";
            _overrideButton.clicked += OnOverride;
            _bottomButtos.Add(_overrideButton);

            var _saveButton = new Button();
            _saveButton.text = "Save as new";
            _saveButton.clicked += OnSave;
            _bottomButtos.Add(_saveButton);

            _mainLayout.Add(_presetSelector);
            _mainLayout.Add(_rightLayout);
            
            _mainLayout.AddToClassList(_ussBrushEditorWindowMain);

            _rightLayout.style.display = DisplayStyle.None;
            OnPresetSelected(_presetSelector.selectedPreset);
        }

        private void OnAutoSpacingChanged(ChangeEvent<bool> autoSpacing)
        {
            if (autoSpacing.newValue == autoSpacing.previousValue) return;
            if (autoSpacing.newValue)
            {
                _spacingSlider.value = _spacingSlider.value / _baseSizeSlider.value;
            }
            else
            {
                _spacingSlider.value = _spacingSlider.value * _baseSizeSlider.value;
            }
        }

        private void OnSave()
        {
            BrushPreset preset = new BrushPreset();
            preset.iconPath = "Icons/BrushIcon";
            preset.name = _nameField.value;
            preset.shape = _brushShapeDropDown.value;
            preset.blendMode = _blendModeDropDown.value;
            preset.spacing = _spacingSlider.value;
            preset.autoSpacing = _autoSpacingToggle.value;
            preset.baseSize = _baseSizeSlider.value;
            preset.sizeHeightRatio = _sizeHeightRatioSlider.value;
            preset.rotation = _rotationSlider.value;
            preset.autoRotation = _autoRotationToggle.value;
            preset.softness = _softnessSlider.value;
            preset.baseOpacity = _baseOpacitySlider.value;
            preset.Save();
        }

        private void OnOverride()
        {
            if (selectedPreset == null) return;
            BrushPreset preset = selectedPreset;
            preset.name = _nameField.value;
            preset.shape = _brushShapeDropDown.value;
            preset.blendMode = _blendModeDropDown.value;
            preset.spacing = _spacingSlider.value;
            preset.autoSpacing = _autoSpacingToggle.value;
            preset.baseSize = _baseSizeSlider.value;
            preset.sizeHeightRatio = _sizeHeightRatioSlider.value;
            preset.rotation = _rotationSlider.value;
            preset.autoRotation = _autoRotationToggle.value;
            preset.softness = _softnessSlider.value;
            preset.baseOpacity = _baseOpacitySlider.value;
            preset.Save();
        }

        private void OnPresetSelected(BrushPreset preset)
        {
            _rightLayout.style.display = DisplayStyle.Flex;

            selectedPreset = preset;
            _nameField.value = preset.name;
            _brushShapeDropDown.value = preset.shape;
            _blendModeDropDown.value = preset.blendMode;
            _autoSpacingToggle.value = preset.autoSpacing;
            _spacingSlider.value = preset.spacing;
            _baseSizeSlider.value = preset.baseSize;
            _sizeHeightRatioSlider.value = preset.sizeHeightRatio;
            _rotationSlider.value = preset.rotation;
            _autoRotationToggle.value = preset.autoRotation;
            _softnessSlider.value = preset.softness;
            _baseOpacitySlider.value = preset.baseOpacity;
        }

        protected override string GetWindowName()
        {
            return "Brush Editor";
        }
    }
}
