using HueHades.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayersFooter : HueHadesElement
{
    private SquareButton _addLayerButton;
    private SquareButton _deleteLayerButton;
    private SquareButton _duplicateLayerButton;
    private SquareButton _moveLayerUpButton;
    private SquareButton _moveLayerDownButton;
    private SquareButton _settingsButton;


    private const string ussLayersWindowFooter = "layers-window-footer";
    public LayersFooter(HueHadesWindow window, CanvasLayersWindow canvaslayersWindow) : base(window)
    {
        AddToClassList(ussLayersWindowFooter);

        _addLayerButton = new SquareButton(icon: "Icons/NewLayerIcon");
        _deleteLayerButton = new SquareButton(icon: "Icons/DeleteLayerIcon");
        _duplicateLayerButton = new SquareButton(icon: "Icons/DuplicateLayerIcon");
        _moveLayerUpButton = new SquareButton(icon: "Icons/LayerUpIcon");
        _moveLayerDownButton = new SquareButton(icon: "Icons/LayerDownIcon");
        _moveLayerDownButton = new SquareButton(icon: "Icons/LayerDownIcon");
        _settingsButton = new SquareButton(icon: "Icons/SettingsIcon");

        hierarchy.Add(_addLayerButton);
        hierarchy.Add(_deleteLayerButton);
        hierarchy.Add(_duplicateLayerButton);
        hierarchy.Add(_moveLayerUpButton);
        hierarchy.Add(_moveLayerDownButton);
        hierarchy.Add(_settingsButton);

        _addLayerButton.clicked += () => { canvaslayersWindow.OnAddLayer(); };
        _deleteLayerButton.clicked += () => { canvaslayersWindow.OnDeleteLayer(); };
        _duplicateLayerButton.clicked += () => { canvaslayersWindow.OnDuplicateLayer(); };
        _moveLayerUpButton.clicked += () => { canvaslayersWindow.OnMoveLayerUp(); };
        _moveLayerDownButton.clicked += () => { canvaslayersWindow.OnMoveLayerDown(); };
        _settingsButton.clicked += () => { canvaslayersWindow.OnSettings(); };
    }
}
