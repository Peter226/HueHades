using HueHades.Core;
using HueHades.Tools;
using HueHades.UI;
using HueHades.Utilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class MoveSelectedToolController : ToolController
{
    private HueHadesWindow _hueHadesWindow;
    private ImageOperatingWindow _imageOperatingWindow;
    private TransformImageHandle _transformHandle;
    public override IToolContext CollectContext(HueHadesWindow window, PointerDownEvent pointerDownEvent)
    {
        return null;
    }

    private ImageCanvas _canvas;
    private ReusableTexture _grabbedImage;
    private ReusableTexture _baseImage;
    private ReusableTexture _grabbedSelection;
    private ReusableTexture _layerTexture;
    private ReusableTexture _selectionTexture;

    private int4 _startBounds;
    private int4 _lastBounds;

    protected override void OnSelected(HueHadesWindow window)
    {
        _hueHadesWindow = window;
        _transformHandle = new TransformImageHandle();
        _imageOperatingWindow = _hueHadesWindow.ActiveOperatingWindow;
        _imageOperatingWindow.WindowDisplay.Add(_transformHandle);
        _hueHadesWindow.ActiveOperatingWindowChanged += OnSelectedWindowChange;

        var canvas = _imageOperatingWindow.Canvas;
        _canvas = canvas;
        var layerTexture = canvas.SelectedLayer.Texture;
        _layerTexture = layerTexture;
        var selection = canvas.Selection;
        var selectionTexture = selection.SelectionTexture;
        _selectionTexture = selectionTexture;
        var selectionArea = selection.SelectedAreaBounds;
        _startBounds = selectionArea;
        _lastBounds = selectionArea;
        var selectionWidth = selectionArea.z - selectionArea.x;
        var selectionHeight = selectionArea.w - selectionArea.y;
        _grabbedImage = RenderTextureUtilities.GetTemporary(selectionWidth, selectionHeight, canvas.Format);
        _grabbedSelection = RenderTextureUtilities.GetTemporary(selectionWidth, selectionHeight, canvas.Format);
        var grabBuffer = RenderTextureUtilities.GetTemporary(selectionWidth, selectionHeight, canvas.Format);
        //var eraseBuffer = RenderTextureUtilities.GetTemporary(selectionWidth, selectionHeight, canvas.Format);
        RenderTextureUtilities.CopyTexture(selectionTexture, selectionArea.x, selectionArea.y, selectionWidth, selectionHeight, _grabbedSelection, 0, 0, HueHades.Common.CanvasTileMode.None, canvas.TileMode);
        RenderTextureUtilities.CopyTexture(layerTexture, selectionArea.x, selectionArea.y, selectionWidth, selectionHeight, grabBuffer, 0, 0, HueHades.Common.CanvasTileMode.None, canvas.TileMode);
        RenderTextureUtilities.Selection.ApplyMaskArea(grabBuffer, _grabbedImage, 0, 0, selectionWidth, selectionHeight, _grabbedSelection, 0, 0);
        RenderTextureUtilities.LayerImageArea(grabBuffer, layerTexture, 0, 0, selectionWidth, selectionHeight, _grabbedSelection, HueHades.Common.ColorBlendMode.Erase, selectionArea.x, selectionArea.y);
        RenderTextureUtilities.ReleaseTemporary(grabBuffer);

        _baseImage = RenderTextureUtilities.GetTemporary(canvas.Dimensions.x, canvas.Dimensions.y, canvas.Format);
        RenderTextureUtilities.CopyTexture(layerTexture, _baseImage);
        
        _transformHandle.Initialize(_imageOperatingWindow);

        _transformHandle.TransformChanged += UpdateTransformation;
        UpdateTransformation();
    }

    private void UpdateTransformation()
    {
        int4 newBounds;
        newBounds = new int4((int)_transformHandle.PositionDelta.x + _startBounds.x, (int)_transformHandle.PositionDelta.y + _startBounds.y, (int)_transformHandle.PositionDelta.x + _startBounds.z, (int)_transformHandle.PositionDelta.y + _startBounds.w);
        var oldWidth = _lastBounds.z - _lastBounds.x;
        var newWidth = newBounds.z - newBounds.x;
        var oldHeight = _lastBounds.w - _lastBounds.y;
        var newHeight = newBounds.w - newBounds.y;

        var startWidth = _startBounds.z - _startBounds.x;
        var startHeight = _startBounds.w - _startBounds.y;

        var startPivot = new Vector2(startWidth * 0.5f, startHeight * 0.5f);
        var newPivot = new Vector2(newWidth * 0.5f, newHeight * 0.5f);
        

        var transformedBuffer = RenderTextureUtilities.GetTemporary(newWidth, newHeight, _canvas.Format);
        var baseBuffer = RenderTextureUtilities.GetTemporary(newWidth, newHeight, _canvas.Format);
        RenderTextureUtilities.CopyTexture(_baseImage, newBounds.x, newBounds.y, newWidth, newHeight, baseBuffer, 0, 0, HueHades.Common.CanvasTileMode.None, _canvas.TileMode);
        RenderTextureUtilities.CopyTexture(_baseImage, _lastBounds.x, _lastBounds.y, oldWidth, oldHeight, _layerTexture, _lastBounds.x, _lastBounds.y, _canvas.TileMode, _canvas.TileMode);
        Vector2 sampleSize = new Vector2(newWidth / (float)startWidth, newHeight / (float)startHeight);
        RenderTextureUtilities.Sampling.Resample(_grabbedImage, transformedBuffer, sampleSize, 0, startPivot, newPivot, SamplerMode.Point);

        RenderTextureUtilities.LayerImageArea(baseBuffer, _layerTexture, 0, 0, newWidth, newHeight, transformedBuffer, HueHades.Common.ColorBlendMode.Default, newBounds.x, newBounds.y, _canvas.TileMode);
        //RenderTextureUtilities.CopyTexture(transformedBuffer, 0, 0, newWidth, newHeight, _layerTexture, newBounds.x, newBounds.y, _canvas.TileMode);
        RenderTextureUtilities.ReleaseTemporary(transformedBuffer);
        RenderTextureUtilities.ReleaseTemporary(baseBuffer);

        _lastBounds = newBounds;

        _canvas.SelectedLayer.LayerChanged?.Invoke();
    }

    private void OnSelectedWindowChange(ImageOperatingWindow newWindow)
    {
        if (_imageOperatingWindow == newWindow) return;
        _imageOperatingWindow.WindowDisplay.Remove(_transformHandle);
        _imageOperatingWindow = newWindow;
        _imageOperatingWindow.WindowDisplay.Add(_transformHandle);
        _transformHandle.Initialize(_imageOperatingWindow);
    }

    protected override void OnDeselected()
    {
        _imageOperatingWindow.WindowDisplay.Remove(_transformHandle);
        _hueHadesWindow.ActiveOperatingWindowChanged -= OnSelectedWindowChange;
        RenderTextureUtilities.ReleaseTemporary(_grabbedSelection);
        RenderTextureUtilities.ReleaseTemporary(_grabbedImage);
    }

    public override Texture GetIcon()
    {
        if (Icon == null)
        {
            Icon = Resources.Load<Texture2D>("Icons/MoveSelectedIcon");
        }
        return Icon;
    }

    protected override ImageTool InitializeTool()
    {
        return new MoveSelectedImageTool();
    }

}
