using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace HueHades.Tools
{
    public class BrushPreset
    {
        private static List<BrushPreset> _presets = new List<BrushPreset>();
        public static List<BrushPreset> Presets { get { return _presets; } set { _presets = value; PresetsChanged?.Invoke(); } }

        public static Action PresetsChanged;

        public BrushShape shape { get; set; }
        public string name { get; set; }
        public Texture icon { get { if (_icon == null) _icon = Resources.Load<Texture2D>(iconPath);  return _icon; } }
        private Texture _icon;
        private string _iconPath;
        public string iconPath { get { return _iconPath; } set { _iconPath = value; if (_icon != null) _icon = Resources.Load<Texture2D>(value); } }
        public Color color { get; set; }
        public float opacity { get; set; }
        public float baseOpacity { get; set; }
        public float rotation { get; set; }
        public bool autoRotation { get; set; }
        public float baseSize { get; set; }
        public float sizeHeightRatio { get; set; }
        public float size { get; set; }
        public float softness { get; set; }
        private AnimationCurve _softnessCurve;
        public AnimationCurve softnessCurve { get { return _softnessCurve; } set { _softnessCurve = value; BakeSoftness(); } }
        private Texture2D _softnessTexture;
        public Texture2D softnessTexture { get { if (_softnessTexture == null) BakeSoftness(); return _softnessTexture; } }
        public float spacing { get; set; }
        public bool autoSpacing { get; set; }
        public Texture2D texture { get; set; }
        public string savePath { get; private set; }

        private const int SoftnessResolution = 1024;

        private void BakeSoftness()
        {
            if (softnessTexture == null)
            {
                _softnessTexture = new Texture2D(SoftnessResolution, 1, TextureFormat.RFloat, true);
            }
            Color[] softnessData = new Color[SoftnessResolution];
            for (int i = 0;i < SoftnessResolution;i++)
            {
                softnessData[i] = new Color(_softnessCurve.Evaluate(i / (float)SoftnessResolution), 1, 1, 1);
            }
            _softnessTexture.SetPixels(softnessData);
            _softnessTexture.Apply();
        }

        public BrushPreset()
        {
            baseSize = 50.0f;
            size = -1;
            opacity = -1;
            baseOpacity = 1.0f;
            spacing = 0.1f;
            softness = 1.0f;
            sizeHeightRatio = 1.0f;
            autoSpacing = true;
            _softnessCurve = new AnimationCurve();
            _softnessCurve.AddKey(0,1);
            _softnessCurve.AddKey(1,0);
            name = "Brush Preset";
        }

        private BrushPreset(BrushData data, string path)
        {
            size = -1;
            opacity = -1;
            name = data.name;
            iconPath = data.iconPath;
            baseOpacity = data.baseOpacity;
            rotation = data.rotation;
            autoRotation = data.autoRotation;
            baseSize = data.baseSize;
            sizeHeightRatio = data.sizeHeightRatio;
            softness = data.softness;
            shape = Enum.Parse<BrushShape>(data.shape);
            spacing = data.spacing;
            autoSpacing = data.autoSpacing;
            _softnessCurve = new AnimationCurve(data.softnessCurve);
            
            if (shape == BrushShape.Texture)
            {
                texture = new Texture2D(2,2);
                texture.LoadImage(File.ReadAllBytes(Path.Combine(path, "texture.png")));
            }
        }

        public void Save()
        {
            if (savePath == null || savePath.Length <= 0)
            {
                savePath = name;
                foreach (char c in Path.GetInvalidPathChars())
                    savePath = savePath.Replace(c.ToString(), "");
                var brushPath = GetBrushPath();

                while (Directory.Exists(Path.Combine(brushPath, savePath)))
                {
                    savePath += "1";
                }

                savePath = Path.Combine(brushPath, savePath);

                Presets.Add(this);
            }

            if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);


            BrushData brushData = new BrushData()
            {
                name = name,
                iconPath = iconPath,
                baseOpacity = baseOpacity,
                rotation = rotation,
                autoRotation = autoRotation,
                baseSize = baseSize,
                sizeHeightRatio = sizeHeightRatio,
                softness = softness,
                shape = Enum.GetName(typeof(BrushShape), shape),
                spacing = spacing,
                autoSpacing = autoSpacing,
                softnessCurve = _softnessCurve.keys
            };

            var dataPath = Path.Combine(savePath, "brushData.json");
            var json = JsonUtility.ToJson(brushData);
            File.WriteAllText(dataPath, json);
            if (shape == BrushShape.Texture)
            {
                var textureData = texture.EncodeToPNG();
                File.WriteAllBytes(dataPath, textureData);
            }

            PresetsChanged?.Invoke();
        }

        private static string GetBrushPath()
        {
            try
            {
                var brushPath = Path.Combine(Application.streamingAssetsPath + "/Brush Presets");
                if (!Directory.Exists(brushPath))
                {
                    Directory.CreateDirectory(brushPath);
                }
                return brushPath;
            }
            catch (Exception e)
            {
                throw new Exception("Unable to create or locate brush path. Likely permission issue.", e);
            }
        }


        public static BrushPreset LoadFromPath(string path)
        {
            try
            {
                var dataPath = Path.Combine(path, "brushData.json");
                string json = File.ReadAllText(dataPath);
                BrushData data = JsonUtility.FromJson<BrushData>(json);
                BrushPreset preset = new BrushPreset(data, path);
                preset.savePath = path;
                return preset;
            }
            catch (Exception e)
            {
                throw new Exception("",e);
            }
        }


        public static void LoadPresets()
        {
            Presets.Clear();
            try
            {
                var possibleBrushDirectories = Directory.GetDirectories(GetBrushPath());

                foreach (var possibleDirectory in possibleBrushDirectories)
                {
                    var brushPreset = LoadFromPath(possibleDirectory);
                    Presets.Add(brushPreset);
                }


            }
            catch (Exception e)
            {
                throw new Exception("Could not load presets.", e);
            }
            PresetsChanged?.Invoke();
        }

        private struct BrushData
        {
            public string name;
            public string iconPath;
            public float baseOpacity;
            public float rotation;
            public bool autoRotation;
            public float baseSize;
            public float sizeHeightRatio;
            public float softness;
            public Keyframe[] softnessCurve;
            public float spacing;
            public bool autoSpacing;
            public string shape;
        }
    }
    
}