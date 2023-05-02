using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HueHades.UI
{
    /// <summary>
    /// Base class used for windows in the program that can be docked in the docking area, or docked freely
    /// </summary>
    public class DockableWindow : HueHadesElement
    {

        private DockingWindow.DockHandle _dockedIn;
        /// <summary>
        /// The handle to the docking window we are currently docked in
        /// </summary>
        public DockingWindow.DockHandle DockedIn { get { return _dockedIn; } }
        private const string ussDockableWindow = "dockable-window";
        private string windowName;

        /// <summary>
        /// Name of the window to display
        /// </summary>
        public string WindowName { get { return windowName; } 
            protected set {
                var lastWindowName = windowName;
                windowName = value;
                if (lastWindowName != windowName) WindowNameChanged?.Invoke(windowName);
            }
        }

        /// <summary>
        /// Fires when the window's name is changed, used for header updating
        /// </summary>
        public Action<string> WindowNameChanged;

        public DockableWindow(HueHadesWindow window) : base(window)
        {
            AddToClassList(ussDockableWindow);
            WindowName = "Unknown";
        }

        /// <summary>
        /// Minimum size the window needs to take up on screen
        /// </summary>
        public virtual Vector2 MinimumSize
        {
            get { return new Vector2(300, 300); }
        }

        /// <summary>
        /// Default size the window will be resized to when docking
        /// </summary>
        public virtual Vector2 DefaultSize{
            get { return new Vector2(500, 300); }
        }

        /// <summary>
        /// Maximum size the window can take up on screen
        /// </summary>
        public virtual Vector2 MaximumSize
        {
            get { return new Vector2(1000000, 1000000); }
        }

        /// <summary>
        /// Removes the window from current dock
        /// </summary>
        public void UnDock()
        {
            if (_dockedIn == null) return;
            _dockedIn.DockingWindow.UnDockWindow(this);
            _dockedIn = null;
        }

        /// <summary>
        /// Dock a dockable window inside a docking window
        /// </summary>
        /// <param name="dockIn">Docking window to be docked in</param>
        /// <returns></returns>
        public DockingWindow.DockHandle Dock(DockingWindow.DockHandle dockIn)
        {
            if (_dockedIn != null)
            {
                UnDock();
            }
            _dockedIn = dockIn.DockingWindow.DockWindow(this);
            return _dockedIn;
        }

        /// <summary>
        /// Dock a dockable window inside a docking window
        /// </summary>
        /// <param name="dockIn">Docking window to be docked in</param>
        /// <param name="dockType">Docking type based on desired position / layout arrangement</param>
        /// <param name="headerIndex">Desired header tab index, if required</param>
        /// <returns></returns>
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