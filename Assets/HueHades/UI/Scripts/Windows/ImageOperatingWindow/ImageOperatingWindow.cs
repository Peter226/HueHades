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
        public static CameraUpdater CameraUpdateManager { get; private set; }
        private static int TexturePropertyID = Shader.PropertyToID("_BaseMap");
        private static int LineWidthPropertyID = Shader.PropertyToID("_LineWidth");
        private static int TilePropertyID = Shader.PropertyToID("_BaseMap_ST");
        private Image _windowDisplay;
        public Image WindowDisplay { get => _windowDisplay; }
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
        private bool _mouseOver;
        private int2 _operatingWindowSize;
        private bool _hasSize;
        private OperatingWindowFooter _footer;
        private float _zoomAmount;
        private Vector2 _canvasWorldScale;
        private bool _tileX;
        private bool _tileY;

        private float _lastUsePressure;
        private Vector2 _lastUsePosition;
        private float _lastUseAngle;
        private bool _doubleDraw;

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
            RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);


            _windowDisplay.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            WindowName = "Image.png";

            _footer = new OperatingWindowFooter(window, this);
            hierarchy.Add(_footer);

            imageCanvas.TileDisplayModeChanged += TileModeChanged;
            imageCanvas.PreviewFilterModeChanged += FilterModeChanged;

            imageCanvas.PreviewChanged += RedrawCamera;
            imageCanvas.CanvasDimensionsChanged += OnCanvasDimensionsChanged;
            imageCanvas.FileNameChanged += OnCanvasNameChange;

            Canvas.DestroyedCanvas += UnDock;
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
            if (Canvas.IsDirty)
            {
                _needsUpdate = true;
                Canvas.RenderPreview();
            }
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

        private void OnCanvasDimensionsChanged(int2 dimensions)
        {
            if (_canvasPropertyBlock == null)
            {
                _canvasPropertyBlock = new MaterialPropertyBlock();
            }
            _canvasObjectRenderer.sharedMaterial = ImageDisplayMaterial;
            _selectionObjectRenderer.sharedMaterial = SelectionDisplayMaterial;
            _canvasObjectRenderer.GetPropertyBlock(_canvasPropertyBlock);
            _canvasPropertyBlock.SetTexture(TexturePropertyID, _imageCanvas.PreviewTexture.texture);
            _canvasObjectRenderer.SetPropertyBlock(_canvasPropertyBlock);

            _selectionObjectRenderer.GetPropertyBlock(_selectionPropertyBlock);
            _selectionPropertyBlock.SetTexture(TexturePropertyID, _imageCanvas.Selection.SelectionTexture.texture);
            _selectionObjectRenderer.SetPropertyBlock(_selectionPropertyBlock);


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
            window.ActiveOperatingWindow = this;

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
            _selectionObject.transform.position += new Vector3(0,0,-0.5f);

            //create camera
            var cameraObject = new GameObject("Camera");
            _camera = cameraObject.AddComponent<Camera>();
            _camera.orthographic = true;
            _camera.orthographicSize = 0.5f;
            _camera.nearClipPlane = 0.0f;
            _camera.farClipPlane = 6.0f;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = Color.clear;
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
            _canvasPropertyBlock.SetTexture(TexturePropertyID, _imageCanvas.PreviewTexture.texture);
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
            _selectionPropertyBlock.SetTexture(TexturePropertyID, _imageCanvas.Selection.SelectionTexture.texture);
            _selectionObjectRenderer.SetPropertyBlock(_selectionPropertyBlock);

            OnCanvasDimensionsChanged(Canvas.Dimensions);

            if (CameraUpdateManager == null)
            {
                CameraUpdateManager = new GameObject("CameraUpdater").AddComponent<CameraUpdater>();
            }
            
            CameraUpdateManager.OnUpdate += OnRedraw;
            RedrawCamera();
            _doubleDraw = true;
        }

        /// <summary>
        /// Converts screen position into pixel position on the canvas texture
        /// </summary>
        /// <param name="pointerPosition"></param>
        /// <returns></returns>
        private Vector2 GetPixelPosition(Vector2 pointerPosition)
        {
            var pos = GetWorldPosition(pointerPosition);
            var matrix = Matrix4x4.TRS(Vector3.zero, _canvasObject.transform.rotation, new Vector3(_canvasWorldScale.x, _canvasWorldScale.y, 1));
            pos = (Vector2)(matrix.inverse * ((Vector3)pos - _canvasObject.transform.position)) + new Vector2(0.5f,0.5f);
            pos.x *= _imageCanvas.Dimensions.x;
            pos.y *= _imageCanvas.Dimensions.y;
            pos.x -= 0.5f; //correct position to pixel's center
            pos.y -= 0.5f;
            return pos;
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

        /// <summary>
        /// Converts a canvas-space position to screen position
        /// </summary>
        /// <param name="canvasPosition"></param>
        /// <returns></returns>
        public Vector2 GetScreenPosition(Vector2 canvasPosition)
        {
            var pos = new Vector3(canvasPosition.x, canvasPosition.y);
            pos.x += 0.5f;
            pos.y += 0.5f;
            pos.x /= _imageCanvas.Dimensions.x;
            pos.y /= _imageCanvas.Dimensions.y;
            pos -= new Vector3(0.5f, 0.5f, 0.0f);

            var matrix = Matrix4x4.TRS(Vector3.zero, _canvasObject.transform.rotation, new Vector3(_canvasWorldScale.x, _canvasWorldScale.y, 1));

            pos = matrix * pos;
            pos += _canvasObject.transform.position;

            pos.y *= -1.0f;

            pos = _camera.WorldToScreenPoint(pos);
            
            //pos = _windowDisplay.contentContainer.LocalToWorld(pos);

            return new Vector2(pos.x, pos.y);
        }

        private void OnPointerDown(PointerDownEvent pointerDownEvent)
        {
            if (_imageCanvas.ActiveLayer is not ImageLayer || _imageCanvas.ActiveLayer == null) return;
            window.ActiveOperatingWindow = this;
            this.CapturePointer(pointerDownEvent.pointerId);
            if (IsMouseButtonPressed(pointerDownEvent.pressedButtons, MouseButton.Middle)) return;
            var tools = window.Tools;
            if (tools == null) return;
            var pressure = pointerDownEvent.pointerType == UnityEngine.UIElements.PointerType.pen ? pointerDownEvent.pressure : 1.0f;
            //TODO: insert layer when layer window is done
            //TODO: check pressure and tilt values to be correct
            tools.OnToolBeginUse(_imageCanvas, _imageCanvas.ActiveLayer.GlobalIndex, GetPixelPosition(pointerDownEvent.position), pressure, pointerDownEvent.altitudeAngle, pointerDownEvent);
            RedrawCamera();
            _lastUsePosition = pointerDownEvent.position;
            _lastUsePressure = pressure;
            _lastUseAngle = pointerDownEvent.altitudeAngle;
            pointerDownEvent.StopImmediatePropagation();
        }

        private void OnPointerMove(PointerMoveEvent pointerMoveEvent) 
        {
            var tools = window.Tools;
            if (tools == null) return;
            var pressure = pointerMoveEvent.pointerType == UnityEngine.UIElements.PointerType.pen ? pointerMoveEvent.pressure : 1.0f;
            tools.OnToolUseUpdate(GetPixelPosition(pointerMoveEvent.position), pressure, pointerMoveEvent.altitudeAngle);
            RedrawCamera();
            _lastUsePosition = pointerMoveEvent.position;
            _lastUsePressure = pressure;
            _lastUseAngle = pointerMoveEvent.altitudeAngle;
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

        private void OnPointerCaptureOut(PointerCaptureOutEvent pointerCaptureOutEvent)
        {
            var tools = window.Tools;
            if (tools == null) return;
            tools.OnToolEndUse(GetPixelPosition(_lastUsePosition), _lastUsePressure, _lastUseAngle);
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

            var materialTileProperty = new Vector4(tileAmountX, tileAmountY, 0, 0);

            _canvasObjectRenderer.GetPropertyBlock(_canvasPropertyBlock);
            _canvasPropertyBlock.SetVector(TilePropertyID, materialTileProperty);
            _canvasObjectRenderer.SetPropertyBlock(_canvasPropertyBlock);

            _selectionObjectRenderer.GetPropertyBlock(_selectionPropertyBlock);
            _selectionPropertyBlock.SetVector(TilePropertyID, materialTileProperty);

            Vector2 lineWidth = new Vector2(1 / _zoomAmount / windowBound.height * (_imageCanvas.Dimensions.y / (float)_imageCanvas.Dimensions.x), 1 / _zoomAmount / windowBound.height);
            lineWidth *= 2.0f;

            _selectionPropertyBlock.SetVector(LineWidthPropertyID, lineWidth);
            _selectionObjectRenderer.SetPropertyBlock(_selectionPropertyBlock);

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
            _selectionObject.transform.position = _canvasObject.transform.position + new Vector3(0,0,-0.5f);
            _selectionObject.transform.rotation = _canvasObject.transform.rotation;
            _selectionObject.transform.localScale = _canvasObject.transform.localScale;
        }


        /// <summary>
        /// Resize canvas when scrolled
        /// </summary>
        /// <param name="e"></param>
        void WheelCallback(WheelEvent e)
        {
            window.ActiveOperatingWindow = this;

            //last zoom
            var _lastZoom = _zoomAmount;
            //new zoom
            _zoomAmount = Mathf.Min(100.0f,Mathf.Max(0.01f,_zoomAmount - e.delta.y * _zoomAmount * 1.0f));
            //relative position the the mouse cursor
            var relativeWorldPos = (Vector3)GetWorldPosition(e.mousePosition) - _canvasObject.transform.position;
            //zoomed relative position to the mouse cursor
            var newRelativeWorldPos = relativeWorldPos * (_zoomAmount / _lastZoom);
            //apply correction zoom
            _canvasObject.transform.position += relativeWorldPos - newRelativeWorldPos;
            UpdateCanvasTransform();
            RedrawCamera();
        }

        /// <summary>
        /// Rotate canvas
        /// </summary>
        /// <param name="e"></param>
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