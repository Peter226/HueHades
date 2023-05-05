using HueHades.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryFooter : HueHadesElement
{
    private SquareButton _undoButton;
    private SquareButton _redoButton;

    private const string ussCanvasHistoryWindowFooter = "canvas-history-window-footer";
    public HistoryFooter(HueHadesWindow window, CanvasHistoryWindow canvasHistoryWindow) : base(window)
    {
        AddToClassList(ussCanvasHistoryWindowFooter);

        _undoButton = new SquareButton(icon: "Icons/UndoIcon");
        _redoButton = new SquareButton(icon: "Icons/RedoIcon");

        hierarchy.Add(_undoButton);
        hierarchy.Add(_redoButton);

        _undoButton.clicked += () => { canvasHistoryWindow.OnUndo(); };
        _redoButton.clicked += () => { canvasHistoryWindow.OnRedo(); };

    }
}
