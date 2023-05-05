using HueHades.Core;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI {
    public abstract class EffectWindow : PopupWindow
    {
        protected bool dataDirty;
        private bool _didApply;

        public EffectWindow(HueHadesWindow window) : base(window)
        {
            var _bottomButtos = new VisualElement();
            _bottomButtos.AddToClassList(Layouts.Horizontal);
            _bottomButtos.AddToClassList(Layouts.SpaceAround);

            var _applyButton = new Button();
            _applyButton.text = "Apply";
            _bottomButtos.Add(_applyButton);
            _applyButton.clicked += BeginApply;


            var _cancelButton = new Button();
            _cancelButton.text = "Cancel";
            _bottomButtos.Add(_cancelButton);
            _bottomButtos.style.flexGrow = 1;
            _bottomButtos.style.alignContent = Align.FlexEnd;
            _bottomButtos.style.alignItems = Align.FlexEnd;
            _cancelButton.clicked += Close;

            hierarchy.Add(_bottomButtos);
        }

        protected sealed override void OnOpen()
        {
            if (window.ActiveOperatingWindow == null)
            {
                Close();
                return;
            }
            ImageOperatingWindow.CameraUpdateManager.OnUpdate += BeginRender;
            
            OnBeginEffect(window.ActiveOperatingWindow.Canvas);
            OnRenderEffect();
        }

        protected sealed override void OnClose()
        {
            if(!_didApply) BeginCancel();
        }

        private void BeginCancel()
        {
            if (window.ActiveOperatingWindow == null)
            {
                return;
            }
            ImageOperatingWindow.CameraUpdateManager.OnUpdate -= BeginRender;
            OnCancelEffect();
        }
        private void BeginApply()
        {
            _didApply = true;
            ImageOperatingWindow.CameraUpdateManager.OnUpdate -= BeginRender;
            OnApplyEffect();
            Close();
        }

        protected override Vector2 GetDefaultSize()
        {
            return new Vector2(380, 210);
        }

        private void BeginRender()
        {
            if (dataDirty)
            {
                OnRenderEffect();
                dataDirty = false;
            }
        }

        /// <summary>
        /// Called when the effect needs to be initialized on canvas
        /// </summary>
        protected abstract void OnBeginEffect(ImageCanvas canvas);

        /// <summary>
        /// Called when the preview of the effect needs to be rendered
        /// </summary>
        protected abstract void OnRenderEffect();

        /// <summary>
        /// Called when the effect is finalized and applied to the canvas
        /// </summary>
        protected abstract void OnApplyEffect();

        /// <summary>
        /// Called when the effect is cancelled, and canvas needs to be reverted
        /// </summary>
        protected abstract void OnCancelEffect();
    }
}
