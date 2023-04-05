using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HueHades.Core;

namespace HueHades.Tools {
    public abstract class ImageTool {

        private bool _isSelected;
        private bool _isUsing;


        public abstract Texture GetIcon();



        public void Select()
        {
            if (_isSelected) return;
            _isSelected = true;
            OnSelected();
        }

        public void Deselect()
        {
            if (!_isSelected) return;
            _isSelected = false;
            OnDeselected();
        }

        protected abstract void OnSelected();

        protected abstract void OnDeselected();


        public void BeginUse(IToolContext toolContext, ImageCanvas canvas, int layer, Vector2 startPoint, float startPressure, float startTilt)
        {
            if (_isUsing) return;
            _isUsing = true;
            OnBeginUse(toolContext, canvas, layer, startPoint, startPressure, startTilt);
        }
        public void UseUpdate(Vector2 currentPoint, float currentPressure, float currentTilt)
        {
            if (_isUsing) OnUseUpdate(currentPoint, currentPressure, currentTilt); ;
        }
        public void EndUse(Vector2 endPoint, float endPressure, float endTilt)
        {
            if (!_isUsing) return;
            _isUsing = false;
            OnEndUse(endPoint, endPressure, endTilt);
        }

        protected abstract void OnBeginUse(IToolContext toolContext, ImageCanvas canvas, int layer, Vector2 startPoint, float startPressure, float startTilt);
        protected abstract void OnUseUpdate(Vector2 currentPoint, float currentPressure, float currentTilt);
        protected abstract void OnEndUse(Vector2 endPoint, float endPressure, float endTilt);
        
    }
}