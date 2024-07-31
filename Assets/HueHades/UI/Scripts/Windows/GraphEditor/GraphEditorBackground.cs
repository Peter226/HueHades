using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class GraphEditorBackground : VisualElement
    {
        private const string ussGraphEditorBackground = "graph-editor-background";
        public GraphEditorBackground() : base()
        {

            AddToClassList(ussGraphEditorBackground);


        }
    }
}