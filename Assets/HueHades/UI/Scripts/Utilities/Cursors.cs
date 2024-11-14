using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using UnityEngine;
using Cursor = UnityEngine.UIElements.Cursor;

namespace HueHades.UI.Utilities
{
    public class Cursors
    {
        public enum CursorType
        {
            Default,
            Hand,
            Move,
            ScaleWest,
            ScaleNorth,
            ScaleNorthWest,
            ScaleNorthEast,
        }

        public static Cursor GetCursor(CursorType cursorType)
        {
            return GetCursorNative(cursorType);
        }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN


        private enum WindowsNativeCursorID : int
        {
            Default = 32512,
            Hand = 32649,
            Move = 32646,
            ScaleWest = 32644,
            ScaleNorth = 32645,
            ScaleNorthWest = 32642,
            ScaleNorthEast = 32643,
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr SetCursor(IntPtr hCursor);

        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);


        [DllImport("user32.dll")]
        private static extern IntPtr LoadImage(IntPtr hInst, IntPtr name, uint type, int cx, int cy, uint fuLoad);



        [StructLayout(LayoutKind.Sequential)]
        private struct IconInfoEx
        {
            public int cbSize;
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
            public ushort wResID;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
            public byte[] szModName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
            public byte[] szResName;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Bitmap
        {
            public int bmType;
            public int bmWidth;
            public int bmHeight;
            public int bmWidthBytes;
            public ushort bmPlanes;
            public ushort bmBitsPixel;
            public IntPtr bmBits;
        }

        [DllImport("user32.dll")]
        private static extern bool GetIconInfoEx(IntPtr hicon, ref IconInfoEx piconinfo);


        [DllImport("gdi32", CharSet = CharSet.Auto, EntryPoint = "GetObject")]
        private static extern int GetObjectBitmap(IntPtr hObject, int nCount, ref Bitmap lpObject);

        [DllImport("Gdi32", EntryPoint = "GetBitmapBits")]
        private extern static long GetBitmapBits([In] IntPtr hbmp, [In] int cbBuffer, [Out] byte[] lpvBits);




        private static Dictionary<WindowsNativeCursorID, Cursor> _loadedNativeCursorsWin = new Dictionary<WindowsNativeCursorID, Cursor>();

        private static Cursor LoadAndGetCursorWin(WindowsNativeCursorID cursorID)
        {
            var c = LoadImage(IntPtr.Zero, new IntPtr((int)cursorID), 2, 32, 32, 0x00008000);
            if (!_loadedNativeCursorsWin.ContainsKey(cursorID))
            {
                IconInfoEx p = new IconInfoEx();
                p.cbSize = Marshal.SizeOf(p);
                var got = GetIconInfoEx(c, ref p);
                if (!got)
                {
                    UnityEngine.Debug.Log(Marshal.GetLastWin32Error());
                }


                //UnityEngine.Debug.Log(got);
                //UnityEngine.Debug.Log($"{p.fIcon};{p.xHotspot};{p.yHotspot};{p.wResID}");

                var bm = new Bitmap();
                var i = GetObjectBitmap(p.hbmColor, Marshal.SizeOf(typeof(Bitmap)), ref bm);

                var width = bm.bmWidth;
                var height = bm.bmHeight;
                var channelCount = bm.bmBitsPixel / sizeof(byte);

                byte[] imagebits = new byte[bm.bmWidth * bm.bmHeight * channelCount];
                //UnityEngine.Debug.Log(imagebits.Length);
                GetBitmapBits(p.hbmColor, imagebits.Length * Marshal.SizeOf(typeof(byte)), imagebits);
                //UnityEngine.Debug.Log(imagebits.Length);


                //UnityEngine.Debug.Log(i);
                //UnityEngine.Debug.Log($"{bm.bmType};{bm.bmWidth};{bm.bmHeight};{bm.bmWidthBytes};{bm.bmPlanes};{bm.bmBitsPixel}");


                //UnityEngine.Debug.Log(imagebits);

                var cursor = new Cursor();
                cursor.hotspot = new UnityEngine.Vector2(p.xHotspot, p.yHotspot);

                UnityEngine.Texture2D cursorTexture = new UnityEngine.Texture2D(bm.bmWidth, bm.bmHeight, UnityEngine.TextureFormat.RGBA32, false);
                cursorTexture.hideFlags = UnityEngine.HideFlags.DontSave;
                cursorTexture.filterMode = UnityEngine.FilterMode.Point;
                

                //UnityEngine.Debug.Log(imagebits.Length);




                cursorTexture.SetPixelData(imagebits, 0);
                cursorTexture.Apply();

                FlipTextureVertically(cursorTexture);

                cursor.texture = cursorTexture;

                _loadedNativeCursorsWin.Add(cursorID, cursor);
            }
            

            return _loadedNativeCursorsWin[cursorID];
        }



        public static void FlipTextureVertically(Texture2D original)
        {
            var originalPixels = original.GetPixels();

            var newPixels = new Color[originalPixels.Length];

            var width = original.width;
            var rows = original.height;

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < rows; y++)
                {
                    newPixels[x + y * width] = originalPixels[x + (rows - y - 1) * width];
                }
            }

            original.SetPixels(newPixels);
            original.Apply();
        }




        private static Cursor GetCursorNative(CursorType cursorType)
        {
            switch (cursorType)
            {
                case CursorType.Default:
                    return new Cursor(); //No point in fetching windows cursor, it defaults to it
                case CursorType.Hand:
                    return LoadAndGetCursorWin(WindowsNativeCursorID.Hand);
                case CursorType.Move:
                    return LoadAndGetCursorWin(WindowsNativeCursorID.Move);
                case CursorType.ScaleWest:
                    return LoadAndGetCursorWin(WindowsNativeCursorID.ScaleWest);
                case CursorType.ScaleNorth:
                    return LoadAndGetCursorWin(WindowsNativeCursorID.ScaleNorth);
                case CursorType.ScaleNorthWest:
                    return LoadAndGetCursorWin(WindowsNativeCursorID.ScaleNorthWest);
                case CursorType.ScaleNorthEast:
                    return LoadAndGetCursorWin(WindowsNativeCursorID.ScaleNorthEast);
            }
            return new Cursor();
        }

#else
        private static void SetCursorNative(CursorType cursorType)
        {
            Debug.LogWarning("Native cursors are not implemented for this platform");
        }
#endif
    }



}