using System;
using UnityEngine;

public interface IReadableTexture
{
    public int width { get; }
    public int height { get; }
    public int actualWidth { get; }
    public int actualHeight { get; }
    public Texture texture { get; }
}

public class ReadableTexture2D : IDisposable, IReadableTexture
{
    private Texture2D _texture;
    public ReadableTexture2D(Texture2D texture)
    {
        _texture = texture;
        _usedWidth = _texture.width;
        _usedHeight = _texture.height;
        _actualHeight = _texture.height;
        _actualWidth = _texture.width;
    }

    private int _usedWidth;
    private int _usedHeight;
    private int _actualWidth;
    private int _actualHeight;

    public int width => _usedWidth;
    public int height => _usedHeight;
    public int actualWidth => _actualWidth;
    public int actualHeight => _actualHeight;

    public Texture texture => _texture;

    public void Dispose()
    {
        UnityEngine.Object.Destroy(_texture);
    }
}


public class ReusableTexture : IDisposable, IReadableTexture
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

    public void Dispose()
    {
        _texture.Release();
    }

    public int width { get { return _usedWidth; } }
    public int height { get { return _usedHeight; } }

    public int actualWidth { get { return _actualWidth; } }
    public int actualHeight { get { return _actualHeight; } }

    public RenderTexture texture { get { return _texture; } }
    public RenderTextureFormat format { get { return _format; } }

    Texture IReadableTexture.texture => _texture;
}
