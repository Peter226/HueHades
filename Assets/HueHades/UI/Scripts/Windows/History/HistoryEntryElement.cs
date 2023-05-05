using HueHades.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class HistoryEntryElement : HueHadesElement
    {
        private Button _selfButton;
        private const string ussHistoryElementButton = "history-element-button";
        private const string ussHistoryElement = "history-element";
        private const string ussHistoryElementLabel = "history-element-label";
        private const string ussHistoryElementButtonSelected = "history-element-button-selected";
        private const string ussHistoryElementButtonInactive = "history-element-button-inactive";
        private HistoryRecord _historyRecord;
        public HistoryRecord HistoryRecord { get { return _historyRecord; } }
        private CanvasHistory _canvasHistory;

        public Action<HistoryEntryElement> EntryActivated;

        public HistoryEntryElement(HueHadesWindow window, HistoryRecord historyRecord, CanvasHistory canvasHistory) : base(window)
        {
            AddToClassList(ussHistoryElement);
            _selfButton = new Button();
            _selfButton.AddToClassList(ussHistoryElementButton);
            _selfButton.clicked += OnElementClicked;

            hierarchy.Add(_selfButton);
            Label label = new Label();
            label.text = historyRecord.name;
            label.AddToClassList(ussHistoryElementLabel);
            _selfButton.Add(label);
            historyRecord.AppliedStateChanged += OnAppliedStateChanged;
            historyRecord.ActiveStateChanged += OnActiveStateChanged;
            if (!historyRecord.applied) _selfButton.AddToClassList(ussHistoryElementButtonInactive);
            if (historyRecord.isActiveRecord) _selfButton.AddToClassList(ussHistoryElementButtonSelected);
            _historyRecord = historyRecord;
            _canvasHistory = canvasHistory;
        }

        private void OnActiveStateChanged(bool active)
        {
            if (!active)
            {
                _selfButton.RemoveFromClassList(ussHistoryElementButtonSelected);
            }
            else
            {
                _selfButton.AddToClassList(ussHistoryElementButtonSelected);
                EntryActivated?.Invoke(this);
            }
        }

        private void OnElementClicked()
        {
            if (_historyRecord.isActiveRecord) return;
            _canvasHistory.SelectRecord(_historyRecord);
        }

        private void OnAppliedStateChanged(bool applied)
        {
            if (applied)
            {
                _selfButton.RemoveFromClassList(ussHistoryElementButtonInactive);
            }
            else
            {
                _selfButton.AddToClassList(ussHistoryElementButtonInactive);
            }
        }
    }
}