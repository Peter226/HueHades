using HueHades.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ToolButton : HueHadesElement
{
    private ToolController _toolController;
    public ToolController ToolController { get { return _toolController; } }
    private const string ussToolButton = "tool-button";
    private const string ussToolButtonSelected = "tool-button-selected";

    public Action<ToolButton> Selected;
    private bool _selected;

    public ToolButton(HueHadesWindow window, ToolController toolController) : base(window)
    {
        this.RegisterCallback<ClickEvent>(OnClicked);
        _toolController = toolController;
        AddToClassList(ussToolButton);

        var image = new Image();
        image.image = _toolController.GetIcon();
        Add(image);
    }

    public void Deselect()
    {
        if (!_selected) return;
        RemoveFromClassList(ussToolButtonSelected);
        _toolController.Deselect();
        _selected = false;
    }


    void OnClicked(ClickEvent clicked)
    {
        if (_selected) return;
        _selected = true;
        AddToClassList(ussToolButtonSelected);
        Selected?.Invoke(this);
        _toolController.Select();
    }




}
