using HueHades.Tools;
using HueHades.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class ToolSettingsWindow : DockableWindow
    {
        private BrushPreset brushPreset;
        private ToolController selectedToolController;
        private const string ussToolSettingsContainer = "tool-settings-content";

        public ToolSettingsWindow(HueHadesWindow window) : base(window)
        {
            window.Tools.ToolSelected += OnToolSelected;
            WindowName = "Tool Settings";
            contentContainer.AddToClassList(ussToolSettingsContainer);
        }

        private void OnToolSelected(ToolController toolController)
        {
            selectedToolController = toolController;
            contentContainer.Clear();

            WindowName = "Tool Settings";
            if (selectedToolController is BrushToolController)
            {
                BrushPresetSelector brushPresetSelector = new BrushPresetSelector(window);
                brushPreset = brushPresetSelector.selectedPreset;

                Slider sizeSlider = new Slider();
                sizeSlider.lowValue = 1.0f;
                sizeSlider.highValue = 50.0f;
                brushPreset.size = brushPreset.baseSize;
                sizeSlider.value = Mathf.Sqrt(brushPreset.baseSize);
                sizeSlider.label = "Size";
                contentContainer.Add(sizeSlider);
                sizeSlider.RegisterValueChangedCallback(OnSizeChanged);

                Slider opacitySlider = new Slider();
                opacitySlider.lowValue = 0.0f;
                opacitySlider.highValue = 1.0f;
                opacitySlider.value = brushPreset.opacity;
                opacitySlider.label = "Opacity";
                opacitySlider.RegisterValueChangedCallback(OnOpacityChanged);

                contentContainer.Add(opacitySlider);

                contentContainer.Add(brushPresetSelector);
                WindowName = "Brush Settings";
            }
        }

        private void OnOpacityChanged(ChangeEvent<float> opacity)
        {
            brushPreset.opacity = opacity.newValue;
        }

        private void OnSizeChanged(ChangeEvent<float> size)
        {
            brushPreset.size = size.newValue * size.newValue;
        }

        public BrushPreset GetActiveBrushPreset()
        {
            brushPreset.color = window.ColorSelector.GetPrimaryColor();
            return brushPreset;
        }

        public override Vector2 DefaultSize
        {
            get { return new Vector2(200, 200); }
        }



    }
}