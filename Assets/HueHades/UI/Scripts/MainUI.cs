using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using HueHades.Tools;
using HueHades.IO;

[RequireComponent(typeof(UIDocument))]
public class MainUI : MonoBehaviour
{
    private UIDocument _uIDocument;
    private HueHadesWindow _window;
    public int2 initialCanvasDimensions = new int2(3508, 2480);


    /// <summary>
    /// Set up shortcuts
    /// </summary>
    private void Start()
    {
        ShortcutSystem.Undo += () => {
            var opWindow = _window.ActiveOperatingWindow;
            if (opWindow == null) return;
            opWindow.Canvas.History.Undo();
        };
        ShortcutSystem.Redo += () => {
            var opWindow = _window.ActiveOperatingWindow;
            if (opWindow == null) return;
            opWindow.Canvas.History.Redo();
        };
        ShortcutSystem.Open += () => {
            CanvasIO.Open();
        };
        ShortcutSystem.Save += () => {
            var opWindow = _window.ActiveOperatingWindow;
            if (opWindow == null) return;
            CanvasIO.Save(opWindow.Canvas);
        };
        ShortcutSystem.New += () => {
            var opWindow = _window.ActiveOperatingWindow;
            if (opWindow == null) return;
            ApplicationManager.Instance.CreateCanvas(new int2(1000,750), Color.white, RenderTextureFormat.ARGBFloat);
        };
    }


    private void Awake()
    {
        ApplicationManager.ApplicationLoaded += OnApplicationLoaded;

        BrushPreset.LoadPresets();
        //fill with dummy data if empty
        if (BrushPreset.Presets.Count <= 0)
        {
            var presets = new List<BrushPreset> {
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
            };

            foreach (var preset in presets) {
                preset.Save();
            }
        }
    }

    private void OnDestroy()
    {
        ApplicationManager.ApplicationLoaded -= OnApplicationLoaded;
    }

    private void OnApplicationLoaded(object sender, ApplicationManager.LifeTimeEventArgs e)
    {
        _uIDocument = GetComponent<UIDocument>();
        _window = _uIDocument.rootVisualElement.Q<HueHadesWindow>();
        _window.Initialized += OnWindowInitialize;
    }

    private void OnWindowInitialize()
    {
        ApplicationManager.Instance.CreateCanvas(initialCanvasDimensions, Color.white, RenderTextureFormat.ARGBFloat);
    }
}

