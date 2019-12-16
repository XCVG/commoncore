using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommonCore.UI
{

    /// <summary>
    /// Module handling UI and theming
    /// </summary>
    [CCEarlyModule] //needed?
    public class UIModule : CCModule
    {
        public static UIModule Instance { get; private set; }

        private Dictionary<string, IGUIPanelData> IGUIPanels = new Dictionary<string, IGUIPanelData>();

        //TODO theming support

        public UIModule()
        {
            Instance = this;

            //test
            //RegisterIGUIPanel("TestPanel", 0, "Test", CoreUtils.LoadResource<GameObject>("UI/TestPanel"));
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

    }

    
}