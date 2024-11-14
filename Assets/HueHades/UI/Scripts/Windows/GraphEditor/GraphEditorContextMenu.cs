using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class GraphEditorContextMenu : VisualElement
    {
        private const string ussGraphEditorContextMenu = "graph-editor-context-menu";
        private const string ussGraphEditorContextLabel = "graph-editor-context-label";
        private const string ussGraphEditorContextButton = "graph-editor-context-button";
        private const string ussGraphEditorContextSeparator = "graph-editor-context-separator";

        public Action OptionSelected;


        public GraphEditorContextMenu() : base()
        {
            AddToClassList(ussGraphEditorContextMenu);

            AddFuction("Create Node...", null);
            AddFuction("Create Sticky Note", null);
            AddSeparator();

            AddFuction("Cut", null);
            AddFuction("Copy", null);
            AddFuction("Paste", null);
            AddSeparator();

            AddFuction("Duplicate", null);
            AddSeparator();

            AddFuction("Delete", null);
            AddFuction("Cut Links", null);
            AddSeparator();

            AddFuction("Select...", null);
            AddFuction("Group Selected", null);
            AddFuction("Ungroup  Selected", null);
        }

        private void AddSeparator()
        {
            var e = new VisualElement();
            e.AddToClassList(ussGraphEditorContextSeparator);
            Add(e);
        }

        public void AddFuction(string label, Action function)
        {
            var b = new Button();
            b.AddToClassList(ussGraphEditorContextButton);
            b.clicked += () => CallFunction(function);

            var l = new Label(label);
            l.AddToClassList(ussGraphEditorContextLabel);
            b.Add(l);

            Add(b);
        }

        private void CallFunction(Action function)
        {
            function?.Invoke();
            OptionSelected?.Invoke();
        }

    }
}