using CommonCore.LockPause;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.UI
{
    /// <summary>
    /// Data passed to handlers on an IGUI repaint hooked script call 
    /// </summary>
    public class IGUIPaintData
    {
        public IGUIPaintEventType PaintEventType { get; private set; }
        public PanelController PanelController { get; private set; }
        public BaseMenuController MenuController { get; private set; }
        public string CurrentTheme { get; private set; }
        public Dictionary<string, object> ExtraData { get; private set; } = new Dictionary<string, object>();

        public IGUIPaintData(IGUIPaintEventType paintEventType, PanelController panelController, BaseMenuController menuController, string currentTheme)
        {
            PaintEventType = paintEventType;
            PanelController = panelController;
            MenuController = menuController;
            CurrentTheme = currentTheme;
        }
    }

    public enum IGUIPaintEventType
    {
        InitialPaint, Repaint
    }
}