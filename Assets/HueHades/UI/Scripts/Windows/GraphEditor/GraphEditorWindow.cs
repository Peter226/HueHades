using System;
using System.Windows.Forms;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class GraphEditorWindow : DockableWindow
    {
        GraphEditorBackground _background;
        private GraphEditorContextMenu _contextMenu;
        public GraphEditorWindow(HueHadesWindow window) : base(window)
        {
            _background = new GraphEditorBackground();
            _contextMenu = new GraphEditorContextMenu();
            contentContainer.Add(_background);
            WindowName = "Image Graph";

            RegisterCallback<MouseDownEvent>(OnMouseDown);
        }


        private void ShowContextMenu(float2 position)
        {
            _contextMenu.style.top = position.y;
            _contextMenu.style.left = position.x;
            window.ShowOverlay(_contextMenu);
        }

        private void HideContextMenu()
        {
            window.HideOverlay(_contextMenu);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 1)
            {
                ShowContextMenu(evt.mousePosition);
            }
        }
    }
}