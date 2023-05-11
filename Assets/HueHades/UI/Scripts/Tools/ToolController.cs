using HueHades.Core;
using HueHades.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public abstract class ToolController
{
    public abstract IToolContext CollectContext(HueHadesWindow window);

    public abstract Texture GetIcon();

    protected abstract ImageTool InitializeTool();

    private ImageTool _tool;
    protected ImageTool tool { get { if (_tool == null) _tool = InitializeTool(); return _tool; } }
    public Texture Icon { get; protected set; }

    private bool _isSelected;
    private bool _isUsing;

    public void Select()
    {
        if (_isSelected) return;
        _isSelected = true;
        OnSelected();
    }

    public void Deselect()
    {
        if (!_isSelected) return;
        _isSelected = false;
        OnDeselected();
    }

    protected virtual void OnSelected() { }

    protected virtual void OnDeselected() { }


    public void BeginUse(IToolContext toolContext, ImageCanvas canvas, int globalLayerIndex, Vector2 startPoint, float startPressure, float startTilt)
    {
        if (_isUsing || !(canvas.SelectedLayer is ImageLayer)) return;
        _isUsing = true;
        OnBeginUse(toolContext, canvas, globalLayerIndex, startPoint, startPressure, startTilt);
        tool.BeginUse(toolContext, canvas, globalLayerIndex, startPoint, startPressure, startTilt);
    }
    public void UseUpdate(Vector2 currentPoint, float currentPressure, float currentTilt)
    {
        if (!_isUsing) return;
        OnUseUpdate(currentPoint, currentPressure, currentTilt);
        tool.UseUpdate(currentPoint, currentPressure, currentTilt);
    }
    public void EndUse(Vector2 endPoint, float endPressure, float endTilt)
    {
        if (!_isUsing) return;
        _isUsing = false;
        tool.EndUse(endPoint, endPressure, endTilt);
        OnEndUse(endPoint, endPressure, endTilt);
    }

    protected virtual void OnBeginUse(IToolContext toolContext, ImageCanvas canvas, int layer, Vector2 startPoint, float startPressure, float startTilt) { }
    protected virtual void OnUseUpdate(Vector2 currentPoint, float currentPressure, float currentTilt) { }
    protected virtual void OnEndUse(Vector2 endPoint, float endPressure, float endTilt) { }

}
