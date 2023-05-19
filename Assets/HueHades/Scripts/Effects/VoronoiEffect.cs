using HueHades.Core;
using HueHades.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HueHades.Effects
{
    public class VoronoiEffect : ImageEffect
    {
        public int Seed { get; set; }
        public int CellsX { get; set; }
        public int CellsY { get; set; }

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

            var selectedLayerBase = canvas.SelectedLayer;

            selectedLayer = selectedLayerBase as ImageLayer;
            copyHandle = selectedLayer.GetOperatingCopy(backupSnapshot);
        }
        public override void ApplyEffect()
        {
            RenderTextureUtilities.Effects.Voronoi(targetBuffer, Seed, CellsX, CellsY, canvas.TileMode);
            selectedLayer.PasteOperatingCopy(backupSnapshot, targetBuffer, copyHandle);

            ModifyLayerHistoryRecord modifyLayerHistoryRecord = new ModifyLayerHistoryRecord(selectedLayer.GlobalIndex, backupSnapshot, selectedLayer.Texture, "Voronoi");
            canvas.History.AddRecord(modifyLayerHistoryRecord);

            RenderTextureUtilities.ReleaseTemporary(backupSnapshot);
            RenderTextureUtilities.ReleaseTemporary(targetBuffer);
        }


        public override void RenderEffect()
        {
            RenderTextureUtilities.Effects.Voronoi(targetBuffer, Seed, CellsX, CellsY, canvas.TileMode);
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