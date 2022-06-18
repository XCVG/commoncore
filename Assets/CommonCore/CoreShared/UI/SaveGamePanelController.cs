using CommonCore.State;
using CommonCore.StringSub;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.UI
{

    public class SaveGamePanelController : PanelController
    {
        private const string SubList = "IGUI_SAVE"; //substitution list for strings

        public bool ApplyTheme = true;

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

            CallPostRepaintHooks();
        }

        public override void SignalUnpaint()
        {
            ClearDetails();
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

            if (DetailImage.sprite != null)
            {
                Destroy(DetailImage.sprite.texture);
                Destroy(DetailImage.sprite);

                DetailImage.sprite = null;
            }
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

            string overrideTheme = null;
            bool applyTheme = ApplyTheme;
            if (ApplyTheme)
            {
                var menuController = GetComponentInParent<BaseMenuController>();
                overrideTheme = menuController.Ref()?.OverrideTheme;
                if (menuController)
                    applyTheme = menuController.ApplyTheme;
            }

            foreach (FileInfo saveFI in savesFInfo)
            {
                try
                {
                    SaveGameInfo saveInfo = new SaveGameInfo(saveFI);
                    if (saveInfo.Type == SaveGameType.Auto || saveInfo.Type == SaveGameType.Quick)
                        continue;

                    GameObject saveGO = Instantiate<GameObject>(SaveItemPrefab, ScrollContent);
                    saveGO.GetComponentInChildren<Text>().text = saveInfo.ShortName;
                    Button b = saveGO.GetComponent<Button>();
                    b.onClick.AddListener(delegate { OnSaveSelected(Path.GetFileNameWithoutExtension(saveFI.Name), saveInfo, b); });
                    if (applyTheme)
                    {
                        ApplyThemeToElements(saveGO.transform, overrideTheme);
                    }
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
                    var metadata = SaveUtils.LoadSaveMetadata(saveInfo.Value.FileName);

                    DetailName.text = metadata?.NiceName ?? saveInfo.Value.ShortName;
                    DetailType.text = saveInfo.Value.Type.ToString();
                    DetailLocation.text = metadata?.Location ?? metadata?.LocationRaw ?? "";
                    DetailDate.text = saveInfo.Value.Date.ToString("yyyy-MM-dd HH:mm");

                    if (metadata?.ThumbnailImage != null)
                    {
                        var tex = new Texture2D(128, 128);
                        tex.LoadImage(metadata.ThumbnailImage);
                        DetailImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                    }
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
                    saveFileName = saveName;
                    //string savePath = CoreParams.SavePath + Path.DirectorySeparatorChar + saveName + ".json";
                    //if (File.Exists(savePath))
                    //    File.Delete(savePath); //this "works" but seems to be bugged- race condition?
                    //unneeded
                }
                else
                {
                    saveFileName = "m_" + SaveUtils.GetSafeName(saveName);                    

                    if(File.Exists(SaveUtils.GetCleanSavePath(saveFileName)))
                    {
                        Modal.PushConfirmModal(Sub.Replace("SaveAlreadyExistsMessage", SubList), Sub.Replace("SaveAlreadyExists", SubList), "Replace", "Cancel", null, 
                            (status, tag, result) => { 
                                if(status == ModalStatusCode.Complete && result)
                                {
                                    CompleteSave(saveName, saveFileName);
                                }
                        });
                        return;
                    }
                }

                CompleteSave(saveName, saveFileName);                

            }
            else
            {
                //can't save!

                Modal.PushMessageModal(Sub.Replace("SaveNotAllowedMessage", SubList), Sub.Replace("SaveNotAllowed", SubList), null, null, true);
            }
        }

        private void CompleteSave(string saveName, string saveFileName)
        {
            if (!string.IsNullOrEmpty(saveName))
            {
                try
                {
                    SharedUtils.SaveGame(saveFileName, true, false, SaveUtils.CreateDefaultMetadata(saveName));
                    Modal.PushMessageModal(Sub.Replace("SaveSuccessMessage", SubList), Sub.Replace("SaveSuccess", SubList), null, null, true);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Save failed! ({e.GetType().Name})");
                    Debug.LogException(e);
                    Modal.PushMessageModal(e.Message, Sub.Replace("SaveFail", SubList), null, null, true);
                }
                SignalPaint();
            }
            else
            {
                Modal.PushMessageModal(Sub.Replace("SaveBadFilenameMessage", SubList), Sub.Replace("SaveFail", SubList), null, null, true);
            }
        }
    }
}