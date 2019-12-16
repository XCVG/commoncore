using CommonCore.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.UI
{

    public class RadioPanelController : PanelController
    {
        [SerializeField]
        private RectTransform SelectionPanelContainer = null;
        [SerializeField]
        private RectTransform SubPanelContainer = null;
        [SerializeField]
        private GameObject ButtonTemplate = null;

        public override void SignalInitialPaint()
        {
            base.SignalInitialPaint();

            SelectionPanelContainer.DestroyAllChildren(); //temporary
            SubPanelContainer.DestroyAllChildren();

            //create ambient/off button
            {
                GameObject buttonGO = Instantiate(ButtonTemplate, SelectionPanelContainer);
                buttonGO.GetComponent<Button>().onClick.AddListener(() => HandleSelectionButtonPressed(null));
                buttonGO.name = "ButtonAmbient";
                buttonGO.GetComponentInChildren<Text>().text = "Ambient";
            }

            //create buttons and panels
            foreach(var musicComponent in AudioPlayer.Instance.GetUserMusicComponents())
            {
                string componentName = musicComponent.GetType().Name;

                GameObject buttonGO = Instantiate(ButtonTemplate, SelectionPanelContainer);
                buttonGO.GetComponent<Button>().onClick.AddListener(() => HandleSelectionButtonPressed(componentName));
                buttonGO.name = "Button_" + componentName;
                buttonGO.GetComponentInChildren<Text>().text = musicComponent.NiceName;

                var panelPrefab = musicComponent.PanelPrefab;
                if (panelPrefab != null)
                {
                    //panelPrefab.SetActive(false); //legit?
                    GameObject panelGO = Instantiate(panelPrefab, SubPanelContainer);
                    panelGO.SetActive(false);
                    panelGO.name = componentName;
                }
            }

        }

        public override void SignalPaint()
        {
            base.SignalPaint();

            var musicComponentName = AudioPlayer.Instance.GetCurrentUserMusicComponent();

            foreach (Transform t in SubPanelContainer)
            {
                if (!string.IsNullOrEmpty(musicComponentName) && t.gameObject.name == musicComponentName)
                    continue;

                t.gameObject.SetActive(false);
            }

            if(!string.IsNullOrEmpty(musicComponentName))
            {
                var t = SubPanelContainer.Find(musicComponentName);
                if (t != null)
                    t.gameObject.SetActive(true);
                else
                    Debug.LogWarning("Can't find a panel for " + musicComponentName);
            }
        }

        public void HandleSelectionButtonPressed(string target)
        {
            AudioPlayer.Instance.SelectUserMusicComponent(target);

            SignalPaint();
        }

    }
}