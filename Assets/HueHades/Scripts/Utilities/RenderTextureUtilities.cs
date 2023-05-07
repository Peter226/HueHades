using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HueHades.Common;
using UnityEngine.Windows;
using static UnityEngine.GraphicsBuffer;

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

        private static int TopLayerPropertyID;
        private static int BottomLayerPropertyID;

        private static int BlendAreaNormalKernel;
        private static int BlendAreaAddKernel;
        private static int BlendAreaMultiplyKernel;
        private static int BlendAreaSubtractKernel;

        private static int MaskChannelsKernel;
        private static int TargetPropertyID;
        private static int MaskPropertyID;

        private static int PositionSizePropertyID;
        private static int RotationMatrixPropertyID;


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
            TileSrcXYDstXYPropertyID = Shader.PropertyToID("TileSrcXYDstXY");

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


            PositionSizePropertyID = Shader.PropertyToID("PositionSize");
            RotationMatrixPropertyID = Shader.PropertyToID("RotationMatrix");

            Gradients.Initialize();
            Brushes.Initialize();
            Effects.Initialize();
            Sampling.Initialize();
            Selection.Initialize();
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
            CopyImageShader.Dispatch(CopyShaderKernel, Mathf.CeilToInt(sourceWidth / (float)warpSizeX), Mathf.CeilToInt(sourceHeight / (float)warpSizeY), 1);
        }

        public static void ClearTexture(ReusableTexture texture, Color clearColor)
        {
            ClearImageShader.SetTexture(ClearColorKernel, ResultPropertyID, texture.texture);
            ClearImageShader.SetVector(ClearColorPropertyID, clearColor);
            ClearImageShader.Dispatch(ClearColorKernel, Mathf.CeilToInt(texture.width / (float)warpSizeX), Mathf.CeilToInt(texture.height / (float)warpSizeY), 1);
        }

        public static void LayerImage(ReusableTexture bottomLayer, ReusableTexture topLayer, ReusableTexture result, ColorBlendMode colorBlendMode)
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

            LayerImageShader.SetTexture(dispatchKernel,BottomLayerPropertyID,bottomLayer.texture);
            LayerImageShader.SetTexture(dispatchKernel,TopLayerPropertyID,topLayer.texture);
            LayerImageShader.SetTexture(dispatchKernel,ResultPropertyID,result.texture);
            LayerImageShader.Dispatch(dispatchKernel, Mathf.CeilToInt(result.width / (float)warpSizeX), Mathf.CeilToInt(result.height / (float)warpSizeY), 1);
        }

        public static void LayerImageArea(ReusableTexture bottomLayer, ReusableTexture target, int sourceX, int sourceY, int sourceWidth, int sourceHeight, ReusableTexture topLayer, ColorBlendMode colorBlendMode, int destinationX = 0, int destinationY = 0, CanvasTileMode destinationTileMode = CanvasTileMode.None, CanvasTileMode sourceTileMode = CanvasTileMode.None)
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

            LayerImageAreaShader.SetInts(TileSrcXYDstXYPropertyID, sourceTileX, sourceTileY, destinationTileX, destinationTileY);
            LayerImageAreaShader.SetTexture(dispatchKernel, BottomLayerPropertyID, bottomLayer.texture);
            LayerImageAreaShader.SetTexture(dispatchKernel, TopLayerPropertyID, topLayer.texture);
            LayerImageAreaShader.SetTexture(dispatchKernel, ResultPropertyID, target.texture);
            LayerImageAreaShader.Dispatch(dispatchKernel, Mathf.CeilToInt(sourceTextureWidth / (float)warpSizeX), Mathf.CeilToInt(sourceTextureHeight / (float)warpSizeY), 1);
        }


        public static void ApplyChannelMask(ReusableTexture target, ReusableTexture mask)
        {
            MaskChannelsShader.SetTexture(MaskChannelsKernel, TargetPropertyID, target.texture);
            MaskChannelsShader.SetTexture(MaskChannelsKernel, MaskPropertyID, mask.texture);
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

            public static void DrawColorGradientRectangle(ReusableTexture target, int rectangleSizeX, int rectangleSizeY, Color colorA, Color colorB, Color colorC, Color colorD)
            {
                if (rectangleSizeX <= 0 || rectangleSizeY <= 0) return;
                DrawColorGradientRectangleShader.SetInts(RectangleSizePropertyID, rectangleSizeX, rectangleSizeY);
                DrawColorGradientRectangleShader.SetVector(ColorAPropertyID, colorA);
                DrawColorGradientRectangleShader.SetVector(ColorBPropertyID, colorB);
                DrawColorGradientRectangleShader.SetVector(ColorCPropertyID, colorC);
                DrawColorGradientRectangleShader.SetVector(ColorDPropertyID, colorD);
                DrawColorGradientRectangleShader.SetTexture(GradientRectangleKernel, ResultPropertyID, target.texture);
                DrawColorGradientRectangleShader.Dispatch(GradientRectangleKernel, Mathf.CeilToInt(Mathf.Min(target.width, rectangleSizeX) / (float)warpSizeX), Mathf.CeilToInt(Mathf.Min(target.height, rectangleSizeY) / (float)warpSizeY), 1);
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
                DrawBrushShader.Dispatch(chosenKernel, Mathf.CeilToInt(target.width / (float)warpSizeX), Mathf.CeilToInt(target.height / (float)warpSizeY), 1);
            }
        }

        public static class Effects
        {
            private static ComputeShader ColorAdjustmentsShader;
            private static int AdjustmentParamsPropertyID;
            private static int ColorAdjustmentsKernel;

            public static void Initialize()
            {
                ColorAdjustmentsShader = Resources.Load<ComputeShader>("Effects/ColorAdjustments");
                AdjustmentParamsPropertyID = Shader.PropertyToID("AdjustmentParams");
                ColorAdjustmentsKernel = ColorAdjustmentsShader.FindKernel("CSMain");
            }

            public static void ColorAdjustments(ReusableTexture input, ReusableTexture result, float hue, float saturation, float brightness, float contrast)
            {
                ColorAdjustmentsShader.SetVector(AdjustmentParamsPropertyID, new Vector4(hue, saturation, brightness, contrast));
                ColorAdjustmentsShader.SetTexture(ColorAdjustmentsKernel, InputPropertyID, input.texture);
                ColorAdjustmentsShader.SetTexture(ColorAdjustmentsKernel, ResultPropertyID, result.texture);
                ColorAdjustmentsShader.Dispatch(ColorAdjustmentsKernel, Mathf.CeilToInt(input.width / (float)warpSizeX), Mathf.CeilToInt(input.height / (float)warpSizeY), 1);
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

                switch (samplerMode)
                {
                    case SamplerMode.Point:
                        ResampleShader.SetTexture(PointKernel, InputPropertyID, source.texture);
                        ResampleShader.SetTexture(PointKernel, TargetPropertyID, target.texture);
                        ResampleShader.Dispatch(PointKernel, Mathf.CeilToInt(target.width / (float)warpSizeX), Mathf.CeilToInt(target.height / (float)warpSizeY), 1);
                        break;
                    case SamplerMode.Linear:

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
                ColorspaceSwitchShader.Dispatch(LinearToGammaKernel, Mathf.CeilToInt(result.width / (float)warpSizeX), Mathf.CeilToInt(result.height / (float)warpSizeY), 1);
            }
            public static void LinearToSRGB(ReusableTexture input, ReusableTexture result)
            {
                ColorspaceSwitchShader.SetTexture(LinearToSRGBKernel, InputPropertyID, input.texture);
                ColorspaceSwitchShader.SetTexture(LinearToSRGBKernel, ResultPropertyID, result.texture);
                ColorspaceSwitchShader.Dispatch(LinearToSRGBKernel, Mathf.CeilToInt(result.width / (float)warpSizeX), Mathf.CeilToInt(result.height / (float)warpSizeY), 1);
            }


        }
        
        public static class Selection
        {



            public static void Initialize()
            {

            }

            public static void DrawRectangle()
            {

            }

            public static void DrawEllipse()
            {

            }

            public static void DrawBrush()
            {

            }

        }




    }
}