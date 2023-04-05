using HueHades.Core;
using HueHades.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HueHades.UI
{
    public class ToolsWindow : DockableWindow
    {
        private const string ussToolWindow = "tool-window";

        private List<ToolButton> toolButtons = new List<ToolButton>();
        ImageTool _selectedTool;
        ToolContextCollector _selectedContextCollector;

        public ToolsWindow(HueHadesWindow window) : base(window)
        {
            if (window.ToolsWindow != null) throw new Exception("Tool window already exists for HueHades window!");
            AddToClassList(ussToolWindow);

            (ImageTool, ToolContextCollector)[] imageTools = new (ImageTool, ToolContextCollector)[] { (new BrushImageTool(), new BrushContextCollector()), (new EraserImageTool(), new EraserContextCollector()) };

            foreach (var (imageTool, contextCollector) in imageTools)
            {
                ToolButton toolButton = new ToolButton(window, imageTool, contextCollector);
                toolButtons.Add(toolButton);
                toolButton.Selected += OnSelectedButton;
                hierarchy.Add(toolButton);
            } 
        }

        public void OnToolBeginUse(ImageCanvas canvas, int layer, Vector2 startPoint, float startPressure, float startTilt)
        {
            if (_selectedTool == null) return;
            _selectedTool.BeginUse(_selectedContextCollector.CollectContext(window), canvas, layer, startPoint, startPressure, startTilt);
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
            _selectedContextCollector = button.ContextCollector;
            foreach (ToolButton toolButton in toolButtons)
            {
                if (toolButton != button) toolButton.Deselect();
            }
        }

        public override string GetWindowName()
        {
            return "Tools";
        }


        public override Vector2 GetDefaultSize()
        {
            return new Vector2(80.0f,80.0f);
        }

    }
}