using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.UI
{

    /// <summary>
    /// Applies themes to an element or elements independent of panels/menus
    /// </summary>
    public class ApplyThemeScript : MonoBehaviour
    {
        [SerializeField, Tooltip("If set to null, will use attached object as target root")]
        private GameObject Target = null;

        [SerializeField, Header("Events")]
        private bool ThemeOnStart = true;
        [SerializeField]
        private bool ThemeOnEnable = false;

        [SerializeField, Header("Theme Options"), Tooltip("If set, will apply theme to children as well")]
        private bool Recurse = true;
        [SerializeField]
        private string ThemeOverride = null;
        [SerializeField, Tooltip("If true, theme will be applied if theme policy is ExplicitOnly")]
        private bool ConsiderExplicit = false;

        private void Start()
        {
            if (ThemeOnStart && !ThemeOnEnable)
                ApplyTheme();
        }

        private void OnEnable()
        {
            if (ThemeOnEnable)
                ApplyTheme();
        }

        public void ApplyTheme()
        {
            if (CoreParams.UIThemeMode == UIThemePolicy.Disabled || (!ConsiderExplicit && CoreParams.UIThemeMode == UIThemePolicy.ExplicitOnly))
                return;

            var uiModule = CCBase.GetModule<UIModule>();
            var targetElement = (Target.Ref() ?? gameObject).Ref()?.transform;
            
            if(Recurse)
            {
                if (!string.IsNullOrEmpty(ThemeOverride))
                {
                    uiModule.ApplyThemeRecurse(targetElement, uiModule.GetThemeByName(ThemeOverride));
                }
                else
                {
                    uiModule.ApplyThemeRecurse(targetElement);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(ThemeOverride))
                {
                    uiModule.ApplyTheme(targetElement, uiModule.GetThemeByName(ThemeOverride));
                }
                else
                {
                    uiModule.ApplyTheme(targetElement);
                }
            }
        }
    }
}