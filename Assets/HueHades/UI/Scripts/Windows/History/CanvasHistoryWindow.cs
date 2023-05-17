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

        HistoryEntryElement activeEntry = null;

        private List<HistoryEntryElement> historyEntryElements = new List<HistoryEntryElement>();

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

        /// <summary>
        /// Called when undo button is pressed
        /// </summary>
        public void OnUndo()
        {
            if (_selectedCanvas == null) return;
            _selectedCanvas.History.Undo();
        }

        /// <summary>
        /// Called when redo button is pressed
        /// </summary>
        public void OnRedo()
        {
            if (_selectedCanvas == null) return;
            _selectedCanvas.History.Redo();
        }

        
        /// <summary>
        /// Select canvas and render associated history when the canvas changes
        /// </summary>
        /// <param name="canvas"></param>
        private void OnCanvasChanged(ImageCanvas canvas)
        {
            _scrollView.Clear();
            if(_selectedCanvas != null)_selectedCanvas.History.OnHistoryChange -= RenderHistory;
            _selectedCanvas = canvas;
            if (canvas == null) return;
            _selectedCanvas.History.OnHistoryChange += RenderHistory;
            RenderHistory();
        }

        /// <summary>
        /// Clear current hierarchy, and draw the history elements in scrollview
        /// </summary>
        void RenderHistory()
        {
            foreach (var historyEntryElement in historyEntryElements)
            {
                historyEntryElement.EntryActivated -= ScrollToElement;
            }
            _scrollView.Clear();
            historyEntryElements.Clear();

            foreach (var historyRecord in _selectedCanvas.History.HistoryRecords)
            {
                HistoryEntryElement historyEntryElement = new HistoryEntryElement(window, historyRecord, _selectedCanvas.History);
                historyEntryElements.Add(historyEntryElement);
                if (historyEntryElement.HistoryRecord.isActiveRecord) ScrollToElement(historyEntryElement);
                _scrollView.Add(historyEntryElement);
            }
        }

        /// <summary>
        /// Scroll to specific element
        /// </summary>
        /// <param name="historyEntryElement"></param>
        void ScrollToElement(HistoryEntryElement historyEntryElement)
        {
            activeEntry = historyEntryElement;
            _scrollView.contentContainer.UnregisterCallback<GeometryChangedEvent>(OnScrollGeometryUpdated);
            _scrollView.contentContainer.RegisterCallback<GeometryChangedEvent>(OnScrollGeometryUpdated);
        }

        /// <summary>
        /// Finish scrolling when the geometry is recalculated
        /// </summary>
        /// <param name="evt"></param>
        private void OnScrollGeometryUpdated(GeometryChangedEvent evt)
        {
            if (activeEntry != null)
            {
                _scrollView.ScrollTo(activeEntry);
            }
            _scrollView.contentContainer.UnregisterCallback<GeometryChangedEvent>(OnScrollGeometryUpdated);
        }

        public override Vector2 DefaultSize
        {
            get{ return new Vector2(200, 200); }
        }

    }
}