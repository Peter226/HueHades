using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HueHades.Utilities;
using System;
using Unity.Mathematics;
using static HueHades.Core.ImageLayer;
using HueHades.Common;

namespace HueHades.Core
{
    public class ImageLayer : LayerBase
    {
        public override ReusableTexture Texture { get { return renderTexture; } }
        
        private Color clearColor;

        public ImageLayer(int2 dimensions, RenderTextureFormat format, Color clearColor) : base(dimensions, format)
        {
            RenderTextureUtilities.ClearTexture(renderTexture, clearColor);
            this.clearColor = clearColor;
        }

        internal override void SetDimensions(int2 dimensions, Action<ResizeLayerEventArgs> onResizeMethod)
        {
            _dimensions = dimensions;
            var oldTexture = renderTexture;
            renderTexture = new ReusableTexture(_dimensions.x, _dimensions.y, oldTexture.format, 0);
            RenderTextureUtilities.ClearTexture(renderTexture, clearColor);
            if (onResizeMethod != null)
            {
                onResizeMethod.Invoke(new ResizeLayerEventArgs(oldTexture, renderTexture));
            }
            oldTexture.Dispose();
        }



        public struct CopyHandle
        {
            private int _offsetX;
            private int _offsetY;
            private int _sizeX;
            private int _sizeY;

            internal int OffsetX { get { return _offsetX; } }
            internal int OffsetY { get { return _offsetY; } }
            internal int SizeX { get { return _sizeX; } }
            internal int SizeY { get { return _sizeY; } }

            public CopyHandle(int offsetX, int offsetY, int sizeX, int sizeY)
            {
                _offsetX = offsetX;
                _offsetY = offsetY;
                _sizeX = sizeX;
                _sizeY = sizeY;
            }
        }

        public CopyHandle GetOperatingCopy(ReusableTexture copyBuffer, int offsetX, int offsetY)
        {
            RenderTextureUtilities.CopyTexture(renderTexture, offsetX, offsetY, copyBuffer.width, copyBuffer.height, copyBuffer);
            return new CopyHandle(offsetX, offsetY, copyBuffer.width, copyBuffer.height);
        }

        public CopyHandle GetOperatingCopy(ReusableTexture copyBuffer)
        {
            RenderTextureUtilities.CopyTexture(renderTexture, copyBuffer);
            return new CopyHandle(0,0, copyBuffer.width, copyBuffer.height);
        }

        public void PasteOperatingArea(ReusableTexture copyBuffer, int destinationX, int destinationY, int sourceOffsetX, int sourceOffsetY, int sourceWidth, int sourceHeight, CanvasTileMode tileMode)
        {
            RenderTextureUtilities.CopyTexture(copyBuffer, sourceOffsetX, sourceOffsetY, sourceWidth, sourceHeight, renderTexture, destinationX, destinationY, tileMode, tileMode);
            LayerChanged?.Invoke();
        }

        public void PasteOperatingCopy(ReusableTexture baseBuffer, ReusableTexture copyBuffer, CopyHandle copyHandle)
        {
            if (CanvasIn.Selection.SelectedArea <= 0)
            {
                RenderTextureUtilities.CopyTexture(copyBuffer, 0, 0, copyHandle.SizeX, copyHandle.SizeY, renderTexture, copyHandle.OffsetX, copyHandle.OffsetY);
            }
            else
            {
                RenderTextureUtilities.Selection.UpdateMaskArea(baseBuffer, copyBuffer, renderTexture, 0, 0, copyHandle.SizeX, copyHandle.SizeY, CanvasIn.Selection.SelectionTexture, copyHandle.OffsetX, copyHandle.OffsetY);
            }
            
            LayerChanged?.Invoke();
        }


        public class ResizeLayerEventArgs : EventArgs
        {
            public ReusableTexture oldTexture { get; private set; }
            public ReusableTexture newTexture { get; private set; }

            public ResizeLayerEventArgs(ReusableTexture oldTexture, ReusableTexture newTexture)
            {
                this.oldTexture = oldTexture;
                this.newTexture = newTexture;
            }

        }
}
}