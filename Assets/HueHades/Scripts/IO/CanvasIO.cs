using HueHades.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using SFB;
using System.IO;
using HueHades.Utilities;
using Unity.Mathematics;
using System.Linq;

namespace HueHades.IO
{

    public static class CanvasIO
    {

        private enum FileExtension
        {
            Png,
            Jpg
        }

        private static byte[] GetCanvasBytes(ImageCanvas imageCanvas, FileExtension encoding)
        {
            var outputTexture = imageCanvas.PreviewTexture.texture;
            RenderTexture gammaTexture = new RenderTexture(outputTexture.width, outputTexture.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.sRGB);
            gammaTexture.enableRandomWrite = true;
            gammaTexture.Create();
            ReusableTexture reusableTexture = new ReusableTexture(gammaTexture, outputTexture.width, outputTexture.height);

            RenderTextureUtilities.Sampling.LinearToSRGB(imageCanvas.PreviewTexture,reusableTexture);

            RenderTexture.active = gammaTexture;
            Texture2D textureBuffer = new Texture2D(outputTexture.width, outputTexture.height, TextureFormat.ARGB32, false);
            textureBuffer.hideFlags = HideFlags.HideAndDontSave;
            textureBuffer.ReadPixels(new Rect(0, 0, outputTexture.width, outputTexture.height), 0, 0);
            RenderTexture.active = null;

            reusableTexture.Dispose();

            byte[] buffer = null;
            switch (encoding)
            {
                case FileExtension.Png:
                    buffer = textureBuffer.EncodeToPNG();
                    break;
                case FileExtension.Jpg:
                    buffer = textureBuffer.EncodeToJPG();
                    break;
            }

            Object.Destroy(textureBuffer);
            return buffer;
        }



        public static void Save(ImageCanvas imageCanvas)
        {
            if (File.Exists(imageCanvas.FilePath))
            {
                File.WriteAllBytes(imageCanvas.FilePath, GetCanvasBytes(imageCanvas, imageCanvas.FilePath.EndsWith(".png") ? FileExtension.Png : FileExtension.Jpg));
            }
            else
            {
                SaveAs(imageCanvas);
            }
        }

        public static void SaveAs(ImageCanvas imageCanvas)
        {

            //StandaloneFileBrowser.SaveFilePanelAsync("Save location", "", imageCanvas.FileName, new ExtensionFilter[] { new ExtensionFilter("png", "png"), new ExtensionFilter("jpg", "jpg") }, (path) => { SaveAsFinish(path, imageCanvas); });
        }

        private static void SaveAsFinish(string path, ImageCanvas imageCanvas)
        {
            if (path == null || path.Length <= 0) return;
            File.WriteAllBytes(path, GetCanvasBytes(imageCanvas, path.EndsWith(".png") ? FileExtension.Png : FileExtension.Jpg));
            imageCanvas.FilePath = path;
            imageCanvas.FileName = Path.GetFileName(path);
        }

        public static ImageCanvas Open()
        {

            //StandaloneFileBrowser.OpenFilePanelAsync("Open image", "", new ExtensionFilter[] { new ExtensionFilter("image", "png", "jpg", "jpeg") }, true, OpenFinish);
            return null;
        }

        private static void OpenFinish(string[] paths)
        {
            if (paths.Length <= 0) return;
            foreach (string path in paths)
            {
                var bytes = File.ReadAllBytes(path);

                Texture2D textureBuffer = new Texture2D(1,1);
                textureBuffer.LoadImage(bytes);

                ReadableTexture2D readableTexture2D = new ReadableTexture2D(textureBuffer);

                var canvas = ApplicationManager.Instance.CreateCanvas(new int2(textureBuffer.width,textureBuffer.height), Color.white, RenderTextureFormat.ARGBFloat);
                canvas.FilePath = path;
                canvas.FileName = Path.GetFileName(path);
                RenderTextureUtilities.CopyTexture(readableTexture2D,canvas.GetGlobalLayers().First().Texture);
                canvas.RenderPreview();

                readableTexture2D.Dispose();

            }
        }

    }
}