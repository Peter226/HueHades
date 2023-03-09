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

        public virtual string GetWindowName()
        {
            return "Unknown";
        }

        public void UnDock()
        {
            _dockedIn.DockingWindow.UnDockWindow(this);
            _dockedIn = null;
        }

        public void Dock(DockingWindow.DockHandle dockIn)
        {
            if (_dockedIn != null)
            {
                UnDock();
            }
            _dockedIn = dockIn.DockingWindow.DockWindow(this);
        }
        public void Dock(DockingWindow.DockHandle dockIn, DockType dockType, int headerIndex = -1)
        {
            if (_dockedIn != null)
            {
                UnDock();
            }
            _dockedIn = dockIn.DockingWindow.DockWindow(this, dockType, headerIndex);
        }

    }
}