using HueHades.Core;
using HueHades.Utilities;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public interface ILayerContainer
{
    public List<LayerBase> Layers { get; }
    public int2 Dimensions { get; }
    public RenderTextureFormat Format { get; }
    public ReusableTexture PreviewTexture { get; }
    public Action PreviewChanged { get; set; }
    public bool IsDirty { get; set; }
}

public static class ILayerContainerExtensions
{

    public static IEnumerable<LayerBase> GetGlobalLayers(this ImageCanvas imageCanvas)
    {
        return imageCanvas.GlobalLayerCollection;
    }


    private static void ProcessHierarchyChange(this ILayerContainer layerContainer)
    {
        if (layerContainer is GroupLayer)
        {
            (layerContainer as GroupLayer).ContainerIn.ProcessHierarchyChange();
        }
        else
        {
            if (layerContainer is ImageCanvas)
            {
                var imageCanvas = layerContainer as ImageCanvas;
                imageCanvas.GlobalLayerCollection.Clear();
                layerContainer.AssignIDs(imageCanvas, 1);
                imageCanvas.HierarchyUpdated?.Invoke();
            }
        }
    }

    private static int AssignIDs(this ILayerContainer layerContainer, ImageCanvas imageCanvas, int startID)
    {
        int id = startID;
        for (int i = 0;i < layerContainer.Layers.Count;i++)
        {
            var layer = layerContainer.Layers[i];
            imageCanvas.GlobalLayerCollection.Add(layer);
            layer.CanvasIn = imageCanvas;
            layer.GlobalIndex = id;
            layer.RelativeIndex = i;
            id++;
            if (layer is GroupLayer)
            {
                id = (layer as GroupLayer).AssignIDs(imageCanvas, id);
            }
        }
        return id;
    }

    public static LayerBase GetLayerByGlobalID(this ImageCanvas canvas, int globalIndex) {

        //return canvas.FindLayerByGlobalID(globalIndex);
        return canvas.GlobalLayerCollection[globalIndex-1];
    }
    private static LayerBase FindLayerByGlobalID(this ILayerContainer layerContainer, int globalIndex)
    {
        var layers = layerContainer.Layers;
        LayerBase lastLayer = null;
        for (int i = 0; i < layers.Count; i++)
        {
            var layer = layers[i];
            if (layer.GlobalIndex == globalIndex)
            {
                return layer;
            }
            if (layer.GlobalIndex > globalIndex)
            {
                break;
            }
            lastLayer = layer;
        }
        if (lastLayer is null) return null;
        return (lastLayer as GroupLayer).FindLayerByGlobalID(globalIndex);
    }

    public static ImageLayer AddLayer(this ImageCanvas imageCanvas, int globalIndexOfContainerIn, int relativeIndexToContainer, Color clearColor)
    {
        ILayerContainer layerContainer = null;
        if (globalIndexOfContainerIn == 0)
        {
            layerContainer = imageCanvas;
        }
        else
        {
            var presumedGroupLayer = imageCanvas.GetLayerByGlobalID(globalIndexOfContainerIn);
            if (presumedGroupLayer is not GroupLayer) throw new Exception("Expected GroupLayer while adding layer to canvas");
            layerContainer = (presumedGroupLayer as GroupLayer);
        }

        var layer = new ImageLayer(layerContainer.Dimensions, layerContainer.Format, clearColor);
        layerContainer.Layers.Insert(relativeIndexToContainer, layer);
        layer.ContainerIn = layerContainer;
        layer.LayerChanged += layerContainer.SetDirty;
        layerContainer.ProcessHierarchyChange();
        layer.LayerChanged();
        return layer;
    }


    public static void AddLayer(this ImageCanvas imageCanvas, LayerBase layer, int globalIndexOfContainerIn, int relativeIndexToContainer)
    {
        ILayerContainer layerContainer = null;
        if (globalIndexOfContainerIn == 0)
        {
            layerContainer = imageCanvas;
        }
        else
        {
            var presumedGroupLayer = imageCanvas.GetLayerByGlobalID(globalIndexOfContainerIn);
            if (presumedGroupLayer is not GroupLayer) throw new Exception("Expected GroupLayer while adding layer to canvas");
            layerContainer = (presumedGroupLayer as GroupLayer);
        }

        layerContainer.Layers.Insert(relativeIndexToContainer, layer);
        layer.ContainerIn = layerContainer;
        layer.LayerChanged += layerContainer.SetDirty;
        layerContainer.ProcessHierarchyChange();
        layer.LayerChanged();
    }

    public static void MoveLayer(this ImageCanvas imageCanvas, int globalLayerIndex, int newContainerGlobalIndex, int newRelativeIndex)
    {
        if (globalLayerIndex == newContainerGlobalIndex) return;
        var layer = imageCanvas.GetLayerByGlobalID(globalLayerIndex);

        ILayerContainer newLayerContainer = null;
        if (newContainerGlobalIndex == 0)
        {
            newLayerContainer = imageCanvas;
        }
        else
        {
            var presumedGroupLayer = imageCanvas.GetLayerByGlobalID(newContainerGlobalIndex);
            if (presumedGroupLayer is not GroupLayer) throw new Exception("Expected GroupLayer while adding layer to canvas");
            newLayerContainer = (presumedGroupLayer as GroupLayer);
        }
        

        var layerContainer = layer.ContainerIn;
        if (layerContainer == newLayerContainer && layer.RelativeIndex < newRelativeIndex)
        {
            newRelativeIndex--;
        }
        layer.LayerChanged -= layerContainer.SetDirty;
        layer.ContainerIn = newLayerContainer;
        layer.LayerChanged += newLayerContainer.SetDirty;
        layerContainer.Layers.RemoveAt(layer.RelativeIndex);
        newLayerContainer.Layers.Insert(newRelativeIndex, layer);
        layerContainer.ProcessHierarchyChange();
        layerContainer.SetDirty();
        newLayerContainer.SetDirty();
    }

    public static void RemoveLayer(this ImageCanvas imageCanvas, int globalLayerIndex)
    {
        var layer = imageCanvas.GetLayerByGlobalID(globalLayerIndex);
        if (layer.CanvasIn.SelectedLayer == layer)
        {
            layer.CanvasIn.SelectedLayer = null;
        }
        var layerContainer = layer.ContainerIn;
        layer.LayerChanged -= layerContainer.SetDirty;
        layer.ContainerIn = null;
        layerContainer.Layers.RemoveAt(layer.RelativeIndex);
        layerContainer.ProcessHierarchyChange();
        layerContainer.SetDirty();
    }

    public static void SetDirty(this ILayerContainer layerContainer)
    {
        layerContainer.IsDirty = true;
    }


    public static void RenderPreview(this ILayerContainer layerContainer)
    {
        layerContainer.IsDirty = false;

        var swapBufferA = layerContainer.PreviewTexture;
        var swapBufferB = RenderTextureUtilities.GetTemporary(swapBufferA.width, swapBufferA.height, swapBufferA.format);
        var swapBufferC = RenderTextureUtilities.GetTemporary(swapBufferA.width, swapBufferA.height, swapBufferA.format);
        if (layerContainer.Layers.Count > 0) {
            var firstLayer = layerContainer.Layers[0];
            if (!firstLayer.LayerSettings.invisible) RenderTextureUtilities.CopyTexture(firstLayer.Texture, swapBufferA);
            else RenderTextureUtilities.ClearTexture(swapBufferA, Color.clear);
        }
        else
        {
            RenderTextureUtilities.ClearTexture(layerContainer.PreviewTexture, Color.clear);
        }
        for (int i = 1;i < layerContainer.Layers.Count;i++)
        {
            var layer = layerContainer.Layers[i];

            if (!layer.LayerSettings.invisible) {
                

                if (layer.LayerSettings.inheritAlpha)
                {
                    RenderTextureUtilities.LayerImage(swapBufferA, layer.Texture, swapBufferC, layer.LayerBlendMode);
                    RenderTextureUtilities.InheritAlpha(swapBufferA, swapBufferC, swapBufferB);
                }
                else
                {
                    RenderTextureUtilities.LayerImage(swapBufferA, layer.Texture, swapBufferB, layer.LayerBlendMode);
                }
                var temp = swapBufferA;
                swapBufferA = swapBufferB;
                swapBufferB = temp;
            }
        }
        if (swapBufferA != layerContainer.PreviewTexture)
        {
            RenderTextureUtilities.CopyTexture(swapBufferA, swapBufferB);
            RenderTextureUtilities.ReleaseTemporary(swapBufferA);
        }
        else
        {
            RenderTextureUtilities.ReleaseTemporary(swapBufferB);
        }
        RenderTextureUtilities.ReleaseTemporary(swapBufferC);
        layerContainer.PreviewChanged?.Invoke();
    }

    public static int GetLayerCount (this ILayerContainer layerContainer)
    {
        return layerContainer.Layers.Count;
    }
}