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
            if (window.Tools.SelectedTool != null)
            {
                OnToolSelected(window.Tools.SelectedTool);
            }
            WindowName = "Tool Settings";
            contentContainer.AddToClassList(ussToolSettingsContainer);
        }

        private void OnToolSelected(ToolController toolController)
        {
            selectedToolController = toolController;
            contentContainer.Clear();

            WindowName = "Tool Settings";
            if (selectedToolController is BrushToolController || selectedToolController is EraserToolController || selectedToolController is SelectionBrushToolController)
            {

                Slider sizeSlider = new Slider();
                sizeSlider.lowValue = 1.0f;
                sizeSlider.highValue = 30.0f;
                sizeSlider.label = "Size";
                contentContainer.Add(sizeSlider);
                sizeSlider.RegisterValueChangedCallback(OnSizeChanged);

                Slider opacitySlider = new Slider();
                opacitySlider.lowValue = 0.001f;
                opacitySlider.highValue = 1.0f;
                opacitySlider.label = "Opacity";
                contentContainer.Add(opacitySlider);
                opacitySlider.RegisterValueChangedCallback(OnOpacityChanged);

                BrushPresetSelector brushPresetSelector = new BrushPresetSelector(window);
                brushPreset = brushPresetSelector.selectedPreset;
                if (brushPreset.opacity < 0) brushPreset.opacity = brushPreset.baseOpacity;
                if (brushPreset.size < 0) brushPreset.size = brushPreset.baseSize;
                sizeSlider.value = Mathf.Sqrt(brushPreset.size);
                opacitySlider.value = brushPreset.opacity;

                brushPresetSelector.PresetSelected += (p) => { 
                    brushPreset = p;
                    if (brushPreset.opacity < 0) brushPreset.opacity = brushPreset.baseOpacity;
                    if (brushPreset.size < 0) brushPreset.size = brushPreset.baseSize;
                    sizeSlider.value = Mathf.Sqrt(brushPreset.size);
                    opacitySlider.value = brushPreset.opacity;
                };



                contentContainer.Add(brushPresetSelector);

                if(selectedToolController is BrushToolController) WindowName = "Brush Settings";
                if(selectedToolController is EraserToolController) WindowName = "Eraser Settings";
                if(selectedToolController is SelectionBrushToolController) WindowName = "Select Brush Settings";
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