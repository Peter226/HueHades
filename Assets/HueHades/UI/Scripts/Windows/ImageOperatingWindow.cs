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
        private ImageCanvas _imageCanvas;

        public ImageOperatingWindow(HueHadesWindow window, ImageCanvas imageCanvas) : base(window)
        {
            _imageDisplay = new Image();
            _imageCanvas = imageCanvas;
            _imageDisplay.image = imageCanvas.PreviewTexture;
            _imageDisplay.uv = new Rect(0,0,1,1);
            hierarchy.Add(_imageDisplay);
            AddToClassList(ussOperatingWindow);
            RegisterCallback<WheelEvent>(WheelCallback);
            RegisterCallback<MouseMoveEvent>(DragMouseCallback);
            RegisterCallback<PointerDownEvent>(OnPointerDowm);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
        }


        private Vector2 GetPixelPosition(Vector2 pointerPosition)
        {
            var pos = _imageDisplay.WorldToLocal(pointerPosition);
            pos /= _imageDisplay.localBound.size;
            pos.y = 1.0f - pos.y;
            pos.x = pos.x * _imageCanvas.Dimensions.x;
            pos.y = pos.y * _imageCanvas.Dimensions.y;
            return pos;
        }



        private void OnPointerDowm(PointerDownEvent pointerDownEvent)
        {
            this.CapturePointer(pointerDownEvent.pointerId);
            var tools = window.ToolsWindow;
            if (tools == null) return;
            //TODO: insert layer when layer window is done
            //TODO: check pressure and tilt values to be correct
            tools.OnToolBeginUse(_imageCanvas, 0, GetPixelPosition(pointerDownEvent.position), pointerDownEvent.pressure, pointerDownEvent.altitudeAngle);
        }

        private void OnPointerMove(PointerMoveEvent pointerMoveEvent) {
            var tools = window.ToolsWindow;
            if (tools == null) return;
            tools.OnToolUseUpdate(GetPixelPosition(pointerMoveEvent.position), pointerMoveEvent.pressure, pointerMoveEvent.altitudeAngle);
        }


        private void OnPointerUp(PointerUpEvent pointerUpEvent)
        {
            this.ReleasePointer(pointerUpEvent.pointerId);
            var tools = window.ToolsWindow;
            if (tools == null) return;
            tools.OnToolEndUse(GetPixelPosition(pointerUpEvent.position), pointerUpEvent.pressure, pointerUpEvent.altitudeAngle);
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