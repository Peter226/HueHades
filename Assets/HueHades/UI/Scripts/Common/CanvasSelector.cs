using HueHades.Core;
using HueHades.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasSelector : DropDownInput<CanvasEntry>
{
    private List<CanvasEntry> _entries = new List<CanvasEntry>();

    public CanvasSelector(HueHadesWindow window) : base(window)
    {
        FetchCanvases();
        this.label = "Canvas";
        ApplicationManager.CanvasCreated += OnCanvasCountChanged;
        ApplicationManager.CanvasClosed += OnCanvasCountChanged;
        window.ActiveOperatingWindowChanged += OnOperatingWindowChanged;
        this.ValueChanged += OnDropdownValueChanged;
    }

    private void OnDropdownValueChanged(CanvasEntry entry)
    {
        if (value.isDefault)
        {
            CanvasSelected?.Invoke(window.ActiveOperatingWindow.Canvas);
        }
        else
        {
            CanvasSelected?.Invoke(entry.canvas);
        }
    }

    private void OnOperatingWindowChanged(ImageOperatingWindow operatingWindow)
    {
        _entries[0].canvas = operatingWindow.Canvas;
        if (value.isDefault)
        {
            CanvasSelected?.Invoke(operatingWindow.Canvas);
        }
    }

    private void OnCanvasCountChanged(object sender, ApplicationManager.CanvasChangeEventArgs e)
    {
        FetchCanvases();
    }

    public Action<ImageCanvas> CanvasSelected;
    

    public ImageCanvas SelectedCanvas { 
        get {
            if (!value.isDefault) return value.canvas;
            return window.ActiveOperatingWindow.Canvas;
        }
    }

    private void FetchCanvases()
    {
        _entries.Clear();
        foreach (var canvas in ApplicationManager.Instance.GetCanvases())
        {
            _entries.Add(new CanvasEntry() { canvas = canvas, isDefault = false });
        }
        _entries.Insert(0, new CanvasEntry() { canvas = window.ActiveOperatingWindow.Canvas, isDefault = true });
        SetDataSource(_entries, (e) => (e.isDefault ? "Recent" : e.canvas.FileName));
    }

}

public class CanvasEntry
{
    public ImageCanvas canvas;
    public bool isDefault;
}