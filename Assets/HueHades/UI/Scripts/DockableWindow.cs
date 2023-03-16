using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class DockableWindow : HueHadesElement
    {

        private DockingWindow.DockHandle _dockedIn;
        private const string ussDockableWindow = "dockable-window";

        public DockableWindow(HueHadesWindow window) : base(window)
        {
            AddToClassList(ussDockableWindow);
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


        public virtual string GetWindowName()
        {
            return "Unknown";
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