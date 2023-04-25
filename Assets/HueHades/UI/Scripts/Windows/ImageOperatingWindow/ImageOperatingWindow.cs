using HueHades.Core;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using HueHades.Utilities;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using HueHades.Common;

namespace HueHades.UI
{
    public class ImageOperatingWindow : DockableWindow
    {
        private static Material ImageDisplayMaterial;
        private static Material SelectionDisplayMaterial;
        private static CameraUpdater CameraUpdateManager;
        private static int TexturePropertyID = Shader.PropertyToID("_BaseMap");
        private static int TilePropertyID = Shader.PropertyToID("_BaseMap_ST");
        private Image _windowDisplay;
        private const string ussOperatingWindow = "operating-window";
        private const string ussOperatingWindowImage = "operating-window-image";
        private ImageCanvas _imageCanvas;
        public ImageCanvas Canvas { get { return _imageCanvas; } }
        private GameObject _operatingWindowHierarchy;
        private GameObject _canvasObject;
        private GameObject _selectionObject;
        private MeshRenderer _canvasObjectRenderer;
        private MeshRenderer _selectionObjectRenderer;
        private Camera _camera;
        private RenderTexture _windowTexture;
        private MaterialPropertyBlock _canvasPropertyBlock;
        private MaterialPropertyBlock _selectionPropertyBlock;
        private bool _needsScale;
        private bool _needsUpdate;
        private int2 _operatingWindowSize;
        private bool _hasSize;
        private OperatingWindowFooter _footer;
        private float _zoomAmount;
        private Vector2 _canvasWorldScale;
        private bool _tileX;
        private bool _tileY;

        public ImageOperatingWindow(HueHadesWindow window, ImageCanvas imageCanvas) : base(window)
        {
            _zoomAmount = 1.0f;
            _windowDisplay = new Image();
            _windowDisplay.AddToClassList(ussOperatingWindowImage);
            _imageCanvas = imageCanvas;
            hierarchy.Add(_windowDisplay);
            AddToClassList(ussOperatingWindow);
            RegisterCallback<WheelEvent>(WheelCallback);
            RegisterCallback<MouseMoveEvent>(DragMouseCallback);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            _windowDisplay.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            WindowName = "Image.png";

            _footer = new OperatingWindowFooter(window, this);
            hierarchy.Add(_footer);

            imageCanvas.TileDisplayModeChanged += TileModeChanged;
        }

        private void TileModeChanged(CanvasTileMode mode)
        {
            _tileX = false;
            _tileY = false;
            switch (mode)
            {
                case CanvasTileMode.TileX:
                    _tileX = true;
                    break;
                case CanvasTileMode.TileY:
                    _tileY = true;
                    break;
                case CanvasTileMode.TileXY:
                    _tileX = true;
                    _tileY = true;
                    break;
            }

            UpdateCanvasTransform();
            RedrawCamera();
        }

        void OnRedraw()
        {
            if (!_hasSize) return;
            if (_needsUpdate)
            {
                if (_needsScale || _windowTexture == null)
                {
                    if (_windowTexture != null)
                    {
                        RenderTexture.ReleaseTemporary(_windowTexture);
                    }
                    _windowTexture = RenderTexture.GetTemporary(_operatingWindowSize.x, _operatingWindowSize.y);
                    _windowDisplay.image = _windowTexture;
                    _camera.targetTexture = _windowTexture;
                    _needsScale = false;
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

        private void OnCanvasDimensionsChanged()
        {
            UpdateCanvasTransform();
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
            GameObject.Destroy(_operatingWindowHierarchy);
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
            //create main hierarchy
            _operatingWindowHierarchy = new GameObject("OpWindow_" + WindowName);
            _operatingWindowHierarchy.SetActive(false);

            //create canvas
            _canvasObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _canvasObjectRenderer = _canvasObject.GetComponent<MeshRenderer>();
            _canvasObject.name = "Canvas";
            _canvasObject.transform.parent = _operatingWindowHierarchy.transform;

            //create selection display
            _selectionObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _selectionObjectRenderer = _selectionObject.GetComponent<MeshRenderer>();
            _selectionObject.name = "Selection";
            _selectionObject.transform.parent = _operatingWindowHierarchy.transform;

            //create camera
            var cameraObject = new GameObject("Camera");
            _camera = cameraObject.AddComponent<Camera>();
            _camera.orthographic = true;
            _camera.orthographicSize = 0.5f;
            _camera.nearClipPlane = 0.0f;
            _camera.farClipPlane = 6.0f;
            _camera.transform.position += new Vector3(0, 0, -3);
            _camera.transform.parent = _operatingWindowHierarchy.transform;

            //initialize canvas material
            if (ImageDisplayMaterial ==  null) {
                ImageDisplayMaterial = Resources.Load<Material>("Materials/ImageDisplay");
            }
            if (_canvasPropertyBlock == null)
            {
                _canvasPropertyBlock = new MaterialPropertyBlock();
            }
            _canvasObjectRenderer.sharedMaterial = ImageDisplayMaterial;
            _canvasObjectRenderer.GetPropertyBlock(_canvasPropertyBlock);
            _canvasPropertyBlock.SetTexture(TexturePropertyID, _imageCanvas.PreviewTexture);
            _canvasObjectRenderer.SetPropertyBlock(_canvasPropertyBlock);

            //initialize selection material
            if (SelectionDisplayMaterial == null)
            {
                SelectionDisplayMaterial = Resources.Load<Material>("Materials/SelectionDisplay");
            }
            if (_selectionPropertyBlock == null)
            {
                _selectionPropertyBlock = new MaterialPropertyBlock();
            }
            _selectionObjectRenderer.sharedMaterial = SelectionDisplayMaterial;
            _selectionObjectRenderer.GetPropertyBlock(_selectionPropertyBlock);
            _selectionPropertyBlock.SetTexture(TexturePropertyID, _imageCanvas.Selection.SelectionTexture);
            _selectionObjectRenderer.SetPropertyBlock(_selectionPropertyBlock);

            OnCanvasDimensionsChanged();

            if (CameraUpdateManager == null)
            {
                CameraUpdateManager = new GameObject("CameraUpdater").AddComponent<CameraUpdater>();
            }
            
            CameraUpdateManager.OnUpdate += OnRedraw;
            RedrawCamera();
        }

        private Vector2 GetPixelPosition(Vector2 pointerPosition)
        {
            var pos = GetWorldPosition(pointerPosition);
            var matrix = Matrix4x4.TRS(Vector3.zero, _canvasObject.transform.rotation, new Vector3(_canvasWorldScale.x, _canvasWorldScale.y, 1));
            pos = (Vector2)(matrix.inverse * ((Vector3)pos - _canvasObject.transform.position)) + new Vector2(0.5f,0.5f);
            pos.x *= _imageCanvas.Dimensions.x;
            pos.y *= _imageCanvas.Dimensions.y;
            return pos;
        }


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
            var tools = window.Tools;
            if (tools == null) return;
            var pressure = pointerDownEvent.pointerType == UnityEngine.UIElements.PointerType.pen ? pointerDownEvent.pressure : 1.0f;
            //TODO: insert layer when layer window is done
            //TODO: check pressure and tilt values to be correct
            tools.OnToolBeginUse(_imageCanvas, 0, GetPixelPosition(pointerDownEvent.position), pressure, pointerDownEvent.altitudeAngle);
            RedrawCamera();
        }

        private void OnPointerMove(PointerMoveEvent pointerMoveEvent) 
        {
            var tools = window.Tools;
            if (tools == null) return;
            var pressure = pointerMoveEvent.pointerType == UnityEngine.UIElements.PointerType.pen ? pointerMoveEvent.pressure : 1.0f;
            tools.OnToolUseUpdate(GetPixelPosition(pointerMoveEvent.position), pressure, pointerMoveEvent.altitudeAngle);
            RedrawCamera();
        }


        private void OnPointerUp(PointerUpEvent pointerUpEvent)
        {
            this.ReleasePointer(pointerUpEvent.pointerId);
            var tools = window.Tools;
            if (tools == null) return;
            var pressure = pointerUpEvent.pointerType == UnityEngine.UIElements.PointerType.pen ? pointerUpEvent.pressure : 1.0f;
            tools.OnToolEndUse(GetPixelPosition(pointerUpEvent.position), pressure, pointerUpEvent.altitudeAngle);
            RedrawCamera();
        }

        void UpdateCanvasTransform()
        {

            float width = _imageCanvas.Dimensions.x / (float)_imageCanvas.Dimensions.y;
            var windowBound = _windowDisplay.worldBound;
            float displayRatio = windowBound.width / windowBound.height;
            float height = 1;

            _canvasWorldScale = new Vector2(width * _zoomAmount, height * _zoomAmount);

            int tileAmountX = 1;
            int tileAmountY = 1;

            if (_tileX)
            {
                tileAmountX = Mathf.CeilToInt(3 * displayRatio / Mathf.Min(1.0f, width * _zoomAmount));
                if (tileAmountX % 2 == 0) tileAmountX++;
                width *= _zoomAmount * tileAmountX;
            }
            else
            {
                width *= _zoomAmount;
            }
            if (_tileY)
            {
                tileAmountY = Mathf.CeilToInt(3 * displayRatio / Mathf.Min(1.0f, height * _zoomAmount));
                if (tileAmountY % 2 == 0) tileAmountY++;
                height *= _zoomAmount * tileAmountY;
            }
            else
            {
                height *= _zoomAmount;
            }

            _canvasObjectRenderer.GetPropertyBlock(_canvasPropertyBlock);
            _canvasPropertyBlock.SetVector(TilePropertyID,new Vector4(tileAmountX,tileAmountY,0,0));
            _canvasObjectRenderer.SetPropertyBlock(_canvasPropertyBlock);

            _canvasObject.transform.localScale = new Vector3(width, height, 1);

            var canvasVectorX = _canvasWorldScale.x * _canvasObject.transform.right;
            var canvasVectorY = _canvasWorldScale.y * _canvasObject.transform.up;

            var projectX = Vector3.Project(_canvasObject.transform.position,canvasVectorX);
            var projectY = Vector3.Project(_canvasObject.transform.position,canvasVectorY);

            var signX = Vector3.Dot(projectX, canvasVectorX) > 0 ? 1 : -1;
            var signY = Vector3.Dot(projectY, canvasVectorY) > 0 ? 1 : -1;

            var xSteps = Mathf.Round(projectX.magnitude / canvasVectorX.magnitude);
            var ySteps = Mathf.Round(projectY.magnitude / canvasVectorY.magnitude);

            projectX -= _tileX ? canvasVectorX * xSteps * signX : Vector3.zero;
            projectY -= _tileY ? canvasVectorY * ySteps * signY : Vector3.zero;

            var correctedPosition = projectX + projectY;
            _canvasObject.transform.position = new Vector3(correctedPosition.x, correctedPosition.y, _canvasObject.transform.position.z);
        }


        void WheelCallback(WheelEvent e)
        {
            var _lastZoom = _zoomAmount;
            _zoomAmount = Mathf.Min(100.0f,Mathf.Max(0.01f,_zoomAmount - e.delta.y * _zoomAmount * 0.3f));

            var relativeWorldPos = (Vector3)GetWorldPosition(e.mousePosition) - _canvasObject.transform.position;

            var newRelativeWorldPos = relativeWorldPos * (_zoomAmount / _lastZoom);

            _canvasObject.transform.position += relativeWorldPos - newRelativeWorldPos;
            UpdateCanvasTransform();
            RedrawCamera();
        }

        void DragMouseCallback(MouseMoveEvent e)
        {

            var currentPosition = e.mousePosition;
            var lastPosition = GetWorldPosition(currentPosition - e.mouseDelta);
            currentPosition = GetWorldPosition(currentPosition);
            var deltaWorldPosition = currentPosition - lastPosition;
            if (e.ctrlKey && IsMouseButtonPressed(e.pressedButtons, MouseButton.Middle)) 
            {
                if (lastPosition != Vector2.zero && currentPosition != Vector2.zero) {

                   
                    _canvasObject.transform.RotateAround(GetWorldPosition(_windowDisplay.worldBound.center),Vector3.forward, -e.mouseDelta.x);
                }
            } 
            else
            {
                if (IsMouseButtonPressed(e.pressedButtons, MouseButton.Middle)) _canvasObject.transform.position += new Vector3(deltaWorldPosition.x, deltaWorldPosition.y, 0);
            }

            UpdateCanvasTransform();
            RedrawCamera();

        }

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