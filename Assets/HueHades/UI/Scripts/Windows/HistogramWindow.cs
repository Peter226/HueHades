using HueHades.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HueHades.UI
{
    public class HistogramWindow : DockableWindow
    {
        HistogramDisplayElement _displayElement;

        CanvasSelector _selector;


        public HistogramWindow(HueHadesWindow window) : base(window)
        {
            _selector = new CanvasSelector(window);
            _displayElement = new HistogramDisplayElement(window);

            _selector.CanvasSelected += OnCanvasSelected;
            contentContainer.Add(_selector);
            contentContainer.Add(_displayElement);
            contentContainer.AddToClassList(Layouts.Vertical);

            WindowName = "Histogram";
        }

        private void OnCanvasSelected(ImageCanvas canvas)
        {
            _displayElement.SourceCanvas = canvas;
        }
    }
}
