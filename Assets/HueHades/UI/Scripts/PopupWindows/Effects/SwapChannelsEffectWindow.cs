using HueHades.Common;
using HueHades.Core;
using HueHades.Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace HueHades.UI
{
    public class SwapChannelsEffectWindow : EffectWindow
    {
        private SwapChannelsEffect _effect;

        private DropDownInput<ColorChannel> _redChannel;
        private DropDownInput<ColorChannel> _greenChannel;
        private DropDownInput<ColorChannel> _blueChannel;
        private DropDownInput<ColorChannel> _alphaChannel;


        public SwapChannelsEffectWindow(HueHadesWindow window) : base(window)
        {
            List<ColorChannel> colorChannels = new List<ColorChannel>() { ColorChannel.Red, ColorChannel.Green, ColorChannel.Blue, ColorChannel.Alpha };
            _redChannel = new DropDownInput<ColorChannel>(window);
            _redChannel.SetDataSource(colorChannels, (c) => Enum.GetName(typeof(ColorChannel), c));
            _redChannel.value = ColorChannel.Red;
            _redChannel.label = "Red    "; 

            _greenChannel = new DropDownInput<ColorChannel>(window);
            _greenChannel.SetDataSource(colorChannels, (c) => Enum.GetName(typeof(ColorChannel), c));
            _greenChannel.value = ColorChannel.Green;
            _greenChannel.label = "Green";

            _blueChannel = new DropDownInput<ColorChannel>(window);
            _blueChannel.SetDataSource(colorChannels, (c) => Enum.GetName(typeof(ColorChannel), c));
            _blueChannel.value = ColorChannel.Blue;
            _blueChannel.label = "Blue   ";

            _alphaChannel = new DropDownInput<ColorChannel>(window);
            _alphaChannel.SetDataSource(colorChannels, (c) => Enum.GetName(typeof(ColorChannel), c));
            _alphaChannel.value = ColorChannel.Alpha;
            _alphaChannel.label = "Alpha ";

            container.Add(_redChannel);
            container.Add(_greenChannel);
            container.Add(_blueChannel);
            container.Add(_alphaChannel);

            _redChannel.ValueChanged += OnColorChannelChange;
            _greenChannel.ValueChanged += OnColorChannelChange;
            _blueChannel.ValueChanged += OnColorChannelChange;
            _alphaChannel.ValueChanged += OnColorChannelChange;
        }

        private void OnColorChannelChange(ColorChannel c)
        {
            _effect.RedChannel = _redChannel.value;
            _effect.GreenChannel = _greenChannel.value;
            _effect.BlueChannel = _blueChannel.value;
            _effect.AlphaChannel = _alphaChannel.value;
            dataDirty = true;
        }


        protected override string GetWindowName()
        {
            return "Swap Channels";
        }

        protected override Vector2 GetDefaultSize()
        {
            var baseSize = base.GetDefaultSize();
            return new Vector2(baseSize.x * 0.5f, baseSize.y);
        }

        protected override void OnApplyEffect()
        {
            _effect.ApplyEffect();
        }

        protected override void OnBeginEffect(ImageCanvas canvas)
        {
            _effect = new SwapChannelsEffect();
            _effect.RedChannel = _redChannel.value;
            _effect.GreenChannel = _greenChannel.value;
            _effect.BlueChannel = _blueChannel.value;
            _effect.AlphaChannel = _alphaChannel.value;

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
}
