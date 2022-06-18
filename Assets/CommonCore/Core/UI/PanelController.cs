using CommonCore.LockPause;
using CommonCore.Scripting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.UI
{
    /// <summary>
    /// Base class for UI panel controllers
    /// </summary>
    public abstract class PanelController : MonoBehaviour
    {

        [SerializeField, Tooltip("If set, Signal* will be called automatically using Unity MonoBehaviour events")]
        protected bool HookupUnityEvents = true;

        private bool InitialPaintDone = false;

        public bool IsUsingUnityEvents => HookupUnityEvents;
        public bool IsDoneInitialPaint => InitialPaintDone;

        private void Start()
        {
            if (HookupUnityEvents && !InitialPaintDone)
                SignalInitialPaint();
        }

        private void OnEnable()
        {
            if (HookupUnityEvents)
            {
                if (!InitialPaintDone)
                    SignalInitialPaint();
                SignalPaint();
            }
        }

        private void OnDisable()
        {
            if (HookupUnityEvents)
            {
                SignalUnpaint();
            }
        }

        private void OnDestroy()
        {
            if(HookupUnityEvents)
            {
                SignalFinalUnpaint();
            }
        }

        /// <summary>
        /// Called when the panel is initially created and shown for the first time
        /// </summary>
        public virtual void SignalInitialPaint()
        {
            InitialPaintDone = true;
        }

        /// <summary>
        /// Called when the panel needs to be repainted- on enable or when dependencies update
        /// </summary>
        public virtual void SignalPaint()
        {

        }

        /// <summary>
        /// Called just before the panel is hidden
        /// </summary>
        public virtual void SignalUnpaint()
        {

        }

        /// <summary>
        /// Called just before the panel is destroyed
        /// </summary>
        public virtual void SignalFinalUnpaint()
        {

        }

        /// <summary>
        /// Applies theme, looking to parent MenuController for a theme override
        /// </summary>
        /// <param name="element"></param>
        protected void ApplyThemeToElements(Transform element)
        {
            var menuController = GetComponentInParent<BaseMenuController>();
            if (menuController && !menuController.ApplyTheme)
                return;
            string overrideTheme = menuController.Ref()?.OverrideTheme;
            ApplyThemeToElements(element, overrideTheme);
        }

        /// <summary>
        /// Applies theme, respecting themeOverride and UIThemeMode
        /// </summary>
        protected static void ApplyThemeToElements(Transform root, string themeOverride)
        {
            if (CoreParams.UIThemeMode == UIThemePolicy.Auto)
            {
                var uiModule = CCBase.GetModule<UIModule>();
                if (!string.IsNullOrEmpty(themeOverride))
                    uiModule.ApplyThemeRecurse(root, uiModule.GetThemeByName(themeOverride));
                else
                    uiModule.ApplyThemeRecurse(root);
            }
        }

        //TODO probably remove this
        protected string GetEffectiveTheme()
        {
            var menuController = GetComponentInParent<BaseMenuController>();
            if(menuController != null && !string.IsNullOrEmpty(menuController.OverrideTheme))
            {
                return menuController.OverrideTheme;
            }

            var uiModule = CCBase.GetModule<UIModule>();
            return uiModule.CurrentTheme.name;
        }

        protected void CallPostInitialPaintHooks()
        {
            var menuController = GetComponentInParent<BaseMenuController>();
            string theme = (menuController != null && !string.IsNullOrEmpty(menuController.OverrideTheme)) ? menuController.OverrideTheme : CCBase.GetModule<UIModule>().CurrentTheme?.name;

            var data = new IGUIPaintData(IGUIPaintEventType.InitialPaint, this, menuController, theme);
            ScriptingModule.CallHooked(ScriptHook.OnIGUIPaint, this, data);
        }

        protected void CallPostRepaintHooks()
        {
            var menuController = GetComponentInParent<BaseMenuController>();
            string theme = (menuController != null && !string.IsNullOrEmpty(menuController.OverrideTheme)) ? menuController.OverrideTheme : CCBase.GetModule<UIModule>().CurrentTheme?.name;

            var data = new IGUIPaintData(IGUIPaintEventType.Repaint, this, menuController, theme);
            ScriptingModule.CallHooked(ScriptHook.OnIGUIPaint, this, data);
        }

    }
}