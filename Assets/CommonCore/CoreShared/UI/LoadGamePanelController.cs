using CommonCore.State;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.UI
{

    public class LoadGamePanelController : PanelController
    {
        public bool ApplyTheme = true;

        public RectTransform ScrollContent;
        public GameObject SaveItemPrefab;

        public Image DetailImage;
        public Text DetailName;
        public Text DetailType;
        public Text DetailDate;
        public Text DetailLocation;
        public Button LoadButton;
        
        private SaveGameInfo[] Saves;
        private int SelectedSaveIndex = -1;

        public override void SignalPaint()
        {
            base.SignalPaint();

            SelectedSaveIndex = -1;

            ClearList();
            ListSaves();
        }

        private void ClearList()
        {
            foreach (Transform t in ScrollContent)
            {
                Destroy(t.gameObject);
            }
            ScrollContent.DetachChildren();

            Saves = null;
            LoadButton.interactable = false;
        }

        private void ListSaves()
        {
            string savePath = CoreParams.SavePath;
            DirectoryInfo saveDInfo = new DirectoryInfo(savePath);
            FileInfo[] savesFInfo = saveDInfo.GetFiles().OrderBy(f => f.CreationTime).Reverse().ToArray();

            List<SaveGameInfo> allSaves = new List<SaveGameInfo>(savesFInfo.Length); //we don't go straight into the array in case of invalids...

            foreach(FileInfo saveFI in savesFInfo)
            {
                try
                {
                    var save = new SaveGameInfo(saveFI);
                    allSaves.Add(save);
                }
                catch(Exception e)
                {
                    Debug.LogError("Failed to read save! " + saveFI.ToString(), this);
                    Debug.LogException(e);
                }
                
            }

            Saves = allSaves.ToArray();

            string overrideTheme = null;
            bool applyTheme = ApplyTheme;
            if (ApplyTheme)
            {
                var menuController = GetComponentInParent<BaseMenuController>();
                overrideTheme = menuController.Ref()?.OverrideTheme;
                if(menuController)
                    applyTheme = menuController.ApplyTheme;
            }

            //inefficient but probably safer
            for (int i = 0; i < Saves.Length; i++)
            {
                var save = Saves[i];
                GameObject saveGO = Instantiate<GameObject>(SaveItemPrefab, ScrollContent);
                saveGO.GetComponentInChildren<Text>().text = save.NiceName;
                Button b = saveGO.GetComponent<Button>();
                int lexI = i;
                b.onClick.AddListener(delegate { OnSaveSelected(lexI, b); }); //scoping is weird here
                if (applyTheme)
                {                    
                    ApplyThemeToElements(saveGO.transform, overrideTheme);
                }
            }
        }

        public void OnSaveSelected(int i, Button b)
        {
            SelectedSaveIndex = i;

            var selectedSave = Saves[i];

            //TODO read metadata, handle different save types differently

            DetailName.text = selectedSave.NiceName;
            DetailType.text = selectedSave.Type.ToString();
            DetailLocation.text = selectedSave.Location;
            DetailDate.text = selectedSave.Date.ToString();

            LoadButton.interactable = true;
        }

        public void OnClickLoad()
        {
            if (SelectedSaveIndex < 0)
                return;

            SharedUtils.LoadGame(Saves[SelectedSaveIndex].FileName, false);
        }

    }


}