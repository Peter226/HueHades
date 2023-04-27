using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class TextureDebuggerWindow : DockableWindow
    {
        public TextureDebuggerWindow(HueHadesWindow window) : base(window)
        {
            TextureDebugger.DrawCommandFired += OnDrawTextures;
            WindowName = "Texture Debugger";
        }

        private void OnDrawTextures(List<(ReusableTexture, string)> textures)
        {
            hierarchy.Clear();
            ScrollView scrollView = new ScrollView();
            scrollView.style.flexGrow = 1;
            scrollView.contentContainer.style.flexDirection = FlexDirection.Row;
            scrollView.contentContainer.style.flexWrap = Wrap.Wrap;
            scrollView.contentContainer.style.flexGrow = 1;

            foreach(var (rt, l) in textures)
            {
                var debugObject = new VisualElement();
                var label = new Label();
                label.text = l;
                var image = new Image();
                image.style.borderRightWidth = 1;
                image.style.borderLeftWidth = 1;
                image.style.borderBottomWidth = 1;
                image.style.borderTopWidth = 1;
                image.style.borderRightColor = Color.white;
                image.style.borderLeftColor = Color.white;
                image.style.borderBottomColor = Color.white;
                image.style.borderTopColor = Color.white;
                image.style.width = 200;
                image.style.height = 200;
                image.image = rt.texture;
                debugObject.Add(label);
                debugObject.Add(image);
                scrollView.Add(debugObject);
            }
            hierarchy.Add(scrollView);
            scrollView.horizontalScrollerVisibility = ScrollerVisibility.Auto;
            scrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;
        }
    }
}