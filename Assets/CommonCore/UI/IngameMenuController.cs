using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.UI
{
    public class IngameMenuController : BaseMenuController
    {
        public string DefaultPanel;

        public bool AllowMenu;
        public bool HandlePause = true;
        public bool Autohide = true;

        public GameObject MainPanel;
        public GameObject ContainerPanel;

        private string CurrentPanel;

        public override void Start()
        {
            base.Start();

            if(Autohide)
            {
                foreach(Transform child in ContainerPanel.transform)
                {
                    child.gameObject.SetActive(false);
                }
                MainPanel.SetActive(false);
            }
        }

        public override void Update()
        {
            base.Update();

            CheckMenuOpen();
        }

        private void CheckMenuOpen()
        {
            bool menuToggled = UnityEngine.Input.GetKeyDown(KeyCode.Escape);

            if(menuToggled)
            {
                //if we're locked out, let the menu be closed but not opened
                if (!AllowMenu)
                {
                    if (MainPanel.activeSelf)
                    {
                        MainPanel.SetActive(false);

                        if(HandlePause)
                        {
                            DoUnpause();
                        }
                    }                        
                }
                else
                {
                    //otherwise, flip state
                    bool newState = !MainPanel.activeSelf;
                    MainPanel.SetActive(newState);

                    if(HandlePause)
                    {
                        if (newState)
                            DoPause();
                        else
                            DoUnpause();
                    }

                    if(newState && !string.IsNullOrEmpty(DefaultPanel))
                    {
                        OnClickSelectButton(DefaultPanel);
                    }
                    
                }
            }

        }

        private void DoPause()
        {
            Time.timeScale = 0;
            
        }

        private void DoUnpause()
        {
            Time.timeScale = 1.0f;
        }

        public void OnClickSelectButton(string menuName)
        {
            foreach(Transform child in ContainerPanel.transform)
            {
                child.gameObject.SetActive(false);
            }

            try
            {
                var childPanel = ContainerPanel.transform.Find(menuName);
                if(childPanel != null)
                {
                    childPanel.gameObject.SetActive(true);
                    childPanel.GetComponent<PanelController>().SignalPaint();
                }
                CurrentPanel = menuName;
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}