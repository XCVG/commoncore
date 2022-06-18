using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.LockPause;
using UnityEngine.UI;
using CommonCore.Scripting;
using CommonCore.State;
using CommonCore.Input;

namespace CommonCore.UI
{
    public class IngameMenuController : BaseMenuController
    {
        public static IngameMenuController Current;

        public string DefaultPanel;

        [Tooltip("If set, will auto pause and unpause the game when opened/closed")]
        public bool HandlePause = true;
        [Tooltip("If set, will automatically hide panels that were left active on start")]
        public bool Autohide = true;
        [Tooltip("If set, will generate panels injected through ConfigModule")]
        public bool GeneratePanels = true;

        public GameObject MainPanel;
        public GameObject ContainerPanel;
        public GameObject ButtonPanel;
        public GameObject EphemeralRoot;

        private string CurrentPanel;

        public override void Start()
        {
            base.Start();

            Current = this;

            //create ephermeral root
            EphemeralRoot = new GameObject("InGameMenu_EphemeralRoot");
            EphemeralRoot.tag = "EphemeralRoot";
            EphemeralRoot.transform.parent = transform.root;

            //generate panels
            try
            {
                var buttonPrefab = CoreUtils.LoadResource<GameObject>("UI/IGUI_MenuButton");
                var uiModule = CCBase.GetModule<UIModule>();
                foreach (var pkvp in uiModule.SortedIGUIPanels)
                {
                    string name = pkvp.Key;
                    var panel = pkvp.Value;

                    if (panel.Builder == null)
                    {
                        Debug.LogError($"Build function for {name} does not exist!");
                        continue;
                    }
                    var panelGO = panel.Builder(ContainerPanel.transform);
                    panelGO.name = name;

                    var buttonGO = Instantiate(buttonPrefab, ButtonPanel.transform);
                    buttonGO.name = "Button_" + name;
                    buttonGO.GetComponentInChildren<Text>().text = panel.NiceName;
                    buttonGO.GetComponentInChildren<Button>().onClick.AddListener(() => OnClickSelectButton(name));

                    if (ApplyTheme && CoreParams.UIThemeMode == UIThemePolicy.Auto)
                    {
                        if (!string.IsNullOrEmpty(OverrideTheme))
                        {
                            uiModule.ApplyThemeRecurse(panelGO.transform, uiModule.GetThemeByName(OverrideTheme));
                            uiModule.ApplyThemeRecurse(buttonGO.transform, uiModule.GetThemeByName(OverrideTheme));
                        }
                        else
                        {
                            uiModule.ApplyThemeRecurse(panelGO.transform);
                            uiModule.ApplyThemeRecurse(buttonGO.transform);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogError("Fatal error occurred generating additional IGUI panels!");
                Debug.LogException(e);
            }

            //do autohide
            if(Autohide)
            {
                foreach(Transform child in ContainerPanel.transform)
                {
                    child.gameObject.SetActive(false);
                }
                MainPanel.SetActive(false);
            }

            //call scripts
            ScriptingModule.CallHooked(ScriptHook.AfterIGUIMenuCreate, this);
        }

        public override void Update()
        {
            base.Update();

            CheckMenuOpen();
        }

        private void CheckMenuOpen()
        {
            if (LockPauseModule.GetInputLockState() == InputLockType.All)
                return;

            bool menuToggled = UnityEngine.Input.GetKeyDown(KeyCode.Escape) || MappedInput.GetButtonDown(Input.DefaultControls.OpenMenu);

            if(menuToggled)
            {
                //if we're locked out, let the menu be closed but not opened
                if (!AllowMenu)
                {
                    if (IsOpen)
                    {
                        Close();
                    }                        
                }
                else
                {
                    //otherwise, flip state
                    Toggle();                    
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
                    var panelController = childPanel.GetComponent<PanelController>();
                    if(panelController != null && !panelController.IsUsingUnityEvents)
                    {
                        panelController.SignalPaint();
                    }                    
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
                return (lockState == null || lockState.Value >= InputLockType.GameOnly) && !GameState.Instance.MenuLocked;
            }
        }

        //new: public manipulation methods

        /// <summary>
        /// Toggles the ingame menu
        /// </summary>
        /// <remarks>This will always succeed even if AllowMenu is false</remarks>
        public void Toggle()
        {
            if (IsOpen)
                Close();
            else
                Open();
        }

        /// <summary>
        /// Opens the ingame menu if it is closed
        /// </summary>
        /// <remarks>This will always succeed even if AllowMenu is false</remarks>
        public void Open()
        {
            if (IsOpen)
                return;

            MainPanel.SetActive(true);

            if (HandlePause)
            {
                DoPause();
            }

            if (!string.IsNullOrEmpty(DefaultPanel))
            {
                OnClickSelectButton(DefaultPanel);
            }

            //run scripts
            ScriptingModule.CallHooked(ScriptHook.OnIGUIMenuOpen, this);
            
        }

        /// <summary>
        /// Closes the ingame menu if it is open
        /// </summary>
        /// <remarks>This will always succeed even if AllowMenu is false</remarks>
        public void Close()
        {
            if (!IsOpen)
                return;

            MainPanel.SetActive(false);

            if (HandlePause)
            {
                DoUnpause();
            }

            foreach (Transform child in ContainerPanel.transform)
            {
                child.gameObject.SetActive(false);
            }

            ClearEphemeral();
        }

        /// <summary>
        /// Is the ingame menu open?
        /// </summary>
        public bool IsOpen => MainPanel.activeSelf;
    }
}