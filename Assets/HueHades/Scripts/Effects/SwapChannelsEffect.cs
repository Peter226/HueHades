using HueHades.Core;
using HueHades.Utilities;
using HueHades.Common;

namespace HueHades.Effects
{
    public class SwapChannelsEffect : ImageEffect
    {

        public ColorChannel RedChannel { get; set; }
        public ColorChannel GreenChannel { get; set; }
        public ColorChannel BlueChannel { get; set; }
        public ColorChannel AlphaChannel { get; set; }

        private ImageCanvas canvas;
        private ReusableTexture backupSnapshot;
        private ReusableTexture targetBuffer;
        private ImageLayer selectedLayer;
        private ImageLayer.CopyHandle copyHandle;

        public override void BeginEffect(ImageCanvas canvas)
        {
            this.canvas = canvas;
            backupSnapshot = RenderTextureUtilities.GetTemporary(canvas.Dimensions.x, canvas.Dimensions.y, canvas.Format);
            targetBuffer = RenderTextureUtilities.GetTemporary(canvas.Dimensions.x, canvas.Dimensions.y, canvas.Format);

            var selectedLayerBase = canvas.ActiveLayer;

            selectedLayer = selectedLayerBase as ImageLayer;
            copyHandle = selectedLayer.GetOperatingCopy(backupSnapshot);
        }
        public override void ApplyEffect()
        {
            RenderTextureUtilities.Effects.SwapChannels(backupSnapshot, targetBuffer, RedChannel, GreenChannel, BlueChannel, AlphaChannel);
            selectedLayer.PasteOperatingCopy(backupSnapshot, targetBuffer, copyHandle);

            ModifyLayerHistoryRecord modifyLayerHistoryRecord = new ModifyLayerHistoryRecord(selectedLayer.GlobalIndex, backupSnapshot, selectedLayer.Texture, "Swap Channels");
            canvas.History.AddRecord(modifyLayerHistoryRecord);

            RenderTextureUtilities.ReleaseTemporary(backupSnapshot);
            RenderTextureUtilities.ReleaseTemporary(targetBuffer);
        }


        public override void RenderEffect()
        {
            RenderTextureUtilities.Effects.SwapChannels(backupSnapshot, targetBuffer, RedChannel, GreenChannel, BlueChannel, AlphaChannel);
            selectedLayer.PasteOperatingCopy(backupSnapshot, targetBuffer, copyHandle);
        }

        public override void CancelEffect()
        {
            selectedLayer.PasteOperatingCopy(backupSnapshot, backupSnapshot, copyHandle);
            RenderTextureUtilities.ReleaseTemporary(backupSnapshot);
            RenderTextureUtilities.ReleaseTemporary(targetBuffer);
        }
    }
}