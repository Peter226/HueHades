using HueHades.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ToolButton : HueHadesElement
{
    private ImageTool _imageTool;
    public ImageTool ImageTool { get { return _imageTool; } }
    private const string ussToolButton = "tool-button";
    private const string ussToolButtonSelected = "tool-button-selected";

    public Action<ToolButton> Selected;
    private bool _selected;

    public ToolButton(HueHadesWindow window, ImageTool imageTool) : base(window)
    {
        this.RegisterCallback<ClickEvent>(OnClicked);
        _imageTool = imageTool;
        AddToClassList(ussToolButton);
    }

    public void Deselect()
    {
        if (!_selected) return;
        RemoveFromClassList(ussToolButtonSelected);
        _imageTool.Deselect();
        _selected = false;
    }


    void OnClicked(ClickEvent clicked)
    {
        if (_selected) return;
        _selected = true;
        AddToClassList(ussToolButtonSelected);
        Selected?.Invoke(this);
        _imageTool.Select();
    }




}
