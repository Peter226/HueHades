
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class TransformImageHandle : VisualElement
    {
        private const string ussTransformImageHandle = "transform-image-handle";

        private List<DragHandle> dragHandles = new List<DragHandle>();

        private bool _dragging;


        private Vector3 _startPosition;


        public Action TransformChanged;

        private Vector3 _positionDelta;
        public Vector3 PositionDelta => _positionDelta;



        public TransformImageHandle()
        {
            AddToClassList(ussTransformImageHandle);

            dragHandles.Add(new DragHandle(-1, 1));
            dragHandles.Add(new DragHandle(0, 1));
            dragHandles.Add(new DragHandle(1, 1));
            dragHandles.Add(new DragHandle(1, 0));
            dragHandles.Add(new DragHandle(1, -1));
            dragHandles.Add(new DragHandle(0, -1));
            dragHandles.Add(new DragHandle(-1, -1));
            dragHandles.Add(new DragHandle(-1, 0));

            foreach (var dragHandle in dragHandles)
            {
                Add(dragHandle);
            }

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            _dragging = false;
            this.ReleasePointer(evt.pointerId);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_dragging) return;
            transform.position += evt.deltaPosition;
            _positionDelta = transform.position - _startPosition;
            TransformChanged?.Invoke();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            _dragging = true;
            this.CapturePointer(evt.pointerId);
            evt.StopImmediatePropagation();
        }



        public void Initialize(ImageOperatingWindow imageOperatingWindow)
        {
            var bounds = imageOperatingWindow.Canvas.Selection.SelectedAreaBounds;
            Vector2 selectionCenter = new Vector2((bounds.z + bounds.x) * 0.5f, (bounds.w + bounds.y) * 0.5f);
            var center = imageOperatingWindow.GetScreenPosition(selectionCenter);
            transform.position = new Vector3(center.x, center.y, 0.0f);

            var boundsOffsetX = new Vector2((bounds.z - bounds.x) * 0.5f, 0);
            var boundsOffsetY = new Vector2(0, (bounds.w - bounds.y) * 0.5f);
            var centerDeltaX = imageOperatingWindow.GetScreenPosition(selectionCenter + boundsOffsetX);
            var centerDeltaY = imageOperatingWindow.GetScreenPosition(selectionCenter + boundsOffsetY);

            var deltaX = Vector3.Distance(center, centerDeltaX);
            var deltaY = Vector3.Distance(center, centerDeltaY);

            transform.rotation = Quaternion.LookRotation(new Vector3(0,0,1), -Vector3.Normalize(centerDeltaY - center));

            style.width = deltaX * 2;
            style.height = deltaY * 2;
            style.left = -deltaX;
            style.top = -deltaY;

            foreach (var dragHandle in dragHandles)
            {
                dragHandle.Initialize(imageOperatingWindow, this, bounds, selectionCenter);
            }

            _startPosition = transform.position;
        }

        private class DragHandle : VisualElement
        {
            private const string ussDragHandle = "transform-image-handle-dragger";
            private const string ussDragHandleImage = "transform-image-handle-dragger-image";
            private Image _image;

            private int _multiplierX;
            private int _multiplierY;

            public DragHandle(int multiplierX, int multiplierY,  string icon = "Icons/TransformHandle")
            {
                AddToClassList(ussDragHandle);
                _image = new Image();
                _image.AddToClassList(ussDragHandleImage);
                _image.image = Icons.GetIcon(icon);
                Add(_image);
                _multiplierX = multiplierX;
                _multiplierY = multiplierY;
            }

            public void Initialize(ImageOperatingWindow imageOperatingWindow, TransformImageHandle transformHandle, int4 bounds, Vector2 selectionCenter)
            {

                if (_multiplierX == -1)
                {
                    style.left = -1;
                }
                if (_multiplierX == 1)
                {
                    style.right = -1;
                }

                if (_multiplierY == -1)
                {
                    style.bottom = -1;
                }
                if (_multiplierY == 1)
                {
                    style.top = -1;
                }
            }
        }

    }
}
