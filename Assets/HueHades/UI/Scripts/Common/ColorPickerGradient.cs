using HueHades.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class ColorPickerGradient : HueHadesElement
    {
        private ReusableTexture _displayTexture;
        private int2 _displaySize;
        private Image _displayImage;

        private GradientMode _gradientMode;
        public GradientMode Mode { get { return _gradientMode; } set { _gradientMode = value; RegenerateTexture(); } }

        private bool _picking;

        float _pickerPosition;

        public Action<float> ValueChanged;
        public Action<float> ValueChangedByUser;
        public float PickerPosition { get { return _pickerPosition; } set { _pickerPosition = value; UpdatePickerRelative(value); ValueChanged?.Invoke(value); _textFieldElement.value = value.ToString("0.###", CultureInfo.InvariantCulture); } }
        private Image _pickerCenter;

        private static Texture2D PickerIcon;
        private static Texture2D AlphaBackground;

        private const string ussGradientColorPickerDisplay = "gradient-color-picker-display";
        private const string ussGradientColorPicker = "gradient-color-picker";
        private const string ussGradientColorPickerField = "gradient-color-picker-field";
        private const string ussGradientColorPickerLabel = "gradient-color-picker-label";

        private Color _colorA;
        private Color _colorB;

        public Color ColorA { get { return _colorA; } set { _colorA = value; RegenerateTexture(); } }
        public Color ColorB { get { return _colorB; } set { _colorB = value; RegenerateTexture(); } }

        private string _label;
        public string label { get { return _label; } set { _label = value; if (value.Length <= 0) _labelElement.style.display = DisplayStyle.None; else _labelElement.style.display = DisplayStyle.Flex; _labelElement.text = value; } }

        private Label _labelElement;
        private TextField _textFieldElement;
        private VisualElement _gradientContainer;

        public bool showInputField { get { return _textFieldElement.style.display == DisplayStyle.None; } set { _textFieldElement.style.display = (value ? DisplayStyle.Flex : DisplayStyle.None); } }

        public ColorPickerGradient(HueHadesWindow window) : base(window)
        {
            AddToClassList(ussGradientColorPicker);

            _labelElement = new Label();
            _labelElement.style.display = DisplayStyle.None;
            _labelElement.AddToClassList(ussGradientColorPickerLabel);
            Add(_labelElement);

            _gradientContainer = new VisualElement();
            _gradientContainer.style.flexGrow = 1;
            _gradientContainer.style.height = 16.0f;
            style.flexDirection = FlexDirection.Row;
            Add(_gradientContainer);

            _textFieldElement = new TextField();
            _textFieldElement.style.display = DisplayStyle.None;
            _textFieldElement.AddToClassList(ussGradientColorPickerField);
            _textFieldElement.RegisterCallback<FocusOutEvent>(OnFieldFocusOut);
            _textFieldElement.RegisterValueChangedCallback(OnFieldValueChange);

            _textFieldElement.RegisterCallback<KeyUpEvent>(OnKeyPressed);

            Add(_textFieldElement);

            _gradientContainer.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            _gradientContainer.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            _gradientContainer.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            _gradientContainer.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _gradientContainer.RegisterCallback<PointerUpEvent>(OnPointerUp);
            _gradientContainer.RegisterCallback<PointerMoveEvent>(OnPointerMove);

            _displayImage = new Image();
            _gradientContainer.Add(_displayImage);
            _displayImage.style.height = 16.0f;
            style.height = 27.0f;
            style.display = DisplayStyle.Flex;

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
            _gradientContainer.Add(_pickerCenter);

            _colorA = new Color(1,1,1,0);
            _colorB = new Color(1,1,1,1);
        }

        /// <summary>
        /// If any key is pressed, invoke the event which only triggers when the picking position was changed by the user
        /// </summary>
        /// <param name="evt"></param>
        private void OnKeyPressed(KeyUpEvent evt)
        {
            if (!(_textFieldElement.value.EndsWith(".") || (_textFieldElement.value.EndsWith("0") && _textFieldElement.value.Contains("."))))
            {
                ValueChangedByUser?.Invoke(PickerPosition);
            }
        }

        /// <summary>
        /// Update field change if the string in the textfield is not "in progress of writing", because it would override certain numbers that are not fully typed yet. 
        /// </summary>
        /// <param name="evt"></param>
        private void OnFieldValueChange(ChangeEvent<string> evt)
        {
            if (!(_textFieldElement.value.EndsWith(".") || (_textFieldElement.value.EndsWith("0") && _textFieldElement.value.Contains("."))) && float.TryParse(_textFieldElement.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float newPick))
            {
                OnFieldChanged();
            }
        }

        /// <summary>
        /// Call field change when enter is pressed while textfield is selected or textfield is unselected
        /// </summary>
        /// <param name="evt"></param>
        private void OnFieldFocusOut(FocusOutEvent evt)
        {
            OnFieldChanged();
            ValueChangedByUser?.Invoke(PickerPosition);
        }

        /// <summary>
        /// When the textfield of the picker changes update the picking position if possible
        /// </summary>
        private void OnFieldChanged()
        {
            if (float.TryParse(_textFieldElement.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float newPick))
            {
                PickerPosition = newPick;
            }
            else
            {
                PickerPosition = _pickerPosition;
            }
        }

        /// <summary>
        /// updates the picker using screen units
        /// </summary>
        /// <param name="position"></param>
        private void UpdatePicker(Vector2 position)
        {
            var localPosition = _displayImage.WorldToLocal(position);

            var pickerPosition = localPosition.x / Mathf.Max(1, _displayImage.worldBound.width);
            PickerPosition = Mathf.Clamp01(pickerPosition);
        }

        /// <summary>
        /// Updates the picker using a relative coordinate to the picker's space (0-1)
        /// </summary>
        /// <param name="position"></param>
        private void UpdatePickerRelative(float position)
        {
            _pickerPosition = Mathf.Clamp01(position);

            _pickerCenter.style.left = PickerPosition * _displayImage.worldBound.width - _pickerCenter.style.width.value.value * 0.5f;
            _pickerCenter.style.top = _displayImage.worldBound.height - _pickerCenter.style.height.value.value * 0.5f;
        }

        /// <summary>
        /// Pick value if picking
        /// </summary>
        /// <param name="evt"></param>
        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_picking) return;
            UpdatePicker(evt.position);
            ValueChangedByUser?.Invoke(PickerPosition);
        }

        /// <summary>
        /// End picking
        /// </summary>
        /// <param name="evt"></param>
        private void OnPointerUp(PointerUpEvent evt)
        {
            _picking = false;
            _gradientContainer.ReleasePointer(evt.pointerId);
        }

        /// <summary>
        /// Start picking
        /// </summary>
        /// <param name="evt"></param>
        private void OnPointerDown(PointerDownEvent evt)
        {
            _picking = true;
            _gradientContainer.CapturePointer(evt.pointerId);
            UpdatePicker(evt.position);
            ValueChangedByUser?.Invoke(PickerPosition);
        }

        /// <summary>
        /// Draws the gradient onto the texture
        /// </summary>
        private void RegenerateTexture()
        {
            if (_displayTexture == null) return;
            switch (_gradientMode)
            {
                case GradientMode.Color:
                    RenderTextureUtilities.Gradients.DrawColorGradient(_displayTexture, _displaySize.x, _colorA, _colorB);
                    break;
                case GradientMode.Hue:
                    RenderTextureUtilities.Gradients.DrawHueGradient(_displayTexture, _displaySize.x);
                    break;
            }
        }

        /// <summary>
        /// Algorithm for resizing the texture
        /// </summary>
        /// <param name="size"></param>
        private void ResizeTexture(int2 size)
        {
            size.y = 1;
            if (size.x <= 0) size.x = 1;
            if (_displayTexture != null)
            {
                RenderTextureUtilities.ReleaseTemporaryGradient(_displayTexture);

            }
            _displayTexture = RenderTextureUtilities.GetTemporaryGradient(size.x, RenderTextureFormat.ARGB32);
            _displayImage.image = _displayTexture.texture;
            _displayImage.uv = new Rect(0, 0, size.x / (float)_displayTexture.actualWidth, _displayImage.worldBound.height);
            _displaySize = size;
            RegenerateTexture();
        }

        /// <summary>
        /// When the window size is change gradient resize the texture
        /// </summary>
        /// <param name="evt"></param>
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            var size = evt.newRect.size;
            ResizeTexture(new int2(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y)));
            UpdatePickerRelative(_pickerPosition);

            var bgSize = size.x * 0.8f;
            _displayImage.style.backgroundSize = new BackgroundSize(bgSize, bgSize);
        }

        /// <summary>
        /// The color picker is no longer visible, texture is not needed anymore and is relased
        /// </summary>
        /// <param name="evt"></param>
        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            if (_displayTexture != null) RenderTextureUtilities.ReleaseTemporaryGradient(_displayTexture);
            _displayTexture = null;
        }

        /// <summary>
        /// Resize the texture according to the current size in panel
        /// </summary>
        /// <param name="evt"></param>
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
