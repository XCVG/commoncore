using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.UI
{

    /// <summary>
    /// Slightly hacky script for closing panels
    /// </summary>
    public class ClosePanelButtonScript : MonoBehaviour
    {
        [SerializeField, Header("Auto Hookup")]
        private bool HookupButton = false;
        [SerializeField]
        private bool HookupPanel = false;
        [SerializeField]
        private bool HookupMenu = false;

        [SerializeField, Header("Explicit Hookup")]
        private BaseMenuController MenuController = null;
        [SerializeField]
        private PanelController PanelController = null;
        [SerializeField]
        private GameObject PanelObject = null;

        private void Start()
        {
            if(HookupButton)
            {
                var button = GetComponent<Button>();
                if (button == null)
                    Debug.LogWarning($"{nameof(ClosePanelButtonScript)} on {name} failed to hook up to button because no button exists!");
                else
                {
                    button.onClick.AddListener(ClosePanel);
                }
            }
        }

        public void ClosePanel()
        {
            //if explicit hookups are available, use them!
            {
                if (MenuController != null && MenuController is MainMenuController mmc)
                {
                    if (closeUsingMainMenuController(mmc))
                        return;
                }

                if(PanelController != null)
                {
                    PanelController.gameObject.SetActive(false);
                    return;
                }

                if(PanelObject != null)
                {
                    PanelObject.SetActive(false);
                    return;
                }
            }

            //attempt to hook/hook up menu first
            if(HookupMenu)
            {
                //we only know about the main menu controller
                var mmc = GetComponentInParent<MainMenuController>();
                if(mmc != null)
                {
                    if(closeUsingMainMenuController(mmc))
                        return;
                }
            }

            //otherwise attempt to hook up to panel and close
            if(HookupPanel)
            {
                var parentPanel = GetComponent<PanelController>().Ref() ?? GetComponentInParent<PanelController>();
                if(parentPanel != null)
                {
                    parentPanel.gameObject.SetActive(false);
                    return;
                }
            }

            Debug.LogWarning($"{nameof(ClosePanelButtonScript)} on {name} failed to hook up to panel or menu");

            

            bool closeUsingMainMenuController(MainMenuController mmc)
            {
                var parentPanel = getPanelController();
                if (parentPanel != null && parentPanel == mmc.CurrentPanel)
                {
                    mmc.CloseCurrentPanel();
                    return true;
                }

                return false; //returns if we were able to close or not
            }

            PanelController getPanelController()
            {
                return PanelController.Ref() ?? GetComponent<PanelController>().Ref() ?? GetComponentInParent<PanelController>().Ref();
            }
            
        }
    }
}