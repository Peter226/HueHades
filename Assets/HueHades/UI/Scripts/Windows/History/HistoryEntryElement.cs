using HueHades.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class HistoryEntryElement : HueHadesElement
    {
        public HistoryEntryElement(HueHadesWindow window, HistoryRecord historyRecord) : base(window)
        {
            Label label = new Label();
            label.text = historyRecord.name;
            hierarchy.Add(label);
        }
    }
}