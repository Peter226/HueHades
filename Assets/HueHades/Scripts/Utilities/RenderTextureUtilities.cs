using System.Collections.Generic;
using UnityEngine;
using HueHades.Common;
using UnityEngine.Windows;
using System;

namespace HueHades.Utilities
{
    public static class RenderTextureUtilities
    {
        private static Dictionary<(int, RenderTextureFormat), List<ReusableTexture>> _renderTexturePool;
        private static Dictionary<(int, RenderTextureFormat), List<ReusableTexture>> _gradientRenderTexturePool;

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
        private static int InheritAlphaKernel;

        private static int TopLayerPropertyID;
        private static int BottomLayerPropertyID;

        private static int OpacityPropertyID;

        private static int BlendAreaNormalKernel;
        private static int BlendAreaAddKernel;
        private static int BlendAreaMultiplyKernel;
        private static int BlendAreaSubtractKernel;
        private static int EraseAreaKernel;

        private static int TargetPropertyID;
        private static int MaskPropertyID;

        private static int PositionSizePropertyID;
        private static int RotationMatrixPropertyID;


        public static void SimpleDispatch(this ComputeShader computeShader, int kernel, int targetWidth, int targetHeight)
        {
            computeShader.Dispatch(kernel, Mathf.CeilToInt(targetWidth / (float)warpSizeX), Mathf.CeilToInt(targetHeight / (float)warpSizeY), 1);
        }


        public static void Dispose()
        {
            Selection.Dispose();
        }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            CopyImageShader = Resources.Load<ComputeShader>("CopyImage");
            CopyShaderKernel = CopyImageShader.FindKernel("CSMain");
            InputPropertyID = Shader.PropertyToID("Input");
            ResultPropertyID = Shader.PropertyToID("Result");
            SrcDstDimPropertyID = Shader.PropertyToID("SrcDstDim");
            DstXYPropertyID = Shader.PropertyToID("DstXY");
            SrcRectPropertyID = Shader.PropertyToID("SrcRect");
            TileSrcXYDstXYPropertyID = Shader.PropertyToID("TileSrcXYDstXY");

            ClearImageShader = Resources.Load<ComputeShader>("ClearImage");
            ClearColorKernel = ClearImageShader.FindKernel("CSMain");
            ClearColorPropertyID = Shader.PropertyToID("ClearColor");

            LayerImageShader = Resources.Load<ComputeShader>("LayerImage");
            BlendNormalKernel = LayerImageShader.FindKernel("NormalBlend");
            BlendAddKernel = LayerImageShader.FindKernel("AddBlend");
            BlendMultiplyKernel = LayerImageShader.FindKernel("MultiplyBlend");
            BlendSubtractKernel = LayerImageShader.FindKernel("SubtractBlend");
            InheritAlphaKernel = LayerImageShader.FindKernel("InheritAlpha");
            TopLayerPropertyID = Shader.PropertyToID("TopLayer");
            BottomLayerPropertyID = Shader.PropertyToID("BottomLayer");

            OpacityPropertyID = Shader.PropertyToID("Opacity");

            LayerImageAreaShader = Resources.Load<ComputeShader>("LayerImageArea");

            BlendAreaNormalKernel = LayerImageAreaShader.FindKernel("NormalBlend");
            BlendAreaAddKernel = LayerImageAreaShader.FindKernel("AddBlend");
            BlendAreaMultiplyKernel = LayerImageAreaShader.FindKernel("MultiplyBlend");
            BlendAreaSubtractKernel = LayerImageAreaShader.FindKernel("SubtractBlend");
            EraseAreaKernel = LayerImageAreaShader.FindKernel("EraseKernel");

            TargetPropertyID = Shader.PropertyToID("Target");
            MaskPropertyID = Shader.PropertyToID("Mask");


            PositionSizePropertyID = Shader.PropertyToID("PositionSize");
            RotationMatrixPropertyID = Shader.PropertyToID("RotationMatrix");

            Gradients.Initialize();
            Brushes.Initialize();
            Effects.Initialize();
            Sampling.Initialize();
            Selection.Initialize();
            Statistics.Initialize();
        }

        public static void InitializePool()
        {
            _renderTexturePool = new Dictionary<(int, RenderTextureFormat), List<ReusableTexture>>();
            _gradientRenderTexturePool = new Dictionary<(int, RenderTextureFormat), List<ReusableTexture>>();
        }

        public static ReusableTexture GetTemporary(int sizeX, int sizeY, RenderTextureFormat format)
        {
            int availableSize;
            int maxSize = Mathf.Max(sizeX, sizeY);

            //calculate which smallest power of two fits our requested texture dimensions inside
            availableSize = Mathf.CeilToInt(Mathf.Pow(2,Mathf.Ceil(Mathf.Log(maxSize,2))));

            var key = (availableSize, format);
            if (_renderTexturePool == null)
            {
                _renderTexturePool = new Dictionary<(int, RenderTextureFormat), List<ReusableTexture>>();
            }
            List<ReusableTexture> pooledTextures;
            if (!_renderTexturePool.TryGetValue(key, out pooledTextures))
            {
                pooledTextures = new List<ReusableTexture>();
                _renderTexturePool.Add(key, pooledTextures);
            }

            ReusableTexture temp;
            if (pooledTextures.Count > 0)
            {
                var index = pooledTextures.Count - 1;
                temp = pooledTextures[index];
                pooledTextures.RemoveAt(index);
                temp.ReuseAs(sizeX, sizeY);
            }
            else
            {
                temp = new ReusableTexture(new RenderTexture(availableSize, availableSize, 0, format, 0), sizeX, sizeY);
                temp.texture.enableRandomWrite = true;
                temp.texture.Create();
            }
            return temp;
        }


        public static ReusableTexture GetTemporaryGradient(int sizeX, RenderTextureFormat format)
        {
            int availableSize;
            int maxSize = sizeX;

            //calculate which smallest power of two fits our requested texture dimensions inside
            availableSize = Mathf.CeilToInt(Mathf.Pow(2,Mathf.Ceil(Mathf.Log(maxSize, 2))));

            var key = (availableSize, format);
            if (_gradientRenderTexturePool == null)
            {
                _gradientRenderTexturePool = new Dictionary<(int, RenderTextureFormat), List<ReusableTexture>>();
            }
            List<ReusableTexture> pooledTextures;
            if (!_gradientRenderTexturePool.TryGetValue(key, out pooledTextures))
            {
                pooledTextures = new List<ReusableTexture>();
                _gradientRenderTexturePool.Add(key, pooledTextures);
            }

            ReusableTexture temp;
            if (pooledTextures.Count > 0)
            {
                var index = pooledTextures.Count - 1;
                temp = pooledTextures[index];
                pooledTextures.RemoveAt(index);
                temp.ReuseAs(sizeX, 1);
            }
            else
            {
                temp = new ReusableTexture(new RenderTexture(availableSize, 1, 0, format, 0), sizeX, 1);
                temp.texture.enableRandomWrite = true;
                temp.texture.Create();
            }
            return temp;
        }




        public static void ReleaseTemporary(ReusableTexture renderTexture)
        {
            _renderTexturePool[(renderTexture.actualWidth, renderTexture.texture.format)].Add(renderTexture);
        }

        public static void ReleaseTemporaryGradient(ReusableTexture gradientRenderTexture)
        {
            _gradientRenderTexturePool[(gradientRenderTexture.actualWidth, gradientRenderTexture.texture.format)].Add(gradientRenderTexture);
        }


        public static void CopyTexture(IReadableTexture from, ReusableTexture to, CanvasTileMode destinationTileMode = CanvasTileMode.None, CanvasTileMode sourceTileMode = CanvasTileMode.None)
        {
            CopyTexture(from, to, 0, 0, destinationTileMode, sourceTileMode);
        }
        public static void CopyTexture(IReadableTexture from, ReusableTexture to, int destinationX, int destinationY, CanvasTileMode destinationTileMode = CanvasTileMode.None, CanvasTileMode sourceTileMode = CanvasTileMode.None)
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

        public static void CopyTexture(IReadableTexture from, int sourceX, int sourceY, int sourceWidth, int sourceHeight, ReusableTexture to, int destinationX = 0, int destinationY = 0, CanvasTileMode destinationTileMode = CanvasTileMode.None, CanvasTileMode sourceTileMode = CanvasTileMode.None)
        {
            int sourceTextureWidth = from.width;
            int sourceTextureHeight = from.height;
            int destinationTextureWidth = to.width;
            int destinationTextureHeight = to.height;

            int sourceTileX = (sourceTileMode == CanvasTileMode.TileX || sourceTileMode == CanvasTileMode.TileXY) ? 1 : 0;
            int sourceTileY = (sourceTileMode == CanvasTileMode.TileY || sourceTileMode == CanvasTileMode.TileXY) ? 1 : 0;
            int destinationTileX = (destinationTileMode == CanvasTileMode.TileX || destinationTileMode == CanvasTileMode.TileXY) ? 1 : 0;
            int destinationTileY = (destinationTileMode == CanvasTileMode.TileY || destinationTileMode == CanvasTileMode.TileXY) ? 1 : 0;

            ClampSourceToTile(ref sourceX, sourceTextureWidth, ref sourceWidth, sourceTileX, ref destinationX);
            ClampSourceToTile(ref sourceY, sourceTextureHeight, ref sourceHeight, sourceTileY, ref destinationY);

            ClampDestinationToTile(ref destinationX, destinationTextureWidth, ref sourceWidth , destinationTileX, ref sourceX);
            ClampDestinationToTile(ref destinationY, destinationTextureHeight, ref sourceHeight, destinationTileY, ref sourceY);

            if (sourceWidth <= 0 || sourceHeight <= 0) return;

            CopyImageShader.SetTexture(CopyShaderKernel, InputPropertyID, from.texture);
            CopyImageShader.SetTexture(CopyShaderKernel, ResultPropertyID, to.texture);
            CopyImageShader.SetInts(SrcDstDimPropertyID, sourceTextureWidth, sourceTextureHeight, destinationTextureWidth, destinationTextureHeight);
            CopyImageShader.SetInts(DstXYPropertyID, destinationX, destinationY);
            CopyImageShader.SetInts(SrcRectPropertyID, sourceX, sourceY, sourceWidth, sourceHeight);

            CopyImageShader.SetInts(TileSrcXYDstXYPropertyID, sourceTileX, sourceTileY, destinationTileX, destinationTileY);
            CopyImageShader.SimpleDispatch(CopyShaderKernel, sourceWidth, sourceHeight);

        }

        public static void ClearTexture(ReusableTexture texture, Color clearColor)
        {
            ClearImageShader.SetTexture(ClearColorKernel, ResultPropertyID, texture.texture);
            ClearImageShader.SetVector(ClearColorPropertyID, clearColor);
            ClearImageShader.SimpleDispatch(ClearColorKernel, texture.width, texture.height);
        }

        public static void InheritAlpha(ReusableTexture bottomAlphaLayer, ReusableTexture topLayer, ReusableTexture result)
        {
            LayerImageShader.SetTexture(InheritAlphaKernel, BottomLayerPropertyID, bottomAlphaLayer.texture);
            LayerImageShader.SetTexture(InheritAlphaKernel, TopLayerPropertyID, topLayer.texture);
            LayerImageShader.SetTexture(InheritAlphaKernel, ResultPropertyID, result.texture);
            LayerImageShader.SimpleDispatch(InheritAlphaKernel, result.width, result.height);
        }


        public static void LayerImage(ReusableTexture bottomLayer, ReusableTexture topLayer, ReusableTexture result, ColorBlendMode colorBlendMode, float opacity = 1)
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

            LayerImageShader.SetFloat(OpacityPropertyID,opacity);
            LayerImageShader.SetTexture(dispatchKernel,BottomLayerPropertyID,bottomLayer.texture);
            LayerImageShader.SetTexture(dispatchKernel,TopLayerPropertyID,topLayer.texture);
            LayerImageShader.SetTexture(dispatchKernel,ResultPropertyID,result.texture);
            LayerImageShader.SimpleDispatch(dispatchKernel, result.width, result.height);
        }

        public static void LayerImageArea(ReusableTexture bottomLayer, ReusableTexture target, int sourceX, int sourceY, int sourceWidth, int sourceHeight, ReusableTexture topLayer, ColorBlendMode colorBlendMode, int destinationX = 0, int destinationY = 0, CanvasTileMode destinationTileMode = CanvasTileMode.None, CanvasTileMode sourceTileMode = CanvasTileMode.None, float opacity = 1)
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
                case ColorBlendMode.Erase:
                    dispatchKernel = EraseAreaKernel;
                    break;
                default:
                    dispatchKernel = BlendAreaNormalKernel;
                    break;
            }

            int sourceTileX = (sourceTileMode == CanvasTileMode.TileX || sourceTileMode == CanvasTileMode.TileXY) ? 1 : 0;
            int sourceTileY = (sourceTileMode == CanvasTileMode.TileY || sourceTileMode == CanvasTileMode.TileXY) ? 1 : 0;
            int destinationTileX = (destinationTileMode == CanvasTileMode.TileX || destinationTileMode == CanvasTileMode.TileXY) ? 1 : 0;
            int destinationTileY = (destinationTileMode == CanvasTileMode.TileY || destinationTileMode == CanvasTileMode.TileXY) ? 1 : 0;
            
            ClampSourceToTile(ref sourceX, sourceTextureWidth, ref sourceWidth, sourceTileX, ref destinationX);
            ClampSourceToTile(ref sourceY, sourceTextureHeight, ref sourceHeight, sourceTileY, ref destinationY);

            ClampDestinationToTile(ref destinationX, destinationTextureWidth, ref sourceWidth, destinationTileX, ref sourceX);
            ClampDestinationToTile(ref destinationY, destinationTextureHeight, ref sourceHeight, destinationTileY, ref sourceY);

            if (sourceWidth <= 0 || sourceHeight <= 0) return;

            LayerImageAreaShader.SetInts(SrcDstDimPropertyID, sourceTextureWidth, sourceTextureHeight, destinationTextureWidth, destinationTextureHeight);
            LayerImageAreaShader.SetInts(DstXYPropertyID, destinationX, destinationY);
            LayerImageAreaShader.SetInts(SrcRectPropertyID, sourceX, sourceY, sourceWidth, sourceHeight);

            LayerImageAreaShader.SetFloat(OpacityPropertyID, opacity);

            LayerImageAreaShader.SetInts(TileSrcXYDstXYPropertyID, sourceTileX, sourceTileY, destinationTileX, destinationTileY);
            LayerImageAreaShader.SetTexture(dispatchKernel, BottomLayerPropertyID, bottomLayer.texture);
            LayerImageAreaShader.SetTexture(dispatchKernel, TopLayerPropertyID, topLayer.texture);
            LayerImageAreaShader.SetTexture(dispatchKernel, ResultPropertyID, target.texture);
            LayerImageAreaShader.SimpleDispatch(dispatchKernel, sourceTextureWidth, sourceTextureHeight);
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

            public static void DrawColorGradientRectangle(ReusableTexture target, int rectangleSizeX, int rectangleSizeY, Color colorA, Color colorB, Color colorC, Color colorD)
            {
                if (rectangleSizeX <= 0 || rectangleSizeY <= 0) return;
                DrawColorGradientRectangleShader.SetInts(RectangleSizePropertyID, rectangleSizeX, rectangleSizeY);
                DrawColorGradientRectangleShader.SetVector(ColorAPropertyID, colorA);
                DrawColorGradientRectangleShader.SetVector(ColorBPropertyID, colorB);
                DrawColorGradientRectangleShader.SetVector(ColorCPropertyID, colorC);
                DrawColorGradientRectangleShader.SetVector(ColorDPropertyID, colorD);
                DrawColorGradientRectangleShader.SetTexture(GradientRectangleKernel, ResultPropertyID, target.texture);
                DrawColorGradientRectangleShader.SimpleDispatch(GradientRectangleKernel, Mathf.Min(target.width, rectangleSizeX), Mathf.Min(target.height, rectangleSizeY));
            }
            public static void DrawColorGradient(ReusableTexture target, int size, Color colorA, Color colorB)
            {
                DrawColorGradientShader.SetInt(SizePropertyID, size);
                DrawColorGradientShader.SetVector(ColorAPropertyID, colorA);
                DrawColorGradientShader.SetVector(ColorBPropertyID, colorB);
                DrawColorGradientShader.SetTexture(GradientKernel, ResultPropertyID, target.texture);
                DrawColorGradientShader.Dispatch(GradientKernel, Mathf.CeilToInt(Mathf.Min(target.width, size) / (float)warpSizeX), 1, 1);
            }
            public static void DrawHueGradient(ReusableTexture target, int size)
            {
                DrawHueGradientShader.SetInt(SizePropertyID, size);
                DrawHueGradientShader.SetTexture(HueGradientKernel, ResultPropertyID, target.texture);
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
            private static int OpacityGradientPropertyID;
            private static int SoftnessPropertyID;

            public static void Initialize()
            {
                DrawBrushShader = Resources.Load<ComputeShader>("Brushes/DrawBrush");
                RectangleKernel = DrawBrushShader.FindKernel("RectangleBrush");
                EllipseKernel = DrawBrushShader.FindKernel("EllipseBrush");
                TextureKernel = DrawBrushShader.FindKernel("TextureBrush");

                BrushColorPropertyID = Shader.PropertyToID("BrushColor");

                OpacityGradientPropertyID = Shader.PropertyToID("OpacityGradient");
                SoftnessPropertyID = Shader.PropertyToID("Softness");
            }

            public static void DrawBrush(ReusableTexture target, Vector2 center, Vector2 size, float rotation, BrushShape brushShape, Color color, ReusableTexture opacityGradient, float softness)
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

                DrawBrushShader.SetFloat(SoftnessPropertyID, 1.0f / softness);
                DrawBrushShader.SetVector(PositionSizePropertyID, new Vector4(center.x, center.y, 1.0f / size.x, 1.0f / size.y * 1.0f));
                DrawBrushShader.SetVector(BrushColorPropertyID, color);

                float cosRotation = Mathf.Cos(rotation / 180.0f * Mathf.PI);
                float sinRotation = Mathf.Sin(rotation / 180.0f * Mathf.PI);

                DrawBrushShader.SetMatrix(RotationMatrixPropertyID, new Matrix4x4(new Vector4(cosRotation, sinRotation,0,0), new Vector4(-sinRotation, cosRotation,0,0), Vector4.zero, Vector4.zero));
                DrawBrushShader.SetTexture(chosenKernel, OpacityGradientPropertyID, opacityGradient.texture);
                DrawBrushShader.SetTexture(chosenKernel, TargetPropertyID, target.texture);
                DrawBrushShader.SimpleDispatch(chosenKernel, target.width, target.height);
            }
        }

        public static class Effects
        {
            private static ComputeShader ColorAdjustmentsShader;
            private static int ColorAdjustmentsKernel;
            private static int AdjustmentParamsPropertyID;

            private static ComputeShader SwapChannelsShader;
            private static int SwapChannelsKernel;
            private static int RedChannelMaskPropertyID;
            private static int GreenChannelMaskPropertyID;
            private static int BlueChannelMaskPropertyID;
            private static int AlphaChannelMaskPropertyID;

            private static ComputeShader VoronoiShader;
            private static int VoronoiKernel;
            private static int VoronoiParametersPropertyID;
            private static int NoiseTilePropertyID;

            private static ComputeShader SimplexShader;
            private static int SimplexKernel;
            private static int SimplexParametersPropertyID;

            public static void Initialize()
            {
                ColorAdjustmentsShader = Resources.Load<ComputeShader>("Effects/ColorAdjustments");
                ColorAdjustmentsKernel = ColorAdjustmentsShader.FindKernel("CSMain");
                AdjustmentParamsPropertyID = Shader.PropertyToID("AdjustmentParams");

                SwapChannelsShader = Resources.Load<ComputeShader>("Effects/SwapChannels");
                SwapChannelsKernel = SwapChannelsShader.FindKernel("CSMain");
                RedChannelMaskPropertyID = Shader.PropertyToID("RedChannelMask");
                GreenChannelMaskPropertyID = Shader.PropertyToID("GreenChannelMask");
                BlueChannelMaskPropertyID = Shader.PropertyToID("BlueChannelMask");
                AlphaChannelMaskPropertyID = Shader.PropertyToID("AlphaChannelMask");

                VoronoiShader = Resources.Load<ComputeShader>("Effects/Noise/Voronoi");
                VoronoiKernel = VoronoiShader.FindKernel("CSMain");
                VoronoiParametersPropertyID = Shader.PropertyToID("VoronoiParameters");
                NoiseTilePropertyID = Shader.PropertyToID("NoiseTile");

                SimplexShader = Resources.Load<ComputeShader>("Effects/Noise/Simplex");
                SimplexKernel = VoronoiShader.FindKernel("CSMain");
                SimplexParametersPropertyID = Shader.PropertyToID("SimplexParameters");
            }


            public static void Voronoi(ReusableTexture result, int seed, int cellsX, int cellsY, CanvasTileMode tileMode)
            {
                VoronoiShader.SetInts(VoronoiParametersPropertyID, seed, cellsX, cellsY);
                var tileX = (tileMode == CanvasTileMode.TileX || tileMode == CanvasTileMode.TileXY) ? 1 : 0;
                var tileY = (tileMode == CanvasTileMode.TileY || tileMode == CanvasTileMode.TileXY) ? 1 : 0;
                VoronoiShader.SetInts(NoiseTilePropertyID, tileX, tileY);
                VoronoiShader.SetInts(SrcRectPropertyID, 0, 0, result.width, result.height);
                VoronoiShader.SetTexture(VoronoiKernel, ResultPropertyID, result.texture);
                VoronoiShader.SimpleDispatch(VoronoiKernel, result.width, result.height);
            }

            public static void Simplex(ReusableTexture result, int seed, int cellsX, int cellsY, CanvasTileMode tileMode)
            {
                SimplexShader.SetInts(SimplexParametersPropertyID, seed, cellsX, cellsY);
                var tileX = (tileMode == CanvasTileMode.TileX || tileMode == CanvasTileMode.TileXY) ? 1 : 0;
                var tileY = (tileMode == CanvasTileMode.TileY || tileMode == CanvasTileMode.TileXY) ? 1 : 0;
                SimplexShader.SetInts(NoiseTilePropertyID, tileX, tileY);
                SimplexShader.SetInts(SrcRectPropertyID, 0, 0, result.width, result.height);
                SimplexShader.SetTexture(SimplexKernel, ResultPropertyID, result.texture);
                SimplexShader.SimpleDispatch(SimplexKernel, result.width, result.height);
            }

            public static void ColorAdjustments(ReusableTexture input, ReusableTexture result, float hue, float saturation, float brightness, float contrast)
            {
                ColorAdjustmentsShader.SetVector(AdjustmentParamsPropertyID, new Vector4(hue, saturation, brightness, contrast));
                ColorAdjustmentsShader.SetTexture(ColorAdjustmentsKernel, InputPropertyID, input.texture);
                ColorAdjustmentsShader.SetTexture(ColorAdjustmentsKernel, ResultPropertyID, result.texture);
                ColorAdjustmentsShader.SimpleDispatch(ColorAdjustmentsKernel, input.width, input.height);
            }

            public static void SwapChannels(ReusableTexture input, ReusableTexture result, ColorChannel red, ColorChannel green, ColorChannel blue, ColorChannel alpha)
            {
                SwapChannelsShader.SetVector(RedChannelMaskPropertyID, new Vector4(red == ColorChannel.Red ? 1 : 0, green == ColorChannel.Red ? 1 : 0, blue == ColorChannel.Red ? 1 : 0, alpha == ColorChannel.Red ? 1 : 0));
                SwapChannelsShader.SetVector(GreenChannelMaskPropertyID, new Vector4(red == ColorChannel.Green ? 1 : 0, green == ColorChannel.Green ? 1 : 0, blue == ColorChannel.Green ? 1 : 0, alpha == ColorChannel.Green ? 1 : 0));
                SwapChannelsShader.SetVector(BlueChannelMaskPropertyID, new Vector4(red == ColorChannel.Blue ? 1 : 0, green == ColorChannel.Blue ? 1 : 0, blue == ColorChannel.Blue ? 1 : 0, alpha == ColorChannel.Blue ? 1 : 0));
                SwapChannelsShader.SetVector(AlphaChannelMaskPropertyID, new Vector4(red == ColorChannel.Alpha ? 1 : 0, green == ColorChannel.Alpha ? 1 : 0, blue == ColorChannel.Alpha ? 1 : 0, alpha == ColorChannel.Alpha ? 1 : 0));
                SwapChannelsShader.SetTexture(SwapChannelsKernel, InputPropertyID, input.texture);
                SwapChannelsShader.SetTexture(SwapChannelsKernel, ResultPropertyID, result.texture);
                SwapChannelsShader.SimpleDispatch(SwapChannelsKernel,input.width, input.height);
            }
        }

        public static class Sampling
        {
            private static ComputeShader ResampleShader;
            private static ComputeShader ColorspaceSwitchShader;
            private static int PointKernel;
            private static int LinearKernel;
            private static int CubicKernel;
            private static int LánczosKernel;
            private static int TargetPivotPropertyID;

            private static int LinearToGammaKernel;
            private static int LinearToSRGBKernel;

            public static void Initialize()
            {
                ResampleShader = Resources.Load<ComputeShader>("Sampling/ResampleImage");
                ColorspaceSwitchShader = Resources.Load<ComputeShader>("Sampling/ColorspaceSwitch");

                PointKernel = ResampleShader.FindKernel("PointKernel");
                LinearKernel = ResampleShader.FindKernel("LinearKernel");
                CubicKernel = ResampleShader.FindKernel("CubicKernel");
                LánczosKernel = ResampleShader.FindKernel("LanczosKernel");

                TargetPivotPropertyID = Shader.PropertyToID("TargetPivot");

                LinearToGammaKernel = ColorspaceSwitchShader.FindKernel("LinearToGamma");
                LinearToSRGBKernel = ColorspaceSwitchShader.FindKernel("LinearToSRGB");
            }

            public static void Resample(ReusableTexture source, ReusableTexture target, Vector2 size, float rotation, Vector2 pivot, Vector2 targetPivot, SamplerMode samplerMode)
            {
                var inverseSize = new Vector2(1.0f / size.x, 1.0f / size.y);

                ResampleShader.SetVector(TargetPivotPropertyID, targetPivot);
                ResampleShader.SetVector(PositionSizePropertyID, new Vector4(pivot.x, pivot.y, inverseSize.x, inverseSize.y));

                float cosRotation = Mathf.Cos(rotation / 180.0f * Mathf.PI);
                float sinRotation = Mathf.Sin(rotation / 180.0f * Mathf.PI);

                ResampleShader.SetMatrix(RotationMatrixPropertyID, new Matrix4x4(new Vector4(cosRotation, sinRotation, 0, 0), new Vector4(-sinRotation, cosRotation, 0, 0), Vector4.zero, Vector4.zero));
                ResampleShader.SetInts(SrcDstDimPropertyID, source.width, source.height, target.width, target.height);


                switch (samplerMode)
                {
                    case SamplerMode.Point:
                        ResampleShader.SetTexture(PointKernel, InputPropertyID, source.texture);
                        ResampleShader.SetTexture(PointKernel, TargetPropertyID, target.texture);
                        ResampleShader.SimpleDispatch(PointKernel, target.width, target.height);
                        break;
                    case SamplerMode.Linear:
                        ResampleShader.SetTexture(LinearKernel, InputPropertyID, source.texture);
                        ResampleShader.SetTexture(LinearKernel, TargetPropertyID, target.texture);
                        ResampleShader.SimpleDispatch(LinearKernel, target.width, target.height);
                        break;
                    case SamplerMode.Cubic:

                        break;
                    case SamplerMode.Lánczos:

                        break;

                }

            }


            public static void Mirror(ReusableTexture input, ReusableTexture result, MirrorMode mirrorMode)
            {
                Vector2 size = new Vector2(1,-1);
                if (mirrorMode == MirrorMode.Horizontal)
                {
                    size = new Vector2(-1,1);
                }

                Vector2 pivot = new Vector2((input.width - 1) * 0.5f, (input.height - 1) * 0.5f);

                Resample(input, result, size, 0, pivot, pivot, SamplerMode.Point);
            }


            public static void Rotate(ReusableTexture input, ReusableTexture result, RotateMode rotateMode)
            {
                Vector2 pivot = new Vector2((input.width - 1) * 0.5f, (input.height - 1) * 0.5f);
                Vector2 pivotTarget = new Vector2((result.width - 1) * 0.5f, (result.height - 1) * 0.5f);

                float angle = 0;
                switch (rotateMode)
                {
                    case RotateMode.Clockwise:
                        angle = 90;
                        break;
                    case RotateMode.CounterClockwise:
                        angle = -90;
                        break;
                    case RotateMode.OneEighty:
                        angle = 180;
                        break;
                }

                Resample(input, result, Vector2.one, angle, pivot, pivotTarget, SamplerMode.Point);
            }


            public static void LinearToGamma(ReusableTexture input, ReusableTexture result)
            {
                ColorspaceSwitchShader.SetTexture(LinearToGammaKernel, InputPropertyID, input.texture);
                ColorspaceSwitchShader.SetTexture(LinearToGammaKernel, ResultPropertyID, result.texture);
                ColorspaceSwitchShader.SimpleDispatch(LinearToGammaKernel, result.width, result.height);
            }
            public static void LinearToSRGB(ReusableTexture input, ReusableTexture result)
            {
                ColorspaceSwitchShader.SetTexture(LinearToSRGBKernel, InputPropertyID, input.texture);
                ColorspaceSwitchShader.SetTexture(LinearToSRGBKernel, ResultPropertyID, result.texture);
                ColorspaceSwitchShader.SimpleDispatch(LinearToSRGBKernel, result.width, result.height);
            }


        }

        public static class Selection
        {
            private static ComputeShader DrawSelectionShader;
            private static ComputeShader SelectionStatsShader;
            private static int SelectionStatsKernel;
            private static ComputeShader LayerSelectionAreaShader;
            private static int AddSelectionKernel;
            private static int SubtractSelectionKernel;


            private static ComputeShader ApplyMaskAreaShader;
            private static ComputeShader UpdateMaskAreaShader;
            private static int SelectionRectangleKernel;
            private static int SelectionEllipseKernel;
            private static int ApplyMaskAreaKernel;
            private static int UpdateMaskAreaKernel;
            private static int SelectionOffsetPropertyID;
            private static int UpdatedPropertyID;

            private static int SelectionStatsPropertyID;
            private static ComputeBuffer SelectionStatsBuffer;
            private static int StatsBufferLength = 5;

            internal static void Dispose()
            {
                SelectionStatsBuffer.Release();
            }

            public static void Initialize()
            {
                DrawSelectionShader = Resources.Load<ComputeShader>("Selection/DrawSelection");
                SelectionStatsShader = Resources.Load<ComputeShader>("Selection/SelectionStats");
                SelectionStatsKernel = SelectionStatsShader.FindKernel("CSMain");
                LayerSelectionAreaShader = Resources.Load<ComputeShader>("Selection/LayerSelectionArea");
                AddSelectionKernel = LayerSelectionAreaShader.FindKernel("AddSelectionKernel");
                SubtractSelectionKernel = LayerSelectionAreaShader.FindKernel("SubtractSelectionKernel");

                ApplyMaskAreaShader = Resources.Load<ComputeShader>("Selection/ApplyMaskArea");
                UpdateMaskAreaShader = Resources.Load<ComputeShader>("Selection/UpdateMaskArea");
                SelectionRectangleKernel = DrawSelectionShader.FindKernel("SelectionRectangle");
                SelectionEllipseKernel = DrawSelectionShader.FindKernel("SelectionEllipse");
                ApplyMaskAreaKernel = ApplyMaskAreaShader.FindKernel("CSMain");
                UpdateMaskAreaKernel = UpdateMaskAreaShader.FindKernel("CSMain");
                SelectionOffsetPropertyID = Shader.PropertyToID("SelectionOffset");

                SelectionStatsPropertyID = Shader.PropertyToID("SelectionStats");

                SelectionStatsBuffer = new ComputeBuffer(StatsBufferLength, sizeof(int));

                UpdatedPropertyID = Shader.PropertyToID("Updated");
            }

            public static void DrawRectangle(ReusableTexture result, Vector2 center, Vector2 size)
            {
                DrawSelection(result, center, size, SelectionRectangleKernel);
            }

            public static void DrawEllipse(ReusableTexture result, Vector2 center, Vector2 size)
            {
                DrawSelection(result, center, size, SelectionEllipseKernel);
            }

            private static void DrawSelection(ReusableTexture result, Vector2 center, Vector2 size, int kernel)
            {
                DrawSelectionShader.SetVector(PositionSizePropertyID, new Vector4(center.x, center.y, size.x, size.y));
                DrawSelectionShader.SetTexture(kernel, ResultPropertyID, result.texture);
                DrawSelectionShader.SimpleDispatch(kernel, result.width, result.height);
            }


            public static void GetSelectionStats(ReusableTexture selection, out int area, out int minX, out int minY, out int maxX, out int maxY)
            {
                var dataArray = new int[StatsBufferLength];
                dataArray[0] = 0;
                dataArray[1] = int.MaxValue;
                dataArray[2] = int.MaxValue;
                dataArray[3] = int.MinValue;
                dataArray[4] = int.MinValue;
                SelectionStatsBuffer.SetData(dataArray, 0, 0, StatsBufferLength);
                SelectionStatsShader.SetTexture(SelectionStatsKernel, InputPropertyID, selection.texture);
                SelectionStatsShader.SetBuffer(SelectionStatsKernel, SelectionStatsPropertyID, SelectionStatsBuffer);
                SelectionStatsShader.SetInts(SrcRectPropertyID, 0, 0, selection.width,selection.height);
                SelectionStatsShader.SimpleDispatch(SelectionStatsKernel, selection.width, selection.height);

                SelectionStatsBuffer.GetData(dataArray, 0, 0, StatsBufferLength);
                area = dataArray[0];
                minX = dataArray[1];
                minY = dataArray[2];
                maxX = dataArray[3];
                maxY = dataArray[4];
            }


            public static void ApplyMaskArea(ReusableTexture input, ReusableTexture result, int sourceX, int sourceY, int sourceWidth, int sourceHeight, ReusableTexture mask, int destinationX = 0, int destinationY = 0, CanvasTileMode destinationTileMode = CanvasTileMode.None, CanvasTileMode sourceTileMode = CanvasTileMode.None)
            {
                int sourceTextureWidth = mask.width;
                int sourceTextureHeight = mask.height;
                int destinationTextureWidth = result.width;
                int destinationTextureHeight = result.height;

                int dispatchKernel = ApplyMaskAreaKernel;

                int sourceTileX = (sourceTileMode == CanvasTileMode.TileX || sourceTileMode == CanvasTileMode.TileXY) ? 1 : 0;
                int sourceTileY = (sourceTileMode == CanvasTileMode.TileY || sourceTileMode == CanvasTileMode.TileXY) ? 1 : 0;
                int destinationTileX = (destinationTileMode == CanvasTileMode.TileX || destinationTileMode == CanvasTileMode.TileXY) ? 1 : 0;
                int destinationTileY = (destinationTileMode == CanvasTileMode.TileY || destinationTileMode == CanvasTileMode.TileXY) ? 1 : 0;

                ClampSourceToTile(ref sourceX, sourceTextureWidth, ref sourceWidth, sourceTileX, ref destinationX);
                ClampSourceToTile(ref sourceY, sourceTextureHeight, ref sourceHeight, sourceTileY, ref destinationY);

                ClampDestinationToTile(ref destinationX, destinationTextureWidth, ref sourceWidth, destinationTileX, ref sourceX);
                ClampDestinationToTile(ref destinationY, destinationTextureHeight, ref sourceHeight, destinationTileY, ref sourceY);

                if (sourceWidth <= 0 || sourceHeight <= 0) return;

                ApplyMaskAreaShader.SetInts(SrcDstDimPropertyID, sourceTextureWidth, sourceTextureHeight, destinationTextureWidth, destinationTextureHeight);
                ApplyMaskAreaShader.SetInts(DstXYPropertyID, destinationX, destinationY);
                ApplyMaskAreaShader.SetInts(SrcRectPropertyID, sourceX, sourceY, sourceWidth, sourceHeight);

                ApplyMaskAreaShader.SetInts(TileSrcXYDstXYPropertyID, sourceTileX, sourceTileY, destinationTileX, destinationTileY);
                ApplyMaskAreaShader.SetTexture(dispatchKernel, InputPropertyID, input.texture);
                ApplyMaskAreaShader.SetTexture(dispatchKernel, MaskPropertyID, mask.texture);
                ApplyMaskAreaShader.SetTexture(dispatchKernel, ResultPropertyID, result.texture);
                ApplyMaskAreaShader.SimpleDispatch(dispatchKernel, sourceTextureWidth, sourceTextureHeight);
            }

            public static void UpdateMaskArea(ReusableTexture input, ReusableTexture updated, ReusableTexture result, int sourceX, int sourceY, int sourceWidth, int sourceHeight, ReusableTexture mask, int destinationX = 0, int destinationY = 0, CanvasTileMode destinationTileMode = CanvasTileMode.None, CanvasTileMode sourceTileMode = CanvasTileMode.None)
            {
                int sourceTextureWidth = mask.width;
                int sourceTextureHeight = mask.height;
                int destinationTextureWidth = result.width;
                int destinationTextureHeight = result.height;

                int dispatchKernel = UpdateMaskAreaKernel;

                int sourceTileX = (sourceTileMode == CanvasTileMode.TileX || sourceTileMode == CanvasTileMode.TileXY) ? 1 : 0;
                int sourceTileY = (sourceTileMode == CanvasTileMode.TileY || sourceTileMode == CanvasTileMode.TileXY) ? 1 : 0;
                int destinationTileX = (destinationTileMode == CanvasTileMode.TileX || destinationTileMode == CanvasTileMode.TileXY) ? 1 : 0;
                int destinationTileY = (destinationTileMode == CanvasTileMode.TileY || destinationTileMode == CanvasTileMode.TileXY) ? 1 : 0;

                ClampSourceToTile(ref sourceX, sourceTextureWidth, ref sourceWidth, sourceTileX, ref destinationX);
                ClampSourceToTile(ref sourceY, sourceTextureHeight, ref sourceHeight, sourceTileY, ref destinationY);

                ClampDestinationToTile(ref destinationX, destinationTextureWidth, ref sourceWidth, destinationTileX, ref sourceX);
                ClampDestinationToTile(ref destinationY, destinationTextureHeight, ref sourceHeight, destinationTileY, ref sourceY);

                if (sourceWidth <= 0 || sourceHeight <= 0) return;

                UpdateMaskAreaShader.SetInts(SrcDstDimPropertyID, sourceTextureWidth, sourceTextureHeight, destinationTextureWidth, destinationTextureHeight);
                UpdateMaskAreaShader.SetInts(DstXYPropertyID, destinationX, destinationY);
                UpdateMaskAreaShader.SetInts(SrcRectPropertyID, sourceX, sourceY, sourceWidth, sourceHeight);

                UpdateMaskAreaShader.SetInts(TileSrcXYDstXYPropertyID, sourceTileX, sourceTileY, destinationTileX, destinationTileY);
                UpdateMaskAreaShader.SetTexture(dispatchKernel, InputPropertyID, input.texture);
                UpdateMaskAreaShader.SetTexture(dispatchKernel, UpdatedPropertyID, updated.texture);
                UpdateMaskAreaShader.SetTexture(dispatchKernel, MaskPropertyID, mask.texture);
                UpdateMaskAreaShader.SetTexture(dispatchKernel, ResultPropertyID, result.texture);
                UpdateMaskAreaShader.SimpleDispatch(dispatchKernel, sourceTextureWidth, sourceTextureHeight);
            }

            public static void LayerSelectionArea(Vector2 selectionOffset, ReusableTexture bottomLayer, ReusableTexture target, int sourceX, int sourceY, int sourceWidth, int sourceHeight, ReusableTexture topLayer, SelectMode selectMode, int destinationX = 0, int destinationY = 0, CanvasTileMode destinationTileMode = CanvasTileMode.None, CanvasTileMode sourceTileMode = CanvasTileMode.None, float opacity = 1)
            {
                int sourceTextureWidth = topLayer.width;
                int sourceTextureHeight = topLayer.height;
                int destinationTextureWidth = target.width;
                int destinationTextureHeight = target.height;

                int dispatchKernel;
                if (selectMode == SelectMode.Fresh || selectMode == SelectMode.Add)
                {
                    dispatchKernel = AddSelectionKernel;
                }
                else
                {
                    dispatchKernel = SubtractSelectionKernel;
                }

                int sourceTileX = (sourceTileMode == CanvasTileMode.TileX || sourceTileMode == CanvasTileMode.TileXY) ? 1 : 0;
                int sourceTileY = (sourceTileMode == CanvasTileMode.TileY || sourceTileMode == CanvasTileMode.TileXY) ? 1 : 0;
                int destinationTileX = (destinationTileMode == CanvasTileMode.TileX || destinationTileMode == CanvasTileMode.TileXY) ? 1 : 0;
                int destinationTileY = (destinationTileMode == CanvasTileMode.TileY || destinationTileMode == CanvasTileMode.TileXY) ? 1 : 0;

                var oldDestinationX = destinationX;
                var oldDestinationY = destinationY;

                ClampSourceToTile(ref sourceX, sourceTextureWidth, ref sourceWidth, sourceTileX, ref destinationX);
                ClampSourceToTile(ref sourceY, sourceTextureHeight, ref sourceHeight, sourceTileY, ref destinationY);

                ClampDestinationToTile(ref destinationX, destinationTextureWidth, ref sourceWidth, destinationTileX, ref sourceX);
                ClampDestinationToTile(ref destinationY, destinationTextureHeight, ref sourceHeight, destinationTileY, ref sourceY);

                if (destinationTileX == 0)
                {
                    selectionOffset.x += destinationX - oldDestinationX;
                }
                if (destinationTileY == 0)
                {
                    selectionOffset.y += destinationY - oldDestinationY;
                }

                if (sourceWidth <= 0 || sourceHeight <= 0) return;

                LayerSelectionAreaShader.SetInts(SelectionOffsetPropertyID, (int)selectionOffset.x, (int)selectionOffset.y);

                LayerSelectionAreaShader.SetInts(SrcDstDimPropertyID, sourceTextureWidth, sourceTextureHeight, destinationTextureWidth, destinationTextureHeight);
                LayerSelectionAreaShader.SetInts(DstXYPropertyID, destinationX, destinationY);
                LayerSelectionAreaShader.SetInts(SrcRectPropertyID, sourceX, sourceY, sourceWidth, sourceHeight);

                LayerSelectionAreaShader.SetFloat(OpacityPropertyID, opacity);

                LayerSelectionAreaShader.SetInts(TileSrcXYDstXYPropertyID, sourceTileX, sourceTileY, destinationTileX, destinationTileY);
                LayerSelectionAreaShader.SetTexture(dispatchKernel, BottomLayerPropertyID, bottomLayer.texture);
                LayerSelectionAreaShader.SetTexture(dispatchKernel, TopLayerPropertyID, topLayer.texture);
                LayerSelectionAreaShader.SetTexture(dispatchKernel, ResultPropertyID, target.texture);
                LayerSelectionAreaShader.SimpleDispatch(dispatchKernel, sourceTextureWidth, sourceTextureHeight);
            }

        }

        public static class Statistics
        {
            private static ComputeShader HistogramCalculateShader;
            private static int HistogramCalculateKernel;

            private static ComputeShader HistogramStatisticsShader;
            private static int HistogramStatisticsKernel;

            private static ComputeShader HistogramDisplayShader;
            private static int HistogramDisplayKernel;

            private static int HistogramSizePropertyID;
            private static int SourceDimensionsPropertyID;

            [System.Serializable]
            public struct HistogramStatistics
            {
                public uint Rmax;
                public uint Gmax;
                public uint Bmax;
                public uint Amax;
            }

            public static void Initialize()
            {
                HistogramCalculateShader = Resources.Load<ComputeShader>("Statistics/HistogramCalculate");
                HistogramCalculateKernel = HistogramCalculateShader.FindKernel("CSMain");

                HistogramStatisticsShader = Resources.Load<ComputeShader>("Statistics/HistogramStatistics");
                HistogramStatisticsKernel = HistogramStatisticsShader.FindKernel("CSMain");

                HistogramDisplayShader = Resources.Load<ComputeShader>("Statistics/HistogramDisplay");
                HistogramDisplayKernel = HistogramDisplayShader.FindKernel("CSMain");

                HistogramSizePropertyID = Shader.PropertyToID("HistogramSize");
                SourceDimensionsPropertyID = Shader.PropertyToID("SourceDimensions");

            }

            public static void CalculateHistogram(ReusableTexture sourceImage, ComputeBuffer targetBuffer)
            {
                HistogramCalculateShader.SetTexture(HistogramCalculateKernel, InputPropertyID, sourceImage.texture);
                HistogramCalculateShader.SetBuffer(HistogramCalculateKernel, ResultPropertyID, targetBuffer);
                HistogramCalculateShader.SetInts(SourceDimensionsPropertyID, sourceImage.width, sourceImage.height);
                HistogramCalculateShader.SimpleDispatch(HistogramCalculateKernel, sourceImage.width, sourceImage.height);
            }

            public static HistogramStatistics GetHistogramStatistics(ComputeBuffer sourceBuffer)
            {
                HistogramStatistics stats;
                uint[] statsArray = new uint[4];

                ComputeBuffer statsBuffer = new ComputeBuffer(1, sizeof(int) * 4);
                statsBuffer.SetData(statsArray);

                HistogramStatisticsShader.SetBuffer(HistogramStatisticsKernel, InputPropertyID, sourceBuffer);
                HistogramStatisticsShader.SetBuffer(HistogramStatisticsKernel, ResultPropertyID, statsBuffer);
                HistogramStatisticsShader.Dispatch(HistogramStatisticsKernel, Mathf.CeilToInt(256 / 32.0f), 1, 1);
                
                statsBuffer.GetData(statsArray);
                stats = new HistogramStatistics
                {
                    Rmax = statsArray[0],
                    Gmax = statsArray[1],
                    Bmax = statsArray[2],
                    Amax = statsArray[3]
                };

                statsBuffer.Release();

                return stats;
            }

            public static void DisplayHistogram(ComputeBuffer sourceBuffer, ReusableTexture targetImage, uint maxHistogramHeight)
            {
                HistogramDisplayShader.SetBuffer(HistogramDisplayKernel, InputPropertyID, sourceBuffer);
                HistogramDisplayShader.SetTexture(HistogramDisplayKernel, ResultPropertyID, targetImage.texture);
                HistogramDisplayShader.SetInts(HistogramSizePropertyID, targetImage.width, targetImage.height, (int)maxHistogramHeight, 0);
                HistogramDisplayShader.SimpleDispatch(HistogramDisplayKernel, targetImage.width, targetImage.height);
            }
        }


    }
}