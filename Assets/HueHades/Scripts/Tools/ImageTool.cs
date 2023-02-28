using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HueHades.Core;

namespace HueHades.Tools {
    public abstract class ImageTool {

        private ImageCanvas _canvas;
        protected ImageCanvas Canvas { get { return _canvas; } }

        public void Select(ImageCanvas canvas)
        {
            _canvas = canvas;
        }

        public void Deselect()
        {
            OnDeselected();
            _canvas = null;
        }

        public abstract void OnSelected();

        public abstract void OnDeselected();

        
    }
}