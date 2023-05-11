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
using HueHades.Core.Utilities;
using HueHades.IO;

[RequireComponent(typeof(UIDocument))]
public class MainUI : MonoBehaviour
{
    private UIDocument _uIDocument;
    private HueHadesWindow _window;
    public int2 initialCanvasDimensions = new int2(3508, 2480);

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

