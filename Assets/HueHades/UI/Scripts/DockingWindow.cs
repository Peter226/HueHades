using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class DockingWindow : HueHadesElement
    {
        private const string ussHeaderBar = "docking-window-header-bar";
        private const string ussDockingWindow = "docking-window";
        private const string ussSplitDockingWindow = "docking-window-split";

        private DockingWindow _dockedIn;
        private HeaderBar _headerBar;
        private bool _isFixedWindow;
        private Dictionary<DockableWindow, HeaderElement> _dockedWindows = new Dictionary<DockableWindow, HeaderElement>();
        private HueHadesElement _windowContainer;
        private DockableWindow _selectedWindow;
        private HueHadesElement _splitHierarchy;
        private DockingWindow _splitA;
        private DockingWindow _splitB;
        private bool _isDockSplit;
        private DockHandle _dockHandle;
        public DockHandle Handle { get { return _dockHandle; } private set { _dockHandle = value; } }

        public DockingWindow(HueHadesWindow window, bool isFixedWindow = false, DockingWindow dockedIn = null) : base(window)
        {
            Handle = new DockHandle(this);
            _headerBar = new HeaderBar(window);
            hierarchy.Add(_headerBar);
            _windowContainer = new HueHadesElement(window);
            hierarchy.Add(_windowContainer);
            _splitHierarchy = new HueHadesElement(window);
            hierarchy.Add(_splitHierarchy);
            _isFixedWindow = isFixedWindow;
            AddToClassList(ussDockingWindow);
            _dockedIn = dockedIn;
        }

        public DockHandle DockWindow(DockableWindow dockableWindow, DockType dockType = DockType.Header, int headerIndex = -1)
        {
            if (_isDockSplit)
            {
                var sizeA = _splitA.worldBound.size;
                var sizeB = _splitB.worldBound.size;
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
                        headerIndex = _headerBar.hierarchy.childCount;
                    }
                    _headerBar.hierarchy.Insert(headerIndex, header);
                    _dockedWindows.Add(dockableWindow, header);
                    SelectWindow(dockableWindow);
                    break;
                case DockType.Free:
                    DockingWindow freeDocker = new DockingWindow(window);
                    window.FreeDockElement.Add(freeDocker);
                    dockableWindow.Dock(freeDocker.Handle);
                    break;
                default:
                    SplitDock(dockType, out DockingWindow oldHierarchy, out DockingWindow newHierarchy);
                    dockableWindow.Dock(newHierarchy.Handle);
                    break;
            }
            return Handle;
        }

        private void SplitDock(DockType dockType, out DockingWindow oldHierarchy, out DockingWindow newHierarchy)
        {
            Rect bounds = this.worldBound;
            AddToClassList(ussSplitDockingWindow);

            _isDockSplit = true;
            oldHierarchy = new DockingWindow(window, dockedIn: this);
            newHierarchy = new DockingWindow(window, dockedIn: this);

            _splitA = oldHierarchy;
            _splitB = newHierarchy;


            if (dockType == DockType.Right || dockType == DockType.Bottom) {
                _splitHierarchy.Add(oldHierarchy);
                _splitHierarchy.Add(newHierarchy);
            }
            else
            {
                _splitHierarchy.Add(newHierarchy);
                _splitHierarchy.Add(oldHierarchy);
            }
            if (dockType == DockType.Left || dockType == DockType.Right)
            {
                _splitHierarchy.style.flexDirection = FlexDirection.Row;
                float thirdWidth = bounds.width / 3.0f;
                newHierarchy.style.width = thirdWidth;
                oldHierarchy.style.width = thirdWidth * 2;
            }
            else
            {
                _splitHierarchy.style.flexDirection = FlexDirection.Column;
                float thirdHeight = bounds.height / 3.0f;
                newHierarchy.style.height = thirdHeight;
                oldHierarchy.style.height = thirdHeight * 2;
            }


            _windowContainer.style.display = DisplayStyle.None;
            _headerBar.style.display = DisplayStyle.None;
            _splitHierarchy.style.display = DisplayStyle.Flex;

            var dockedWindows = _dockedWindows.Keys.ToList();

            for (int i = 0; i < dockedWindows.Count; i++)
            {
                var dockedWindow = dockedWindows[i];
                dockedWindow.Dock(oldHierarchy.Handle);
            }

        }


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

            if (_splitA._isDockSplit || _splitB._isDockSplit)
            {
                _isDockSplit = true;
                _windowContainer.style.display = DisplayStyle.None;
                _headerBar.style.display = DisplayStyle.None;
                _splitHierarchy.style.display = DisplayStyle.Flex;

                
                if (_splitA._isDockSplit)
                {
                    _splitA = _splitA._splitA;
                    _splitB = _splitA._splitB;

                }
                if (_splitB._isDockSplit)
                {
                    _splitA = _splitB._splitA;
                    _splitB = _splitB._splitB;
                }
                _splitHierarchy.Add(_splitA);
                _splitHierarchy.Add(_splitB);

                _splitA._dockedIn = this;
                _splitB._dockedIn = this;

            }
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
            

            oldSplitA.Handle.SetReference(this);
            oldSplitB.Handle.SetReference(this);


        } 
            

        public void UnDockWindow(DockableWindow dockableWindow)
        {
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
                        SelectWindow(((HeaderElement)_headerBar.hierarchy[0]).GetDockedWindow());
                    }
                }
            }

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


        public class DockHandle
        {
            private DockingWindow _reference;
            public DockingWindow DockingWindow { get { return _reference; } }

            public DockHandle(DockingWindow dockingWindow)
            {
                _reference = dockingWindow;
            }
            public void SetReference(DockingWindow dockingWindow)
            {
                _reference = dockingWindow;
            }
        }


        private class HeaderBar : HueHadesElement
        {
            

            public HeaderBar(HueHadesWindow window) : base(window)
            {
                AddToClassList(ussHeaderBar);
                
            }

            private class HeaderBarMoveManipulator : PointerManipulator
            {
                public HeaderBar Header { get; private set; }   

                public HeaderBarMoveManipulator(HeaderBar target)
                {
                    this.target = target;
                    Header = target;
                }

                protected override void RegisterCallbacksOnTarget()
                {
                    target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
                    target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
                }

                protected override void UnregisterCallbacksFromTarget()
                {
                    target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
                    target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
                }

                private void OnPointerEnter(PointerEnterEvent pointerEnterEvent)
                {
                    
                }
                private void OnPointerLeave(PointerLeaveEvent pointerEnterEvent)
                {

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


            public DockableWindow GetDockedWindow()
            {
                return _dockedWindow;
            }

            public HeaderElement(HueHadesWindow window, DockableWindow dockableWindow, DockingWindow dockingIn) : base(window)
            {
                AddToClassList(ussHeaderBar);
                Label label = new Label(dockableWindow.GetWindowName());
                label.AddToClassList(ussHeaderLabel);
                hierarchy.Add(label);

                Button closeButton = new Button();
                closeButton.AddToClassList(ussHeaderClose);
                closeButton.text = "X";
                hierarchy.Add(closeButton);
                closeButton.RegisterCallback<ClickEvent>(OnCloseClicked);

                RegisterCallback<ClickEvent>(OnClicked);
                _dockingIn = dockingIn;
                _dockedWindow = dockableWindow;
            }


            private void OnCloseClicked(ClickEvent clickEvent)
            {
                _dockedWindow.UnDock();
            }

            private void OnClicked(ClickEvent clickEvent)
            {
                _dockingIn.SelectWindow(_dockedWindow);
            }


            public void Select()
            {
                RemoveFromClassList(ussHeaderBar);
                AddToClassList(ussHeaderBarSelected);
            }
            public void Deselect()
            {
                RemoveFromClassList(ussHeaderBarSelected);
                AddToClassList(ussHeaderBar);
            }
        }


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


            // This method stores the starting position of target and the pointer, 
            // makes target capture the pointer, and denotes that a drag is now in progress.
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


            // This method checks whether a drag is in progress and whether target has captured the pointer. 
            // If both are true, calculates a new position for target within the bounds of the window.
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
                        correctedPosition.y = overlap.parent.LocalToWorld(overlap.transform.position).y;

                        var headerBarChildIndex = 0;
                        for (int i = 0; i < overlap.childCount; i++)
                        {
                            var child = overlap.ElementAt(i);
                            if (child.worldBound.center.x <= pointer.x) headerBarChildIndex++;
                        }

                        overlap.Insert(headerBarChildIndex, dragPreview);
                        dragPreview.style.width = _header.localBound.width;
                        dragPreview.style.height = _header.localBound.height;
                        dragPreview.style.top = 0;
                        dragPreview.style.left = 0;

                        dragDockType = DockType.Header;
                        dragDockTarget = (DockingWindow)((HeaderBar)overlap).parent;
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

                if (enabled && target.HasPointerCapture(evt.pointerId))
                {
                    target.ReleasePointer(evt.pointerId);
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
