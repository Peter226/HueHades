using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class DockableWindow : HueHadesElement
    {

        private DockingWindow.DockHandle _dockedIn;
        public DockingWindow.DockHandle DockedIn { get { return _dockedIn; } }
        private const string ussDockableWindow = "dockable-window";
        private string windowName;
        public string WindowName { get { return windowName; } 
            protected set {
                var lastWindowName = windowName;
                windowName = value;
                if (lastWindowName != windowName) WindowNameChanged?.Invoke(windowName);
            }
        }
        public Action<string> WindowNameChanged;

        public DockableWindow(HueHadesWindow window) : base(window)
        {
            AddToClassList(ussDockableWindow);
            WindowName = "Unknown";
        }

        public virtual Vector2 GetMinimumSize()
        {
            return new Vector2(300,300);
        }

        public virtual Vector2 GetDefaultSize()
        {
            return new Vector2(500, 300);
        }

        public virtual Vector2 GetMaximumSize()
        {
            return new Vector2(1000000, 1000000);
        }

        public void UnDock()
        {
            _dockedIn.DockingWindow.UnDockWindow(this);
            _dockedIn = null;
        }


        public DockingWindow.DockHandle Dock(DockingWindow.DockHandle dockIn)
        {
            if (_dockedIn != null)
            {
                UnDock();
            }
            _dockedIn = dockIn.DockingWindow.DockWindow(this);
            return _dockedIn;
        }
        public DockingWindow.DockHandle Dock(DockingWindow.DockHandle dockIn, DockType dockType, int headerIndex = -1)
        {
            if (_dockedIn != null)
            {
                UnDock();
            }
            _dockedIn = dockIn.DockingWindow.DockWindow(this, dockType, headerIndex);
            return _dockedIn;
        }

    }
}