using HueHades.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HueHades.Core
{
    public class CanvasHistory : IDisposable
    {

        private ImageCanvas _canvas;

        /// <summary>
        /// Currently known history records
        /// </summary>
        public List<HistoryRecord> HistoryRecords { get { return _historyRecords; } }
        private List<HistoryRecord> _historyRecords = new List<HistoryRecord>();

        /// <summary>
        /// Called when the amount or state of history records have changed
        /// Mostly used for UI updates
        /// </summary>
        public Action OnHistoryChange;

        /// <summary>
        /// The record at the current state of the canvas
        /// </summary>
        private HistoryRecord _activeRecord;

        /// <summary>
        /// Maximum RAM usage for history in bytes - large canvases use a lot of memory
        /// </summary>
        private const long MaxHistoryMemory = 2000000000;
        private long _currentMemoryConsumption = 0;

        public CanvasHistory(ImageCanvas canvas)
        {
            _canvas = canvas;
        }

        /// <summary>
        /// Add a record to history
        /// </summary>
        /// <param name="historyRecord"></param>
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

        public void Dispose()
        {
            foreach (var record in HistoryRecords)
            {
                record.Dispose();
            }
            HistoryRecords.Clear();
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
            AppliedStateChanged?.Invoke(applied);
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

        public NewLayerHistoryRecord(int globalContainerIndex, int relativeLayerIndex, int newGlobalIndexOfLayer, Color clearColor)
        {
            _globalContainerIndex = globalContainerIndex;
            _newGlobalIndexOfLayer = newGlobalIndexOfLayer;
            _relativeLayerIndex = relativeLayerIndex;
            _clearColor = clearColor;
        }

        private int _globalContainerIndex;
        private int _newGlobalIndexOfLayer;
        private int _relativeLayerIndex;

        protected override void OnRedo(ImageCanvas canvas)
        {
            canvas.AddLayer(_globalContainerIndex, _relativeLayerIndex, _clearColor);
        }

        protected override void OnUndo(ImageCanvas canvas)
        {
            canvas.RemoveLayer(_newGlobalIndexOfLayer);
        }

        public override void Dispose() { }
    }




    public class DuplicateLayerHistoryRecord : HistoryRecord
    {
        public override string name { get { return "Duplicate Layer"; } }
        public override int MemoryConsumption { get { return _snapshot.width * _snapshot.height * sizeof(float) * 4; } }

        private ReusableTexture _snapshot;
        private LayerSettings _layerSettings;

        public DuplicateLayerHistoryRecord(ImageLayer duplicatedLayer, int globalContainerIndex, int relativeLayerIndex, int newGlobalIndexOfLayer)
        {
            _globalContainerIndex = globalContainerIndex;
            _newGlobalIndexOfLayer = newGlobalIndexOfLayer;
            _relativeLayerIndex = relativeLayerIndex;
            _layerSettings = duplicatedLayer.LayerSettings;
            _snapshot = RenderTextureUtilities.GetTemporary(duplicatedLayer.Dimensions.x, duplicatedLayer.Dimensions.y, duplicatedLayer.Format);
            RenderTextureUtilities.CopyTexture(duplicatedLayer.Texture, _snapshot);
        }

        private int _globalContainerIndex;
        private int _newGlobalIndexOfLayer;
        private int _relativeLayerIndex;

        protected override void OnRedo(ImageCanvas canvas)
        {
            var layer = canvas.AddLayer(_globalContainerIndex, _relativeLayerIndex, Color.clear);
            layer.SetLayerSettings(_layerSettings, false);
            RenderTextureUtilities.CopyTexture(_snapshot, layer.Texture);
        }

        protected override void OnUndo(ImageCanvas canvas)
        {
            canvas.RemoveLayer(_newGlobalIndexOfLayer);
        }

        public override void Dispose() {
            _snapshot.Dispose();
        }
    }










    public class MoveLayerHistoryRecord : HistoryRecord
    {
        public override string name { get { return "Move Layer"; } }
        public override int MemoryConsumption { get { return 1; } }

        public MoveLayerHistoryRecord(int globalContainerIndex, int relativeLayerIndex, int globalLayerIndex, int newGlobalContainerIndex, int newRelativeLayerIndex, int newGlobalLayerIndex)
        {
            _globalContainerIndex = globalContainerIndex;
            _relativeLayerIndex = relativeLayerIndex;
            _globalLayerIndex = globalLayerIndex;
            _newGlobalContainerIndex = newGlobalContainerIndex;
            _newRelativeLayerIndex = newRelativeLayerIndex;
            _newGlobalLayerIndex = newGlobalLayerIndex;
        }

        private int _globalContainerIndex;
        private int _relativeLayerIndex;
        private int _globalLayerIndex;
        private int _newGlobalContainerIndex;
        private int _newRelativeLayerIndex;
        private int _newGlobalLayerIndex;

        protected override void OnRedo(ImageCanvas canvas)
        {
            canvas.MoveLayer(_globalLayerIndex, _newGlobalContainerIndex, _newRelativeLayerIndex);
        }

        protected override void OnUndo(ImageCanvas canvas)
        {
            canvas.MoveLayer(_newGlobalLayerIndex, _globalContainerIndex, _relativeLayerIndex);
        }

        public override void Dispose() { }
    }





    public class RemoveLayerHistoryRecord : HistoryRecord
    {
        public override string name { get { return "Remove Layer"; } }
        public override int MemoryConsumption { get { return _layer.Dimensions.x * _layer.Dimensions.y * 4 * sizeof(float); } }


        public RemoveLayerHistoryRecord(LayerBase layer, int globalContainerIndex, int relativeLayerIndex, int globalLayerIndex)
        {
            _globalContainerIndex = globalContainerIndex;
            _globalLayerIndex = globalLayerIndex;
            _relativeLayerIndex = relativeLayerIndex;
            _layer = layer;
        }

        private int _globalContainerIndex;
        private int _globalLayerIndex;
        private int _relativeLayerIndex;
        private LayerBase _layer;

        protected override void OnRedo(ImageCanvas canvas)
        {
            canvas.RemoveLayer(_globalLayerIndex);
        }

        protected override void OnUndo(ImageCanvas canvas)
        {
            canvas.AddLayer(_layer, _globalContainerIndex, _relativeLayerIndex);
        }

        public override void Dispose() {
            if(applied) _layer.Dispose();
        }
    }


    public class ModifyLayerHistoryRecord : HistoryRecord
    {
        private string _name;
        public override string name { get { return _name; } }
        private ReusableTexture _result;
        private ReusableTexture _input;

        public override int MemoryConsumption { get { return _result.width * _result.height * 4 * 2 * sizeof(float); } }

        public ModifyLayerHistoryRecord(int globalLayerIndex, ReusableTexture beforeLayer, ReusableTexture afterLayer, string recordName)
        {
            _globalLayerIndex = globalLayerIndex;
            _name = recordName;
            _input = RenderTextureUtilities.GetTemporary(beforeLayer.width, beforeLayer.height, beforeLayer.format);
            _result = RenderTextureUtilities.GetTemporary(afterLayer.width, afterLayer.height, afterLayer.format);
            
            RenderTextureUtilities.CopyTexture(beforeLayer, _input);
            RenderTextureUtilities.CopyTexture(afterLayer, _result);
        }

        private int _globalLayerIndex;
        protected override void OnRedo(ImageCanvas canvas)
        {
            var layer = canvas.GetLayerByGlobalID(_globalLayerIndex);
            RenderTextureUtilities.CopyTexture(_result, layer.Texture);
            canvas.RenderPreview();
        }

        protected override void OnUndo(ImageCanvas canvas)
        {
            var layer = canvas.GetLayerByGlobalID(_globalLayerIndex);
            RenderTextureUtilities.CopyTexture(_input, layer.Texture);
            canvas.RenderPreview();
        }

        public override void Dispose() { 
            RenderTextureUtilities.ReleaseTemporary( _result );
            RenderTextureUtilities.ReleaseTemporary( _input );
        }
    }


    public class ModifyLayerSettingsHistoryRecord : HistoryRecord
    {
        public override string name { get { return "Changed Layer Settings"; } }
        public override int MemoryConsumption { get { return 1; } }

        private LayerSettings _oldSettings;
        private LayerSettings _newSettings;

        public ModifyLayerSettingsHistoryRecord(int globalIndexOfLayer, LayerSettings oldLayerSettings, LayerSettings newLayerSettings)
        {
            _globalIndexOfLayer = globalIndexOfLayer;
            _oldSettings = oldLayerSettings;
            _newSettings = newLayerSettings;
        }

        private int _globalIndexOfLayer;

        protected override void OnRedo(ImageCanvas canvas)
        {
            canvas.GetLayerByGlobalID(_globalIndexOfLayer).SetLayerSettings(_newSettings, false);
        }

        protected override void OnUndo(ImageCanvas canvas)
        {
            canvas.GetLayerByGlobalID(_globalIndexOfLayer).SetLayerSettings(_oldSettings, false);
        }

        public override void Dispose() { }
    }

}
