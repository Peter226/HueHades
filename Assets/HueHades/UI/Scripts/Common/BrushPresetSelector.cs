using HueHades.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class BrushPresetSelector : HueHadesElement
    {
        private const string _ussBrushPresetSelector = "brush-preset-selector";
        private const string _ussBrushPresetLabel = "brush-preset-label";
        private const string _ussBrushPresetIcon = "brush-preset-icon";

        private ScrollView _scrollView;
        public BrushPreset selectedPreset { get; set; }
        public Action<BrushPreset> PresetSelected;

        private List<PresetContainer> _presetContainers = new List<PresetContainer>();

        public BrushPresetSelector(HueHadesWindow window) : base(window)
        {
            AddToClassList(_ussBrushPresetSelector);

            Label label = new Label();
            label.text = "Presets";
            hierarchy.Add(label);
            _scrollView = new ScrollView();
            hierarchy.Add(_scrollView);

            BrushPreset.PresetsChanged += RegeneratePresetList;
            RegeneratePresetList();
            if (BrushPreset.Presets.Count > 0)
            {
                OnPresetSelected(BrushPreset.Presets[0]);
            }
        }

        

        private void OnPresetSelected(BrushPreset preset)
        {
            selectedPreset = preset;
            PresetSelected?.Invoke(preset);
            foreach (PresetContainer presetContainer in _presetContainers)
            {
                presetContainer.isSelected = presetContainer.preset == preset;
            }
        }

        private void RegeneratePresetList()
        {
            _scrollView.Clear();
            _presetContainers.Clear();
            foreach (BrushPreset brushPreset in BrushPreset.Presets)
            {
                PresetContainer presetContainer = new PresetContainer(brushPreset);
                presetContainer.PresetSelected += OnPresetSelected;
                _scrollView.Add(presetContainer);
                _presetContainers.Add(presetContainer);
            }
        }

        private class PresetContainer : Button
        {
            private const string _ussBrushPresetContainer = "brush-preset-container";
            private const string _ussBrushPresetContainerSelected = "brush-preset-container-selected";
            public BrushPreset preset { get; set; }
            private bool _isSelected;
            public bool isSelected { get { return _isSelected; } set {
                    if (_isSelected && !value) RemoveFromClassList(_ussBrushPresetContainerSelected);
                    if (!_isSelected && value) AddToClassList(_ussBrushPresetContainerSelected);
                    _isSelected = value; 
                } }

            public PresetContainer(BrushPreset preset)
            {
                this.preset = preset;
                AddToClassList(_ussBrushPresetContainer);
                Image presetIcon = new Image();
                presetIcon.AddToClassList(_ussBrushPresetIcon);
                Add(presetIcon);
                Label presetLabel = new Label();
                presetLabel.AddToClassList(_ussBrushPresetLabel);
                Add(presetLabel);
                presetLabel.text = preset.name;
                presetIcon.image = preset.icon;

                clicked += OnClicked;
            }

            void OnClicked()
            {
                PresetSelected?.Invoke(preset);
            }

            public Action<BrushPreset> PresetSelected;
        }

    }
}
