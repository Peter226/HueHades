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
        private const string _ussBrushEditorWindowMain = "brush-editor-main-layout";
        private const string _ussBrushEditorWindowRight = "brush-editor-right-layout";
        private BrushPreset _selectedPreset;
        public BrushPreset selectedPreset { get { return _selectedPreset; } set { if (_selectedPreset != value) OnPresetChanged(); _selectedPreset = value; } }

        public BrushEditorWindow(HueHadesWindow window) : base(window)
        {
            _mainLayout = new VisualElement();
            container.Add(_mainLayout);

            _rightLayout = new VisualElement();
            _rightLayout.AddToClassList(_ussBrushEditorWindowRight);

            _presetSelector = new BrushPresetSelector(window);
            _presetSelector.PresetSelected += OnPresetSelected;

            TextField nameField = new TextField();
            nameField.label = "Preset Name";
            _rightLayout.Add(nameField);

            DropDownInput<BrushShape> dropdown = new DropDownInput<BrushShape>(window);
            dropdown.SetDataSource(
                new List<BrushShape>() {
                    BrushShape.Rectangle,
                    BrushShape.Ellipse,
                    BrushShape.Texture
                },
                (shape) => { return Enum.GetName(typeof(BrushShape), shape); }
            );
            _rightLayout.Add(dropdown);


            _mainLayout.Add(_presetSelector);
            _mainLayout.Add(_rightLayout);
            
            _mainLayout.AddToClassList(_ussBrushEditorWindowMain);
        }


        void OnPresetChanged()
        {

        }


        private void OnPresetSelected(BrushPreset preset)
        {
            selectedPreset = preset;
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
