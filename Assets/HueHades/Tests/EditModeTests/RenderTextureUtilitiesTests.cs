using HueHades.Utilities;
using NUnit.Framework;
using UnityEngine;
using Unity.Mathematics;

public class RenderTextureUtilitiesTests
{
    [OneTimeSetUp]
    public void InitStatic()
    {
        RenderTextureUtilities.Initialize();
    }

    [OneTimeTearDown]
    public void CleanupStatic()
    {
        RenderTextureUtilities.Dispose();
    }

    /// <summary>
    /// Test if we are able to get a temporary ReusableTexture
    /// </summary>
    [Test]
    public void Get_Temporary()
    {
        var texture = RenderTextureUtilities.GetTemporary(30,5,RenderTextureFormat.ARGBFloat);
        Assert.IsNotNull(texture, "Failed to get reusable texture");
        Assert.AreEqual(new int2(30,5),new int2(texture.width, texture.height), "Expected usable texture size does not match");
        Assert.AreEqual(new int2(32,32),new int2(texture.actualWidth, texture.actualHeight), "Expected buffer size does not match");
        RenderTextureUtilities.ReleaseTemporary(texture);
    }

    /// <summary>
    /// Test if we are able to re-use the ReusableTexture
    /// </summary>
    [Test]
    public void Get_Temporary_Reuse()
    {
        var texture = RenderTextureUtilities.GetTemporary(30, 5, RenderTextureFormat.ARGBFloat);
        RenderTextureUtilities.ReleaseTemporary(texture);
        var texture2 = RenderTextureUtilities.GetTemporary(27, 31, RenderTextureFormat.ARGBFloat);

        Assert.IsNotNull(texture2, "Failed to get reusable texture");
        Assert.AreEqual(texture, texture2, "Failed to reuse texture");
        RenderTextureUtilities.ReleaseTemporary(texture);
    }

    /// <summary>
    /// Test if we are able to get a temporary ReusableTexture used for gradients
    /// </summary>
    [Test]
    public void Get_Temporary_Gradient()
    {
        var texture = RenderTextureUtilities.GetTemporaryGradient(30, RenderTextureFormat.ARGBFloat);
        Assert.IsNotNull(texture, "Failed to get reusable gradient texture");
        Assert.AreEqual(new int2(30, 1), new int2(texture.width, texture.height), "Expected usable texture size does not match");
        Assert.AreEqual(new int2(32, 1), new int2(texture.actualWidth, texture.actualHeight), "Expected buffer size does not match");
        RenderTextureUtilities.ReleaseTemporaryGradient(texture);
    }

    /// <summary>
    /// Test if we are able to re-use the ReusableTexture used for gradients
    /// </summary>
    [Test]
    public void Get_Temporary_Gradient_Reuse()
    {
        var texture = RenderTextureUtilities.GetTemporaryGradient(30, RenderTextureFormat.ARGBFloat);
        RenderTextureUtilities.ReleaseTemporaryGradient(texture);
        var texture2 = RenderTextureUtilities.GetTemporaryGradient(27, RenderTextureFormat.ARGBFloat);

        Assert.IsNotNull(texture2, "Failed to get reusable gradient texture");
        Assert.AreEqual(texture, texture2, "Failed to reuse gradient texture");

        RenderTextureUtilities.ReleaseTemporaryGradient(texture);
    }

}
