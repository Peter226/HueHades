using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class PopupWindow : HueHadesElement
{
    private VisualElement _parentElement;
    protected VisualElement container { get; private set; }
    private const string _ussPopupWindow = "popup-window";
    private const string _ussPopupWindowContainer = "popup-window-container";
    private const string _ussPopupWindowTitle = "popup-window-title";
    private const string _ussPopupWindowTitleBar = "popup-window-title-bar";
    private const string _ussPopupWindowClose = "popup-window-close";

    public PopupWindow(HueHadesWindow window) : base(window)
    {
        VisualElement titleBar = new VisualElement();
        container = new VisualElement();
        container.AddToClassList(_ussPopupWindowContainer);
        container.Add(titleBar);
        Label label = new Label();
        label.text = GetWindowName();
        label.AddToClassList(_ussPopupWindowTitle);
        titleBar.Add(label);
        AddToClassList(_ussPopupWindow);

        Button windowClose = new Button();
        windowClose.text = "✕";
        windowClose.clicked += Close;
        titleBar.Add(windowClose);

        titleBar.AddToClassList(_ussPopupWindowTitleBar);
        windowClose.AddToClassList(_ussPopupWindowClose);

        hierarchy.Add(container);

        label.AddManipulator(new PopupMoveManipulator(label, this));
    }

    /// <summary>
    /// Open the window after creating the instance
    /// </summary>
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
        this.style.height = defaultSize.y;

        OnOpen();
    }

    /// <summary>
    /// Close the window
    /// </summary>
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

    /// <summary>
    /// Window name that will be displayed on the header
    /// </summary>
    /// <returns></returns>
    protected virtual string GetWindowName()
    {
        return "Unknown";
    }

    /// <summary>
    /// The window's size as it'll appear on screen
    /// </summary>
    /// <returns></returns>
    protected virtual Vector2 GetDefaultSize()
    {
        return new Vector2(400, 300);
    }


    /// <summary>
    /// Manipulator for moving the window
    /// </summary>
    private class PopupMoveManipulator : PointerManipulator
    {
        VisualElement _window;
        private bool _dragging;

        public PopupMoveManipulator(VisualElement target, VisualElement window)
        {
            this.target = target;
            _window = window;
        }


        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_dragging) return;
            _window.style.left = _window.style.left.value.value + evt.deltaPosition.x;
            _window.style.top = _window.style.top.value.value + evt.deltaPosition.y;
        }

        private void OnPointerDown(PointerDownEvent pointerEvent)
        {
            _dragging = true;
            target.CapturePointer(pointerEvent.pointerId);
        }
        private void OnPointerUp(PointerUpEvent pointerEvent)
        {
            _dragging = false;
            target.ReleasePointer(pointerEvent.pointerId);
        }
    }



    protected virtual void OnOpen() { }

    protected virtual void OnClose() { }

}
