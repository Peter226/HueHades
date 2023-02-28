using HueHades.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
            hierarchy.Add(_imageDisplay);
            AddToClassList(ussOperatingWindow);
            RegisterCallback<WheelEvent>(WheelCallback);
            RegisterCallback<MouseMoveEvent>(DragMouseCallback);
        }

        public override string GetWindowName()
        {
            return "Image.png";
        }



        void WheelCallback(WheelEvent e)
        {
            _imageDisplay.transform.scale *= 1 - (e.delta.y * 0.03f);
        }

        void DragMouseCallback(MouseMoveEvent e)
        {
            if (e.ctrlKey && IsMouseButtonPressed(e.pressedButtons, MouseButton.Middle)) { _imageDisplay.transform.rotation *= Quaternion.Euler(0, 0, e.mouseDelta.x); } else
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