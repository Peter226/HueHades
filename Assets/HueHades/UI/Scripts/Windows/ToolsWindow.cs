using HueHades.Core;
using HueHades.Tools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HueHades.UI
{
    public class ToolsWindow : DockableWindow
    {
        private const string ussToolWindow = "tool-window";

        private List<ToolButton> toolButtons = new List<ToolButton>();
        ImageTool _selectedTool;

        public ToolsWindow(HueHadesWindow window) : base(window)
        {
            if (window.ToolsWindow != null) throw new Exception("Tool window already exists for HueHades window!");
            AddToClassList(ussToolWindow);
            for (int i = 0;i < 10;i++)
            {
                ToolButton toolButton = new ToolButton(window, new BrushImageTool());
                toolButtons.Add(toolButton);
                toolButton.Selected += OnSelectedButton;
                hierarchy.Add(toolButton);
            }
        }


        public void OnToolBeginUse(ImageCanvas canvas, int layer, Vector2 startPoint, float startPressure, float startTilt)
        {
            if (_selectedTool == null) return;
            _selectedTool.BeginUse(canvas, layer, startPoint, startPressure, startTilt);
        }
        public void OnToolEndUse(Vector2 endPoint, float endPressure, float endTilt)
        {
            if (_selectedTool == null) return;
            _selectedTool.EndUse(endPoint, endPressure, endTilt);
        }
        public void OnToolUseUpdate(Vector2 currentPoint, float currentPressure, float currentTilt)
        {
            if (_selectedTool == null) return;
            _selectedTool.UseUpdate(currentPoint, currentPressure, currentTilt);
        }


        private void OnSelectedButton(ToolButton button)
        {
            _selectedTool = button.ImageTool;
            foreach (ToolButton toolButton in toolButtons)
            {
                if (toolButton != button) toolButton.Deselect();
            }
        }

        public override string GetWindowName()
        {
            return "Tools";
        }
    }
}