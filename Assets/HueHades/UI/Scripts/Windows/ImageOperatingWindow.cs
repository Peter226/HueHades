using HueHades.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace HueHades.UI
{
    public class ImageOperatingWindow : DockableWindow
    {
        private static Material ImageDisplayMaterial;
        private static int TexturePropertyID = Shader.PropertyToID("_BaseMap");
        private Image _windowDisplay;
        private const string ussOperatingWindow = "operating-window";
        private const string ussOperatingWindowImage = "operating-window-image";
        private ImageCanvas _imageCanvas;
        private GameObject _operatingWindowHierarchy;
        private GameObject _canvasObject;
        private GameObject _selectionObject;
        private MeshRenderer _canvasObjectRenderer;
        private Camera _camera;
        private RenderTexture _windowTexture;
        private MaterialPropertyBlock _materialPropertyBlock;

        public ImageOperatingWindow(HueHadesWindow window, ImageCanvas imageCanvas) : base(window)
        {
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
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void RedrawCamera()
        {
            _operatingWindowHierarchy.SetActive(true);
            _camera.Render();
            _operatingWindowHierarchy.SetActive(false);
        }

        private void OnCanvasDimensionsChanged()
        {
            _canvasObject.transform.localScale = new Vector3(_imageCanvas.Dimensions.x / (float)_imageCanvas.Dimensions.y,1,1);
        }


        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (_windowTexture != null)
            {
                RenderTexture.ReleaseTemporary(_windowTexture);
            }
            _windowTexture = RenderTexture.GetTemporary(Mathf.CeilToInt(Mathf.Max(1, evt.newRect.width)), Mathf.CeilToInt(Mathf.Max(1,evt.newRect.height)));
            _windowDisplay.image = _windowTexture;
            _camera.targetTexture = _windowTexture;
            RedrawCamera();
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            GameObject.Destroy(_operatingWindowHierarchy);
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
            _operatingWindowHierarchy = new GameObject("OpWindow_" + GetWindowName());
            _operatingWindowHierarchy.SetActive(false);

            //create canvas
            _canvasObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _canvasObjectRenderer = _canvasObject.GetComponent<MeshRenderer>();
            _canvasObject.name = "Canvas";
            _canvasObject.transform.parent = _operatingWindowHierarchy.transform;

            //create camera
            var cameraObject = new GameObject("Camera");
            _camera = cameraObject.AddComponent<Camera>();
            _camera.orthographic = true;
            _camera.orthographicSize = 0.5f;
            _camera.nearClipPlane = 0.0f;
            _camera.farClipPlane = 6.0f;
            _camera.transform.position += new Vector3(0, 0, -3);
            _camera.transform.parent = _operatingWindowHierarchy.transform;

            //initialize camera material
            if (ImageDisplayMaterial ==  null) {
                ImageDisplayMaterial = Resources.Load<Material>("Materials/ImageDisplay");
            }
            if (_materialPropertyBlock == null)
            {
                _materialPropertyBlock = new MaterialPropertyBlock();
            }
            _canvasObjectRenderer.sharedMaterial = ImageDisplayMaterial;
            _canvasObjectRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetTexture(TexturePropertyID, _imageCanvas.PreviewTexture);
            _canvasObjectRenderer.SetPropertyBlock(_materialPropertyBlock);

            //signal redraw
            OnCanvasDimensionsChanged();
        }

        private Vector2 GetPixelPosition(Vector2 pointerPosition)
        {
            var pos = GetWorldPosition(pointerPosition);
            Debug.Log(pos);
            pos = (Vector2)(_canvasObject.transform.worldToLocalMatrix * ((Vector3)pos - _canvasObject.transform.position)) + new Vector2(0.5f,0.5f);
            Debug.Log(pos);
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
            var tools = window.ToolsWindow;
            if (tools == null) return;
            var pressure = pointerDownEvent.pointerType == UnityEngine.UIElements.PointerType.pen ? pointerDownEvent.pressure : 1.0f;
            //TODO: insert layer when layer window is done
            //TODO: check pressure and tilt values to be correct
            tools.OnToolBeginUse(_imageCanvas, 0, GetPixelPosition(pointerDownEvent.position), pressure, pointerDownEvent.altitudeAngle);
            RedrawCamera();
        }

        private void OnPointerMove(PointerMoveEvent pointerMoveEvent) {
            
            var tools = window.ToolsWindow;
            if (tools == null) return;
            var pressure = pointerMoveEvent.pointerType == UnityEngine.UIElements.PointerType.pen ? pointerMoveEvent.pressure : 1.0f;
            tools.OnToolUseUpdate(GetPixelPosition(pointerMoveEvent.position), pressure, pointerMoveEvent.altitudeAngle);
            RedrawCamera();
        }


        private void OnPointerUp(PointerUpEvent pointerUpEvent)
        {
            this.ReleasePointer(pointerUpEvent.pointerId);
            var tools = window.ToolsWindow;
            if (tools == null) return;
            var pressure = pointerUpEvent.pointerType == UnityEngine.UIElements.PointerType.pen ? pointerUpEvent.pressure : 1.0f;
            tools.OnToolEndUse(GetPixelPosition(pointerUpEvent.position), pressure, pointerUpEvent.altitudeAngle);
            RedrawCamera();
        }

        public override string GetWindowName()
        {
            return "Image.png";
        }



        void WheelCallback(WheelEvent e)
        {
            _canvasObject.transform.localScale *= 1 - (e.delta.y * 0.1f * _canvasObject.transform.localScale.z);
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

                   
                    _canvasObject.transform.rotation *= Quaternion.Euler(0, 0, -e.mouseDelta.x);
                }
            } 
            else
            {
                if (IsMouseButtonPressed(e.pressedButtons, MouseButton.Middle)) _canvasObject.transform.position += new Vector3(deltaWorldPosition.x, deltaWorldPosition.y, 0);
            }
            RedrawCamera();
        }


        void OnImageChange()
        {

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