using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Icons
{
    private static Dictionary<string, Texture> _icons = new Dictionary<string, Texture>();

    /// <summary>
    /// Returns a texture for an icon path, loads icon if not loaded
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Texture GetIcon(string path)
    {
        Texture icon;
        if (_icons.TryGetValue(path, out icon))
        {
            return icon;
        }
        icon = Resources.Load<Texture>(path);
        _icons.Add(path, icon);
        return icon;
    }
    
}
