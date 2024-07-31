using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class GraphEditorContextMenu : VisualElement
    {
        private const string ussGraphEditorContextMenu = "graph-editor-context-menu";

        public GraphEditorContextMenu() : base()
        {
            AddToClassList(ussGraphEditorContextMenu);

            Add(new Label("Test"));
            Add(new Label("Label"));
            Add(new Label("Idk"));
        }
    }
}