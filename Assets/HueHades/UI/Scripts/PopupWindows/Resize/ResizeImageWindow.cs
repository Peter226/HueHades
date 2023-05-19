using HueHades.Core;
using HueHades.Core.Utilities;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Mathematics;

namespace HueHades.UI
{
    public class ResizeImageWindow : PopupWindow
    {
        private ImageCanvas _canvas;

        private IntegerField _width;
        private IntegerField _height;

        public ResizeImageWindow(HueHadesWindow window, ImageCanvas canvas) : base(window)
        {
            _canvas = canvas;

            var _fields = new VisualElement();
            _fields.AddToClassList(Layouts.Horizontal);
            _fields.AddToClassList(Layouts.SpaceAround);
            container.Add(_fields);

            _width = new IntegerField();
            _width.label = "Width";
            _width.SetValueWithoutNotify(_canvas.Dimensions.x);
            _width.RegisterValueChangedCallback(OnFieldChanged);
            _fields.Add(_width);

            _height = new IntegerField();
            _height.label = "Height";
            _height.SetValueWithoutNotify(_canvas.Dimensions.y);
            _height.RegisterValueChangedCallback(OnFieldChanged);
            _fields.Add(_height);


            var _bottomButtos = new VisualElement();
            _bottomButtos.AddToClassList(Layouts.Horizontal);
            _bottomButtos.AddToClassList(Layouts.SpaceAround);

            var _applyButton = new Button();
            _applyButton.text = "Apply";
            _bottomButtos.Add(_applyButton);
            _applyButton.clicked += Apply;

            var _cancelButton = new Button();
            _cancelButton.text = "Cancel";
            _bottomButtos.Add(_cancelButton);
            _bottomButtos.style.flexGrow = 1;
            _bottomButtos.style.alignContent = Align.FlexEnd;
            _bottomButtos.style.alignItems = Align.FlexEnd;
            _cancelButton.clicked += Close;

            hierarchy.Add(_bottomButtos);
        }

        private void OnFieldChanged(ChangeEvent<int> evt)
        {
            _width.SetValueWithoutNotify(Mathf.Clamp(_width.value, 1, 8192));
            _height.SetValueWithoutNotify(Mathf.Clamp(_height.value, 1, 8192));
        }

        private void Apply()
        {
            CanvasUtilities.ResizeCanvas(_canvas, new int2(_width.value, _height.value),true);
            Close();
        }

        protected override string GetWindowName()
        {
            return "Resize Image";
        }
        protected override Vector2 GetDefaultSize()
        {
            return base.GetDefaultSize() * new Vector2(0.7f,0.4f);
        }
    }
}
