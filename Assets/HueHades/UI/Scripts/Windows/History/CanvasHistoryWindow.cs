using HueHades.Core;
using HueHades.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class CanvasHistoryWindow : DockableWindow
    {
        private ScrollView _scrollView;
        private CanvasSelector _canvasSelector;
        private HistoryFooter _historyFooter;
        private ImageCanvas _selectedCanvas;

        private VisualElement _container;

        private const string ussCanvasHistoryWindowContainer = "canvas-history-window-container";
        private const string ussCanvasHistoryWindowScrollview = "canvas-history-window-scrollview";

        public CanvasHistoryWindow(HueHadesWindow window) : base(window)
        {
            WindowName = "History";
            
            _container = new VisualElement();
            hierarchy.Add(_container);
            _container.AddToClassList(ussCanvasHistoryWindowContainer);

            _canvasSelector = new CanvasSelector(window);
            _container.Add(_canvasSelector);
            
            _scrollView = new ScrollView();
            _container.Add(_scrollView);
            _scrollView.AddToClassList(ussCanvasHistoryWindowScrollview);

            _historyFooter = new HistoryFooter(window, this);
            _container.Add( _historyFooter);

            _canvasSelector.CanvasSelected += OnCanvasChanged;
            OnCanvasChanged(_canvasSelector.SelectedCanvas);
        }

        public void OnUndo()
        {
            if (_selectedCanvas == null) return;
            _selectedCanvas.History.Undo();
        }

        public void OnRedo()
        {
            if (_selectedCanvas == null) return;
            _selectedCanvas.History.Redo();
        }

        

        private void OnCanvasChanged(ImageCanvas canvas)
        {
            _scrollView.Clear();
            if(_selectedCanvas != null)_selectedCanvas.History.OnHistoryChange -= RenderHistory;
            _selectedCanvas = canvas;
            if (canvas == null) return;
            _selectedCanvas.History.OnHistoryChange += RenderHistory;
            RenderHistory();
        }

        void RenderHistory()
        {
            _scrollView.Clear();

            foreach (var historyRecord in _selectedCanvas.History.HistoryRecords)
            {
                HistoryEntryElement historyEntryElement = new HistoryEntryElement(window, historyRecord);
                _scrollView.Add(historyEntryElement);
            }

        }



        public override Vector2 DefaultSize
        {
            get{ return new Vector2(200, 200); }
        }

    }
}