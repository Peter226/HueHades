using HueHades.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class LayerElement : VisualElement
    {
        public LayerBase layer;
        public int layerIndex;

        private const string ussLayerElement = "layer-element";
        private const string ussLayerElementDisplay = "layer-element-display";
        private const string ussLayerElementDisplaySelected = "layer-element-display-selected";
        private const string ussLayerElementDisplayActive = "layer-element-display-active";
        private const string ussLayerElementGroupDisplay = "layer-element-group-display";
        private const string ussLayerElementImage = "layer-element-image";
        private const string ussLayerElementLabel = "layer-element-label";
        private const string ussLayerElementVisibility = "layer-element-visibility";
        private const string ussLayerElementAlphaLock = "layer-element-alpha-lock";
        private const string ussGroupLayerElement = "group-layer-element";

        private Image _image;
        private Label _label;
        private ToggleButton _visibilityToggle;
        private ToggleButton _alphaInheritToggle;

        private Button _layerDisplay;
        private VisualElement _groupDisplay;
        public VisualElement GroupDisplay => _groupDisplay;

        public LayerElement(LayerBase layer, int layerIndex, bool selected = false, bool active = false)
        {
            this.layer = layer;
            this.layerIndex = layerIndex;

            _layerDisplay = new Button();
            if (selected)
            {
                _layerDisplay.AddToClassList(ussLayerElementDisplaySelected);
            }
            if (active)
            {
                _layerDisplay.AddToClassList(ussLayerElementDisplayActive);
            }

            _groupDisplay = new VisualElement();
            Add(_layerDisplay);
            _layerDisplay.AddToClassList(ussLayerElementDisplay);
            Add(_groupDisplay);
            _groupDisplay.AddToClassList(ussLayerElementGroupDisplay);

            AddToClassList(ussLayerElement);
            _image = new Image();
            _image.AddToClassList(ussLayerElementImage);
            _image.image = layer.Texture.texture;
            layer.LayerChanged += OnUpdateTexture;
            _layerDisplay.Add(_image);

            _label = new Label();
            _label.text = layer.Name;
            _label.AddToClassList(ussLayerElementLabel);
            _layerDisplay.Add(_label);

            _visibilityToggle = new ToggleButton(icon: "Icons/VisibleIcon", toggledIcon: "Icons/InvisibleIcon");
            _alphaInheritToggle = new ToggleButton(icon: "Icons/AlphaUnlockedIcon", toggledIcon: "Icons/AlphaLockedIcon");

            _visibilityToggle.AddToClassList(ussLayerElementVisibility);
            _alphaInheritToggle.AddToClassList(ussLayerElementAlphaLock);

            _visibilityToggle.Toggled = layer.LayerSettings.invisible;
            _alphaInheritToggle.Toggled = layer.LayerSettings.inheritAlpha;

            layer.LayerChanged += () =>
            {
                _visibilityToggle.SetToggleStateSilent(layer.LayerSettings.invisible);
                _alphaInheritToggle.SetToggleStateSilent(layer.LayerSettings.inheritAlpha);
                _image.image = layer.Texture.texture;
            };

            _visibilityToggle.ValueChanged += (t) =>
            {
                var layerSettings = layer.LayerSettings;
                layerSettings.invisible = t;
                layer.SetLayerSettings(layerSettings);
            };
            _alphaInheritToggle.ValueChanged += (t) =>
            {
                var layerSettings = layer.LayerSettings;
                layerSettings.inheritAlpha = t;
                layer.SetLayerSettings(layerSettings);
            };

            if (layer.RelativeIndex == 0)
            {
                _alphaInheritToggle.style.opacity = 0.0f;
            }


            _layerDisplay.Add(_visibilityToggle);
            _layerDisplay.Add(_alphaInheritToggle);

            if (layer is GroupLayer)
            {
                AddToClassList(ussGroupLayerElement);
            }

            _layerDisplay.clickable.activators.Clear();

            _layerDisplay.RegisterCallback<MouseDownEvent>(e => {
                if (e.shiftKey)
                {
                    if (layer.CanvasIn.ActiveLayer != null)
                    {
                        layer.CanvasIn.SelectRange(layer.CanvasIn.ActiveLayer, layer, true);
                    }
                    else
                    {
                        layer.CanvasIn.SelectLayer(layer);
                    }
                    return;
                }
                if (e.ctrlKey)
                {
                    layer.CanvasIn.SelectLayer(layer, true);
                    return;
                }
                layer.CanvasIn.SelectLayer(layer);
            });
        }

        void OnUpdateTexture()
        {
            _image.image = layer.Texture.texture;
        }

    }
}