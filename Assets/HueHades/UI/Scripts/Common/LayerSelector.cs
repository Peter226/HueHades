using HueHades.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LayerSelector : HueHadesElement
{
    ScrollView _layerScrollView;
    private ImageCanvas _canvas;
    public ImageCanvas Canvas { get { return _canvas; } set { _canvas = value; ReBuildView(); } }

    public LayerSelector(HueHadesWindow window) : base(window)
    {
        _layerScrollView = new ScrollView();
        hierarchy.Add(_layerScrollView);
    }

    private void ReBuildView()
    {
        _layerScrollView.Clear();
        if (_canvas == null)
        {
            Label label = new Label();
            label.text = "No canvas selected";
            _layerScrollView.Add(label);
            return;
        }
        if (_canvas.GetLayerCount() <= 0)
        {
            Label label = new Label();
            label.text = "No layers in canvas";
            _layerScrollView.Add(label);
        }
    }



}
