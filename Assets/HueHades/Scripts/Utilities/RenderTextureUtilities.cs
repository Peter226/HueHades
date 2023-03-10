using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HueHades.Common;
using UnityEngine.Profiling;

namespace HueHades.Utilities
{
    public static class RenderTextureUtilities
    {

        private static Dictionary<(int, RenderTextureFormat), List<RenderTexture>> _renderTexturePool;

        public static ComputeShader CopyImageShader;
        public static ComputeShader ClearImageShader;
        public static ComputeShader LayerImageShader;

        private static int CopyShaderKernel;
        private static int InputPropertyID;
        private static int ResultPropertyID;
        private static int SrcDstDimPropertyID;
        private static int DstXYPropertyID;
        private static int SrcRectPropertyID;
        private static int TileSrcXYDstXYPropertyID;
        private const int warpSizeX = 8;
        private const int warpSizeY = 8;

        private static int ClearColorPropertyID;
        private static int ClearColorKernel;

        private static int BlendNormalKernel;
        private static int BlendAddKernel;
        private static int BlendMultiplyKernel;
        private static int BlendSubtractKernel;
        private static int TopLayerPropertyID;
        private static int BottomLayerPropertyID;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            CopyImageShader = Resources.Load<ComputeShader>("CopyImage");
            CopyShaderKernel = CopyImageShader.FindKernel("CSMain");
            InputPropertyID = Shader.PropertyToID("Input");
            ResultPropertyID = Shader.PropertyToID("Result");
            SrcDstDimPropertyID = Shader.PropertyToID("SrcDstDim");
            DstXYPropertyID = Shader.PropertyToID("DstXY");
            SrcRectPropertyID = Shader.PropertyToID("SrcRect");
            TileSrcXYDstXYPropertyID = Shader.PropertyToID("TileSrcXYDstXYRect");

            ClearImageShader = Resources.Load<ComputeShader>("ClearImage");
            ClearColorKernel = ClearImageShader.FindKernel("CSMain");
            ClearColorPropertyID = Shader.PropertyToID("ClearColor");

            LayerImageShader = Resources.Load<ComputeShader>("LayerImage");
            BlendNormalKernel = LayerImageShader.FindKernel("NormalBlend");
            BlendAddKernel = LayerImageShader.FindKernel("AddBlend");
            BlendMultiplyKernel = LayerImageShader.FindKernel("MultiplyBlend");
            BlendSubtractKernel = LayerImageShader.FindKernel("SubtractBlend");
            TopLayerPropertyID = Shader.PropertyToID("TopLayer");
            BottomLayerPropertyID = Shader.PropertyToID("BottomLayer");

        }

        public static void InitializePool()
        {
            _renderTexturePool = new Dictionary<(int, RenderTextureFormat), List<RenderTexture>>();
        }

        public static RenderTexture GetTemporary(int sizeX, int sizeY, RenderTextureFormat format, out int availableSize)
        {
            int maxSize = Mathf.Max(sizeX, sizeY);

            availableSize = 1;
            while (availableSize < maxSize)
            {
                availableSize *= 2;
            }

            var key = (availableSize, format);
            if (_renderTexturePool == null)
            {
                _renderTexturePool = new Dictionary<(int, RenderTextureFormat), List<RenderTexture>>();
            }
            List<RenderTexture> pooledTextures;
            if (!_renderTexturePool.TryGetValue(key, out pooledTextures))
            {
                pooledTextures = new List<RenderTexture>();
                _renderTexturePool.Add(key, pooledTextures);
            }

            RenderTexture temp;
            if (pooledTextures.Count > 0)
            {
                var index = pooledTextures.Count - 1;
                temp = pooledTextures[index];
                pooledTextures.RemoveAt(index);
            }
            else
            {
                temp = new RenderTexture(availableSize, availableSize, 0, format, 0);
                temp.enableRandomWrite = true;
                temp.Create();
            }
            return temp;
        }

        public static void ReleaseTemporary(RenderTexture renderTexture)
        {
            _renderTexturePool[(renderTexture.width, renderTexture.format)].Add(renderTexture);
        }

        public static void CopyTexture(RenderTexture from, RenderTexture to, CanvasTileMode destinationTileMode = CanvasTileMode.None, CanvasTileMode sourceTileMode = CanvasTileMode.None)
        {
            CopyTexture(from, to, 0, 0, destinationTileMode, sourceTileMode);
        }
        public static void CopyTexture(RenderTexture from, RenderTexture to, int destinationX, int destinationY, CanvasTileMode destinationTileMode = CanvasTileMode.None, CanvasTileMode sourceTileMode = CanvasTileMode.None)
        {
            CopyTexture(from, 0, 0, from.width, from.height, to, destinationX, destinationY, destinationTileMode, sourceTileMode);
        }
        public static void CopyTexture(RenderTexture from, int sourceX, int sourceY, int sourceWidth, int sourceHeight, RenderTexture to, int destinationX = 0, int destinationY = 0, CanvasTileMode destinationTileMode = CanvasTileMode.None, CanvasTileMode sourceTileMode = CanvasTileMode.None)
        {
            Profiler.BeginSample("Set Variables");
            CopyImageShader.SetTexture(CopyShaderKernel, InputPropertyID, from);
            CopyImageShader.SetTexture(CopyShaderKernel, ResultPropertyID, to);
            CopyImageShader.SetInts(SrcDstDimPropertyID, from.width, from.height, to.width, to.height);
            CopyImageShader.SetInts(DstXYPropertyID, destinationX, destinationY);
            CopyImageShader.SetInts(SrcRectPropertyID, sourceX, sourceY, sourceWidth, sourceHeight);
            Profiler.EndSample();
            Profiler.BeginSample("Byte stuff");
            byte sourceTileX = (sourceTileMode == CanvasTileMode.TileX || sourceTileMode == CanvasTileMode.TileXY) ? byte.MaxValue : byte.MinValue;
            byte sourceTileY = (sourceTileMode == CanvasTileMode.TileY || sourceTileMode == CanvasTileMode.TileXY) ? byte.MaxValue : byte.MinValue;
            byte destinationTileX = (destinationTileMode == CanvasTileMode.TileX || destinationTileMode == CanvasTileMode.TileXY) ? byte.MaxValue : byte.MinValue;
            byte destinationTileY = (destinationTileMode == CanvasTileMode.TileY || destinationTileMode == CanvasTileMode.TileXY) ? byte.MaxValue : byte.MinValue;
            Profiler.EndSample();
            int tileComposite = 0;
            tileComposite |= sourceTileX;
            tileComposite <<= 8;
            tileComposite |= sourceTileY;
            tileComposite <<= 8;
            tileComposite |= destinationTileX;
            tileComposite <<= 8;
            tileComposite |= destinationTileY;

            CopyImageShader.SetInt(TileSrcXYDstXYPropertyID, tileComposite);
            CopyImageShader.Dispatch(CopyShaderKernel, Mathf.CeilToInt(sourceWidth / (float)warpSizeX), Mathf.CeilToInt(sourceHeight / (float)warpSizeY), 1);
        }

        public static void ClearTexture(RenderTexture texture, Color clearColor)
        {
            ClearImageShader.SetTexture(ClearColorKernel, ResultPropertyID, texture);
            ClearImageShader.SetVector(ClearColorPropertyID, clearColor);
            ClearImageShader.Dispatch(ClearColorKernel, Mathf.CeilToInt(texture.width / (float)warpSizeX), Mathf.CeilToInt(texture.height / (float)warpSizeY), 1);
        }

        public static void LayerImage(RenderTexture bottomLayer, RenderTexture topLayer, RenderTexture result, ColorBlendMode colorBlendMode)
        {
            int dispatchKernel;
            switch (colorBlendMode)
            {
                case ColorBlendMode.Default:
                    dispatchKernel = BlendNormalKernel;
                    break;
                case ColorBlendMode.Add:
                    dispatchKernel = BlendAddKernel;
                    break;
                case ColorBlendMode.Multiply:
                    dispatchKernel = BlendMultiplyKernel;
                    break;
                case ColorBlendMode.Subtract:
                    dispatchKernel = BlendSubtractKernel;
                    break;
                default:
                    dispatchKernel = BlendNormalKernel;
                    break;
            }

            LayerImageShader.SetTexture(dispatchKernel,BottomLayerPropertyID,bottomLayer);
            LayerImageShader.SetTexture(dispatchKernel,TopLayerPropertyID,topLayer);
            LayerImageShader.SetTexture(dispatchKernel,ResultPropertyID,result);
            LayerImageShader.Dispatch(dispatchKernel, Mathf.CeilToInt(result.width / (float)warpSizeX), Mathf.CeilToInt(result.height / (float)warpSizeY), 1);
        }
    }
}