using CommonCore.State;
using CommonCore.StringSub;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.UI
{

    public class SaveGamePanelController : PanelController
    {
        private const string SubList = "IGUI_SAVE"; //substitution list for strings

        public RectTransform ScrollContent;
        public GameObject SaveItemPrefab;
        public InputField SaveNameField;
        public Button SaveButton;

        [Header("Detail Fields")]
        public Image DetailImage;
        public Text DetailName;
        public Text DetailType;
        public Text DetailDate;
        public Text DetailLocation;

        private string SelectedSaveName;
        private SaveGameInfo? SelectedSaveInfo;

        public override void SignalInitialPaint()
        {
            base.SignalInitialPaint();
        }

        public override void SignalPaint()
        {
            base.SignalPaint();

            SelectedSaveName = null;
            SelectedSaveInfo = null;

            SetButtonVisibility();
            ClearList();
            ClearDetails();
            ListSaves();            
        }

        private void SetButtonVisibility()
        {
            if(GameState.Instance.SaveLocked || GameState.Instance.ManualSaveLocked)
            {
                SaveButton.interactable = false;
            }
            else
            {
                SaveButton.interactable = true;
            }
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

        private void ClearDetails()
        {
            DetailName.text = string.Empty;
            DetailType.text = string.Empty;
            DetailLocation.text = string.Empty;
            DetailDate.text = string.Empty;

            //SaveButton.interactable = false;
        }

        private void ListSaves()
        {
            //create "new save" entry
            {
                GameObject saveGO = Instantiate<GameObject>(SaveItemPrefab, ScrollContent);
                saveGO.GetComponent<Image>().color = new Color(0.9f, 0.9f, 0.9f);
                saveGO.GetComponentInChildren<Text>().text = Sub.Replace("NewSave", SubList);
                Button b = saveGO.GetComponent<Button>();
                b.onClick.AddListener(delegate { OnSaveSelected(null, null, b); });
            }            

            string savePath = CoreParams.SavePath;
            DirectoryInfo saveDInfo = new DirectoryInfo(savePath);
            FileInfo[] savesFInfo = saveDInfo.GetFiles().OrderBy(f => f.CreationTime).Reverse().ToArray();

            foreach (FileInfo saveFI in savesFInfo)
            {
                try
                {
                    SaveGameInfo saveInfo = new SaveGameInfo(saveFI);
                    if (saveInfo.Type == SaveGameType.Auto || saveInfo.Type == SaveGameType.Quick)
                        continue;

                    GameObject saveGO = Instantiate<GameObject>(SaveItemPrefab, ScrollContent);
                    saveGO.GetComponentInChildren<Text>().text = saveInfo.NiceName;
                    Button b = saveGO.GetComponent<Button>();
                    b.onClick.AddListener(delegate { OnSaveSelected(Path.GetFileNameWithoutExtension(saveFI.Name), saveInfo, b); });

                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to read save! " + saveFI.ToString(), this);
                    Debug.LogException(e);
                }

            }

        }

        public void OnSaveSelected(string saveName, SaveGameInfo? saveInfo, Button b)
        {
            SelectedSaveName = saveName;
            SelectedSaveInfo = saveInfo;

            ClearDetails();
            SetButtonVisibility();

            if (saveName != null)
            {
                //selected an existing save
                if(saveInfo.HasValue)
                {
                    DetailName.text = saveInfo.Value.NiceName;
                    DetailType.text = saveInfo.Value.Type.ToString();
                    DetailLocation.text = saveInfo.Value.Location;
                    DetailDate.text = saveInfo.Value.Date.ToString();
                }

                SaveNameField.interactable = false;
                SaveNameField.text = saveName;
            }
            else
            {
                //making a new save                

                DetailType.text = Sub.Replace("NewSave", SubList);

                SaveNameField.interactable = true;
                SaveNameField.text = string.Empty;
            }
        }

        public void OnClickSave()
        {
            //Debug.Log("OnClickSave");

            //new save if saveName=null

            //otherwise save over old save

            if (!GameState.Instance.SaveLocked && !GameState.Instance.ManualSaveLocked)
            {
                string saveName = SaveNameField.text;
                string saveFileName;

                if (SelectedSaveName != null)
                {
                    //assume save name is already okay

                    saveName = SelectedSaveName; //we know it already has a prefix
                    saveFileName = saveName + ".json";
                    string savePath = CoreParams.SavePath + @"\" + saveName + ".json";
                    if (File.Exists(savePath))
                        File.Delete(savePath); //this "works" but seems to be bugged- race condition?
                }
                else
                {
                    saveFileName = "m_" + SaveUtils.GetSafeName(saveName) + ".json";
                }

                if (!string.IsNullOrEmpty(saveName))
                {
                    try
                    {
                        SharedUtils.SaveGame(saveFileName, true);
                        Modal.PushMessageModal(Sub.Replace("SaveSuccessMessage", SubList), Sub.Replace("SaveSuccess", SubList), null, null);
                    }
                    catch(Exception e)
                    {
                        Debug.LogError($"Save failed! ({e.GetType().Name})");
                        Debug.LogException(e);
                        Modal.PushMessageModal(e.Message, Sub.Replace("SaveFail", SubList), null, null);
                    }
                    SignalPaint();
                }
                else
                {
                    Modal.PushMessageModal(Sub.Replace("SaveBadFilenameMessage", SubList), Sub.Replace("SaveFail", SubList), null, null);
                }

            }
            else
            {
                //can't save!

                Modal.PushMessageModal(Sub.Replace("SaveNotAllowedMessage", SubList), Sub.Replace("SaveNotAllowed", SubList), null, null);
            }
        }
    }
}