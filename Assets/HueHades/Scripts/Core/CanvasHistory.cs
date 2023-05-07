using HueHades.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HueHades.Core
{
    public class CanvasHistory
    {
        private List<HistoryRecord> _historyRecords = new List<HistoryRecord>();
        private ImageCanvas _canvas;
        public List<HistoryRecord> HistoryRecords { get { return _historyRecords; } }

        public Action OnHistoryChange;
        private HistoryRecord _activeRecord;


        private const long MaxHistoryMemory = 2000000000;
        private long _currentMemoryConsumption = 0;

        public CanvasHistory(ImageCanvas canvas)
        {
            _canvas = canvas;
        }

        public void AddRecord(HistoryRecord historyRecord)
        {
            _currentMemoryConsumption += historyRecord.MemoryConsumption;
            for (int i = _historyRecords.Count - 1;i >= 0;i--)
            {
                var record = _historyRecords[i];
                if (!record.applied)
                {
                    var recordToDispose = _historyRecords[i];
                    _currentMemoryConsumption -= recordToDispose.MemoryConsumption;
                    recordToDispose.Dispose();
                    _historyRecords.RemoveAt(i);
                }
                else
                {
                    break;
                }
            }

            if (_activeRecord != null) _activeRecord.isActiveRecord = false;
            _activeRecord = historyRecord;
            _activeRecord.isActiveRecord = true;

            _historyRecords.Add(historyRecord);

            while (_currentMemoryConsumption > MaxHistoryMemory && _historyRecords.Count > 1)
            {
                var recordToDispose = _historyRecords[0];
                _currentMemoryConsumption -= recordToDispose.MemoryConsumption;
                recordToDispose.Dispose();
                _historyRecords.RemoveAt(0);
            }

            OnHistoryChange?.Invoke();
        }

        public void Undo()
        {
            if (_historyRecords.Count <= 1) return;
            if (_activeRecord == _historyRecords[0]) return;
            _activeRecord.Undo(_canvas);
            var newActiveRecord = _historyRecords[_historyRecords.IndexOf(_activeRecord) - 1];

            _activeRecord.isActiveRecord = false;
            _activeRecord = newActiveRecord;
            _activeRecord.isActiveRecord = true;
        }
        public void Redo()
        {
            if(_historyRecords.Count <= 1) return;
            if (_activeRecord == _historyRecords[_historyRecords.Count - 1]) return;
            
            var newActiveRecord = _historyRecords[_historyRecords.IndexOf(_activeRecord) + 1];
            newActiveRecord.Redo(_canvas);

            _activeRecord.isActiveRecord = false;
            _activeRecord = newActiveRecord;
            _activeRecord.isActiveRecord = true;
        }

        public void SelectRecord(HistoryRecord historyRecord)
        {
            if (_historyRecords.Count <= 1) return;
            _activeRecord.isActiveRecord = false;
            if (historyRecord.applied)
            {
                foreach (var record in _historyRecords.Reverse<HistoryRecord>())
                {
                    if (record != historyRecord)
                    {
                        if (record.applied) record.Undo(_canvas);
                    }
                    else
                    {
                        record.isActiveRecord = true;
                        _activeRecord = record;
                        break;
                    }
                }
            }
            else
            {
                foreach (var record in _historyRecords)
                {
                    if (record != historyRecord)
                    {
                        if(!record.applied) record.Redo(_canvas);
                    }
                    else
                    {
                        record.Redo(_canvas);
                        record.isActiveRecord = true;
                        _activeRecord = record;
                        break;
                    }
                }
            }

            

        }

    }

    public abstract class HistoryRecord : IDisposable
    {
        public abstract int MemoryConsumption { get; }

        public virtual string name { get { return "Unknown"; } }
        private bool _applied = true;
        public bool applied { get { return _applied; } protected set { _applied = value; } }
        private bool _isActiveRecord;
        public bool isActiveRecord { get { return _isActiveRecord; } set { bool changed = _isActiveRecord != value; _isActiveRecord = value; if (changed) ActiveStateChanged?.Invoke(_isActiveRecord); } }

        /// <summary>
        /// Called when the record is applied, or undone
        /// </summary>
        public Action<bool> AppliedStateChanged;
        public Action<bool> ActiveStateChanged;

        internal void Redo(ImageCanvas canvas)
        {
            if (applied) return;
            applied = true;
            OnRedo(canvas);
            AppliedStateChanged.Invoke(applied);
        }

        internal void Undo(ImageCanvas canvas)
        {
            if (!applied) return;
            applied = false;
            OnUndo(canvas);
            AppliedStateChanged?.Invoke(applied);
        }

        protected abstract void OnRedo(ImageCanvas canvas);
        protected abstract void OnUndo(ImageCanvas canvas);

        public abstract void Dispose();
    }

    public class NewLayerHistoryRecord : HistoryRecord
    {
        public override string name { get { return "New Layer"; } }
        public override int MemoryConsumption { get { return 1; } }

        private Color _clearColor;

        public NewLayerHistoryRecord(int layerIndex, Color clearColor)
        {
            _layerIndex = layerIndex;
            _clearColor = clearColor;
        }

        private int _layerIndex;
        protected override void OnRedo(ImageCanvas canvas)
        {
            canvas.AddLayer(_layerIndex, _clearColor);
        }

        protected override void OnUndo(ImageCanvas canvas)
        {
            canvas.RemoveLayer(_layerIndex);
        }

        public override void Dispose() { }
    }

    public class ModifyLayerHistoryRecord : HistoryRecord
    {
        private string _name;
        public override string name { get { return _name; } }
        private ReusableTexture _result;
        private ReusableTexture _input;

        public override int MemoryConsumption { get { return _result.width * _result.height * 4 * 2 * sizeof(float); } }

        public ModifyLayerHistoryRecord(int layerIndex, ReusableTexture beforeLayer, ReusableTexture afterLayer, string recordName)
        {
            _layerIndex = layerIndex;
            _name = recordName;
            _input = RenderTextureUtilities.GetTemporary(beforeLayer.width, beforeLayer.height, beforeLayer.format);
            _result = RenderTextureUtilities.GetTemporary(afterLayer.width, afterLayer.height, afterLayer.format);
            
            RenderTextureUtilities.CopyTexture(beforeLayer, _input);
            RenderTextureUtilities.CopyTexture(afterLayer, _result);
        }

        private int _layerIndex;
        protected override void OnRedo(ImageCanvas canvas)
        {
            var layer = canvas.GetLayer(_layerIndex);
            RenderTextureUtilities.CopyTexture(_result, layer.Texture);
            canvas.RenderPreview();
        }

        protected override void OnUndo(ImageCanvas canvas)
        {
            var layer = canvas.GetLayer(_layerIndex);
            RenderTextureUtilities.CopyTexture(_input, layer.Texture);
            canvas.RenderPreview();
        }

        public override void Dispose() { 
            RenderTextureUtilities.ReleaseTemporary( _result );
            RenderTextureUtilities.ReleaseTemporary( _input );
        }
    }


}
