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

    private const string ussLayersWindowFooter = "layers-window-footer";
    public LayersFooter(HueHadesWindow window, CanvasLayersWindow canvaslayersWindow) : base(window)
    {
        AddToClassList(ussLayersWindowFooter);

        _addLayerButton = new SquareButton(icon: "Icons/NewLayerIcon");
        _deleteLayerButton = new SquareButton(icon: "Icons/DeleteLayerIcon");
        _duplicateLayerButton = new SquareButton(icon: "Icons/DuplicateLayerIcon");
        _moveLayerUpButton = new SquareButton(icon: "Icons/LayerUpIcon");
        _moveLayerDownButton = new SquareButton(icon: "Icons/LayerDownIcon");

        hierarchy.Add(_addLayerButton);
        hierarchy.Add(_deleteLayerButton);
        hierarchy.Add(_duplicateLayerButton);
        hierarchy.Add(_moveLayerUpButton);
        hierarchy.Add(_moveLayerDownButton);

        /*_addLayerButton.clicked += () => { canvasHistoryWindow.OnAddLayer(); };
        _deleteLayerButton.clicked += () => { canvasHistoryWindow.OnDeleteLayer(); };
        _duplicateLayerButton.clicked += () => { canvasHistoryWindow.OnDuplicateLayer(); };
        _moveLayerUpButton.clicked += () => { canvasHistoryWindow.OnMoveLayerUp(); };
        _moveLayerDownButton.clicked += () => { canvasHistoryWindow.OnMoveLayerDown(); };*/
    }
}
