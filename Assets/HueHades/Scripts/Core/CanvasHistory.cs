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

        public CanvasHistory(ImageCanvas canvas)
        {
            _canvas = canvas;
        }

        public void Undo()
        {

        }
        public void Redo()
        {

        }


        private class HistoryRecord
        {
            private List<ImageLayer> _layers;
        }


    }
}
