using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HueHades.Utilities;

namespace HueHades.Core
{
    public class ImageLayer
    {
        private RenderTexture _renderTexture;


        public void GetOperatingCopy(RenderTexture copyBuffer)
        {
            RenderTextureUtilities.CopyTexture(_renderTexture, copyBuffer);
        }

        public void PasteOperatingCopy(RenderTexture copyBuffer)
        {
            RenderTextureUtilities.CopyTexture(copyBuffer, _renderTexture);
        }
    }
}