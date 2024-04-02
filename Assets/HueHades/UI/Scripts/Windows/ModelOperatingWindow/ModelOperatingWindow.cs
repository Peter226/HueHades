using HueHades.Utilities;
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class ModelOperatingWindow : DockableWindow
    {
        public static CameraUpdater CameraUpdateManager { get; private set; }
        private Image _windowDisplay;
        public Image WindowDisplay { get => _windowDisplay; }
        private const string ussOperatingWindow = "operating-window";
        private const string ussOperatingWindowImage = "operating-window-image";

        private GameObject _operatingWindowHierarchy;
        private Camera _camera;
        private RenderTexture _windowTexture;
        private bool _needsScale;
        private bool _needsUpdate;
        private bool _mouseOver;
        private int2 _operatingWindowSize;
        private bool _hasSize;
        private float _zoomAmount;

        private float _lastUseAngle;
        private bool _doubleDraw;

        private Vector3 _pivot = Vector3.zero;

        public ModelOperatingWindow(HueHadesWindow window) : base(window)
        {
            _zoomAmount = 1.0f;
            _windowDisplay = new Image();
            _windowDisplay.AddToClassList(ussOperatingWindowImage);
            hierarchy.Add(_windowDisplay);
            AddToClassList(ussOperatingWindow);
            RegisterCallback<WheelEvent>(WheelCallback);
            RegisterCallback<MouseMoveEvent>(DragMouseCallback);
            //RegisterCallback<PointerDownEvent>(OnPointerDown);
            //RegisterCallback<PointerMoveEvent>(OnPointerMove);
            //RegisterCallback<PointerUpEvent>(OnPointerUp);
            //RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);


            _windowDisplay.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            WindowName = "Model View";
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            _mouseOver = false;
        }

        private void OnPointerEnter(PointerEnterEvent evt)
        {
            _mouseOver = true;
        }

        private void OnCanvasNameChange(string fileName)
        {
            WindowName = fileName;
        }

        private void FilterModeChanged(FilterMode obj)
        {
            RedrawCamera();
        }


        void OnRedraw()
        {
            if (!_hasSize) return;
            if (_needsUpdate || _doubleDraw || _mouseOver)
            {
                if (_needsScale || _windowTexture == null)
                {
                    if (_windowTexture != null)
                    {
                        RenderTexture.ReleaseTemporary(_windowTexture);
                    }
                    _windowTexture = RenderTexture.GetTemporary(_operatingWindowSize.x, _operatingWindowSize.y, 24, RenderTextureFormat.DefaultHDR);
                    /*RenderTexture.active = _windowTexture;
                    GL.Clear(true,true, Color.clear);
                    RenderTexture.active = null;*/
                    _windowDisplay.image = _windowTexture;
                    _camera.targetTexture = _windowTexture;
                    _needsScale = false;
                }

                if (!_needsUpdate)
                {
                    _doubleDraw = false;
                }

                _needsUpdate = false;
                _operatingWindowHierarchy.SetActive(true);
                _camera.Render();
                _operatingWindowHierarchy.SetActive(false);

            }
        }

        private void RedrawCamera()
        {
            _needsUpdate = true;
        }


        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            _hasSize = true;
            _operatingWindowSize = new int2(Mathf.CeilToInt(Mathf.Max(1, evt.newRect.width)), Mathf.CeilToInt(Mathf.Max(1, evt.newRect.height)));
            _needsScale = true;
            RedrawCamera();
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                GameObject.Destroy(_operatingWindowHierarchy);
            }
            else
            {
                GameObject.DestroyImmediate(_operatingWindowHierarchy);
            }
#else
            GameObject.Destroy(_operatingWindowHierarchy);
#endif
            CameraUpdateManager.OnUpdate -= OnRedraw;
            if (_windowTexture != null)
            {
                RenderTexture.ReleaseTemporary(_windowTexture);
            }
            _windowTexture = null;
        }

        /// <summary>
        /// This method is called when the window is attached to a UI panel.
        /// The implementation creates all objects required for rendering the canvas environment,
        /// since we cannot rely on the UI to draw the canvas (need shader effects)
        /// </summary>
        /// <param name="evt"></param>
        private void OnAttachToPanel(AttachToPanelEvent evt)
        {

            var skyboxMaterial = Resources.Load<Material>("3D/SkyBoxes/Default");

            //create main hierarchy
            _operatingWindowHierarchy = new GameObject("OpWindow_" + WindowName);
            _operatingWindowHierarchy.SetActive(false);

            //create camera
            var cameraObject = new GameObject("Camera");
            _camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<Skybox>().material = skyboxMaterial;

            var _additionalCameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
            _additionalCameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            _additionalCameraData.antialiasingQuality = AntialiasingQuality.Medium;
            _additionalCameraData.renderPostProcessing = true;
            _additionalCameraData.SetRenderer(1);

            _camera.fieldOfView = 90f;
            _camera.nearClipPlane = 0.1f;
            _camera.farClipPlane = 1000.0f;
            _camera.allowHDR = true;

            _camera.clearFlags = CameraClearFlags.Skybox;
            
            _camera.transform.position += new Vector3(0, 0, -3);
            _camera.transform.parent = _operatingWindowHierarchy.transform;

            //create light
            var light = new GameObject("Directional Light");
            light.transform.Rotate(new Vector3(30,-60,0));
            var lightComponent = light.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            light.transform.parent = _operatingWindowHierarchy.transform;
            lightComponent.intensity = 2.0f;

            var testObject = new GameObject();
            testObject.transform.parent = _operatingWindowHierarchy.transform;

            var gltf = testObject.AddComponent<GLTFast.GltfAsset>();
            gltf.Load("https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/DamagedHelmet/glTF/DamagedHelmet.gltf");

            if (CameraUpdateManager == null)
            {
                CameraUpdateManager = new GameObject("CameraUpdater").AddComponent<CameraUpdater>();
            }

            CameraUpdateManager.OnUpdate += OnRedraw;
            RedrawCamera();
            _doubleDraw = true;
        }


        /// <summary>
        /// Converts screen position into world position
        /// </summary>
        /// <param name="pointerPosition"></param>
        /// <returns></returns>
        private Vector2 GetWorldPosition(Vector2 pointerPosition)
        {
            var pos = _windowDisplay.contentContainer.WorldToLocal(pointerPosition);
            pos = _camera.ScreenToWorldPoint(pos);
            pos.y *= -1.0f;
            return pos;
        }



        private void OnPointerDown(PointerDownEvent pointerDownEvent)
        {
            this.CapturePointer(pointerDownEvent.pointerId);
            if (IsMouseButtonPressed(pointerDownEvent.pressedButtons, MouseButton.Middle)) return;
        }
        

        /// <summary>
        /// Resize canvas when scrolled
        /// </summary>
        /// <param name="e"></param>
        void WheelCallback(WheelEvent e)
        {
            _camera.transform.position += (_camera.transform.position - _pivot) * e.delta.y * 20.0f;
            RedrawCamera();
        }

        /// <summary>
        /// Rotate canvas
        /// </summary>
        /// <param name="e"></param>
        void DragMouseCallback(MouseMoveEvent e)
        {
            if (IsMouseButtonPressed(e.pressedButtons, MouseButton.Middle))
            {
                if (e.shiftKey)
                {
                    var speed = Vector3.Distance(_pivot, _camera.transform.position) * 0.01f;

                    var moveDelta = _camera.transform.right * -e.mouseDelta.x + _camera.transform.up * e.mouseDelta.y;
                    moveDelta *= speed;

                    _camera.transform.position += moveDelta;
                    _pivot += moveDelta;
                }
                else
                {
                    _camera.transform.RotateAround(_pivot, _camera.transform.right, e.mouseDelta.y);
                    _camera.transform.RotateAround(_pivot, Vector3.up, e.mouseDelta.x);
                }
            }



            
            RedrawCamera();
        }

        /// <summary>
        /// Helper function to check if specific mouse button is pressed from flags
        /// </summary>
        /// <param name="pressedButtons"></param>
        /// <param name="mouseButton"></param>
        /// <returns></returns>
        private bool IsMouseButtonPressed(int pressedButtons, MouseButton mouseButton)
        {
            if ((pressedButtons & (int)mouseButton) != 0) return true;
            return false;
        }


        [Flags]
        private enum MouseButton
        {
            Left = 1,
            Right = 2,
            Middle = 4
        }

    }
}
