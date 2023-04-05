using HueHades.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class ColorPickerGradient : HueHadesElement
    {
        private RenderTexture _displayTexture;
        private int2 _displaySize;
        private Image _displayImage;

        private GradientMode _gradientMode;
        public GradientMode Mode { get { return _gradientMode; } set { _gradientMode = value; RegenerateTexture(); } }

        private Color _hueColor;
        public Color HueColor { get { return _hueColor; } set { _hueColor = value; RegenerateTexture(); } }

        private bool _picking;

        float _pickerPosition;

        public Action<float> OnValueChanged;
        public float PickerPosition { get { return _pickerPosition; } set { _pickerPosition = value; UpdatePickerRelative(value); OnValueChanged?.Invoke(value); } }
        private Image _pickerCenter;

        private static Texture2D PickerIcon;
        private static Texture2D AlphaBackground;

        private const string ussGradientColorPickerDisplay = "gradient-color-picker-display";

        public ColorPickerGradient(HueHadesWindow window) : base(window)
        {

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<AttachToPanelEvent>(OnDetachFromPanel);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);

            _displayImage = new Image();
            hierarchy.Add(_displayImage);
            _displayImage.style.height = 16.0f;
            style.height = 16.0f;
            style.display = DisplayStyle.Flex;
            style.flexGrow = 0;
            style.marginTop = 8;
            style.marginBottom = 8;

            HueColor = Color.cyan;

            _pickerCenter = new Image();
            if (PickerIcon == null)
            {
                PickerIcon = Resources.Load<Texture2D>("Icons/GradientPickerIcon");
            }
            if (AlphaBackground == null)
            {
                AlphaBackground = Resources.Load<Texture2D>("Backgrounds/AlphaBackground");
            }
            _displayImage.style.backgroundImage = AlphaBackground;
            _displayImage.AddToClassList(ussGradientColorPickerDisplay);
            _displayImage.style.backgroundRepeat = new BackgroundRepeat(Repeat.Repeat,Repeat.Repeat);

            _pickerCenter.image = PickerIcon;

            _pickerCenter.style.position = Position.Absolute;
            _pickerCenter.style.width = 10.0f;
            _pickerCenter.style.height = 10.0f;
            _pickerCenter.pickingMode = PickingMode.Ignore;
            Add(_pickerCenter);
        }


        private void UpdatePicker(Vector2 position)
        {
            var localPosition = _displayImage.WorldToLocal(position);

            var pickerPosition = localPosition.x / Mathf.Max(1, _displayImage.worldBound.width);
            PickerPosition = Mathf.Clamp01(pickerPosition);
        }
        private void UpdatePickerRelative(float position)
        {
            _pickerPosition = Mathf.Clamp01(position);

            _pickerCenter.style.left = PickerPosition * _displayImage.worldBound.width - _pickerCenter.style.width.value.value * 0.5f;
            _pickerCenter.style.top = _displayImage.worldBound.height - _pickerCenter.style.height.value.value * 0.5f;
        }


        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_picking) return;
            UpdatePicker(evt.position);
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
        }

        private void RegenerateTexture()
        {
            if (_displayTexture == null) return;
            switch (_gradientMode)
            {
                case GradientMode.Color:
                    RenderTextureUtilities.Gradients.DrawColorGradient(_displayTexture, _displaySize.x, new Color(1, 1, 1, 0), Color.white);
                    break;
                case GradientMode.Hue:
                    RenderTextureUtilities.Gradients.DrawHueGradient(_displayTexture, _displaySize.x);
                    break;
            }
        }

        private void ResizeTexture(int2 size)
        {
            size.y = 1;
            if (_displayTexture != null)
            {
                RenderTextureUtilities.ReleaseTemporaryGradient(_displayTexture);

            }
            _displayTexture = RenderTextureUtilities.GetTemporaryGradient(size.x, RenderTextureFormat.ARGB32, out int availableSize);
            _displayImage.image = _displayTexture;
            _displayImage.uv = new Rect(0, 0, size.x / (float)availableSize, 16);
            _displaySize = size;
            RegenerateTexture();
        }



        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            var size = evt.newRect.size;
            ResizeTexture(new int2(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y)));
            UpdatePickerRelative(_pickerPosition);

            var bgSize = size.x * 0.8f;
            _displayImage.style.backgroundSize = new BackgroundSize(bgSize, bgSize);
        }

        private void OnDetachFromPanel(AttachToPanelEvent evt)
        {
            if (_displayTexture != null) RenderTextureUtilities.ReleaseTemporaryGradient(_displayTexture);
            _displayTexture = null;
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (_displaySize.x > 0 && _displaySize.y > 0)
            {
                ResizeTexture(_displaySize);
            }
        }

        public enum GradientMode
        {
            Color,
            Hue
        }


    }
}
