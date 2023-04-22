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
            _nameField.label = "Preset Name";
            _rightContent.Add(_nameField);

            _brushShapeDropDown = new DropDownInput<BrushShape>(window);
            _brushShapeDropDown.SetDataSource(
                new List<BrushShape>() {
                    BrushShape.Rectangle,
                    BrushShape.Ellipse,
                    BrushShape.Texture
                },
                (shape) => { return Enum.GetName(typeof(BrushShape), shape); }
            );
            _rightContent.Add(_brushShapeDropDown);

            var _bottomButtos = new VisualElement();
            _bottomButtos.AddToClassList(Layouts.Horizontal);
            _bottomButtos.AddToClassList(Layouts.SpaceAround);
            _rightLayout.Add(_bottomButtos);

            var _overrideButton = new Button();
            _overrideButton.text = "Override preset";
            _bottomButtos.Add(_overrideButton);

            var _saveButton = new Button();
            _saveButton.text = "Save as new";
            _bottomButtos.Add(_saveButton);

            _mainLayout.Add(_presetSelector);
            _mainLayout.Add(_rightLayout);
            
            _mainLayout.AddToClassList(_ussBrushEditorWindowMain);

            OnPresetSelected(_presetSelector.selectedPreset);
        }


        private void OnPresetSelected(BrushPreset preset)
        {
            selectedPreset = preset;
            _nameField.value = preset.name;
            _brushShapeDropDown.selectedValue = preset.shape;
        }

        protected override void OnOpen()
        {

        }

        protected override string GetWindowName()
        {
            return "Brush Editor";
        }
    }
}
