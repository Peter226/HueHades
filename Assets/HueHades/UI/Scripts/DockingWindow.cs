using Codice.CM.SEIDInfo;
using System.Collections;
using System.Collections.Generic;
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

        private DockingWindow _dockedIn;
        private HeaderBar _headerBar;
        private bool _isFixedWindow;
        private Dictionary<DockableWindow, HeaderElement> _dockedWindows = new Dictionary<DockableWindow, HeaderElement>();
        private HueHadesElement _windowContainer;
        private DockableWindow _selectedWindow;

        public DockingWindow(HueHadesWindow window, bool isFixedWindow = false) : base(window)
        {
            _headerBar = new HeaderBar(window);
            hierarchy.Add(_headerBar);
            _windowContainer = new HueHadesElement(window);
            hierarchy.Add(_windowContainer);
            _isFixedWindow = isFixedWindow;
            AddToClassList(ussDockingWindow);
        }

        public void DockWindow(DockableWindow dockableWindow, DockType dockType = DockType.Any)
        {
            HeaderElement header = new HeaderElement(window, dockableWindow, this);
            header.AddManipulator(new HeaderDragManipulator(header));
            _headerBar.hierarchy.Add(header);
            _dockedWindows.Add(dockableWindow, header);
            SelectWindow(dockableWindow);
        }

        public void UnDockWindow(DockableWindow dockableWindow)
        {
            if (_dockedWindows.TryGetValue(dockableWindow, out HeaderElement header))
            {
                _headerBar.hierarchy.Remove(header);
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




        private class HeaderBar : HueHadesElement
        {
            

            public HeaderBar(HueHadesWindow window) : base(window)
            {
                AddToClassList(ussHeaderBar);
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

            private DragTargetType dragTargetType { get; set; }

            private HeaderBar barTarget { get; set; }
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
                dragTargetType = DragTargetType.Free;
            }

            private void BeginDragging()
            {
                dragTargetType = DragTargetType.Free;
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
                        //Mathf.Clamp(targetStartPosition.x + pointerDelta.x, 0, target.panel.visualTree.worldBound.width),
                        //Mathf.Clamp(targetStartPosition.y + pointerDelta.y, 0, target.panel.visualTree.worldBound.height));
                        Mathf.Clamp(targetStartPosition.x + pointerDelta.x, 0, _header.HueHadesWindowIn.worldBound.width - _header.worldBound.width),
                        Mathf.Clamp(targetStartPosition.y + pointerDelta.y, 0, _header.HueHadesWindowIn.worldBound.height - _header.worldBound.height));
                    ShowDragTarget(evt.position, destination);
                }
            }

            private void ShowDragTarget(Vector2 pointer, Vector2 destination)
            {

                dragTargetType = DragTargetType.Free;

                if (dragPreview.parent != null)
                {
                    dragPreview.parent.Remove(dragPreview);
                }

                var correctedPosition = destination;

                UQueryBuilder<HueHadesElement> queryBuilder = new UQueryBuilder<HueHadesElement>(_header.HueHadesWindowIn);


                var headerBars = queryBuilder.Where((e) => (e.ClassListContains(ussHeaderBar) || e.ClassListContains(ussDockingWindow)));
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

                        dragTargetType = DragTargetType.Header;
                        barTarget = (HeaderBar)overlap;
                        barTargetIndex = headerBarChildIndex;
                    }
                    else
                    {
                        if (overlapType == typeof(DockingWindow))
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
                                }
                                else
                                {
                                    if (smallest == rightDiffAdjusted)
                                    {
                                        newBound.xMin += overlapWidth * twoThirds;
                                    }
                                    else
                                    {
                                        if (smallest == topDiffAdjusted)
                                        {
                                            newBound.yMax -= overlapHeight * twoThirds;
                                        }
                                        else
                                        {
                                            newBound.yMin += overlapHeight * twoThirds;
                                        }
                                    }
                                }

                                _header.HueHadesWindowIn.ShowOverlay(dragPreview, isBackground: true);
                                dragPreview.style.width = newBound.width;
                                dragPreview.style.height = newBound.height;
                                dragPreview.style.top = newBound.yMin;
                                dragPreview.style.left = newBound.xMin;

                                dragTargetType = DragTargetType.Split;
                            }
                        }
                    }

                    
                }

                if (dragTargetType == DragTargetType.Free)
                {
                    _header.HueHadesWindowIn.ShowOverlay(dragPreview, isBackground: true);

                    var windowBounds = _header.HueHadesWindowIn.worldBound;
                    var bounds = new Rect(pointer.x - windowBounds.width * 0.125f, pointer.y - windowBounds.height * 0.25f, windowBounds.width * 0.25f, windowBounds.height * 0.5f);
                    


                    dragPreview.style.width = bounds.width;
                    dragPreview.style.height = bounds.height;
                    dragPreview.style.top = bounds.yMin;
                    dragPreview.style.left = bounds.xMin;
                }




                /*int maxIndex = -1;
                List<List<int>> elementIndexes = new List<List<int>>();
                for (int i = 0;i < overlappingHeaderBars.Count;i++)
                {
                    var indexes = new List<int>();
                    elementIndexes.Add(indexes);
                    var headerBar = overlappingHeaderBars[i];
                    VisualElement parent = null;
                    VisualElement lastParent = headerBar;
                    do {
                        parent = lastParent.parent;
                        int index = parent.IndexOf(lastParent);
                        indexes.Add(index);
                        lastParent = parent;
                    } while (parent != headerBar.HueHadesWindowIn && parent != null);
                }*/




                target.transform.position = correctedPosition;

            }





            // This method checks whether a drag is in progress and whether target has captured the pointer. 
            // If both are true, makes target release the pointer.
            private void PointerUpHandler(PointerUpEvent evt)
            {
                initiatedDrag = false;
                startedDrag = false;
                if (dragPreview != null)
                {
                    if(dragPreview.parent != null) dragPreview.parent.Remove(dragPreview);
                    dragPreview = null;
                }

                if (dragTargetType == DragTargetType.Header)
                {
                    barTarget.Insert(barTargetIndex,_header);
                }





                if (enabled && target.HasPointerCapture(evt.pointerId))
                {
                    target.ReleasePointer(evt.pointerId);
                    target.RemoveFromClassList(ussDraggingHeader);
                }
            }

            // This method checks whether a drag is in progress. If true, queries the root 
            // of the visual tree to find all slots, decides which slot is the closest one 
            // that overlaps target, and sets the position of target so that it rests on top 
            // of that slot. Sets the position of target back to its original position 
            // if there is no overlapping slot.
            private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
            {
                if (enabled)
                {
                    /*VisualElement slotsContainer = root.Q<VisualElement>("slots");
                    UQueryBuilder<VisualElement> allSlots =
                        slotsContainer.Query<VisualElement>(className: "slot");
                    UQueryBuilder<VisualElement> overlappingSlots =
                        allSlots.Where(OverlapsTarget);
                    VisualElement closestOverlappingSlot =
                        FindClosestSlot(overlappingSlots);
                    Vector3 closestPos = Vector3.zero;
                    if (closestOverlappingSlot != null)
                    {
                        closestPos = RootSpaceOfSlot(closestOverlappingSlot);
                        closestPos = new Vector2(closestPos.x - 5, closestPos.y - 5);
                    }
                    target.transform.position =
                        closestOverlappingSlot != null ?
                        closestPos :
                        targetStartPosition;
                    */
                    
                    target.transform.position = Vector3.zero;
                    enabled = false;
                }
            }

            private bool OverlapsPosition(VisualElement element, Vector2 position)
            {
                return new Rect(position, Vector3.zero).Overlaps(element.worldBound);
            }
            

            private enum DragTargetType
            {
                Header,
                Free,
                Split
            }



        }





    }

    public enum DockType {
        Free,
        Any,
        Left,
        Right,
        Top,
        Bottom
    }
}
