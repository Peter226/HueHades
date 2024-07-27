using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using HueHades.Core;
using HueHades.Utilities;
public class ApplicationManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance
    /// </summary>
    public static ApplicationManager Instance { get; private set; }
    public static EventHandler<LifeTimeEventArgs> ApplicationLoaded;

    /// <summary>
    /// Called when a canvas was created and is loaded
    /// </summary>
    public static EventHandler<CanvasChangeEventArgs> CanvasCreated;
    /// <summary>
    /// Called when a canvas was closed and unloaded
    /// </summary>
    public static EventHandler<CanvasChangeEventArgs> CanvasClosed;
    /// <summary>
    /// Called when a canvas is selected, usually by the main menu bar
    /// </summary>
    public static EventHandler<CanvasChangeEventArgs> CanvasSelected;

    private List<ImageCanvas> _canvases = new List<ImageCanvas>();

    /// <summary>
    /// Initialize singleton and texture buffer pools
    /// </summary>
    private void Awake()
    {
        RenderTextureUtilities.InitializePool();
        Instance = this;
        //Application.targetFrameRate = Mathf.CeilToInt((float)Screen.currentResolution.refreshRateRatio.value);
    }

    /// <summary>
    /// Signal to other systems that the application is loaded
    /// </summary>
    private void Start()
    {
        ApplicationLoaded?.Invoke(this, new LifeTimeEventArgs());
    }

    /// <summary>
    /// Cleanup
    /// </summary>
    private void OnDestroy()
    {
        CanvasCreated = null;
    }

    /// <summary>
    /// Get the amount of canvases loaded in the application
    /// </summary>
    public int CanvasCount { get { return _canvases.Count; } }

    /// <summary>
    /// Get a specified canvas that is loaded in the application
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ImageCanvas GetCanvas(int index)
    {
        return _canvases[index];
    }

    /// <summary>
    /// Get the canvases currently loaded in the application
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ImageCanvas> GetCanvases()
    {
        return _canvases;
    }


    /// <summary>
    /// Use this to create a new canvas. The canvas will be properly initialized, and will be opened in the UI if needed
    /// </summary>
    /// <param name="dimensions">Width and height of the canvas</param>
    /// <param name="initialColor">The starting color of the first layer</param>
    /// <param name="format">Format of the RenderTextures that are storing the color information in the canvas</param>
    /// <returns></returns>
    public ImageCanvas CreateCanvas(int2 dimensions, Color initialColor, RenderTextureFormat format)
    {
        var canvas = new ImageCanvas(dimensions, format, initialColor);
        _canvases.Add(canvas);
        CanvasCreated?.Invoke(this, new CanvasChangeEventArgs(canvas));
        return canvas;
    }

    /// <summary>
    /// Closes a canvas, frees canvas resources, alerts UI and other event subscribers
    /// </summary>
    /// <param name="canvas"></param>
    public void CloseCanvas(ImageCanvas canvas)
    {
        if (_canvases.Contains(canvas))
        {
            _canvases.Remove(canvas);
            CanvasClosed?.Invoke(this, new CanvasChangeEventArgs(canvas));
            canvas.Dispose();
        }
    }

    /// <summary>
    /// Used to select a canvas, which alerts the UI to display the canvas
    /// </summary>
    /// <param name="canvas"></param>
    public void SelectCanvas(ImageCanvas canvas)
    {
        CanvasSelected?.Invoke(this, new CanvasChangeEventArgs(canvas));
    }


    /// <summary>
    /// Event arguments for lifecycle events 
    /// </summary>
    public class LifeTimeEventArgs : EventArgs
    {

    }

    /// <summary>
    /// Event arguments that carry a canvas object for canvas specific lifecycle events
    /// </summary>
    public class CanvasChangeEventArgs : EventArgs
    {
        public ImageCanvas Canvas { get; private set; }
        public CanvasChangeEventArgs(ImageCanvas canvas)
        {
            Canvas = canvas;
        }


    }
}