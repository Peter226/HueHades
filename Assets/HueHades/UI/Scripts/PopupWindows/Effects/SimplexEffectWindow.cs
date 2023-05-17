using HueHades.Core;
using HueHades.Effects;
using HueHades.UI;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

public class SimplexEffectWindow : EffectWindow
{
    private SimplexEffect _effect;

    private SliderInt _seed;
    private SliderInt _cellsX;
    private SliderInt _cellsY;
    private DropDownInput<VoronoiOutput> _outputType;

    public SimplexEffectWindow(HueHadesWindow window) : base(window)
    {
        _seed = new SliderInt();
        _seed.label = "Seed   ";
        _seed.lowValue = 0;
        _seed.highValue = 1000000;
        _seed.showInputField = true;
        _seed.RegisterCallback<FocusOutEvent>((f) => OnSeedChanged(null));
        _seed.RegisterValueChangedCallback(OnSeedChanged);

        _cellsX = new SliderInt();
        _cellsX.lowValue = 1;
        _cellsX.highValue = 1024;
        _cellsX.value = 16;
        _cellsX.label = "Cells X";
        _cellsX.showInputField = true;
        _cellsX.RegisterCallback<FocusOutEvent>((f) => OnCellsXChanged(null));
        _cellsX.RegisterValueChangedCallback(OnCellsXChanged);

        _cellsY = new SliderInt();
        _cellsY.lowValue = 1;
        _cellsY.highValue = 1024;
        _cellsY.value = 16;
        _cellsY.label = "Cells Y";
        _cellsY.showInputField = true;
        _cellsY.RegisterCallback<FocusOutEvent>((f) => OnCellsYChanged(null));
        _cellsY.RegisterValueChangedCallback(OnCellsYChanged);


        //_outputType = new DropDownInput<VoronoiOutput>(window);


        container.Add(_seed);
        container.Add(_cellsX);
        container.Add(_cellsY);
        //container.Add(_outputType);
    }

    private void OnCellsXChanged(ChangeEvent<int> cellsXEvent)
    {
        _effect.CellsX = _cellsX.value;
        dataDirty = true;
    }

    private void OnCellsYChanged(ChangeEvent<int> cellsYEvent)
    {
        _effect.CellsY = _cellsY.value;
        dataDirty = true;
    }

    private void OnSeedChanged(ChangeEvent<int> seedEvent)
    {
        _effect.Seed = _seed.value;
        dataDirty = true;
    }

    protected override string GetWindowName()
    {
        return "OpenSimplex";
    }


    protected override void OnApplyEffect()
    {
        _effect.ApplyEffect();
    }

    protected override void OnBeginEffect(ImageCanvas canvas)
    {
        _effect = new SimplexEffect();
        _effect.Seed = _seed.value;
        _effect.CellsX = _cellsX.value;
        _effect.CellsY = _cellsY.value;

        if (!_effect.CanExecute(canvas))
        {
            Close();
            return;
        }

        _effect.BeginEffect(canvas);
    }

    protected override void OnCancelEffect()
    {
        _effect.CancelEffect();
    }

    protected override void OnRenderEffect()
    {
        _effect.RenderEffect();
    }
}
