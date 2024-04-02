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

        /// <summary>
        /// Call this to make a single canvas selected in the hierarchy
        /// </summary>
        /// <param name="canvas"></param>
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

        /// <summary>
        /// Button to add layer pressed
        /// </summary>
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

        /// <summary>
        /// Button to delete layer pressed
        /// </summary>
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

        /// <summary>
        /// Button to duplicate layer pressed
        /// </summary>
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

        /// Button to move layer up pressed
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

        /// <summary>
        /// /// Button to move layer down pressed
        /// </summary>
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

        /// <summary>
        /// Button to open layer settings pressed
        /// </summary>
        internal void OnSettings()
        {
            if (_selectedCanvas != null && _selectedCanvas.SelectedLayer != null)
            {
                LayerSettingsWindow layerSettings = new LayerSettingsWindow(window, _selectedCanvas.SelectedLayer);
                layerSettings.Open();
            }
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
    }
}