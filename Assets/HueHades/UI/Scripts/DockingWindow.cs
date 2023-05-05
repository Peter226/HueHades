using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class DockingWindow : HueHadesElement
    {
        private const string ussHeaderBar = "docking-window-header-bar";
        private const string ussDockingWindow = "docking-window";
        private const string ussSplitDockingWindow = "docking-window-split";
        private const string ussDockingWindowContent = "docking-window-content";
        private const string ussSplitDockHierarchy = "docking-split-hierarchy";

        private DockingWindow _dockedIn;
        private HeaderBar _headerBar;
        private bool _isFixedWindow;
        private bool _isFreeWindow;
        private Dictionary<DockableWindow, HeaderElement> _dockedWindows = new Dictionary<DockableWindow, HeaderElement>();
        private HueHadesElement _windowContainer;
        private DockableWindow _selectedWindow;
        private HueHadesElement _splitHierarchy;
        private DockingWindow _splitA;
        private FlexDirection _splitDirection;
        private DockingWindow _splitB;
        private bool _isDockSplit;
        private DockHandle _dockHandle;

        /// <summary>
        /// Handle of the docking window
        /// </summary>
        public DockHandle Handle { get { return _dockHandle; } private set { _dockHandle = value; } }

        private float _lastWidth;
        private float _lastHeight;

        private VisualElement _parentElement;

        /// <summary>
        /// Create a new docking window
        /// </summary>
        /// <param name="window">main window of our application</param>
        /// <param name="isFixedWindow">is this window the fixed docking window of our application</param>
        /// <param name="dockedIn">possible parent docking window</param>
        public DockingWindow(HueHadesWindow window, bool isFixedWindow = false, DockingWindow dockedIn = null, bool isFreeWindow = false) : base(window)
        {
            Handle = new DockHandle(this);
            _headerBar = new HeaderBar(window, this);
            hierarchy.Add(_headerBar);
            _windowContainer = new HueHadesElement(window);
            _windowContainer.AddToClassList(ussDockingWindowContent);
            hierarchy.Add(_windowContainer);
            _splitHierarchy = new HueHadesElement(window);
            _splitHierarchy.AddToClassList(ussSplitDockHierarchy);
            _splitHierarchy.style.display = DisplayStyle.None;
            hierarchy.Add(_splitHierarchy);
            _isFixedWindow = isFixedWindow;
            AddToClassList(ussDockingWindow);
            _dockedIn = dockedIn;
            this.AddManipulator(new DockResizeManipulator(this));
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            _isFreeWindow = isFreeWindow;
        }

        /// <summary>
        /// Called when attached to panel, used for registering geometry events for rescaling child windows
        /// </summary>
        /// <param name="evt"></param>
        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (parent == null) return;
            _parentElement = parent;
            if (_isFixedWindow)
            {
                _parentElement.RegisterCallback<GeometryChangedEvent>(OnParentResized);
                _lastWidth = _parentElement.worldBound.width;
                _lastHeight = _parentElement.worldBound.height;
            }
        }

        /// <summary>
        /// Called when detached from panel, used for unregistering geometry events
        /// </summary>
        /// <param name="evt"></param>
        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            if (_parentElement == null) return;
            if (_isFixedWindow)
            {
                _parentElement.UnregisterCallback<GeometryChangedEvent>(OnParentResized);
            }

        }

        /// <summary>
        /// Resizes the dock to the specified size
        /// </summary>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        private void ResizeDock(float width, float height)
        {
            float currentWidth = style.width.value.value;
            float currentHeight = style.height.value.value;
            ResizeDockRatio(width / currentWidth, height / currentHeight);
        }

        /// <summary>
        /// Resizes the dock to the specific ratio compared to the current size
        /// </summary>
        /// <param name="widthRatio">Width ratio comparing the new size to the current one</param>
        /// <param name="heightRatio">Height ratio comparing the new size to the current one</param>
        private void ResizeDockRatio(float widthRatio, float heightRatio)
        {
            style.width = style.width.value.value * widthRatio;
            style.height = style.height.value.value * heightRatio;
            if (_isDockSplit)
            {
                _splitA.ResizeDockRatio(widthRatio, heightRatio);
                _splitB.ResizeDockRatio(widthRatio, heightRatio);
            }
        }

        /// <summary>
        /// Called when the parent window is resized of a fixed docking window, used for rescaling children.
        /// </summary>
        /// <param name="evt"></param>
        private void OnParentResized(GeometryChangedEvent evt)
        {
            if (_parentElement == null) return;
            if (!_isFixedWindow) return;

            float newWidth = _parentElement.worldBound.width;
            float newHeight = _parentElement.worldBound.height;

            Debug.Log(newWidth);


            if (_isDockSplit)
            {
                float deltaWidthRatio = 1;
                float deltaHeightRatio = 1;
                if (_lastWidth != 0)
                {
                    deltaWidthRatio = newWidth / _lastWidth;
                }
                if (_lastHeight != 0)
                {
                    deltaHeightRatio = newHeight / _lastHeight;
                }
                _splitA.ResizeDockRatio(deltaWidthRatio, deltaHeightRatio);
                _splitB.ResizeDockRatio(deltaWidthRatio, deltaHeightRatio);
            }

            _lastWidth = newWidth;
            _lastHeight = newHeight;
        }

        /// <summary>
        /// Used for docking DockableWindows using the specified parameter settings
        /// </summary>
        /// <param name="dockableWindow">Window to dock</param>
        /// <param name="dockType">DockType referring to the preferred position and layout arrangement of the window</param>
        /// <param name="headerIndex">Insert the window's header button into a specified position in the header bar</param>
        /// <returns></returns>
        public DockHandle DockWindow(DockableWindow dockableWindow, DockType dockType = DockType.Header, int headerIndex = -1)
        {
            if (_isDockSplit)
            {
                Vector2 sizeA;
                Vector2 sizeB;
                
                sizeA.x = _splitA.style.width.value.value;
                sizeA.y = _splitA.style.height.value.value;
                sizeB.x = _splitB.style.width.value.value;
                sizeB.y = _splitB.style.height.value.value;

                if (sizeA.x * sizeA.y > sizeB.x * sizeB.y)
                {
                    return _splitA.DockWindow(dockableWindow, dockType, headerIndex);
                }
                else
                {
                    return _splitB.DockWindow(dockableWindow, dockType, headerIndex);
                }
            }
            switch (dockType)
            {
                case DockType.Header:
                    HeaderElement header = new HeaderElement(window, dockableWindow, this);
                    header.AddManipulator(new HeaderDragManipulator(header));
                    if (headerIndex < 0)
                    {
                        headerIndex = _headerBar.Container.hierarchy.childCount;
                    }
                    _headerBar.Container.hierarchy.Insert(headerIndex, header);
                    _dockedWindows.Add(dockableWindow, header);
                    SelectWindow(dockableWindow);

                    if (_dockedIn == null && !_isFixedWindow)
                    {
                        var defaultWindowSize = dockableWindow.DefaultSize;
                        style.position = Position.Absolute;
                        style.width = defaultWindowSize.x;
                        style.height = defaultWindowSize.y;
                        style.left = (window.worldBound.width - defaultWindowSize.x) * 0.5f;
                        style.top = (window.worldBound.height - defaultWindowSize.y) * 0.5f;
                    }

                    break;
                case DockType.Free:
                    DockingWindow freeDocker = new DockingWindow(window, isFreeWindow: true);
                    window.FreeDockElement.Add(freeDocker);
                    return dockableWindow.Dock(freeDocker.Handle);
                default:
                    float newSize;
                    if (dockType == DockType.Bottom || dockType == DockType.Top)
                    {
                        newSize = dockableWindow.DefaultSize.y;
                    }
                    else
                    {
                        newSize = dockableWindow.DefaultSize.x;
                    }
                    SplitDock(dockType, out DockingWindow oldHierarchy, out DockingWindow newHierarchy, newSize);
                    return dockableWindow.Dock(newHierarchy.Handle);
            }
            return Handle;
        }

        /// <summary>
        /// Used for splitting up the dock into two pieces when docking a window. Only called when DockType is Right, Left, Top or Bottom
        /// </summary>
        /// <param name="dockType">Position of the new window</param>
        /// <param name="oldHierarchy">The window used for docking the current hierarchy in the split</param>
        /// <param name="newHierarchy">The window used for docking the new hierarchy in the split</param>
        /// <param name="newSize">The size of the new window, depending on the DockType</param>
        private void SplitDock(DockType dockType, out DockingWindow oldHierarchy, out DockingWindow newHierarchy, float newSize = -1.0f)
        {
            //Activate split display
            _splitHierarchy.style.display = DisplayStyle.Flex;
            //Calculate bounds of the window
            Rect bounds = this.worldBound;
            if (float.IsNaN(bounds.width) || float.IsNaN(bounds.height))
            {
                bounds.width = this.style.width.value.value;
                bounds.height = this.style.height.value.value;
            }
            AddToClassList(ussSplitDockingWindow);

            _isDockSplit = true;
            oldHierarchy = new DockingWindow(window, dockedIn: this);
            newHierarchy = new DockingWindow(window, dockedIn: this);

            //Assign splits in hierarchy
            if (dockType == DockType.Right || dockType == DockType.Bottom) {
                _splitHierarchy.Add(oldHierarchy);
                _splitHierarchy.Add(newHierarchy);

                _splitA = oldHierarchy;
                _splitB = newHierarchy;
            }
            else
            {
                _splitHierarchy.Add(newHierarchy);
                _splitHierarchy.Add(oldHierarchy);

                _splitA = newHierarchy;
                _splitB = oldHierarchy;
            }

            //Scale new windows and set layout options
            if (dockType == DockType.Left || dockType == DockType.Right)
            {
                _splitHierarchy.style.flexDirection = FlexDirection.Row;
                _splitDirection = FlexDirection.Row;
                float newWidth = bounds.width / 3.0f;
                if (newSize > 0)
                {
                    newWidth = Mathf.Min(newSize, newWidth);
                }
                float oldWidth = bounds.width - newWidth;

                newHierarchy.style.width = newWidth;
                oldHierarchy.style.width = oldWidth;
                newHierarchy.style.height = bounds.height;
                oldHierarchy.style.height = bounds.height;
            }
            else
            {
                _splitHierarchy.style.flexDirection = FlexDirection.Column;
                _splitDirection = FlexDirection.Column;
                float newHeight = bounds.height / 3.0f;
                if (newSize > 0)
                {
                    newHeight = Mathf.Min(newSize, newHeight);
                }
                float oldWidth = bounds.height - newHeight;

                newHierarchy.style.height = newHeight;
                oldHierarchy.style.height = newHeight * 2;
                newHierarchy.style.width = bounds.width;
                oldHierarchy.style.width = bounds.width;
            }

            _windowContainer.style.display = DisplayStyle.None;
            _headerBar.style.display = DisplayStyle.None;
            _splitHierarchy.style.display = DisplayStyle.Flex;

            //Dock the old hierarchy
            var dockedWindows = _dockedWindows.Keys.ToList();
            for (int i = 0; i < dockedWindows.Count; i++)
            {
                var dockedWindow = dockedWindows[i];
                dockedWindow.Dock(oldHierarchy.Handle);
            }

            _splitA.name = nameof(DockingWindow)+"A";
            _splitB.name = nameof(DockingWindow)+"B";

        }

        /// <summary>
        /// Merges the dock if split, used when one of the splits becomes empty
        /// </summary>
        void MergeDock()
        {
            if (!_isDockSplit) return;
            RemoveFromClassList(ussSplitDockingWindow);
            _isDockSplit = false;
            
            var dockedWindowsA = _splitA._dockedWindows.Keys.ToList();
            var dockedWindowsB = _splitB._dockedWindows.Keys.ToList();

            var oldSplitA = _splitA;
            var oldSplitB = _splitB;

            _splitHierarchy.Remove(_splitA);
            _splitHierarchy.Remove(_splitB);

            //Check if one of the splits is split
            if (_splitA._isDockSplit || _splitB._isDockSplit)
            {
                _isDockSplit = true;
                _windowContainer.style.display = DisplayStyle.None;
                _headerBar.style.display = DisplayStyle.None;
                _splitHierarchy.style.display = DisplayStyle.Flex;

                //check which one is split and scale it
                if (_splitA._isDockSplit)
                {
                    var splitGrowWidthRatio = style.width.value.value / _splitA.style.width.value.value;
                    var splitGrowHeightRatio = style.height.value.value / _splitA.style.height.value.value;
                    _splitDirection = _splitA._splitDirection;
                    _splitHierarchy.style.flexDirection = _splitA._splitHierarchy.style.flexDirection;
                    _splitB = _splitA._splitB;
                    _splitA = _splitA._splitA;
                    _splitA.ResizeDockRatio(splitGrowWidthRatio, splitGrowHeightRatio);
                    _splitB.ResizeDockRatio(splitGrowWidthRatio, splitGrowHeightRatio);
                }
                else
                {
                    if (_splitB._isDockSplit)
                    {
                        var splitGrowWidthRatio = style.width.value.value / _splitB.style.width.value.value;
                        var splitGrowHeightRatio = style.height.value.value / _splitB.style.height.value.value;
                        _splitDirection = _splitB._splitDirection;
                        _splitHierarchy.style.flexDirection = _splitB._splitHierarchy.style.flexDirection;
                        _splitA = _splitB._splitA;
                        _splitB = _splitB._splitB;
                        _splitA.ResizeDockRatio(splitGrowWidthRatio, splitGrowHeightRatio);
                        _splitB.ResizeDockRatio(splitGrowWidthRatio, splitGrowHeightRatio);
                    }
                }
                
                //assign new splits
                _splitHierarchy.Add(_splitA);
                _splitHierarchy.Add(_splitB);

                _splitA._dockedIn = this;
                _splitB._dockedIn = this;
            }
            //if not, dock windows into self
            else
            {
                for (int i = 0; i < dockedWindowsA.Count; i++)
                {
                    var dockedWindow = dockedWindowsA[i];
                    dockedWindow.Dock(Handle);
                }
                for (int i = 0; i < dockedWindowsB.Count; i++)
                {
                    var dockedWindow = dockedWindowsB[i];
                    dockedWindow.Dock(Handle);
                }

                _windowContainer.style.display = DisplayStyle.Flex;
                _headerBar.style.display = DisplayStyle.Flex;
                _splitHierarchy.style.display = DisplayStyle.None;
            }
            
            //assign new references in unused docker's handles
            oldSplitA.Handle.SetReference(this);
            oldSplitB.Handle.SetReference(this);
        } 
            
        /// <summary>
        /// Removes window from hierarchy, destroys docker if necessary
        /// </summary>
        /// <param name="dockableWindow">Window to undock</param>
        public void UnDockWindow(DockableWindow dockableWindow)
        {
            //Try to remove window
            int dockedWindowCount = _dockedWindows.Count;
            if (_dockedWindows.TryGetValue(dockableWindow, out HeaderElement header))
            {
                header.parent.Remove(header);
                _dockedWindows.Remove(dockableWindow);
                if (_selectedWindow == dockableWindow)
                {
                    _windowContainer.hierarchy.Clear();
                    if (_dockedWindows.Count > 0)
                    {
                        SelectWindow(((HeaderElement)_headerBar.Container.hierarchy[0]).GetDockedWindow());
                    }
                }
            }

            //If dock is empty, merge parent dock or destroy free dock
            if (!_isDockSplit && _dockedWindows.Count <= 0 && dockedWindowCount > 0 && !_isFixedWindow)
            {
                if (_dockedIn != null)
                {
                    _dockedIn.MergeDock();
                }
                else
                {
                    parent.Remove(this);
                }
                
            }
        }

        /// <summary>
        /// Select a window in header bar to show
        /// </summary>
        /// <param name="dockableWindow"></param>
        private void SelectWindow(DockableWindow dockableWindow)
        {
            if (_selectedWindow != null)
            {
                _windowContainer.hierarchy.Clear();
                if (_dockedWindows.TryGetValue(_selectedWindow, out HeaderElement header))
                {
                    header.Deselect();
                }
            }

            _selectedWindow = dockableWindow;
            _windowContainer.hierarchy.Add(dockableWindow);
            _dockedWindows[dockableWindow].Select();
        }

        /// <summary>
        /// Class used for storing a reference for a docking window. The reference may change while docking and undocking
        /// </summary>
        public class DockHandle
        {
            private DockingWindow _reference;
            public DockingWindow DockingWindow { get { return _reference; } }

            public DockHandle(DockingWindow dockingWindow)
            {
                _reference = dockingWindow;
            }

            /// <summary>
            /// Change the reference
            /// </summary>
            /// <param name="dockingWindow"></param>
            public void SetReference(DockingWindow dockingWindow)
            {
                _reference = dockingWindow;
            }
        }

        /// <summary>
        /// Header element of a docking window. Shows the current docked window's names
        /// </summary>
        private class HeaderBar : HueHadesElement
        {
            private DockingWindow _dockingWindow;
            public VisualElement Container { get; private set; }
            private VisualElement _filler;

            public HeaderBar(HueHadesWindow window, DockingWindow dockingWindow) : base(window)
            {
                AddToClassList(ussHeaderBar);
                _dockingWindow = dockingWindow;
                _filler = new VisualElement();
                Container = new VisualElement();
                Add(Container);
                Add(_filler);
                _filler.style.left = 0;
                _filler.style.right = 0;
                _filler.style.top = 0;
                _filler.style.bottom = 0;
                _filler.style.flexGrow = 1;
                Container.style.flexDirection = FlexDirection.Row;
                _filler.AddManipulator(new HeaderBarMoveManipulator(this, _filler));
            }

            /// <summary>
            /// Manipulator for moving the header bar when docking window is free docked
            /// </summary>
            private class HeaderBarMoveManipulator : PointerManipulator
            {
                public HeaderBar Header { get; private set; }
                private bool _dragging;

                public HeaderBarMoveManipulator(HeaderBar headerBar, VisualElement target)
                {
                    this.target = target;
                    Header = headerBar;
                }

                /// <summary>
                /// Register events when element is active
                /// </summary>
                protected override void RegisterCallbacksOnTarget()
                {
                    target.RegisterCallback<PointerDownEvent>(OnPointerDown);
                    target.RegisterCallback<PointerUpEvent>(OnPointerUp);
                    target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
                }

                
                /// <summary>
                /// Unregister events when no longer needed
                /// </summary>
                protected override void UnregisterCallbacksFromTarget()
                {
                    target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
                    target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
                    target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                }

                /// <summary>
                /// Drag free window using header
                /// </summary>
                /// <param name="evt"></param>
                private void OnPointerMove(PointerMoveEvent evt)
                {
                    if (!_dragging) return;
                    var rootDock = Header._dockingWindow;
                    while (rootDock != null && !rootDock._isFreeWindow)
                    {
                        var parentDock = rootDock._dockedIn;
                        if (parentDock == null || (parentDock._splitDirection == FlexDirection.Column && parentDock._splitB == rootDock))
                        {
                            return;
                        }
                        rootDock = parentDock;
                    }
                    if (rootDock == null) return;
                    rootDock.style.left = rootDock.style.left.value.value + evt.deltaPosition.x;
                    rootDock.style.top = rootDock.style.top.value.value + evt.deltaPosition.y;
                }

                /// <summary>
                /// Begin dragging
                /// </summary>
                /// <param name="pointerEvent"></param>
                private void OnPointerDown(PointerDownEvent pointerEvent)
                {
                    _dragging = true;
                    target.CapturePointer(pointerEvent.pointerId);
                }

                /// <summary>
                /// End dragging
                /// </summary>
                /// <param name="pointerEvent"></param>
                private void OnPointerUp(PointerUpEvent pointerEvent)
                {
                    _dragging = false;
                    target.ReleasePointer(pointerEvent.pointerId);
                }

            }



        }
        private class HeaderElement : HueHadesElement
        {
            private const string ussHeaderBar = "docking-window-header-element";
            private const string ussHeaderBarSelected = "docking-window-header-element-selected";
            private const string ussHeaderLabel = "docking-window-header-label";
            private const string ussHeaderClose = "docking-window-header-close";
            private DockingWindow _dockingIn;
            private DockableWindow _dockedWindow;
            private Label _label;

            public DockableWindow GetDockedWindow()
            {
                return _dockedWindow;
            }

            public HeaderElement(HueHadesWindow window, DockableWindow dockableWindow, DockingWindow dockingIn) : base(window)
            {
                AddToClassList(ussHeaderBar);
                _label = new Label(dockableWindow.WindowName);
                dockableWindow.WindowNameChanged += OnWindowNameChanged;

                _label.AddToClassList(ussHeaderLabel);
                hierarchy.Add(_label);

                Button closeButton = new Button();
                closeButton.AddToClassList(ussHeaderClose);
                closeButton.text = "✕";
                hierarchy.Add(closeButton);
                closeButton.RegisterCallback<ClickEvent>(OnCloseClicked);

                RegisterCallback<ClickEvent>(OnClicked);
                _dockingIn = dockingIn;
                _dockedWindow = dockableWindow;
            }

            /// <summary>
            /// SUbscribed to the event when the window's name changes, so the label can update
            /// </summary>
            /// <param name="windowName"></param>
            private void OnWindowNameChanged(string windowName)
            {
                _label.text = windowName;
            }

            /// <summary>
            /// Called when close button is clicked, undocks the window
            /// </summary>
            /// <param name="clickEvent"></param>
            private void OnCloseClicked(ClickEvent clickEvent)
            {
                _dockedWindow.UnDock();
            }

            /// <summary>
            /// Selects the assigned dockable window in the dock's view so it is in the foreground
            /// </summary>
            /// <param name="clickEvent"></param>
            private void OnClicked(ClickEvent clickEvent)
            {
                _dockingIn.SelectWindow(_dockedWindow);
            }

            /// <summary>
            /// Assign uss classes to display header element as selected
            /// </summary>
            public void Select()
            {
                RemoveFromClassList(ussHeaderBar);
                AddToClassList(ussHeaderBarSelected);
            }

            /// <summary>
            /// Assign uss classes to display header element as not selected
            /// </summary>
            public void Deselect()
            {
                RemoveFromClassList(ussHeaderBarSelected);
                AddToClassList(ussHeaderBar);
            }
        }

        /// <summary>
        /// Manipulator for resizing the docking windows
        /// </summary>
        private class DockResizeManipulator : PointerManipulator
        {
            private DockingWindow _targetWindow;

            public DockResizeManipulator(DockingWindow targetWindow)
            {
                target = targetWindow;
                _targetWindow = targetWindow;
            }

            private bool _dragging;
            private bool _draggingSide;
            private Vector2 _lastMousePosition;

            protected override void RegisterCallbacksOnTarget()
            {
                target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
                target.RegisterCallback<MouseDownEvent>(OnMouseDown);
                target.RegisterCallback<MouseUpEvent>(OnMouseUp);
            }

            /// <summary>
            /// When mouse is down, check if we are at a valid scaling position. 
            /// This is the dividing line in split windows, and the edge of free docks.
            /// </summary>
            /// <param name="mouseDownEvent"></param>
            private void OnMouseDown(MouseDownEvent mouseDownEvent)
            {
                if (_targetWindow._isDockSplit)
                {
                    float dist;
                    if (_targetWindow._splitDirection == FlexDirection.Row)
                    {
                        dist = Mathf.Abs(_targetWindow._splitA.worldBound.xMax - mouseDownEvent.mousePosition.x);
                    }
                    else
                    {
                        dist = Mathf.Abs(_targetWindow._splitA.worldBound.yMax - mouseDownEvent.mousePosition.y);
                    }
                    if (dist <= 5.0f)
                    {
                        _dragging = true;
                        target.CaptureMouse();
                    }
                }


                if (_targetWindow._isFreeWindow)
                {
                    float leftDist = Mathf.Abs(_targetWindow.worldBound.xMin - mouseDownEvent.mousePosition.x);
                    float rightDist = Mathf.Abs(_targetWindow.worldBound.xMax - mouseDownEvent.mousePosition.x);
                    float topDist = Mathf.Abs(_targetWindow.worldBound.yMin - mouseDownEvent.mousePosition.y);
                    float bottomDist = Mathf.Abs(_targetWindow.worldBound.yMax - mouseDownEvent.mousePosition.y);
                    float dist = Mathf.Min(leftDist, rightDist, topDist, bottomDist);
                    if (dist <= 5.0f)
                    {
                        _draggingSide = true;
                        target.CaptureMouse();
                    }
                }



                _lastMousePosition = mouseDownEvent.mousePosition;
            }

            /// <summary>
            /// End draggin, release mouse
            /// </summary>
            /// <param name="mouseUpEvent"></param>
            private void OnMouseUp(MouseUpEvent mouseUpEvent)
            {
                if (_dragging || _draggingSide)
                {
                    _dragging = false;
                    _draggingSide = false;
                    target.ReleaseMouse();
                }
            }

            protected override void UnregisterCallbacksFromTarget()
            {
                target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
                target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
                target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            }

            /// <summary>
            /// Resize windows
            /// </summary>
            /// <param name="mouseMoveEvent"></param>
            private void OnMouseMove(MouseMoveEvent mouseMoveEvent)
            {
                if (_dragging)
                {
                    if (_targetWindow._isDockSplit)
                    {
                        if (_targetWindow._splitDirection == FlexDirection.Row)
                        {
                            float moveAmount = mouseMoveEvent.mousePosition.x - _lastMousePosition.x;
                            _targetWindow._splitA.ResizeDock(_targetWindow._splitA.style.width.value.value + moveAmount, _targetWindow._splitA.style.height.value.value);
                            _targetWindow._splitB.ResizeDock(_targetWindow._splitB.style.width.value.value - moveAmount, _targetWindow._splitB.style.height.value.value);
                        }
                        else
                        {
                            float moveAmount = mouseMoveEvent.mousePosition.y - _lastMousePosition.y;
                            _targetWindow._splitA.ResizeDock(_targetWindow._splitA.style.width.value.value, _targetWindow._splitA.style.height.value.value + moveAmount);
                            _targetWindow._splitB.ResizeDock(_targetWindow._splitB.style.width.value.value, _targetWindow._splitB.style.height.value.value - moveAmount);
                        }
                    }
                }

                if (_draggingSide)
                {
                    if (_targetWindow._isFreeWindow)
                    {


                        float leftDist = Mathf.Abs(_targetWindow.worldBound.xMin - _lastMousePosition.x);
                        float rightDist = Mathf.Abs(_targetWindow.worldBound.xMax - _lastMousePosition.x);
                        float topDist = Mathf.Abs(_targetWindow.worldBound.yMin - _lastMousePosition.y);
                        float bottomDist = Mathf.Abs(_targetWindow.worldBound.yMax - _lastMousePosition.y);

                        if (leftDist <= 5.0f)
                        {
                            _targetWindow.style.width = _targetWindow.style.width.value.value - mouseMoveEvent.mouseDelta.x;
                            _targetWindow.style.left = _targetWindow.style.left.value.value + mouseMoveEvent.mouseDelta.x;
                        }
                        else
                        {
                            if (rightDist <= 5.0f )
                            {
                                _targetWindow.style.width = _targetWindow.style.width.value.value + mouseMoveEvent.mouseDelta.x;
                            }
                        }

                        if (topDist <= 5.0f)
                        {
                            _targetWindow.style.height = _targetWindow.style.height.value.value - mouseMoveEvent.mouseDelta.y;
                            _targetWindow.style.top = _targetWindow.style.top.value.value + mouseMoveEvent.mouseDelta.y;
                        }
                        else
                        {
                            if (bottomDist <= 5.0f)
                            {
                                _targetWindow.style.height = _targetWindow.style.height.value.value + mouseMoveEvent.mouseDelta.y;
                            }
                        }
                    }
                }



                _lastMousePosition = mouseMoveEvent.mousePosition;
            }
        }

        /// <summary>
        /// Used for moving a docked window using it's header into another docking window or reordering header list
        /// </summary>
        private class HeaderDragManipulator : PointerManipulator
        {
            private HeaderElement _header;
            private const string ussDraggingHeader = "dragged-header";
            private const string ussDragPreview = "drag-result-preview";

            public HeaderDragManipulator(HeaderElement target)
            {
                this.target = target;
                _header = target;
            }


            protected override void RegisterCallbacksOnTarget()
            {
                target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
                target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
                target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
                target.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
            }

            protected override void UnregisterCallbacksFromTarget()
            {
                target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
                target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
                target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
                target.UnregisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
            }


            private Vector2 targetStartPosition { get; set; }

            private Vector3 pointerStartPosition { get; set; }

            private bool enabled { get; set; }
            private bool initiatedDrag { get; set; }
            private bool startedDrag { get; set; }

            private VisualElement dragPreview { get; set; }

            private DockType dragDockType { get; set; }
            private DockingWindow dragDockTarget { get; set; }
            private int barTargetIndex { get; set; }

            /// <summary>
            /// This method stores the starting position of target and the pointer,
            /// makes target capture the pointer, and denotes that a drag is now in progress.
            /// </summary>
            /// <param name="evt"></param>
            private void PointerDownHandler(PointerDownEvent evt)
            {
                targetStartPosition = target.LocalToWorld(target.transform.position);
                pointerStartPosition = evt.position;
                target.CapturePointer(evt.pointerId);
                initiatedDrag = true;
                enabled = true;
                dragDockType = DockType.Free;
                barTargetIndex = -1;
            }

            /// <summary>
            /// Initialize the drag preview, and the needed variables for dragging
            /// </summary>
            private void BeginDragging()
            {
                dragDockType = DockType.Free;
                barTargetIndex = -1;
                dragPreview = new VisualElement();
                dragPreview.AddToClassList(ussDragPreview);
                target.AddToClassList(ussDraggingHeader);
                _header.parent.Remove(_header);
                _header.HueHadesWindowIn.ShowOverlay(_header);
                targetStartPosition = target.parent.WorldToLocal(targetStartPosition);
                target.transform.position = targetStartPosition;
            }

            /// <summary>
            /// This method checks whether a drag is in progress and whether target has captured the pointer. 
            /// If both are true, calculates a new position for target within the bounds of the window.
            /// </summary>
            /// <param name="evt"></param>
            private void PointerMoveHandler(PointerMoveEvent evt)
            {
                if (!startedDrag && initiatedDrag && Vector2.Distance(evt.position, pointerStartPosition) >= _header.worldBound.height * 0.25f)
                {
                    BeginDragging();
                    initiatedDrag = false;
                    startedDrag = true;
                }
                if (startedDrag && enabled && target.HasPointerCapture(evt.pointerId))
                {
                    
                    Vector3 pointerDelta = evt.position - pointerStartPosition;

                    var destination = new Vector2(
                        Mathf.Clamp(targetStartPosition.x + pointerDelta.x, 0, _header.HueHadesWindowIn.worldBound.width - _header.worldBound.width),
                        Mathf.Clamp(targetStartPosition.y + pointerDelta.y, 0, _header.HueHadesWindowIn.worldBound.height - _header.worldBound.height));
                    ShowDragTarget(evt.position, destination);
                }
            }


            /// <summary>
            /// Place the preview element in the desired position, to show where the new docking position will be
            /// </summary>
            /// <param name="pointer"></param>
            /// <param name="destination"></param>
            private void ShowDragTarget(Vector2 pointer, Vector2 destination)
            {

                dragDockType = DockType.Free;
                barTargetIndex = -1;

                if (dragPreview.parent != null)
                {
                    dragPreview.parent.Remove(dragPreview);
                }

                var correctedPosition = destination;

                UQueryBuilder<HueHadesElement> queryBuilder = new UQueryBuilder<HueHadesElement>(_header.HueHadesWindowIn);


                var headerBars = queryBuilder.Where((e) => (e.ClassListContains(ussHeaderBar) || (e.ClassListContains(ussDockingWindow) && !((DockingWindow)e)._isDockSplit)));
                var overlap = headerBars.Where((e) => OverlapsPosition(e, pointer)).Last();

                if (overlap != null)
                {
                    var overlapType = overlap.GetType();

                    if (overlapType == typeof(HeaderBar))
                    {
                        var headerBar = (HeaderBar)overlap;
                        correctedPosition.y = overlap.parent.LocalToWorld(overlap.transform.position).y;

                        var headerBarChildIndex = 0;
                        for (int i = 0; i < headerBar.Container.childCount; i++)
                        {
                            var child = headerBar.Container.ElementAt(i);
                            if (child.worldBound.center.x <= pointer.x) headerBarChildIndex++;
                        }

                        headerBar.Container.Insert(headerBarChildIndex, dragPreview);
                        dragPreview.style.width = _header.localBound.width;
                        dragPreview.style.height = _header.localBound.height;
                        dragPreview.style.top = 0;
                        dragPreview.style.left = 0;

                        dragDockType = DockType.Header;
                        dragDockTarget = (DockingWindow)(headerBar).parent;
                        barTargetIndex = headerBarChildIndex;
                    }
                    else
                    {
                        if (overlapType == typeof(DockingWindow))
                        {
                            dragDockTarget = (DockingWindow)overlap;
                            if (dragDockTarget._dockedWindows.Count > 1 || dragDockTarget._dockedWindows.First().Value != _header)
                            {
                                var overlapBound = overlap.worldBound;

                                var leftDiff = Mathf.Abs(overlapBound.xMin - pointer.x);
                                var rightDiff = Mathf.Abs(overlapBound.xMax - pointer.x);
                                var topDiff = Mathf.Abs(overlapBound.yMin - pointer.y);
                                var bottomDiff = Mathf.Abs(overlapBound.yMax - pointer.y);

                                var adjustRatio = overlapBound.width / overlapBound.height;
                                var leftDiffAdjusted = leftDiff;
                                var rightDiffAdjusted = rightDiff;
                                var topDiffAdjusted = topDiff * adjustRatio;
                                var bottomDiffAdjusted = bottomDiff * adjustRatio;

                                var smallest = Mathf.Min(leftDiffAdjusted, rightDiffAdjusted, topDiffAdjusted, bottomDiffAdjusted);

                                var overlapWidth = overlapBound.width;
                                var overlapHeight = overlapBound.height;

                                var twoThirds = 2.0f / 3.0f;
                                var oneThird = 1 - twoThirds;
                                var oneThirdWidth = overlapWidth * oneThird;
                                var oneThirdHeight = overlapHeight * oneThird;

                                var newBound = overlapBound;

                                if (leftDiff <= oneThirdWidth || rightDiff <= oneThirdWidth || topDiff <= oneThirdHeight || bottomDiff <= oneThirdHeight) {

                                    if (smallest == leftDiffAdjusted)
                                    {
                                        newBound.xMax -= overlapWidth * twoThirds;
                                        dragDockType = DockType.Left;
                                    }
                                    else
                                    {
                                        if (smallest == rightDiffAdjusted)
                                        {
                                            newBound.xMin += overlapWidth * twoThirds;
                                            dragDockType = DockType.Right;
                                        }
                                        else
                                        {
                                            if (smallest == topDiffAdjusted)
                                            {
                                                newBound.yMax -= overlapHeight * twoThirds;
                                                dragDockType = DockType.Top;
                                            }
                                            else
                                            {
                                                newBound.yMin += overlapHeight * twoThirds;
                                                dragDockType = DockType.Bottom;
                                            }
                                        }
                                    }

                                    _header.HueHadesWindowIn.ShowOverlay(dragPreview, isBackground: true);
                                    dragPreview.style.width = newBound.width;
                                    dragPreview.style.height = newBound.height;
                                    dragPreview.style.top = newBound.yMin;
                                    dragPreview.style.left = newBound.xMin;

                                }
                            }
                        }
                    }

                    
                }

                if (dragDockType == DockType.Free)
                {
                    _header.HueHadesWindowIn.ShowOverlay(dragPreview, isBackground: true);

                    var windowBounds = _header.HueHadesWindowIn.worldBound;
                    var bounds = new Rect(pointer.x - windowBounds.width * 0.125f, pointer.y - windowBounds.height * 0.25f, windowBounds.width * 0.25f, windowBounds.height * 0.5f);

                    dragPreview.style.width = bounds.width;
                    dragPreview.style.height = bounds.height;
                    dragPreview.style.top = bounds.yMin;
                    dragPreview.style.left = bounds.xMin;
                }


                target.transform.position = correctedPosition;

            }


            /// <summary>
            /// End dragging
            /// </summary>
            /// <param name="evt"></param>
            private void PointerUpHandler(PointerUpEvent evt)
            {
                initiatedDrag = false;
                startedDrag = false;
                if (dragPreview != null)
                {
                    if(dragPreview.parent != null) dragPreview.parent.Remove(dragPreview);
                    dragPreview = null;
                }

                if(dragDockTarget != null) _header.GetDockedWindow().Dock(dragDockTarget.Handle, dragDockType, barTargetIndex);

                if (enabled)
                {
                    target.ReleasePointer(evt.pointerId);
                    _header.GetDockedWindow().CapturePointer(evt.pointerId);
                    _header.GetDockedWindow().ReleasePointer(evt.pointerId);
                    
                    target.RemoveFromClassList(ussDraggingHeader);
                }
            }

            private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
            {
                if (enabled)
                {
                    target.transform.position = Vector3.zero;
                    enabled = false;
                }
            }

            /// <summary>
            /// Check if a visual element's bounds contain a point
            /// </summary>
            /// <param name="element"></param>
            /// <param name="position"></param>
            /// <returns></returns>
            private bool OverlapsPosition(VisualElement element, Vector2 position)
            {
                return new Rect(position, Vector3.zero).Overlaps(element.worldBound);
            }

        }

    }

    public enum DockType {
        Free,
        Header,
        Left,
        Right,
        Top,
        Bottom
    }
}
