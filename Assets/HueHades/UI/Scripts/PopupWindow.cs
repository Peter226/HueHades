using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class PopupWindow : HueHadesElement
{
    private VisualElement _parentElement;
    private const string _ussPopupWindow = "popup-window";
    private const string _ussPopupWindowTitle = "popup-window-title";

    public PopupWindow(HueHadesWindow window) : base(window)
    {
        Label label = new Label();
        label.text = GetWindowName();
        label.AddToClassList(_ussPopupWindowTitle);
        this.Add(label);
        AddToClassList(_ussPopupWindow);
    }

    public void Open()
    {
        if (_parentElement != null) return;
        _parentElement = window.PopupWindowParentElement;
        _parentElement.Insert(_parentElement.childCount, this);

        var windowBounds = window.worldBound;
        var defaultSize = GetDefaultSize();
        this.style.left = (windowBounds.width - defaultSize.x) * 0.5f;
        this.style.top = (windowBounds.height - defaultSize.y) * 0.5f;
        this.style.width = defaultSize.x;
        this.style.bottom = defaultSize.y;

        OnOpen();
        
    }

    public void Close()
    {
        OnClose();
        if (_parentElement == null) return;
        if (_parentElement.Contains(this))
        {
            _parentElement.Remove(this);
        }
        _parentElement = null;
    }

    protected virtual string GetWindowName()
    {
        return "Unknown";
    }

    protected virtual Vector2 GetDefaultSize()
    {
        return new Vector2(400, 300);
    }


    protected abstract void OnOpen();

    protected virtual void OnClose() { }

}