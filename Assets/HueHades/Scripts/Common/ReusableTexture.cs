using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReusableTexture
{
    private int _usedWidth;
    private int _usedHeight;
    private int _actualWidth;
    private int _actualHeight;
    private RenderTexture _texture;
    private RenderTextureFormat _format;

    public ReusableTexture(RenderTexture renderTexture, int width, int height)
    {
        _usedWidth = width;
        _usedHeight = height;

        _actualWidth = renderTexture.width;
        _actualHeight = renderTexture.height;

        _texture = renderTexture;
        _format = renderTexture.format;
    }

    public ReusableTexture(int width, int height, RenderTextureFormat format, int mipCount)
    {
        _texture = new RenderTexture(width, height, 0, format, mipCount);
        _texture.enableRandomWrite = true;
        _texture.wrapMode = TextureWrapMode.Clamp;
        _texture.filterMode = FilterMode.Bilinear;
        _texture.Create();
        _usedWidth = width;
        _usedHeight = height;
        _actualWidth = width;
        _actualHeight = height;
        _format = format;
    }

    public void ReuseAs(int width, int height) {
        _usedWidth = width;
        _usedHeight = height;
    }


    public int width { get { return _usedWidth; } }
    public int height { get { return _usedHeight; } }

    public int actualWidth { get { return _actualWidth; } }
    public int actualHeight { get { return _actualHeight; } }

    public RenderTexture texture { get { return _texture; } }
    public RenderTextureFormat format { get { return _format; } }
}
