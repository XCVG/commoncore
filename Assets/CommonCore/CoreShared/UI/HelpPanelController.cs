using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.UI
{

    /// <summary>
    /// Controller for the help panel on menus
    /// </summary>
    /// <remarks>Loads help panel contents from a prefab</remarks>
    public class HelpPanelController : PanelController
    {
        private static readonly string PrefabPath = "UI/Panels/HelpPanelContents";

        [SerializeField]
        private bool LoadPrefab = true;

        public override void SignalInitialPaint()
        {
            base.SignalInitialPaint();

            if(LoadPrefab)
            {
                var panelPrefab = CoreUtils.LoadResource<GameObject>(PrefabPath);
                if(panelPrefab != null)
                {
                    var panel = Instantiate(panelPrefab, transform);
                    panel.transform.SetAsFirstSibling();
                }
                else
                {
                    Debug.LogError($"HelpPanelController couldn't find prefab {PrefabPath}");
                }
            }

        }
    }
}