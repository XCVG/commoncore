using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CommonCore;
using CommonCore.DebugLog;
using CommonCore.State;

namespace CommonCore.UI
{

    public class LoadGamePanelController : MonoBehaviour
    {
        public RectTransform ScrollContent;
        public GameObject SaveItemPrefab;

        public Image DetailImage;
        public Text DetailName;
        public Text DetailType;
        public Text DetailDate;
        public Text DetailLocation;
        public Button LoadButton;

        private SaveItem[] Saves;
        private SaveItem SelectedSave;

        void OnEnable()
        {
            SelectedSave = null;

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

            List<SaveItem> allSaves = new List<SaveItem>(savesFInfo.Length); //we don't go straight into the array in case of invalids...

            foreach(FileInfo saveFI in savesFInfo)
            {
                try
                {
                    SaveType type = SaveType.Manual;
                    string niceName = Path.GetFileNameWithoutExtension(saveFI.Name);
                    if(niceName.Contains("_"))
                    {
                        //split nicename
                        string[] splitName = niceName.Split('_');
                        if(splitName.Length >= 2)
                        {
                            niceName = splitName[1];
                            if (splitName[0] == "q")
                                type = SaveType.Quick;
                            else if (splitName[0] == "a")
                                type = SaveType.Auto;
                            else
                                niceName = Path.GetFileNameWithoutExtension(saveFI.Name); //undo our oopsie if it turns out someone is trolling with prefixes
                        }

                    }

                    var save = new SaveItem(niceName, saveFI.Name, type, string.Empty, saveFI.CreationTime);
                    allSaves.Add(save);
                }
                catch(Exception e)
                {
                    CDebug.LogEx("Failed to load save!" + saveFI.ToString(), LogLevel.Error, this);
                    CDebug.LogException(e);
                }
                
            }

            Saves = allSaves.ToArray();

            //inefficient but probably safer
            for(int i = 0; i < Saves.Length; i++)
            {
                var save = Saves[i];
                GameObject saveGO = Instantiate<GameObject>(SaveItemPrefab, ScrollContent);
                saveGO.GetComponentInChildren<Text>().text = save.NiceName;
                Button b = saveGO.GetComponent<Button>();
                int lexI = i;
                b.onClick.AddListener(delegate { OnSaveSelected(lexI, b); }); //scoping is weird here
            }
        }

        public void OnSaveSelected(int i, Button b)
        {
            SelectedSave = Saves[i];

            DetailName.text = SelectedSave.NiceName;
            DetailType.text = SelectedSave.Type.ToString();
            DetailLocation.text = SelectedSave.Location;
            DetailDate.text = SelectedSave.Date.ToString();

            LoadButton.interactable = true;
        }

        public void OnClickLoad()
        {
            if (SelectedSave == null)
                return;

            SharedUtils.LoadGame(SelectedSave.FileName);
        }

        private enum SaveType
        {
            Manual, Quick, Auto
        }

        private class SaveItem
        {
            public string NiceName;
            public string FileName;
            public SaveType Type;
            public string Location;
            public DateTime Date;

            public SaveItem(string niceName, string fileName, SaveType type, string location, DateTime date)
            {
                NiceName = niceName;
                FileName = fileName;
                Type = type;
                Location = location;
                Date = date;
            }
            
        }
    }


}