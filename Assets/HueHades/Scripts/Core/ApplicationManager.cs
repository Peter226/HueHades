using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using HueHades.Core;
using HueHades.Utilities;
public class ApplicationManager : MonoBehaviour
{
    public static ApplicationManager Instance;
    public static EventHandler<LifeTimeEventArgs> OnApplicationLoaded;
    public static EventHandler<CanvasChangeEventArgs> OnCanvasCreated;

    private void Awake()
    {
        RenderTextureUtilities.InitializePool();
        Instance = this;
    }

    private void OnDestroy()
    {
        OnCanvasCreated = null;
    }


    private void Start()
    {
        OnApplicationLoaded?.Invoke(this, new LifeTimeEventArgs());
    }


    private List<ImageCanvas> _canvases = new List<ImageCanvas>();

    public int CanvasCount { get { return _canvases.Count; } }
    public ImageCanvas GetCanvas(int index)
    {
        return _canvases[index];
    }

    public IEnumerable<ImageCanvas> GetCanvases()
    {
        return _canvases;
    }

    public ImageCanvas CreateCanvas(int2 dimensions, Color initialColor, RenderTextureFormat format)
    {
        var canvas = new ImageCanvas(dimensions, format);
        _canvases.Add(canvas);
        OnCanvasCreated?.Invoke(this, new CanvasChangeEventArgs(canvas));
        return canvas;
    }

    public class LifeTimeEventArgs : EventArgs
    {

    }
    public class CanvasChangeEventArgs : EventArgs
    {
        public ImageCanvas Canvas { get; private set; }
        public CanvasChangeEventArgs(ImageCanvas canvas)
        {
            Canvas = canvas;
        }


    }
}