using HueHades.Core.Utilities;
using HueHades.IO;
using HueHades.UI;
using Unity.Mathematics;
using UnityEngine;

public static class MenuBarFunctions
{
    [MenuBarItem("Effects_4/Color Adjustments_1")]
    public class ColorAdjustmentsMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            ColorAdjustmentsEffectWindow effectWindow = new ColorAdjustmentsEffectWindow(window);
            effectWindow.Open();
        }
    }

    [MenuBarItem("Effects_4/Swap Channels_2")]
    public class SwapChannelsMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            SwapChannelsEffectWindow effectWindow = new SwapChannelsEffectWindow(window);
            effectWindow.Open();
        }
    }


    [MenuBarItem("Effects_4/Noise_3/Simplex_1")]
    public class SimplexMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            SimplexEffectWindow effectWindow = new SimplexEffectWindow(window);
            effectWindow.Open();
        }
    }


    [MenuBarItem("Effects_4/Noise_3/Voronoi_2")]
    public class VoronoiMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            VoronoiEffectWindow effectWindow = new VoronoiEffectWindow(window);
            effectWindow.Open();
        }
    }


    [MenuBarItem("Window_5/Tools_1")]
    public class ToolsMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            if (window.Tools != null) window.Tools.UnDock();
            window.Tools = null;
            ToolsWindow dockableWindow = new ToolsWindow(window);
            dockableWindow.Dock(window.MainDock.Handle, DockType.Free);
            window.Tools = dockableWindow;
        }
    }

    [MenuBarItem("Window_5/Colors_2")]
    public class ColorPickerMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            if (window.ColorSelector != null) window.ColorSelector.UnDock();
            window.ColorSelector = null;
            ColorSelectorWindow dockableWindow = new ColorSelectorWindow(window);
            dockableWindow.Dock(window.MainDock.Handle, DockType.Free);
            window.ColorSelector = dockableWindow;
        }
    }

    [MenuBarItem("Window_5/Tool Settings_3")]
    public class ToolSettingsMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            if (window.ToolSettings != null) window.ToolSettings.UnDock();
            window.ToolSettings = null;
            ToolSettingsWindow dockableWindow = new ToolSettingsWindow(window);
            dockableWindow.Dock(window.MainDock.Handle, DockType.Free);
            window.ToolSettings = dockableWindow;
        }
    }


    [MenuBarItem("Window_5/Layers_4")]
    public class LayersMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            CanvasLayersWindow dockableWindow = new CanvasLayersWindow(window);
            dockableWindow.Dock(window.MainDock.Handle, DockType.Free);
        }
    }

    [MenuBarItem("Window_5/History_5")]
    public class HistoryMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            CanvasHistoryWindow dockableWindow = new CanvasHistoryWindow(window);
            dockableWindow.Dock(window.MainDock.Handle, DockType.Free);
        }
    }

    [MenuBarItem("Window_5/Histogram_6")]
    public class HistogramWindowMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            HistogramWindow histogramWindow = new HistogramWindow(window);
            histogramWindow.Dock(window.MainDock.Handle, DockType.Bottom);
        }
    }

    [MenuBarItem("Window_5/Model View_7")]
    public class ModelOperatingWindowMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            ModelOperatingWindow modeOperatingWindow = new ModelOperatingWindow(window);
            modeOperatingWindow.Dock(window.MainDock.Handle);
        }
    }

    [MenuBarItem("Window_5/Image Graph_8")]
    public class ImageGraphWindowMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            GraphEditorWindow graphEditorWindow = new GraphEditorWindow(window);
            graphEditorWindow.Dock(window.MainDock.Handle);
        }
    }

    [MenuBarItem("Window_5/Texture Debugger_100")]
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
            var opWindow = window.ActiveOperatingWindow;
            if (opWindow != null)
            {
                CanvasUtilities.MirrorCanvas(opWindow.Canvas, MirrorMode.Horizontal);
            }
        }
    }

    [MenuBarItem("Image_3/Mirror vertical_4")]
    public class MirrorVerticalMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            var opWindow = window.ActiveOperatingWindow;
            if (opWindow != null)
            {
                CanvasUtilities.MirrorCanvas(opWindow.Canvas, MirrorMode.Vertical);
            }
        }
    }

    [MenuBarItem("Image_3/Rotate 90 Clockwise_5")]
    public class RotateClockwiseMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            var opWindow = window.ActiveOperatingWindow;
            if (opWindow != null)
            {
                CanvasUtilities.RotateCanvas(opWindow.Canvas, RotateMode.Clockwise);
            }
        }
    }

    [MenuBarItem("Image_3/Rotate 90 Counter Clockwise_5")]
    public class RotateCounterClockwiseMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            var opWindow = window.ActiveOperatingWindow;
            if (opWindow != null)
            {
                CanvasUtilities.RotateCanvas(opWindow.Canvas, RotateMode.CounterClockwise);
            }
        }
    }

    [MenuBarItem("Image_3/Rotate 180_5")]
    public class RotateOneEightyMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            var opWindow = window.ActiveOperatingWindow;
            if (opWindow != null)
            {
                CanvasUtilities.RotateCanvas(opWindow.Canvas, RotateMode.OneEighty);
            }
        }
    }


    [MenuBarItem("Image_3/Resize..._1")]
    public class ResizeMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            var opWindow = window.ActiveOperatingWindow;
            if (opWindow != null)
            {
                ResizeImageWindow resizeImageWindow = new ResizeImageWindow(window, opWindow.Canvas);
                resizeImageWindow.Open();
            }
        }
    }



    [MenuBarItem("Edit_2/Undo_1", "Icons/UndoIcon", true)]
    public class UndoMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            var opWindow = window.ActiveOperatingWindow;
            if (opWindow != null)
            {
                opWindow.Canvas.History.Undo();
            }
        }
    }

    [MenuBarItem("Edit_2/Redo_2", "Icons/RedoIcon", true)]
    public class RedoMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            var opWindow = window.ActiveOperatingWindow;
            if (opWindow != null)
            {
                opWindow.Canvas.History.Redo();
            }
        }
    }


    [MenuBarItem("File_1/New_1", "Icons/NewCanvasIcon", true)]
    public class NewFileMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            ApplicationManager.Instance.CreateCanvas(new int2(1000, 750), Color.white, RenderTextureFormat.ARGBFloat);
        }
    }

    [MenuBarItem("File_1/Open_2", "Icons/FolderIcon", true)]
    public class OpenFileMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            CanvasIO.Open();
        }
    }

    [MenuBarItem("File_1/Save_3", "Icons/SaveIcon", true)]
    public class SaveMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            var opWindow = window.ActiveOperatingWindow;
            if (opWindow != null)
            {
                CanvasIO.Save(opWindow.Canvas);
            }
        }
    }

    [MenuBarItem("File_1/Save as..._4", "Icons/SaveAsIcon")]
    public class SaveAsMenuBarFunction : IMenuBarFunction
    {
        public void Execute(HueHadesWindow window)
        {
            var opWindow = window.ActiveOperatingWindow;
            if (opWindow != null)
            {
                CanvasIO.SaveAs(opWindow.Canvas);
            }
        }
    }


}
