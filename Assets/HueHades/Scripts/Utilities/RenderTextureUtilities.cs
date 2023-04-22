using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HueHades.Common;
using UnityEngine.Profiling;
using UnityEngine.TerrainUtils;

namespace HueHades.Utilities
{
    public static class RenderTextureUtilities
    {

        private static Dictionary<(int, RenderTextureFormat), List<RenderTexture>> _renderTexturePool;
        private static Dictionary<(int, RenderTextureFormat), List<RenderTexture>> _gradientRenderTexturePool;

        public static ComputeShader CopyImageShader;
        public static ComputeShader ClearImageShader;
        public static ComputeShader LayerImageShader;
        public static ComputeShader LayerImageAreaShader;
        public static ComputeShader MaskChannelsShader;

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

        private static int BlendAreaNormalKernel;
        private static int BlendAreaAddKernel;
        private static int BlendAreaMultiplyKernel;
        private static int BlendAreaSubtractKernel;

        private static int MaskChannelsKernel;
        private static int TargetPropertyID;
        private static int MaskPropertyID;

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

            LayerImageAreaShader = Resources.Load<ComputeShader>("LayerImageArea");

            BlendAreaNormalKernel = LayerImageAreaShader.FindKernel("NormalBlend");
            BlendAreaAddKernel = LayerImageAreaShader.FindKernel("AddBlend");
            BlendAreaMultiplyKernel = LayerImageAreaShader.FindKernel("MultiplyBlend");
            BlendAreaSubtractKernel = LayerImageAreaShader.FindKernel("SubtractBlend");


            MaskChannelsShader = Resources.Load<ComputeShader>("MaskChannels");
            MaskChannelsKernel = MaskChannelsShader.FindKernel("CSMain");
            TargetPropertyID = Shader.PropertyToID("Target");
            MaskPropertyID = Shader.PropertyToID("Mask");

            Gradients.Initialize();
            Brushes.Initialize();
        }

        public static void InitializePool()
        {
            _renderTexturePool = new Dictionary<(int, RenderTextureFormat), List<RenderTexture>>();
            _gradientRenderTexturePool = new Dictionary<(int, RenderTextureFormat), List<RenderTexture>>();
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


        public static RenderTexture GetTemporaryGradient(int sizeX, RenderTextureFormat format, out int availableSize)
        {
            int maxSize = sizeX;

            availableSize = 1;
            while (availableSize < maxSize)
            {
                availableSize *= 2;
            }

            var key = (availableSize, format);
            if (_gradientRenderTexturePool == null)
            {
                _gradientRenderTexturePool = new Dictionary<(int, RenderTextureFormat), List<RenderTexture>>();
            }
            List<RenderTexture> pooledTextures;
            if (!_gradientRenderTexturePool.TryGetValue(key, out pooledTextures))
            {
                pooledTextures = new List<RenderTexture>();
                _gradientRenderTexturePool.Add(key, pooledTextures);
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
                temp = new RenderTexture(availableSize, 1, 0, format, 0);
                temp.enableRandomWrite = true;
                temp.Create();
            }
            return temp;
        }




        public static void ReleaseTemporary(RenderTexture renderTexture)
        {
            _renderTexturePool[(renderTexture.width, renderTexture.format)].Add(renderTexture);
        }

        public static void ReleaseTemporaryGradient(RenderTexture gradientRenderTexture)
        {
            _gradientRenderTexturePool[(gradientRenderTexture.width, gradientRenderTexture.format)].Add(gradientRenderTexture);
        }


        public static void CopyTexture(RenderTexture from, RenderTexture to, CanvasTileMode destinationTileMode = CanvasTileMode.None, CanvasTileMode sourceTileMode = CanvasTileMode.None)
        {
            CopyTexture(from, to, 0, 0, destinationTileMode, sourceTileMode);
        }
        public static void CopyTexture(RenderTexture from, RenderTexture to, int destinationX, int destinationY, CanvasTileMode destinationTileMode = CanvasTileMode.None, CanvasTileMode sourceTileMode = CanvasTileMode.None)
        {
            CopyTexture(from, 0, 0, from.width, from.height, to, destinationX, destinationY, destinationTileMode, sourceTileMode);
        }

        private static int Mod(int num, int mod) {

            var m = num % mod;
            return m < 0 ? m + mod : m;
        }
        private static void ClampSourceToTile(ref int indexToClamp, int maxLength, ref int length, int tile, ref int destination)
        {
            if (tile != byte.MinValue)
            {
                indexToClamp = Mod(indexToClamp, maxLength);
            }
            else
            {
                if (indexToClamp < 0)
                {
                    length += indexToClamp;
                    destination -= indexToClamp;
                    if (length < 0) length = 0;
                    indexToClamp = 0;
                }
            }
        }
        private static void ClampDestinationToTile(ref int indexToClamp, int maxLength, ref int length, int tile, ref int source)
        {
            if (tile != byte.MinValue)
            {
                indexToClamp = Mod(indexToClamp, maxLength);
            }
            else
            {
                if (indexToClamp < 0)
                {
                    source -= indexToClamp;
                    length += indexToClamp;
                    if (length < 0) length = 0;
                    indexToClamp = 0;
                }
            }
        }

        public static void CopyTexture(RenderTexture from, int sourceX, int sourceY, int sourceWidth, int sourceHeight, RenderTexture to, int destinationX = 0, int destinationY = 0, CanvasTileMode destinationTileMode = CanvasTileMode.None, CanvasTileMode sourceTileMode = CanvasTileMode.None)
        {
            int sourceTextureWidth = from.width;
            int sourceTextureHeight = from.height;
            int destinationTextureWidth = to.width;
            int destinationTextureHeight = to.height;

            byte sourceTileX = (sourceTileMode == CanvasTileMode.TileX || sourceTileMode == CanvasTileMode.TileXY) ? byte.MaxValue : byte.MinValue;
            byte sourceTileY = (sourceTileMode == CanvasTileMode.TileY || sourceTileMode == CanvasTileMode.TileXY) ? byte.MaxValue : byte.MinValue;
            byte destinationTileX = (destinationTileMode == CanvasTileMode.TileX || destinationTileMode == CanvasTileMode.TileXY) ? byte.MaxValue : byte.MinValue;
            byte destinationTileY = (destinationTileMode == CanvasTileMode.TileY || destinationTileMode == CanvasTileMode.TileXY) ? byte.MaxValue : byte.MinValue;
            int tileComposite = 0;
            tileComposite |= sourceTileX;
            tileComposite <<= 8;
            tileComposite |= sourceTileY;
            tileComposite <<= 8;
            tileComposite |= destinationTileX;
            tileComposite <<= 8;
            tileComposite |= destinationTileY;

            ClampSourceToTile(ref sourceX, sourceTextureWidth, ref sourceWidth, sourceTileX, ref destinationX);
            ClampSourceToTile(ref sourceY, sourceTextureHeight, ref sourceHeight, sourceTileY, ref destinationY);

            ClampDestinationToTile(ref destinationX, destinationTextureWidth, ref sourceWidth , sourceTileX, ref sourceX);
            ClampDestinationToTile(ref destinationY, destinationTextureHeight, ref sourceHeight, sourceTileY, ref sourceY);

            if (sourceWidth <= 0 || sourceHeight <= 0) return;

            CopyImageShader.SetTexture(CopyShaderKernel, InputPropertyID, from);
            CopyImageShader.SetTexture(CopyShaderKernel, ResultPropertyID, to);
            CopyImageShader.SetInts(SrcDstDimPropertyID, sourceTextureWidth, sourceTextureHeight, destinationTextureWidth, destinationTextureHeight);
            CopyImageShader.SetInts(DstXYPropertyID, destinationX, destinationY);
            CopyImageShader.SetInts(SrcRectPropertyID, sourceX, sourceY, sourceWidth, sourceHeight);

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

        public static void LayerImageArea(RenderTexture bottomLayer, RenderTexture target, int sourceX, int sourceY, int sourceWidth, int sourceHeight, RenderTexture topLayer, ColorBlendMode colorBlendMode, int destinationX = 0, int destinationY = 0, CanvasTileMode destinationTileMode = CanvasTileMode.None, CanvasTileMode sourceTileMode = CanvasTileMode.None)
        {
            int sourceTextureWidth = topLayer.width;
            int sourceTextureHeight = topLayer.height;
            int destinationTextureWidth = target.width;
            int destinationTextureHeight = target.height;

            int dispatchKernel;
            switch (colorBlendMode)
            {
                case ColorBlendMode.Default:
                    dispatchKernel = BlendAreaNormalKernel;
                    break;
                case ColorBlendMode.Add:
                    dispatchKernel = BlendAreaAddKernel;
                    break;
                case ColorBlendMode.Multiply:
                    dispatchKernel = BlendAreaMultiplyKernel;
                    break;
                case ColorBlendMode.Subtract:
                    dispatchKernel = BlendAreaSubtractKernel;
                    break;
                default:
                    dispatchKernel = BlendAreaNormalKernel;
                    break;
            }



            byte sourceTileX = (sourceTileMode == CanvasTileMode.TileX || sourceTileMode == CanvasTileMode.TileXY) ? byte.MaxValue : byte.MinValue;
            byte sourceTileY = (sourceTileMode == CanvasTileMode.TileY || sourceTileMode == CanvasTileMode.TileXY) ? byte.MaxValue : byte.MinValue;
            byte destinationTileX = (destinationTileMode == CanvasTileMode.TileX || destinationTileMode == CanvasTileMode.TileXY) ? byte.MaxValue : byte.MinValue;
            byte destinationTileY = (destinationTileMode == CanvasTileMode.TileY || destinationTileMode == CanvasTileMode.TileXY) ? byte.MaxValue : byte.MinValue;
            int tileComposite = 0;
            tileComposite |= sourceTileX;
            tileComposite <<= 8;
            tileComposite |= sourceTileY;
            tileComposite <<= 8;
            tileComposite |= destinationTileX;
            tileComposite <<= 8;
            tileComposite |= destinationTileY;

            ClampSourceToTile(ref sourceX, sourceTextureWidth, ref sourceWidth, sourceTileX, ref destinationX);
            ClampSourceToTile(ref sourceY, sourceTextureHeight, ref sourceHeight, sourceTileY, ref destinationY);

            ClampDestinationToTile(ref destinationX, destinationTextureWidth, ref sourceWidth, sourceTileX, ref sourceX);
            ClampDestinationToTile(ref destinationY, destinationTextureHeight, ref sourceHeight, sourceTileY, ref sourceY);

            if (sourceWidth <= 0 || sourceHeight <= 0) return;

            LayerImageAreaShader.SetInts(SrcDstDimPropertyID, sourceTextureWidth, sourceTextureHeight, destinationTextureWidth, destinationTextureHeight);
            LayerImageAreaShader.SetInts(DstXYPropertyID, destinationX, destinationY);
            LayerImageAreaShader.SetInts(SrcRectPropertyID, sourceX, sourceY, sourceWidth, sourceHeight);

            LayerImageAreaShader.SetInt(TileSrcXYDstXYPropertyID, tileComposite);
            LayerImageAreaShader.SetTexture(dispatchKernel, BottomLayerPropertyID, bottomLayer);
            LayerImageAreaShader.SetTexture(dispatchKernel, TopLayerPropertyID, topLayer);
            LayerImageAreaShader.SetTexture(dispatchKernel, ResultPropertyID, target);
            LayerImageAreaShader.Dispatch(dispatchKernel, Mathf.CeilToInt(sourceTextureWidth / (float)warpSizeX), Mathf.CeilToInt(sourceTextureHeight / (float)warpSizeY), 1);
        }


        public static void ApplyChannelMask(RenderTexture target, RenderTexture mask)
        {
            MaskChannelsShader.SetTexture(MaskChannelsKernel, TargetPropertyID, target);
            MaskChannelsShader.SetTexture(MaskChannelsKernel, MaskPropertyID, mask);
            MaskChannelsShader.Dispatch(MaskChannelsKernel, Mathf.CeilToInt(target.width / (float)warpSizeX), Mathf.CeilToInt(target.height / (float)warpSizeY), 1);
        }


        public static class Gradients
        {
            public static ComputeShader DrawColorGradientRectangleShader;
            public static ComputeShader DrawColorGradientShader;
            public static ComputeShader DrawHueGradientShader;
            private static int GradientRectangleKernel;
            private static int GradientKernel;
            private static int HueGradientKernel;
            private static int ColorAPropertyID;
            private static int ColorBPropertyID;
            private static int ColorCPropertyID;
            private static int ColorDPropertyID;
            private static int RectangleSizePropertyID;
            private static int SizePropertyID;

            public static void Initialize()
            {
                DrawColorGradientRectangleShader = Resources.Load<ComputeShader>("Gradients/DrawColorGradientRectangle");
                DrawColorGradientShader = Resources.Load<ComputeShader>("Gradients/DrawColorGradient");
                DrawHueGradientShader = Resources.Load<ComputeShader>("Gradients/DrawHueGradient");
                GradientRectangleKernel = DrawColorGradientRectangleShader.FindKernel("CSMain");
                GradientKernel = DrawColorGradientShader.FindKernel("CSMain");
                HueGradientKernel = DrawHueGradientShader.FindKernel("CSMain");
                ColorAPropertyID = Shader.PropertyToID("ColorA");
                ColorBPropertyID = Shader.PropertyToID("ColorB");
                ColorCPropertyID = Shader.PropertyToID("ColorC");
                ColorDPropertyID = Shader.PropertyToID("ColorD");
                RectangleSizePropertyID = Shader.PropertyToID("RectangleSize");
                SizePropertyID = Shader.PropertyToID("Size");
            }

            public static void DrawColorGradientRectangle(RenderTexture target, int rectangleSizeX, int rectangleSizeY, Color colorA, Color colorB, Color colorC, Color colorD)
            {
                if (rectangleSizeX <= 0 || rectangleSizeY <= 0) return;
                DrawColorGradientRectangleShader.SetInts(RectangleSizePropertyID, rectangleSizeX, rectangleSizeY);
                DrawColorGradientRectangleShader.SetVector(ColorAPropertyID, colorA);
                DrawColorGradientRectangleShader.SetVector(ColorBPropertyID, colorB);
                DrawColorGradientRectangleShader.SetVector(ColorCPropertyID, colorC);
                DrawColorGradientRectangleShader.SetVector(ColorDPropertyID, colorD);
                DrawColorGradientRectangleShader.SetTexture(GradientRectangleKernel, ResultPropertyID, target);
                DrawColorGradientRectangleShader.Dispatch(GradientRectangleKernel, Mathf.CeilToInt(Mathf.Min(target.width, rectangleSizeX) / (float)warpSizeX), Mathf.CeilToInt(Mathf.Min(target.height, rectangleSizeY) / (float)warpSizeY), 1);
            }
            public static void DrawColorGradient(RenderTexture target, int size, Color colorA, Color colorB)
            {
                DrawColorGradientShader.SetInt(SizePropertyID, size);
                DrawColorGradientShader.SetVector(ColorAPropertyID, colorA);
                DrawColorGradientShader.SetVector(ColorBPropertyID, colorB);
                DrawColorGradientShader.SetTexture(GradientKernel, ResultPropertyID, target);
                DrawColorGradientShader.Dispatch(GradientKernel, Mathf.CeilToInt(Mathf.Min(target.width, size) / (float)warpSizeX), 1, 1);
            }
            public static void DrawHueGradient(RenderTexture target, int size)
            {
                DrawHueGradientShader.SetInt(SizePropertyID, size);
                DrawHueGradientShader.SetTexture(HueGradientKernel, ResultPropertyID, target);
                DrawHueGradientShader.Dispatch(HueGradientKernel, Mathf.CeilToInt(Mathf.Min(target.width, size) / (float)warpSizeX), 1, 1);
            }
        }

        public static class Brushes
        {
            public static ComputeShader DrawBrushShader;
            private static int RectangleKernel;
            private static int EllipseKernel;
            private static int TextureKernel;

            private static int BrushColorPropertyID;
            private static int PositionSizePropertyID;
            private static int RotationMatrixPropertyID;
            private static int OpacityGradientPropertyID;

            public static void Initialize()
            {
                DrawBrushShader = Resources.Load<ComputeShader>("Brushes/DrawBrush");
                RectangleKernel = DrawBrushShader.FindKernel("RectangleBrush");
                EllipseKernel = DrawBrushShader.FindKernel("EllipseBrush");
                TextureKernel = DrawBrushShader.FindKernel("TextureBrush");

                BrushColorPropertyID = Shader.PropertyToID("BrushColor");
                PositionSizePropertyID = Shader.PropertyToID("PositionSize");
                RotationMatrixPropertyID = Shader.PropertyToID("RotationMatrix");
                OpacityGradientPropertyID = Shader.PropertyToID("OpacityGradient");
            }

            public static void DrawBrush(RenderTexture target, Vector2 center, Vector2 size, float rotation, BrushShape brushShape, Color color, RenderTexture opacityGradient)
            {
                int chosenKernel;
                switch (brushShape)
                {
                    case BrushShape.Rectangle:
                        chosenKernel = RectangleKernel;
                        break;
                    case BrushShape.Ellipse:
                        chosenKernel = EllipseKernel;
                        break;
                    case BrushShape.Texture:
                        chosenKernel = TextureKernel;
                      break;
                    default:
                        chosenKernel = RectangleKernel;
                        break;
                }

                DrawBrushShader.SetVector(PositionSizePropertyID, new Vector4(center.x, center.y, 1.0f / size.x, 1.0f / size.y * 2.0f));
                DrawBrushShader.SetVector(BrushColorPropertyID, color);

                float cosRotation = Mathf.Cos(rotation / 180.0f * Mathf.PI);
                float sinRotation = Mathf.Sin(rotation / 180.0f * Mathf.PI);

                DrawBrushShader.SetMatrix(RotationMatrixPropertyID, new Matrix4x4(new Vector4(cosRotation, sinRotation,0,0), new Vector4(-sinRotation, cosRotation,0,0), Vector4.zero, Vector4.zero));
                DrawBrushShader.SetTexture(chosenKernel, OpacityGradientPropertyID, opacityGradient);
                DrawBrushShader.SetTexture(chosenKernel, TargetPropertyID, target);
                DrawBrushShader.Dispatch(chosenKernel, Mathf.CeilToInt(target.width / (float)warpSizeX), Mathf.CeilToInt(target.height / (float)warpSizeY), 1);
            }

            

        }




    }
}