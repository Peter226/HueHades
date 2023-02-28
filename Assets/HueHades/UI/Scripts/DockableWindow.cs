using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    public class DockableWindow : HueHadesElement
    {

        private DockingWindow _dockedIn;

        public DockableWindow(HueHadesWindow window) : base(window)
        {
            
        }

        public virtual string GetWindowName()
        {
            return "Unknown";
        }

        public void UnDock()
        {
            _dockedIn.UnDockWindow(this);
        }

        public void Dock(DockingWindow dockIn)
        {
            if (_dockedIn != null)
            {
                UnDock();
            }
            _dockedIn = dockIn;
            dockIn.DockWindow(this);

        }
    }
}