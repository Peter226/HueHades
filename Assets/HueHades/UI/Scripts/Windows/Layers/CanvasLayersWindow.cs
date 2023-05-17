using HueHades.Core;
using HueHades.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class CanvasLayersWindow : DockableWindow
    {
        private const string ussCanvasLayersWindow = "canvas-layers-window";

        private CanvasLayersDisplay _canvasLayersDisplay;
        private CanvasSelector _canvasSelector;
        private ImageCanvas _selectedCanvas;

        public CanvasLayersWindow(HueHadesWindow window) : base(window)
        {
            WindowName = "Layers";
            AddToClassList(ussCanvasLayersWindow);
            _canvasSelector = new CanvasSelector(window);
            _canvasLayersDisplay = new CanvasLayersDisplay(window);
            SelectCanvas(_canvasSelector.SelectedCanvas);
            _canvasSelector.CanvasSelected += SelectCanvas;
            Add(_canvasSelector);

            Add(_canvasLayersDisplay);

            Add(new LayersFooter(window, this));
        }


        private void SelectCanvas(ImageCanvas canvas)
        {
            if (_selectedCanvas != null)
            {
                _selectedCanvas.HierarchyUpdated -= OnCanvasHierarchyUpdated;
                _selectedCanvas.LayerSelected -= OnCanvasLayerSelected;
            }
            _selectedCanvas = canvas;
            if (_selectedCanvas == null) return;
            _canvasLayersDisplay.DrawCanvasLayers(_selectedCanvas);
            _selectedCanvas.HierarchyUpdated += OnCanvasHierarchyUpdated;
            _selectedCanvas.LayerSelected += OnCanvasLayerSelected;
        }

        private void OnCanvasLayerSelected(LayerBase selectedLayer)
        {
            _canvasLayersDisplay.DrawCanvasLayers(_selectedCanvas, true);
        }

        private void OnCanvasHierarchyUpdated()
        {
            _canvasLayersDisplay.DrawCanvasLayers(_selectedCanvas, true);
        }

        internal void OnAddLayer()
        {
            if (_selectedCanvas == null) return;
            int globalContainerIndex = 0;
            int relativeLayerIndex = _selectedCanvas.Layers.Count;
            if (_selectedCanvas.SelectedLayer != null)
            {
                var container = _selectedCanvas.SelectedLayer.ContainerIn;
                
                if (container is GroupLayer)
                {
                    globalContainerIndex = (container as GroupLayer).GlobalIndex;
                }
                relativeLayerIndex = _selectedCanvas.SelectedLayer.RelativeIndex + 1;
            }

            var layer = _selectedCanvas.AddLayer(globalContainerIndex, relativeLayerIndex, Color.clear);
            _selectedCanvas.History.AddRecord(new NewLayerHistoryRecord(globalContainerIndex, relativeLayerIndex, layer.GlobalIndex, Color.clear));
            _selectedCanvas.SelectedLayer = layer;
        }

        internal void OnDeleteLayer()
        {
            if (_selectedCanvas != null && _selectedCanvas.SelectedLayer != null)
            {
                var layer = _selectedCanvas.SelectedLayer;
                if (layer.RelativeIndex > 0)
                {
                    _selectedCanvas.SelectedLayer = layer.ContainerIn.Layers[layer.RelativeIndex - 1];
                }
                else
                {
                    if (layer.ContainerIn.GetLayerCount() > 1)
                    {
                        _selectedCanvas.SelectedLayer = layer.ContainerIn.Layers[layer.RelativeIndex + 1];
                    }
                    else
                    {
                        _selectedCanvas.SelectedLayer = null;
                    }
                }
                int containerGlobalIndex = 0;
                if (layer.ContainerIn is GroupLayer)
                {
                    containerGlobalIndex = (layer.ContainerIn as GroupLayer).GlobalIndex;
                }
                var globalIndex = layer.GlobalIndex;
                var relativeIndex = layer.RelativeIndex;
                _selectedCanvas.RemoveLayer(layer.GlobalIndex);
                _selectedCanvas.History.AddRecord(new RemoveLayerHistoryRecord(layer, containerGlobalIndex, relativeIndex, globalIndex));
            }
        }

        internal void OnDuplicateLayer()
        {

            if (_selectedCanvas != null && _selectedCanvas.SelectedLayer != null)
            {
                var layer = _selectedCanvas.SelectedLayer;


                if (_selectedCanvas == null) return;
                int globalContainerIndex = 0;
                int relativeLayerIndex = _selectedCanvas.Layers.Count;
                
                var container = _selectedCanvas.SelectedLayer.ContainerIn;

                if (container is GroupLayer)
                {
                    globalContainerIndex = (container as GroupLayer).GlobalIndex;
                }
                relativeLayerIndex = _selectedCanvas.SelectedLayer.RelativeIndex + 1;

                var newLayer = _selectedCanvas.AddLayer(globalContainerIndex, relativeLayerIndex, Color.clear);
                RenderTextureUtilities.CopyTexture(layer.Texture, newLayer.Texture);
                
                _selectedCanvas.History.AddRecord(new DuplicateLayerHistoryRecord(layer as ImageLayer, globalContainerIndex, relativeLayerIndex, newLayer.GlobalIndex));
                
                _selectedCanvas.SelectedLayer = newLayer;
            }
        }

        internal void OnMoveLayerUp()
        {
            if (_selectedCanvas != null && _selectedCanvas.SelectedLayer != null)
            {
                var layer = _selectedCanvas.SelectedLayer;
                int relativeIndex = layer.RelativeIndex;
                int globalIndex = layer.GlobalIndex;
                int containerGlobalIndex = layer.ContainerIn.GetGlobalIndex();
                _selectedCanvas.MoveLayer(layer.GlobalIndex, layer.ContainerIn.GetGlobalIndex(), layer.RelativeIndex + 1);
                int newRelativeIndex = layer.RelativeIndex;
                int newGlobalIndex = layer.GlobalIndex;
                int newContainerGlobalIndex = layer.ContainerIn.GetGlobalIndex();
                _selectedCanvas.History.AddRecord(new MoveLayerHistoryRecord(containerGlobalIndex, relativeIndex, globalIndex, newContainerGlobalIndex, newRelativeIndex, newGlobalIndex));
            }
        }

        internal void OnMoveLayerDown()
        {
            if (_selectedCanvas != null && _selectedCanvas.SelectedLayer != null)
            {
                var layer = _selectedCanvas.SelectedLayer;
                int relativeIndex = layer.RelativeIndex;
                int globalIndex = layer.GlobalIndex;
                int containerGlobalIndex = layer.ContainerIn.GetGlobalIndex();
                _selectedCanvas.MoveLayer(layer.GlobalIndex, layer.ContainerIn.GetGlobalIndex(), layer.RelativeIndex - 1);
                int newRelativeIndex = layer.RelativeIndex;
                int newGlobalIndex = layer.GlobalIndex;
                int newContainerGlobalIndex = layer.ContainerIn.GetGlobalIndex();
                _selectedCanvas.History.AddRecord(new MoveLayerHistoryRecord(containerGlobalIndex, relativeIndex, globalIndex, newContainerGlobalIndex, newRelativeIndex, newGlobalIndex));
            }
        }

        internal void OnSettings()
        {
            throw new NotImplementedException();
        }

        public override Vector2 DefaultSize
        {
            get { return new Vector2(200, 200); }
        }


        private class CanvasLayersDisplay : HueHadesElement
        {
            private const string ussCanvasLayersWindow = "canvas-layers-display";
            private const string ussCanvasLayersWindowScrollView = "canvas-layers-display-scrollview";
            private const string ussCanvasLayersWindowScrollViewContainer = "canvas-layers-display-scrollview-container";

            private ScrollView _scrollView;

            private ImageLayer _selectedLayer;
            private int _selectedLayerIndex;

            public ImageLayer SelectedLayer => _selectedLayer;
            public int SelectedLayerIndex => _selectedLayerIndex;

            public CanvasLayersDisplay(HueHadesWindow window) : base(window)
            {
                AddToClassList(ussCanvasLayersWindow);
                _scrollView = new ScrollView();
                Add(_scrollView);
                _scrollView.AddToClassList(ussCanvasLayersWindowScrollView);
                _scrollView.contentContainer.AddToClassList(ussCanvasLayersWindowScrollViewContainer);
            }

            public void DrawCanvasLayers(ImageCanvas canvas, bool keepScroll = false)
            {
                var scrollPosition = _scrollView.scrollOffset;
                _scrollView.Clear();

                DrawLayerContainer(canvas,_scrollView);

                if (keepScroll)
                {
                    _scrollView.scrollOffset = scrollPosition;
                }
            }


            private void DrawLayerContainer(ILayerContainer layerContainer, VisualElement elementContainer)
            {
                var layerCount = layerContainer.GetLayerCount();
                for (int i = 0;i < layerCount;i++)
                {
                    var layer = layerContainer.Layers[i];
                    var layerElement = new LayerElement(layer, i, layer.CanvasIn.SelectedLayer == layer);
                    elementContainer.Add(layerElement);
                    if (layer is GroupLayer)
                    {
                        DrawLayerContainer(layer as GroupLayer, layerElement.GroupDisplay);
                    }
                }
            }

        }



        private class LayerElement : VisualElement
        {
            private bool _isSelected;
            public bool IsSelected => _isSelected;
            public LayerBase layer;
            public int layerIndex;

            private const string ussLayerElement = "layer-element";
            private const string ussLayerElementDisplay = "layer-element-display";
            private const string ussLayerElementDisplaySelected = "layer-element-display-selected";
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

            public LayerElement(LayerBase layer, int layerIndex, bool selected = false)
            {
                this.layer = layer;
                this.layerIndex = layerIndex;

                _layerDisplay = new Button();
                if (selected)
                {
                    _layerDisplay.AddToClassList(ussLayerElementDisplaySelected);
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

                _visibilityToggle.OnToggle += (t) => { 
                    var layerSettings = layer.LayerSettings;
                    layerSettings.invisible = t;
                    layer.SetLayerSettings(layerSettings);
                };
                _alphaInheritToggle.OnToggle += (t) => {
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

                _layerDisplay.clicked += () => { layer.CanvasIn.SelectedLayer = layer; };
            }

            void OnUpdateTexture()
            {
                _image.image = layer.Texture.texture;
            }

        }
    }
}