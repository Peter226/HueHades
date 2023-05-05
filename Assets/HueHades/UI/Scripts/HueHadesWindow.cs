using HueHades.Core;
using HueHades.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HueHadesWindow : VisualElement
{
    VisualElement _popupElement;
    VisualElement _freeDockElement;
    VisualElement _popupWindowelement;
    private bool _initialized;
    public Action OnInitialized;
    private ToolsWindow _toolsWindow;
    private ToolSettingsWindow _toolSettingsWindow;
    private ColorSelectorWindow _colorSelectorWindow;
    private CanvasHistoryWindow _historyWindow;
    private CanvasLayersWindow _layersWindow;

    public ToolsWindow Tools { get { return _toolsWindow; } set { _toolsWindow = value; } }
    public ToolSettingsWindow ToolSettings { get { return _toolSettingsWindow; } set { _toolSettingsWindow = value; } }
    public ColorSelectorWindow ColorSelector { get { return _colorSelectorWindow; } set { _colorSelectorWindow = value; } }
    public CanvasHistoryWindow History { get { return _historyWindow; } set { _historyWindow = value; } }
    public CanvasLayersWindow Layers { get { return _layersWindow; } set { _layersWindow = value; } }

    public Action<ImageOperatingWindow> ActiveOperatingWindowChanged;

    private ImageOperatingWindow _activeOperatingWindow;
    public ImageOperatingWindow ActiveOperatingWindow { get { return _activeOperatingWindow; } set { _activeOperatingWindow = value; ActiveOperatingWindowChanged?.Invoke(_activeOperatingWindow); } }

    public VisualElement FreeDockElement
    {
        get
        {
            if (_freeDockElement == null)
            {
                _freeDockElement = this.Q<VisualElement>("FreeDock");
            }
            return _freeDockElement;
        }
    }


    public VisualElement PopupWindowParentElement
    {
        get
        {
            if (_popupWindowelement == null)
            {
                _popupWindowelement = this.Q<VisualElement>("PopupWindows");
            }
            return _popupWindowelement;
        }
    }



    private Dictionary<ImageCanvas, ImageOperatingWindow> _operatingWindows = new Dictionary<ImageCanvas, ImageOperatingWindow>();
    private DockingWindow _mainDock;
    public DockingWindow MainDock { get { return _mainDock; } }

    public new class UxmlFactory : UxmlFactory<HueHadesWindow, UxmlTraits> {}



    public HueHadesWindow()
    {
        var menuBar = new MenuBar(this);
        hierarchy.Insert(0, menuBar);

        VisualElement dockParent = new VisualElement();
        dockParent.style.flexGrow = 1;
        _mainDock = new DockingWindow(this, true);
        dockParent.Add(_mainDock);
        hierarchy.Insert(1, dockParent);
        if (Application.isPlaying) ApplicationManager.OnCanvasCreated += OnCanvasCreated;
        this.RegisterCallback<GeometryChangedEvent>(OnGeometryChange);
        _initialized = false;
    }

    private void OnGeometryChange(GeometryChangedEvent evt)
    {
        if (!_initialized)
        {
            var dockingWindowBounds = _mainDock.worldBound;
            _mainDock.style.width = dockingWindowBounds.width;
            _mainDock.style.height = dockingWindowBounds.height;
            _initialized = true;
            OnInitialized?.Invoke();
        }
    }

    //if this throws an error, set script execution order to ApplicationManager run first
    private void OnCanvasCreated(object sender, ApplicationManager.CanvasChangeEventArgs args)
    {
        ImageOperatingWindow imageOperatingWindow = new ImageOperatingWindow(this, args.Canvas);
        imageOperatingWindow.Dock(_mainDock.Handle);

        if (_toolsWindow == null)
        {
            _toolsWindow = new ToolsWindow(this);
            _toolsWindow.Dock(_mainDock.Handle, DockType.Left);
        }
        if (_toolSettingsWindow == null)
        {
            _toolSettingsWindow = new ToolSettingsWindow(this);
            _toolSettingsWindow.Dock(_mainDock.Handle,DockType.Right);
        }
        if (_colorSelectorWindow == null)
        {
            _colorSelectorWindow = new ColorSelectorWindow(this);
            _colorSelectorWindow.Dock(_toolSettingsWindow.DockedIn, DockType.Top);
        }
        if (_historyWindow == null)
        {
            _historyWindow = new CanvasHistoryWindow(this);
            _historyWindow.Dock(_toolSettingsWindow.DockedIn, DockType.Bottom);
        }
        if (_layersWindow == null)
        {
            _layersWindow = new CanvasLayersWindow(this);
            _layersWindow.Dock(_historyWindow.DockedIn, DockType.Header, 0);
        }

    }

    public void ShowOverlay(VisualElement overlay, VisualElement forElement = null, OverlayPlacement placement = OverlayPlacement.Bottom, bool isBackground = false)
    {
        if (_popupElement == null)
        {
            _popupElement = this.Q<VisualElement>("PopupOverlays");
            _popupElement.pickingMode = PickingMode.Ignore;
        }

        if (!isBackground)
        {
            _popupElement.Add(overlay);
        }
        else
        {
            _popupElement.Insert(0, overlay);
        }

        if (forElement != null)
        {
            Vector2 point;
            switch (placement)
            {
                case OverlayPlacement.Bottom:
                    point = _popupElement.WorldToLocal(forElement.LocalToWorld(new Vector2(0, forElement.layout.height)));
                    overlay.style.left = point.x;
                    overlay.style.top = point.y;
                    break;
                case OverlayPlacement.Right:
                    point = _popupElement.WorldToLocal(forElement.LocalToWorld(new Vector2(forElement.layout.width, 0)));
                    overlay.style.left = point.x;
                    overlay.style.top = point.y;
                    break;
            }

        }
    }
    public void HideOverlay(VisualElement overlay)
    {
        if (_popupElement == null)
        {
            _popupElement = this.Q<VisualElement>("PopupOverlays");
        }
        if (!_popupElement.Contains(overlay)) return;
        _popupElement.Remove(overlay);
    }
}
