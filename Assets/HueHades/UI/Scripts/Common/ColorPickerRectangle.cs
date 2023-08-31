using HueHades.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class ColorPickerRectangle : HueHadesElement
    {

        private ReusableTexture _displayTexture;
        private int2 _displaySize;
        private Image _displayImage;

        private Color _hueColor;
        public Color HueColor { get { return _hueColor; } set { _hueColor = value; RegenerateTexture(); } }

        private bool _picking;

        public Action<Vector2> OnValueChanged;
        public Action<Vector2> OnValueChangedByUser;

        Vector2 _pickerPosition;
        public Vector2 PickerPosition { get { return _pickerPosition; } set { _pickerPosition = value; UpdatePickerRelative(value); OnValueChanged?.Invoke(value); } }

        private Image _pickerCenter;

        private static Texture2D PickerIcon;

        public ColorPickerRectangle(HueHadesWindow window) : base(window)
        {

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);

            _displayImage = new Image();
            hierarchy.Add(_displayImage);
            style.display = DisplayStyle.Flex;
            style.flexGrow = 1;
            HueColor = Color.cyan;

            _pickerCenter = new Image();
            if (PickerIcon == null)
            {
                PickerIcon = Resources.Load<Texture2D>("Icons/PickerCenterIcon");
            }
            _pickerCenter.image = PickerIcon;

            _pickerCenter.style.position = Position.Absolute;
            _pickerCenter.style.width = 10.0f;
            _pickerCenter.style.height = 10.0f;
            _pickerCenter.pickingMode = PickingMode.Ignore;
            Add(_pickerCenter);

            _pickerPosition = Vector2.one;
        }


        private void UpdatePicker(Vector2 position)
        {
            var localPosition = _displayImage.WorldToLocal(position);

            _pickerPosition = new Vector2(localPosition.x / Mathf.Max(1, _displayImage.worldBound.width), localPosition.y / Mathf.Max(1,_displayImage.worldBound.height));
            _pickerPosition.x = Mathf.Clamp01(_pickerPosition.x);
            _pickerPosition.y = Mathf.Clamp01(_pickerPosition.y);

            _pickerCenter.style.left = _pickerPosition.x * _displayImage.worldBound.width - _pickerCenter.style.width.value.value * 0.5f;
            _pickerCenter.style.top = _pickerPosition.y * _displayImage.worldBound.height - _pickerCenter.style.height.value.value * 0.5f;
        }
        private void UpdatePickerRelative(Vector2 position)
        {
            _pickerPosition.x = Mathf.Clamp01(position.x);
            _pickerPosition.y = Mathf.Clamp01(position.y);

            _pickerCenter.style.left = _pickerPosition.x * _displayImage.worldBound.width - _pickerCenter.style.width.value.value * 0.5f;
            _pickerCenter.style.top = _pickerPosition.y * _displayImage.worldBound.height - _pickerCenter.style.height.value.value * 0.5f;
        }


        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_picking) return;
            UpdatePicker(evt.position);
            OnValueChangedByUser?.Invoke(PickerPosition);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            _picking = false;
            this.ReleasePointer(evt.pointerId);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            _picking = true;
            this.CapturePointer(evt.pointerId);
            UpdatePicker(evt.position);
            OnValueChangedByUser?.Invoke(PickerPosition);
        }

        private void RegenerateTexture()
        {
            if (_displayTexture == null) return;
            RenderTextureUtilities.Gradients.DrawColorGradientRectangle(_displayTexture, _displaySize.x, _displaySize.y, Color.white, _hueColor, Color.black, Color.black);
        }

        private void ResizeTexture(int2 size)
        {
            if (_displayTexture != null)
            {
                RenderTextureUtilities.ReleaseTemporary(_displayTexture);
                
            }
            _displayTexture = RenderTextureUtilities.GetTemporary(size.x, size.y, RenderTextureFormat.ARGB32);
            _displayImage.image = _displayTexture.texture;
            _displayImage.uv = new Rect(0,0, size.x / (float)_displayTexture.actualWidth, size.y / (float)_displayTexture.actualHeight);
            _displaySize = size;
            RegenerateTexture();
        }



        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            var size = evt.newRect.size;
            ResizeTexture(new int2(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y)));
            UpdatePickerRelative(_pickerPosition);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            if(_displayTexture != null) RenderTextureUtilities.ReleaseTemporary( _displayTexture );
            _displayTexture = null;
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (_displaySize.x > 0 && _displaySize.y > 0)
            {
                ResizeTexture(_displaySize);
            }
        }
    }
}
