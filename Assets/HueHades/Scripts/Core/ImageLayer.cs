using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HueHades.Utilities;
using System;
using Unity.Mathematics;
using static HueHades.Core.ImageLayer;

namespace HueHades.Core
{
    public class ImageLayer
    {
        private RenderTexture _renderTexture;
        internal RenderTexture Texture { get { return _renderTexture; } }
        public Action LayerChanged;
        private int2 _dimensions;

        public ImageLayer(int2 dimensions, RenderTextureFormat format)
        {
            _dimensions = dimensions;
            _renderTexture = new RenderTexture(_dimensions.x, _dimensions.y, 0, format, 0);
            _renderTexture.enableRandomWrite = true;
            _renderTexture.Create();
            RenderTextureUtilities.ClearTexture(_renderTexture, Color.white);
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

        public CopyHandle GetOperatingCopy(RenderTexture copyBuffer, int offsetX, int offsetY)
        {
            RenderTextureUtilities.CopyTexture(_renderTexture, offsetX, offsetY, copyBuffer.width, copyBuffer.height, copyBuffer);
            return new CopyHandle(offsetX, offsetY, copyBuffer.width, copyBuffer.height);
        }

        public CopyHandle GetOperatingCopy(RenderTexture copyBuffer)
        {
            RenderTextureUtilities.CopyTexture(_renderTexture, copyBuffer);
            return new CopyHandle(0,0, copyBuffer.width, copyBuffer.height);
        }

        public void ApplyBufferArea(RenderTexture copyBuffer, int destinationX, int destinationY, int sourceOffsetX, int sourceOffsetY, int sourceWidth, int sourceHeight)
        {
            RenderTextureUtilities.CopyTexture(copyBuffer, sourceOffsetX, sourceOffsetY, sourceWidth, sourceHeight, _renderTexture, destinationX, destinationY);
            LayerChanged?.Invoke();
        }

        public void PasteOperatingCopy(RenderTexture copyBuffer, CopyHandle copyHandle)
        {
            RenderTextureUtilities.CopyTexture(copyBuffer, 0, 0, copyHandle.SizeX, copyHandle.SizeY, _renderTexture, copyHandle.OffsetX, copyHandle.OffsetY);
            LayerChanged?.Invoke();
        }
    }
}