using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.LockPause;

namespace CommonCore.UI
{
    public class IngameMenuController : BaseMenuController
    {
        public static IngameMenuController Current;

        public string DefaultPanel;

        public bool HandlePause = true;
        public bool Autohide = true;

        public GameObject MainPanel;
        public GameObject ContainerPanel;
        public GameObject EphemeralRoot;

        private string CurrentPanel;

        public override void Start()
        {
            base.Start();

            Current = this;

            EphemeralRoot = new GameObject("InGameMenu_EphemeralRoot");
            EphemeralRoot.transform.parent = transform.root;

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

                        foreach (Transform child in ContainerPanel.transform)
                        {
                            child.gameObject.SetActive(false);
                        }

                        ClearEphemeral();
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
                    
                    if(!newState)
                    {
                        foreach (Transform child in ContainerPanel.transform)
                        {
                            child.gameObject.SetActive(false);
                        }

                        ClearEphemeral();
                    }
                    
                }
            }

        }

        private void DoPause()
        {
            LockPauseModule.PauseGame(PauseLockType.AllowMenu, this);
            LockPauseModule.LockControls(InputLockType.GameOnly, this);
        }

        private void DoUnpause()
        {
            LockPauseModule.UnlockControls(this);
            LockPauseModule.UnpauseGame(this);
        }

        private void ClearEphemeral()
        {
            //destroy ephemeral modals (a bit hacky)
            foreach (Transform child in EphemeralRoot.transform)
            {
                Destroy(child.gameObject);
            }
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

        private bool AllowMenu
        {
            get
            {
                var lockState = LockPauseModule.GetInputLockState();
                return (lockState == null || lockState.Value >= InputLockType.GameOnly);
            }
        }
    }
}