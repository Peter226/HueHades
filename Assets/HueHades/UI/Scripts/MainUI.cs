using HueHades.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using HueHades.UI;

[RequireComponent(typeof(UIDocument))]
public class MainUI : MonoBehaviour
{
    private UIDocument _uIDocument;
    private HueHadesWindow _window;
    public int2 initialCanvasDimensions = new int2(3508, 2480);

    private void Awake()
    {
        ApplicationManager.OnApplicationLoaded += ApplicationLoaded;
    }

    private void OnDestroy()
    {
        ApplicationManager.OnApplicationLoaded -= ApplicationLoaded;
    }

    private void ApplicationLoaded(object sender, ApplicationManager.LifeTimeEventArgs e)
    {
        _uIDocument = GetComponent<UIDocument>();
        _window = _uIDocument.rootVisualElement.Q<HueHadesWindow>();
        ApplicationManager.Instance.CreateCanvas(initialCanvasDimensions, Color.white, RenderTextureFormat.ARGBFloat);
        ApplicationManager.Instance.CreateCanvas(initialCanvasDimensions, Color.white, RenderTextureFormat.ARGBFloat);
        ApplicationManager.Instance.CreateCanvas(initialCanvasDimensions, Color.white, RenderTextureFormat.ARGBFloat);
        ApplicationManager.Instance.CreateCanvas(initialCanvasDimensions, Color.white, RenderTextureFormat.ARGBFloat);
        ApplicationManager.Instance.CreateCanvas(initialCanvasDimensions, Color.white, RenderTextureFormat.ARGBFloat);
    }
}

[MenuBarItem("Edit/Tools/Brush Editor")]
public class EditBrushMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("executed");
        BrushEditorWindow brushEditorWindow = new BrushEditorWindow(window);
        brushEditorWindow.Open();
    }
}


[MenuBarItem("Image/Mirror horizontal")]
public class MirrorHorizontalMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Mirror horizontal");
    }
}

[MenuBarItem("Image/Mirror vertical")]
public class MirrorVerticalMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Mirror vertical");
    }
}


[MenuBarItem("Image/Resize...")]
public class ResizeMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Resizing image");
    }
}

[MenuBarItem("Image/Resize canvas...")]
public class ResizeCanvasMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Resizing canvas");
    }
}


[MenuBarItem("Edit/Undo")]
public class UndoMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Undo");
    }
}

[MenuBarItem("Edit/Redo")]
public class RedoMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Redo");
    }
}


[MenuBarItem("File/Save")]
public class SaveMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Saving file");
    }
}

[MenuBarItem("File/Save as...")]
public class SaveAsMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Saving as file");
    }
}