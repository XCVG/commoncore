using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CommonCore.DebugLog;
using CommonCore.State;

namespace CommonCore.UI
{

    public class SaveGamePanelController : PanelController
    {
        public RectTransform ScrollContent;
        public GameObject SaveItemPrefab;
        public InputField SaveNameField;

        private string SelectedSave;

        public override void SignalInitialPaint()
        {
            base.SignalInitialPaint();
        }

        public override void SignalPaint()
        {
            base.SignalPaint();
            
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

            SaveNameField.interactable = true;
            SaveNameField.text = string.Empty;
        }

        private void ListSaves()
        {
            //create "new save" entry
            {
                GameObject saveGO = Instantiate<GameObject>(SaveItemPrefab, ScrollContent);
                saveGO.GetComponent<Image>().color = new Color(0.9f, 0.9f, 0.9f);
                saveGO.GetComponentInChildren<Text>().text = "New Save";
                Button b = saveGO.GetComponent<Button>();
                b.onClick.AddListener(delegate { OnSaveSelected(null, b); });
            }            

            string savePath = CoreParams.SavePath;
            DirectoryInfo saveDInfo = new DirectoryInfo(savePath);
            FileInfo[] savesFInfo = saveDInfo.GetFiles().OrderBy(f => f.CreationTime).Reverse().ToArray();

            foreach (FileInfo saveFI in savesFInfo)
            {
                try
                {
                    string niceName = Path.GetFileNameWithoutExtension(saveFI.Name);
                    if (niceName.Contains("_"))
                    {
                        //split nicename
                        string[] splitName = niceName.Split('_');
                        if (splitName.Length >= 2)
                        {
                            if (splitName[0] == "q" || splitName[0] == "a")
                                niceName = splitName[1];
                            else
                                niceName = Path.GetFileNameWithoutExtension(saveFI.Name); //undo our oopsie if it turns out someone is trolling with prefixes
                        }

                    }

                    GameObject saveGO = Instantiate<GameObject>(SaveItemPrefab, ScrollContent);
                    saveGO.GetComponentInChildren<Text>().text = niceName;
                    Button b = saveGO.GetComponent<Button>();
                    b.onClick.AddListener(delegate { OnSaveSelected(Path.GetFileNameWithoutExtension(saveFI.Name), b); });

                }
                catch (Exception e)
                {
                    CDebug.LogEx("Failed to load save!" + saveFI.ToString(), LogLevel.Error, this);
                    CDebug.LogException(e);
                }

            }

        }

        public void OnSaveSelected(string saveName, Button b)
        {
            SelectedSave = saveName;

            if(saveName != null)
            {
                SaveNameField.interactable = false;
                SaveNameField.text = saveName;
            }
            else
            {
                SaveNameField.interactable = true;
                SaveNameField.text = string.Empty;
            }
        }

        public void OnClickSave()
        {
            Debug.Log("OnClickSave");

            //new save if saveName=null

            //otherwise save over old save

            if (!GameState.Instance.SaveLocked)
            {
                //TODO text entry modal
                string saveName = SaveNameField.text;
                string savePath;

                if(SelectedSave != null)
                {
                    saveName = SelectedSave; //TODO need to trim?
                    savePath = CoreParams.SavePath + @"\" + saveName + ".json";
                    if (File.Exists(savePath))
                        File.Delete(savePath);
                }
                else
                    savePath = CoreParams.SavePath + @"\" + saveName + ".json";

                if (!string.IsNullOrEmpty(saveName))
                {
                    BaseSceneController.Current.Commit();
                    GameState.SerializeToFile(savePath);
                    Modal.PushMessageModal("", "Saved Successfully", null, null);
                    SignalPaint();
                }
                else
                {
                    Modal.PushMessageModal("You need to enter a filename!", "Save Failed", null, null);
                }

            }
            else
            {
                //can't save!

                Modal.PushMessageModal("You cannot save here!", "Save Not Allowed", null, null);
            }
        }
    }
}