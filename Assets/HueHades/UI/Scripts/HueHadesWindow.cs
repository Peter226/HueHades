using HueHades.Core;
using HueHades.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HueHadesWindow : VisualElement
{
    VisualElement _popupElement;
    VisualElement _freeDockElement;
    VisualElement _popupWindowelement;
    private ToolsWindow _toolsWindow;
    public ToolsWindow ToolsWindow { get { return _toolsWindow; } }


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
    private DockingWindow _dockingWindow;

    public new class UxmlFactory : UxmlFactory<HueHadesWindow, UxmlTraits> { }



    public HueHadesWindow()
    {
        var menuBar = new MenuBar(this);
        hierarchy.Insert(0, menuBar);
        _dockingWindow = new DockingWindow(this, true);
        hierarchy.Insert(1, _dockingWindow);
        if (Application.isPlaying) ApplicationManager.OnCanvasCreated += OnCanvasCreated;

    }

    //if this throws an error, set script execution order to ApplicationManager run first
    private void OnCanvasCreated(object sender, ApplicationManager.CanvasChangeEventArgs args)
    {
        ImageOperatingWindow imageOperatingWindow = new ImageOperatingWindow(this, args.Canvas);
        imageOperatingWindow.Dock(_dockingWindow.Handle);

        if (_toolsWindow == null)
        {
            _toolsWindow = new ToolsWindow(this);
            _toolsWindow.Dock(_dockingWindow.Handle, DockType.Left);
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
