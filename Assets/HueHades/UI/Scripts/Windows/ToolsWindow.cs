using HueHades.Core;
using HueHades.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class ToolsWindow : DockableWindow
    {
        private const string ussToolWindow = "tool-window";

        private List<ToolButton> toolButtons = new List<ToolButton>();
        ToolController _selectedTool;
        public ToolController SelectedTool { get { return _selectedTool; } }

        public Action<ToolController> ToolSelected;
        ToolButton defaultToolButton;

        public ToolsWindow(HueHadesWindow window) : base(window)
        {
            if (window.Tools != null) throw new Exception("Tool window already exists for HueHades window!");
            AddToClassList(ussToolWindow);

            ToolController[] toolControllers = new ToolController[] {
                new BrushToolController(),
                new EraserToolController(),
                new SelectionRectangleToolController(),
                new SelectionEllipseToolController(),
                new SelectionBrushToolController(),
                new MoveSelectedToolController(),
                new ColorPickerToolController(),
                new MoveSelectionToolController(),
                new StampToolController(),
                
            };

            foreach (var toolController in toolControllers)
            {
                ToolButton toolButton = new ToolButton(window, toolController);
                if (toolController is BrushToolController)
                {
                    defaultToolButton = toolButton;
                }
                toolButtons.Add(toolButton);
                toolButton.Selected += OnSelectedButton;
                hierarchy.Add(toolButton);
            }
            WindowName = "Tools";

            // Select brush tool to start out with
            if (_selectedTool == null)
            {
                defaultToolButton.OnForceClick();
            }
        }

        public void OnToolBeginUse(ImageCanvas canvas, int layer, Vector2 startPoint, float startPressure, float startTilt)
        {
            if (_selectedTool == null) return;
            _selectedTool.BeginUse(_selectedTool.CollectContext(window), canvas, layer, startPoint, startPressure, startTilt);
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
            _selectedTool = button.ToolController;
            ToolSelected?.Invoke(_selectedTool);
            foreach (ToolButton toolButton in toolButtons)
            {
                if (toolButton != button) toolButton.Deselect();
            }
        }

        public override Vector2 DefaultSize
        {
            get { return new Vector2(80.0f, 80.0f); }
        }

    }
}