using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HueHades.Core
{
    public class CanvasHistory
    {
        private List<HistoryRecord> _historyRecords = new List<HistoryRecord>();
        private ImageCanvas _canvas;
        public List<HistoryRecord> HistoryRecords { get { return _historyRecords; } }

        public Action OnHistoryChange;

        public CanvasHistory(ImageCanvas canvas)
        {
            _canvas = canvas;
        }

        public void AddRecord(HistoryRecord historyRecord)
        {
            _historyRecords.Add(historyRecord);
            OnHistoryChange?.Invoke();
        }

        public void Undo()
        {

        }
        public void Redo()
        {

        }

        public void SelectRecord(HistoryRecord historyRecord)
        {

        }

    }

    public abstract class HistoryRecord
    {
        public virtual string name { get { return "Unknown"; } }
        public bool applied { get; protected set; }

        internal void Redo(ImageCanvas canvas)
        {
            if (applied) return;
            applied = true;
            OnRedo(canvas);
        }

        internal void Undo(ImageCanvas canvas)
        {
            if (!applied) return;
            applied = false;
            OnUndo(canvas);
        }

        protected abstract void OnRedo(ImageCanvas canvas);
        protected abstract void OnUndo(ImageCanvas canvas);
    }

    public class NewLayerHistoryRecord : HistoryRecord
    {
        public override string name { get { return "New Layer"; } }

        public NewLayerHistoryRecord(int layerIndex)
        {
            _layerIndex = layerIndex;
        }

        private int _layerIndex;
        protected override void OnRedo(ImageCanvas canvas)
        {
            canvas.AddLayer(_layerIndex);
        }

        protected override void OnUndo(ImageCanvas canvas)
        {
            canvas.RemoveLayer(_layerIndex);
        }
    }
}
