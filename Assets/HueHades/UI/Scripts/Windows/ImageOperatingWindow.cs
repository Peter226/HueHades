using HueHades.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

namespace HueHades.UI
{
    public class ImageOperatingWindow : DockableWindow
    {
        private Image _imageDisplay;
        private const string ussOperatingWindow = "operating-window";
        private const string ussOperatingWindowImage = "operating-window-image";
        private ImageCanvas _imageCanvas;

        public ImageOperatingWindow(HueHadesWindow window, ImageCanvas imageCanvas) : base(window)
        {
            _imageDisplay = new Image();
            _imageDisplay.AddToClassList(ussOperatingWindowImage);
            _imageCanvas = imageCanvas;
            _imageDisplay.image = imageCanvas.PreviewTexture;
            _imageDisplay.style.width = imageCanvas.Dimensions.x;
            _imageDisplay.style.height = imageCanvas.Dimensions.y;
            _imageDisplay.uv = new Rect(0,0,1,1);
            hierarchy.Add(_imageDisplay);
            AddToClassList(ussOperatingWindow);
            RegisterCallback<WheelEvent>(WheelCallback);
            RegisterCallback<MouseMoveEvent>(DragMouseCallback);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

       


        private Vector2 GetPixelPosition(Vector2 pointerPosition)
        {
            var pos = _imageDisplay.contentContainer.WorldToLocal(pointerPosition);
            pos.x /= _imageDisplay.contentRect.width;
            pos.y /= _imageDisplay.contentRect.height;
            pos.y = 1.0f - pos.y;
            pos.x *= _imageCanvas.Dimensions.x;
            pos.y *= _imageCanvas.Dimensions.y;
            return pos;
        }



        private void OnPointerDown(PointerDownEvent pointerDownEvent)
        {
            this.CapturePointer(pointerDownEvent.pointerId);
            var tools = window.ToolsWindow;
            if (tools == null) return;
            var pressure = pointerDownEvent.pointerType == UnityEngine.UIElements.PointerType.pen ? pointerDownEvent.pressure : 1.0f;
            //TODO: insert layer when layer window is done
            //TODO: check pressure and tilt values to be correct
            tools.OnToolBeginUse(_imageCanvas, 0, GetPixelPosition(pointerDownEvent.position), pressure, pointerDownEvent.altitudeAngle);
        }

        private void OnPointerMove(PointerMoveEvent pointerMoveEvent) {
            
            var tools = window.ToolsWindow;
            if (tools == null) return;
            var pressure = pointerMoveEvent.pointerType == UnityEngine.UIElements.PointerType.pen ? pointerMoveEvent.pressure : 1.0f;
            tools.OnToolUseUpdate(GetPixelPosition(pointerMoveEvent.position), pressure, pointerMoveEvent.altitudeAngle);
        }


        private void OnPointerUp(PointerUpEvent pointerUpEvent)
        {
            this.ReleasePointer(pointerUpEvent.pointerId);
            var tools = window.ToolsWindow;
            if (tools == null) return;
            var pressure = pointerUpEvent.pointerType == UnityEngine.UIElements.PointerType.pen ? pointerUpEvent.pressure : 1.0f;
            tools.OnToolEndUse(GetPixelPosition(pointerUpEvent.position), pressure, pointerUpEvent.altitudeAngle);
        }

        public override string GetWindowName()
        {
            return "Image.png";
        }



        void WheelCallback(WheelEvent e)
        {
            _imageDisplay.transform.scale *= 1 - (e.delta.y * 0.1f * _imageDisplay.transform.scale.x);
        }

        void DragMouseCallback(MouseMoveEvent e)
        {
            if (e.ctrlKey && IsMouseButtonPressed(e.pressedButtons, MouseButton.Middle)) 
            {
                var currentPosition = e.mousePosition;
                var lastPosition = currentPosition - e.mouseDelta;

                if (lastPosition != Vector2.zero && currentPosition != Vector2.zero) {

                   
                    _imageDisplay.transform.rotation *= Quaternion.Euler(0, 0, e.mouseDelta.x);
                }
            } 
            else
            {
                if (IsMouseButtonPressed(e.pressedButtons, MouseButton.Middle)) _imageDisplay.transform.position += new Vector3(e.mouseDelta.x, e.mouseDelta.y, 0);
            }
        }


        void OnImageChange()
        {

        }

        private bool IsMouseButtonPressed(int pressedButtons, MouseButton mouseButton)
        {
            if ((pressedButtons & (int)mouseButton) != 0) return true;
            return false;
        }


        [Flags]
        private enum MouseButton
        {
            Left = 1,
            Right = 2,
            Middle = 4
        }

    }
}