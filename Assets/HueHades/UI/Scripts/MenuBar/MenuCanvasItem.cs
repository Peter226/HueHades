using HueHades.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuCanvasItem : HueHadesElement
{

    private Button _button;
    private Button _closeButton;
    private Image _image;

    private const string ussMenuCanvasItem = "menu-canvas-item";
    private const string ussMenuCanvasItemButton = "menu-canvas-item-button";
    private const string ussMenuCanvasItemCloseButton = "menu-canvas-item-close-button";
    private const string ussMenuCanvasItemImage = "menu-canvas-item-image";

    public MenuCanvasItem(HueHadesWindow window, ImageCanvas canvas) : base(window)
    {
        AddToClassList(ussMenuCanvasItem);
        _button = new Button();
        _button.AddToClassList(ussMenuCanvasItemButton);
        _image = new Image();
        _image.AddToClassList(ussMenuCanvasItemImage);
        _closeButton = new Button();
        _closeButton.AddToClassList(ussMenuCanvasItemCloseButton);
        _closeButton.text = "✕";
        _closeButton.clicked += () => ApplicationManager.Instance.CloseCanvas(canvas);
    
        _image.image = canvas.PreviewTexture.texture;
        canvas.PreviewChanged += () => { _image.image = canvas.PreviewTexture.texture; };

        Add(_button);
        _button.Add(_image);
        _button.Add(_closeButton);

        _button.clicked += () => ApplicationManager.Instance.SelectCanvas(canvas);
    }
}
