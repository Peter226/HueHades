using HueHades.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureDebugger
{
    private static bool SessionRunning;
    private static List<(ReusableTexture, string)> DebugTextures = new List<(ReusableTexture, string)>();
    public static Action<List<(ReusableTexture, string)>> DrawCommandFired;


    public static void BeginSession(string sessionName)
    {
        if (DrawCommandFired == null || DrawCommandFired.GetInvocationList().Length <= 0) return;
        DebugTextures.ForEach(x => RenderTextureUtilities.ReleaseTemporary(x.Item1));
        DebugTextures.Clear();
        SessionRunning = true;
    }

    public static void DebugRenderTexture(ReusableTexture renderTexture, string label)
    {
        if (!SessionRunning) return;
        var rt = RenderTextureUtilities.GetTemporary(renderTexture.width, renderTexture.height, renderTexture.texture.format);
        RenderTextureUtilities.CopyTexture(renderTexture, rt);
        DebugTextures.Add((rt, label));
    }

    public static void EndSession(string sessionName)
    {
        if (!SessionRunning) return;
        SessionRunning = false;
        DrawCommandFired?.Invoke(DebugTextures);
        DebugTextures.ForEach(x => RenderTextureUtilities.ReleaseTemporary(x.Item1));
        DebugTextures.Clear();
    }

}
