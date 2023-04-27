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
using HueHades.Tools;

[RequireComponent(typeof(UIDocument))]
public class MainUI : MonoBehaviour
{
    private UIDocument _uIDocument;
    private HueHadesWindow _window;
    public int2 initialCanvasDimensions = new int2(3508, 2480);

    private void Awake()
    {
        ApplicationManager.OnApplicationLoaded += ApplicationLoaded;

        BrushPreset.LoadPresets();
        //fill with dummy data if empty
        if (BrushPreset.Presets.Count <= 0)
        {
            BrushPreset.Presets = new List<BrushPreset> {
                new BrushPreset() {
                    name = "Preset 1",
                    iconPath = "Icons/BrushIcon",
                    shape = BrushShape.Rectangle
                },
                new BrushPreset() {
                    name = "Preset 2",
                    iconPath = "Icons/BrushIcon",
                    shape = BrushShape.Ellipse
                },
                new BrushPreset() {
                    name = "Preset 3",
                    iconPath = "Icons/BrushIcon"
                },
                new BrushPreset() {
                    name = "Preset 4",
                    iconPath = "Icons/BrushIcon"
                },
                new BrushPreset() {
                    name = "Preset 5",
                    iconPath = "Icons/BrushIcon"
                },
                new BrushPreset() {
                    name = "Preset 6",
                    iconPath = "Icons/BrushIcon"
                },
                new BrushPreset() {
                    name = "Preset 7",
                    iconPath = "Icons/BrushIcon"
                }
                ,
                new BrushPreset() {
                    name = "Preset 8",
                    iconPath = "Icons/BrushIcon"
                }
                ,
                new BrushPreset() {
                    name = "Preset 9",
                    iconPath = "Icons/BrushIcon"
                }
                ,
                new BrushPreset() {
                    name = "Preset 10",
                    iconPath = "Icons/BrushIcon"
                }
                ,
                new BrushPreset() {
                    name = "Preset 11",
                    iconPath = "Icons/BrushIcon"
                }
            };

            foreach (var preset in BrushPreset.Presets) {
                preset.Save();
            }

        }

        
    }

    private void OnDestroy()
    {
        ApplicationManager.OnApplicationLoaded -= ApplicationLoaded;
    }

    private void ApplicationLoaded(object sender, ApplicationManager.LifeTimeEventArgs e)
    {
        _uIDocument = GetComponent<UIDocument>();
        _window = _uIDocument.rootVisualElement.Q<HueHadesWindow>();
        _window.OnInitialized += OnWindowInitialize;
    }

    private void OnWindowInitialize()
    {
        ApplicationManager.Instance.CreateCanvas(initialCanvasDimensions, Color.white, RenderTextureFormat.ARGBFloat);
        ApplicationManager.Instance.CreateCanvas(initialCanvasDimensions, Color.white, RenderTextureFormat.ARGBFloat);
        ApplicationManager.Instance.CreateCanvas(initialCanvasDimensions, Color.white, RenderTextureFormat.ARGBFloat);
        ApplicationManager.Instance.CreateCanvas(initialCanvasDimensions, Color.white, RenderTextureFormat.ARGBFloat);
        ApplicationManager.Instance.CreateCanvas(initialCanvasDimensions, Color.white, RenderTextureFormat.ARGBFloat);
    }
}

[MenuBarItem("Window_4/Texture Debugger_1")]
public class DebuggerWindowMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        TextureDebuggerWindow textureDebuggerWindow = new TextureDebuggerWindow(window);
        textureDebuggerWindow.Dock(window.MainDock.Handle, DockType.Bottom);
    }
}

[MenuBarItem("Edit_2/Tools_3/Brush Editor_1")]
public class EditBrushMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        BrushEditorWindow brushEditorWindow = new BrushEditorWindow(window);
        brushEditorWindow.Open();
    }
}


[MenuBarItem("Image_3/Mirror horizontal_3")]
public class MirrorHorizontalMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Mirror horizontal");
    }
}

[MenuBarItem("Image_3/Mirror vertical_4")]
public class MirrorVerticalMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Mirror vertical");
    }
}


[MenuBarItem("Image_3/Resize..._1")]
public class ResizeMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Resizing image");
    }
}

[MenuBarItem("Image_3/Resize canvas..._2")]
public class ResizeCanvasMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Resizing canvas");
    }
}


[MenuBarItem("Edit_2/Undo_1")]
public class UndoMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Undo");
    }
}

[MenuBarItem("Edit_2/Redo_2")]
public class RedoMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Redo");
    }
}


[MenuBarItem("File_1/Save_1")]
public class SaveMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Saving file");
    }
}

[MenuBarItem("File_1/Save as..._2")]
public class SaveAsMenuBarFunction : IMenuBarFunction
{
    public void Execute(HueHadesWindow window)
    {
        Debug.Log("Saving as file");
    }
}