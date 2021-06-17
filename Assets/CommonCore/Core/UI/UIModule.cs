using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommonCore.UI
{

    /// <summary>
    /// Module handling UI and theming
    /// </summary>
    [CCEarlyModule]
    public class UIModule : CCModule
    {
        private Dictionary<string, IGUIPanelData> IGUIPanels = new Dictionary<string, IGUIPanelData>();
        private Dictionary<string, UIThemeAsset> Themes = new Dictionary<string, UIThemeAsset>();

        //WIP theming support

        public UIModule()
        {
            //test
            //RegisterIGUIPanel("TestPanel", 0, "Test", CoreUtils.LoadResource<GameObject>("UI/TestPanel"));

            Log($"Theme mode: {CoreParams.UIThemeMode}");

            if (CoreParams.UIThemeMode != UIThemePolicy.Disabled)
            {

                //load UI themes and set default theme
                var themes = CoreUtils.LoadResources<UIThemeAsset>("UI/Themes/");
                foreach (var theme in themes)
                {
                    RegisterTheme(theme);
                }

                if (!string.IsNullOrEmpty(CoreParams.DefaultUITheme))
                {
                    if (Themes.TryGetValue(CoreParams.DefaultUITheme, out var defaultTheme))
                    {
                        CurrentTheme = defaultTheme;
                        Log($"Using default theme \"{CoreParams.DefaultUITheme}\"");
                    }
                    else
                    {
                        LogWarning($"Couldn't find default theme \"{CoreParams.DefaultUITheme}\"");
                    }
                }
                else
                {
                    Log("No default theme specified. Themes will not be applied automatically.");
                }
            }
        }

        public override void OnAddonLoaded(AddonLoadData data)
        {
            var themes = data.LoadedResources
                .Where(kvp => kvp.Key.StartsWith("UI/Themes/", StringComparison.OrdinalIgnoreCase) && kvp.Value.Resource is UIThemeAsset)
                .Select(kvp => kvp.Value.Resource as UIThemeAsset);
            foreach(var theme in themes)
            {
                RegisterTheme(theme);
            }
        }

        /// <summary>
        /// Registers a panel to be displayed in the ingame menu
        /// </summary>
        public void RegisterIGUIPanel(string name, int priority, string niceName, GameObject prefab)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab), "Prefab must be non-null!");

            if (IGUIPanels.ContainsKey(name))
            {
                LogWarning($"A IGUI panel \"{name}\" is already registered");
                IGUIPanels.Remove(name);
            }

            IGUIPanels.Add(name, new IGUIPanelData(priority, niceName, prefab));
        }

        /// <summary>
        /// Unregisters an ingame menu panel
        /// </summary>
        public void UnregisterIGUIPanel(string name)
        {
            IGUIPanels.Remove(name);
        }

        /// <summary>
        /// A sorted view (highest to lowest priority) of the IGUI panel prefabs
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, IGUIPanelData>> SortedIGUIPanels => IGUIPanels.OrderByDescending(d => d.Value.Priority).ToArray();


        public UIThemeAsset CurrentTheme { get; set; }

        /// <summary>
        /// Applies the current theme to an element
        /// </summary>
        public void ApplyTheme(Transform element)
        {
            ApplyTheme(element, CurrentTheme);
        }

        /// <summary>
        /// Applies a theme to an element
        /// </summary>
        public void ApplyTheme(Transform element, UIThemeAsset theme)
        {
            if (CoreParams.UIThemeMode == UIThemePolicy.Disabled)
            {
                LogWarning($"Can't apply theme to element because theme engine is disabled!");
                return;
            }

            if (theme == null)
            {
                LogWarning($"Can't apply theme to element because theme is null!");
                return;
            }

            ThemeEngine.ApplyThemeToElement(element, theme);
        }

        /// <summary>
        /// Applies the current theme to an element and its children
        /// </summary>
        public void ApplyThemeRecurse(Transform root)
        {
            ApplyThemeRecurse(root, CurrentTheme);
        }

        /// <summary>
        /// Applies a theme to an element and its children
        /// </summary>
        public void ApplyThemeRecurse(Transform root, UIThemeAsset theme)
        {
            if (CoreParams.UIThemeMode == UIThemePolicy.Disabled)
            {
                LogWarning($"Can't apply theme to element because theme engine is disabled!");
                return;
            }

            if (theme == null)
            {
                LogWarning($"Can't apply theme to element because theme is null!");
                return;
            }

            ThemeEngine.ApplyThemeToAll(root, theme);
        }

        /// <summary>
        /// Registers a theme
        /// </summary>
        public void RegisterTheme(UIThemeAsset theme)
        {
            if (CoreParams.UIThemeMode == UIThemePolicy.Disabled)
                return;

            string name = theme.name;

            if (Themes.ContainsKey(name))
            {
                LogWarning($"A theme named \"{name}\" is already registered and will be overwritten!");
            }

            Themes[name] = theme;
        }

        /// <summary>
        /// Gets a registered theme by name
        /// </summary>
        public UIThemeAsset GetThemeByName(string name)
        {
            if (CoreParams.UIThemeMode == UIThemePolicy.Disabled)
                return null;

            if (Themes.TryGetValue(name, out var theme))
                return theme;

            return null;
        }

        /// <summary>
        /// Gets a list of loaded/registered themes
        /// </summary>
        public string GetThemesList()
        {
            if (CoreParams.UIThemeMode == UIThemePolicy.Disabled)
                return "Theme engine is disabled";

            StringBuilder sb = new StringBuilder(32 * Themes.Count);

            foreach (var name in Themes.Keys)
            {
                sb.AppendLine(name);
            }

            return sb.ToString();
        }

        public IEnumerable<string> EnumerateThemeNames()
        {
            if (CoreParams.UIThemeMode == UIThemePolicy.Disabled)
                throw new InvalidOperationException();

            return Themes.Keys.ToArray();
        }

        public IEnumerable<UIThemeAsset> EnumerateThemes()
        {
            if (CoreParams.UIThemeMode == UIThemePolicy.Disabled)
                throw new InvalidOperationException();

            return Themes.Values.ToArray();
        }

        
    }

    
}