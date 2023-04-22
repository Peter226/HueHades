using HueHades.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HueHades.UI
{
    public class OperatingWindowFooter : HueHadesElement
    {
        private const string ussOperatingWindowFooter = "operating-window-footer";
        private ToggleButton _seamlessToggleHorizontal;
        private ToggleButton _seamlessToggleVertical;
        private ImageOperatingWindow operatingWindow;

        public OperatingWindowFooter(HueHadesWindow window, ImageOperatingWindow imageOperatingWindow) : base(window)
        {
            AddToClassList(ussOperatingWindowFooter);

            _seamlessToggleHorizontal = new ToggleButton(icon: "Icons/TileHorizontalCanvasIcon");
            hierarchy.Add(_seamlessToggleHorizontal);
            _seamlessToggleHorizontal.tooltip = "Tile Horizontally";
            _seamlessToggleHorizontal.OnToggle += OnTileChange;

            _seamlessToggleVertical = new ToggleButton(icon: "Icons/TileVerticalCanvasIcon");
            hierarchy.Add(_seamlessToggleVertical);
            _seamlessToggleVertical.tooltip = "Tile Vertically";
            _seamlessToggleVertical.OnToggle += OnTileChange;

            operatingWindow = imageOperatingWindow;
        }

        private void OnTileChange(bool obj)
        {
            bool horizontal = _seamlessToggleHorizontal.Toggled;
            bool vertical = _seamlessToggleVertical.Toggled;

            Debug.Log(horizontal + " " + vertical);

            CanvasTileMode tileMode;

            if (horizontal && vertical)
            {
                tileMode = CanvasTileMode.TileXY;
            }
            else
            {
                if (horizontal && !vertical)
                {
                    tileMode = CanvasTileMode.TileX;
                }
                else
                {
                    if (!horizontal && vertical)
                    {
                        tileMode = CanvasTileMode.TileY;
                    }
                    else
                    {
                        tileMode = CanvasTileMode.None;
                    }
                }
            }

            operatingWindow.Canvas.TileMode = tileMode;
            operatingWindow.Canvas.TileDisplayMode = tileMode;
        }

    }
}