using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CommonCore.Audio;
using CommonCore.State;
using CommonCore.StringSub;
using CommonCore.LockPause;
using CommonCore.RpgGame.UI;
using CommonCore.World;
using CommonCore.DebugLog;
using CommonCore.UI;
using CommonCore.Scripting;
using System.Linq;
using CommonCore.Config;

namespace CommonCore.RpgGame.Dialogue
{

    public class DialogueController : MonoBehaviour
    {
        public static string CurrentDialogue { get; set; }
        public static DialogueFinishedDelegate CurrentCallback { get; set; }
        //public static bool AutoPauseGame { get; set; }
        public static string CurrentTarget { get; set; } //hacky but oh well

        public static DialogueTrace Trace { get; private set; }

        public bool ApplyTheme = true;
        public string OverrideTheme;

        public RectTransform ChoicePanel;
        public RectTransform NameTextPanel;
        public Text TextTitle;
        public Text TextMain;
        public Image BackgroundImage;
        public Image FaceImage;
        //public Button[] ButtonsChoice;
        public GameObject ButtonPrefab;
        public Button ButtonContinue;
        public Button ButtonAlternateContinue;
        public ScrollRect ScrollChoice;
        public RectTransform ScrollChoiceContent;

        public AudioSource VoiceAudioSource;
        public GameObject CameraPrefab;
        public DialogueCameraController CameraController;
        public DialogueNavigator Navigator;

        private string CurrentFrameName;
        private string CurrentSceneName;
        private DialogueScene CurrentScene;
        private Dictionary<string, Frame> CurrentSceneFrames { get { return CurrentScene.Frames; } }

        //private string CurrentFrameName;
        private Frame CurrentFrameObject;

        private float DefaultPanelHeight;
        private Coroutine WaitAndAdvanceCoroutine = null;

        private HashSet<string> HiddenObjects = new HashSet<string>();

        void Awake()
        {
            name = "DialogueSystem";
        }

        void Start()
        {
            //GameState.Instance.CurrentDialogue = "intro.intro1";

            //if (AutoPauseGame)
            //    LockPauseModule.PauseGame(this.gameObject);

            DefaultPanelHeight = ChoicePanel.rect.height; //ordering of this wrt ApplyThemeToPanel may matter later

            ApplyThemeToPanel();

            Trace = new DialogueTrace();

            ScriptingModule.CallNamedHooked("DialogueOnOpen", this);

            var loc = ParseLocation(CurrentDialogue);

            if(loc.Key == null && GameParams.DialogueDefaultToThisScene)
            {
                //use default
                LoadScene(loc.Value); //this has always been a hack
                PresentNewFrame(CurrentScene.Default);
            }
            else if(loc.Value == null && GameParams.DialogueDefaultToThisScene)
            {
                LoadScene(loc.Key);
                PresentNewFrame(CurrentScene.Default);
            }
            else
            {
                LoadScene(loc.Key);
                PresentNewFrame(loc.Value);
            }

        }

        private void LoadScene(string scene)
        {
            //TODO bodge some dynamic shit in here for running Immersive Monologues (need to add some junk to DialogueModule as well)
            //maybe if scene==_dynamicPreload we use a property or something
            //if (scene.Equals(DialogueModule.DynamicDialogueName, StringComparison.OrdinalIgnoreCase))
            //    throw new NotImplementedException($"{nameof(DialogueModule.DynamicDialogueName)} is not yet implemented!");
            //this should "just work" actually

            CurrentScene = CCBase.GetModule<DialogueModule>().GetDialogue(scene);
            CurrentSceneName = scene;
        }

        private void PresentNewFrame(string s)
        {
            TryCallScript(CurrentFrameObject?.Scripts?.OnUnpresent);

            CurrentFrameName = s;
            if (!CurrentSceneFrames.ContainsKey(s))
                Debug.LogError($"[Dialogue] Can't find frame \"{s}\"");
            PresentNewFrame(CurrentSceneFrames[s]);
        }
        
        private void PresentNewFrame(Frame f)
        {
            TryCallScript(f?.Scripts?.BeforePresent, f);           

            //special handling for blank frames
            if (f is BlankFrame)
            {
                CurrentFrameObject = f;
                ScriptingModule.CallNamedHooked("DialogueOnPresent", this, CurrentFrameObject);
                TryCallScript(f?.Scripts?.OnPresent, f);                
                OnChoiceButtonClick(0);
                return;
            }

            //create trace node
            var traceNode = new DialogueTraceNode();
            traceNode.Path = $"{CurrentSceneName}.{CurrentFrameName}";

            //present music
            if (!string.IsNullOrEmpty(f.Music))
            {
                if (!(AudioPlayer.Instance.IsMusicSetToPlay(MusicSlot.Cinematic) && AudioPlayer.Instance.GetMusicName(MusicSlot.Cinematic) == f.Music))
                {
                    AudioPlayer.Instance.SetMusic(f.Music, MusicSlot.Cinematic, 1.0f, true, false);
                    AudioPlayer.Instance.StartMusic(MusicSlot.Cinematic);
                }
            }
            else if (f.Music != null) //null = no change, empty = no music
            {
                AudioPlayer.Instance.ClearMusic(MusicSlot.Cinematic);
            }

            //present audio
            string voiceClipName = $"{CurrentSceneName}/{CurrentFrameName}";
            string voiceClipNameOverride = f.Options.VoiceOverride;
            if (!string.IsNullOrEmpty(voiceClipNameOverride))
            {
                if (voiceClipNameOverride.StartsWith("/"))
                    voiceClipName = voiceClipNameOverride.TrimStart('/');
                else
                    voiceClipName = $"{CurrentSceneName}/{voiceClipNameOverride}";
            }
            if (VoiceAudioSource.isPlaying)
                VoiceAudioSource.Stop();
            var voiceClip = CCBase.GetModule<AudioModule>().GetSound(voiceClipName, SoundType.Voice, !GameParams.DialogueVerboseLogging); //GetModule<T> is now preferred
            if (voiceClip != null)
            {
                VoiceAudioSource.clip = voiceClip;
                VoiceAudioSource.volume = f.Options.VoiceVolume ?? 1f;
                VoiceAudioSource.Play();
            }

            //present background
            BackgroundImage.sprite = null;
            BackgroundImage.gameObject.SetActive(false);
            if (!string.IsNullOrEmpty(f.Background))
            {
                var sprite = CoreUtils.LoadResource<Sprite>("Dialogue/bg/" + f.Background);
                if (sprite != null)
                {
                    BackgroundImage.sprite = sprite;
                    BackgroundImage.gameObject.SetActive(true);
                }
                else
                {
                    if (GameParams.DialogueVerboseLogging)
                        CDebug.LogEx($"Couldn't find face sprite Dialogue/bg/{f.Background}", LogLevel.Verbose, this);
                }
            }

            //size panel
            float faceYOffset = 0;
            var framePanelHeight = f.Options.PanelHeight;
            var panelHeight = framePanelHeight == ChoicePanelHeight.Default ? GameParams.DialoguePanelHeight : framePanelHeight;
            switch (panelHeight)
            {
                case ChoicePanelHeight.Half:
                    if (!Mathf.Approximately(ChoicePanel.rect.height, DefaultPanelHeight / 2f))
                        ChoicePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, DefaultPanelHeight / 2f);
                    faceYOffset = -(DefaultPanelHeight / 2f);
                    break;
                case ChoicePanelHeight.Variable:
                    CDebug.LogEx($"{nameof(ChoicePanelHeight)} {ChoicePanelHeight.Variable} is not supported!", LogLevel.Warning, this);
                    break;
                case ChoicePanelHeight.Fixed:
                    ChoicePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, f.Options.PanelHeightPixels);
                    break;
                default:
                    if (!Mathf.Approximately(ChoicePanel.rect.height, DefaultPanelHeight))
                        ChoicePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, DefaultPanelHeight); //correct?
                    break;
            }
            
            //show/hide name text
            if(f.Options.HideNameText)
            {
                NameTextPanel.gameObject.SetActive(false);
            }
            else
            {
                NameTextPanel.gameObject.SetActive(true);
            }

            //present image
            FaceImage.sprite = null;
            FaceImage.gameObject.SetActive(false);
            if (!string.IsNullOrEmpty(f.Image))
            {
                //attempt to present image
                var sprite = CoreUtils.LoadResource<Sprite>("Dialogue/char/" + f.Image);
                if (sprite != null)
                {
                    //Debug.Log(sprite.name);

                    Vector2 canvasSize = ((RectTransform)FaceImage.canvas.transform).rect.size;

                    float spriteX = sprite.texture.width * (100f / sprite.pixelsPerUnit);
                    float spriteY = sprite.texture.height * (100f / sprite.pixelsPerUnit);

                    switch (f.ImagePosition)
                    {
                        case FrameImagePosition.Fill: //works
                            FaceImage.rectTransform.localPosition = Vector3.zero;
                            FaceImage.rectTransform.sizeDelta = canvasSize;
                            break;                        
                        case FrameImagePosition.Contain: //works
                            {
                                FaceImage.rectTransform.localPosition = Vector3.zero;

                                float imageRatio = (float)sprite.texture.width / (float)sprite.texture.height; //force float division!
                                float rectRatio = canvasSize.x / canvasSize.y;

                                if (imageRatio > rectRatio) //image is wider than rect
                                {                                    
                                    FaceImage.rectTransform.sizeDelta = new Vector2(canvasSize.x, canvasSize.x / imageRatio);
                                }
                                else //image is narrower than rect
                                {                                    
                                    FaceImage.rectTransform.sizeDelta = new Vector2(canvasSize.y * imageRatio, canvasSize.y);
                                }
                            }
                            break;
                        case FrameImagePosition.Cover: //works
                            {
                                FaceImage.rectTransform.localPosition = Vector3.zero;

                                float imageRatio = (float)sprite.texture.width / (float)sprite.texture.height;
                                float rectRatio = canvasSize.x / canvasSize.y;

                                if (imageRatio > rectRatio) //image is wider than rect
                                {                                    
                                    FaceImage.rectTransform.sizeDelta = new Vector2(canvasSize.y * imageRatio, canvasSize.y);
                                }
                                else //image is narrower than rect
                                {                                    
                                    FaceImage.rectTransform.sizeDelta = new Vector2(canvasSize.x, canvasSize.x / imageRatio);
                                }
                            }
                            break;
                        case FrameImagePosition.Character: //works
                            FaceImage.rectTransform.localPosition = new Vector3(0, faceYOffset + (GameParams.DialogueDrawPortraitHigh ? 140 : 100), 0);
                            FaceImage.rectTransform.sizeDelta = new Vector2(spriteX, spriteY);
                            break;
                        case FrameImagePosition.CharacterBottom: //works
                            {
                                float yPos = (-(canvasSize.y / 2f)) + (spriteY / 2f); //I think we want SpriteY and not pixels directly

                                FaceImage.rectTransform.localPosition = new Vector3(0, yPos, 0);
                                FaceImage.rectTransform.sizeDelta = new Vector2(spriteX, spriteY);
                            }
                            break;
                        case FrameImagePosition.Battler: //deliberately broken
                            CDebug.LogEx($"FrameImagePosition {f.ImagePosition} is not supported!", LogLevel.Warning, this);
                            FaceImage.rectTransform.localPosition = Vector3.zero;
                            FaceImage.rectTransform.sizeDelta = new Vector2(spriteX, spriteY);
                            break;
                        default: //works
                            FaceImage.rectTransform.localPosition = Vector3.zero;
                            FaceImage.rectTransform.sizeDelta = new Vector2(spriteX, spriteY); ;
                            break;
                    }

                    FaceImage.sprite = sprite;
                    FaceImage.gameObject.SetActive(true);
                }
                else
                {
                    if (GameParams.DialogueVerboseLogging)
                        CDebug.LogEx($"Couldn't find face sprite Dialogue/char/{f.Image}", LogLevel.Verbose, this);
                }
            }

            //present camera
            try
            {
                if (!string.IsNullOrEmpty(f.CameraDirection) && !f.CameraDirection.StartsWith("Default", StringComparison.OrdinalIgnoreCase))
                {
                    if (CameraController == null)
                    {
                        var cameraGo = Instantiate(CameraPrefab, CoreUtils.GetWorldRoot());
                        CameraController = cameraGo.GetComponent<DialogueCameraController>();
                    }

                    CameraController.Activate(f.CameraDirection);
                }
                else
                {
                    CameraController.Ref()?.Deactivate();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to point camera ({f.CameraDirection})");
                Debug.LogException(e);
            }

            //present hidden objects
            var objectsToHide = f.Options.HideObjects;
            if (objectsToHide != null)
            {
                var hiddenObjectsToShow = HiddenObjects.Except(objectsToHide);
                UnhideObjects(hiddenObjectsToShow);

                var newObjectsToHide = objectsToHide.Except(HiddenObjects);
                HideObjects(newObjectsToHide);

                HiddenObjects.Clear();
                HiddenObjects.UnionWith(objectsToHide);
            }
            else
            {
                UnhideAllObjects();
            }

            //present text
            string nameText = Sub.Macro(f.NameText);
            TextTitle.text = nameText;
            string mainText = Sub.Macro(f.Text);
            TextMain.text = mainText;

            //save text to trace (note use of null instead of null-or-empty)
            traceNode.Speaker = (f.Options.TraceSpeaker == null) ? (string.IsNullOrEmpty(nameText) ? GetDefaultTraceSpeaker(f) : nameText) : Sub.Macro(f.Options.TraceSpeaker);
            traceNode.Text = (f.Options.TraceText == null) ? mainText : Sub.Macro(f.Options.TraceText);

            //clear buttons
            Navigator.ClearButtons();
            foreach (Transform t in ScrollChoiceContent)
            {
                Destroy(t.gameObject);
            }
            ScrollChoiceContent.DetachChildren();
            ButtonAlternateContinue.gameObject.SetActive(false);

            //present buttons and frame
            if (f is ChoiceFrame choiceFrame)
            {
                ChoicePanel.gameObject.SetActive(true);

                ScrollChoice.gameObject.SetActive(true);
                ButtonContinue.gameObject.SetActive(false);

                //ChoiceFrame cf = (ChoiceFrame)f;
                var buttons = new List<Button>();
                for (int i = 0; i < choiceFrame.Choices.Length; i++)
                {
                    ChoiceNode cn = choiceFrame.Choices[i];

                    string prependText = string.Empty;
                    bool showChoice = true;
                    bool lockChoice = false;

                    if (cn.ShowCondition != null)
                    {
                        showChoice = cn.ShowCondition.Evaluate();
                    }
                    if (cn.HideCondition != null && showChoice)
                    {
                        showChoice = !cn.HideCondition.Evaluate();
                    }

                    //skill checks
                    if (cn.SkillCheck != null)
                    {
                        bool isPossible = cn.SkillCheck.CheckIfPossible();

                        if (!GameParams.ShowImpossibleSkillChecks && !isPossible)
                            showChoice = false;

                        if (!GameParams.AttemptImpossibleSkillChecks && !isPossible)
                            lockChoice = true;

                        string passValue = cn.SkillCheck.CheckType == SkillCheckType.Soft ? $"{(int)(cn.SkillCheck.GetApproximatePassChance() * 100)}%" : cn.SkillCheck.Value.ToString();

                        prependText = $"[{Sub.Replace(cn.SkillCheck.Target, "RPG_AV")} {passValue}] ";
                    }

                    if (showChoice)
                    {
                        GameObject choiceGO = Instantiate<GameObject>(ButtonPrefab, ScrollChoiceContent);
                        Button b = choiceGO.GetComponent<Button>();
                        b.interactable = !lockChoice;
                        b.gameObject.SetActive(true);
                        b.transform.Find("Text").GetComponent<Text>().text = prependText + Sub.Macro(cn.Text);
                        int idx = i;
                        b.onClick.AddListener(delegate { OnChoiceButtonClick(idx); });
                        buttons.Add(b);
                    }

                }

                Navigator.AttachButtons(buttons);

            }
            else if (f is ImageFrame imageFrame)
            {
                string nextText = string.IsNullOrEmpty(f.NextText) ? Sub.Replace("DefaultNextText", "IGUI_DIALOGUE", false) : f.NextText;

                if(imageFrame.AllowSkip)
                {
                    ChoicePanel.gameObject.SetActive(false);
                    ScrollChoice.gameObject.SetActive(false);

                    Button b = ButtonAlternateContinue;
                    b.gameObject.SetActive(true);
                    b.transform.Find("Text").GetComponent<Text>().text = nextText;
                    Navigator.AttachButtons(new Button[] { b });
                }
                else
                {
                    ChoicePanel.gameObject.SetActive(false);
                    ButtonAlternateContinue.gameObject.SetActive(false);
                }

                if(imageFrame.HideSkip)
                {
                    CDebug.LogEx("Image frame HideSkip is deprecated and not implemented (use AllowSkip=false instead)", LogLevel.Warning, this);
                }

                if(imageFrame.UseTimer)
                {
                    StartWaitAndAdvance(imageFrame.TimeToShow);
                }
            }
            else if(f is TextFrame textFrame)
            {
                ChoicePanel.gameObject.SetActive(true);
                ScrollChoice.gameObject.SetActive(false);

                string nextText = string.IsNullOrEmpty(f.NextText) ? Sub.Replace("DefaultNextText", "IGUI_DIALOGUE", false) : f.NextText;

                Button b = ButtonContinue;
                b.gameObject.SetActive(textFrame.AllowSkip);
                b.transform.Find("Text").GetComponent<Text>().text = nextText;
                Navigator.AttachButtons(new Button[] { b });

                if (textFrame.UseTimer)
                {
                    StartWaitAndAdvance(textFrame.TimeToShow);
                }
            }
            else
            {
                throw new NotImplementedException($"Frame type {f.GetType().Name} is not supported");
            }

            //apply theme
            ApplyThemeToPanel();

            CurrentFrameObject = f;

            ScriptingModule.CallNamedHooked("DialogueOnPresent", this, CurrentFrameObject);
            TryCallScript(f?.Scripts?.OnPresent, f);

            if (f.Options.TraceIgnore)
                traceNode.Ignored = true;
            Trace.Nodes.Add(traceNode);
        }

        private void ApplyThemeToPanel()
        {
            if (ApplyTheme && CoreParams.UIThemeMode == UIThemePolicy.Auto)
            {
                var uiModule = CCBase.GetModule<UIModule>();
                if (!string.IsNullOrEmpty(OverrideTheme))
                {
                    uiModule.ApplyThemeRecurse(ChoicePanel, uiModule.GetThemeByName(OverrideTheme));
                    uiModule.ApplyThemeRecurse(ButtonAlternateContinue.transform, uiModule.GetThemeByName(OverrideTheme));
                }
                else
                {
                    uiModule.ApplyThemeRecurse(ChoicePanel);
                    uiModule.ApplyThemeRecurse(ButtonAlternateContinue.transform);
                }
            }
        }

        public void OnChoiceButtonClick(int idx)
        {
            AbortWaitAndAdvance();

            TryCallScript(CurrentFrameObject?.Scripts?.OnChoice);

            string choice = null;
            if (CurrentFrameObject is ChoiceFrame)
            {
                var cf = (ChoiceFrame)CurrentFrameObject;

                ChoiceNode choiceNode = cf.Choices[idx];
                if (choiceNode.SkillCheck != null)
                {
                    choice = choiceNode.SkillCheck.EvaluateSkillCheck();
                }
                else if (choiceNode.NextConditional != null)
                {
                    choice = choiceNode.EvaluateConditional();

                    if (choice == null)
                        choice = choiceNode.Next;
                }
                else
                {
                    choice = choiceNode.Next;
                }

                //exec microscripts
                if (choiceNode.NextMicroscript != null)
                {
                    choiceNode.EvaluateMicroscript();
                }
                if (GameParams.DialogueAlwaysExecuteFrameMicroscript && cf.NextMicroscript != null)
                {
                    cf.EvaluateMicroscript();
                }

                //handle trace
                Trace.Nodes.Add(new DialogueTraceNode()
                {
                    Choice = idx,
                    Ignored = CurrentFrameObject.Options.TraceIncludeChoices ? choiceNode.TraceIgnore : !choiceNode.TraceShow,
                    Path = $"{CurrentSceneName}.{CurrentFrameName}",
                    Speaker = choiceNode.TraceSpeaker == null ? GetDefaultTraceSpeaker(CurrentFrameObject) : Sub.Macro(choiceNode.TraceSpeaker),
                    Text = choiceNode.TraceText == null ? choiceNode.Text : Sub.Macro(choiceNode.TraceText)
                });
            }
            else
            {
                if (CurrentFrameObject.NextConditional != null && CurrentFrameObject.NextConditional.Length > 0)
                    choice = CurrentFrameObject.EvaluateConditional();
                else
                    choice = CurrentFrameObject.Next;

                if (CurrentFrameObject.NextMicroscript != null)
                {
                    CurrentFrameObject.EvaluateMicroscript();
                }

                //handle trace
                Trace.Nodes.Add(new DialogueTraceNode() { 
                    Choice = -1, 
                    Ignored = !CurrentFrameObject.Options.TraceIncludeNextText, 
                    Path = $"{CurrentSceneName}.{CurrentFrameName}",
                    Speaker = CurrentFrameObject.Options.TraceNextTextSpeaker == null ?  GetDefaultTraceSpeaker(CurrentFrameObject) : Sub.Macro(CurrentFrameObject.Options.TraceNextTextSpeaker),
                    Text = CurrentFrameObject.Options.TraceNextTextText == null ? CurrentFrameObject.NextText : Sub.Macro(CurrentFrameObject.Options.TraceNextTextText)
                });
            }



            //Debug.Log(choice);

            GotoNext(choice);

        }        

        //this one really isn't flexible enough to be useful...
        private KeyValuePair<string, string> ParseLocation(string loc)
        {
            if (!loc.Contains("."))
                if(GameParams.DialogueDefaultToThisScene)
                    return new KeyValuePair<string, string>(null, loc);
                else
                    return new KeyValuePair<string, string>(loc, null);

            var arr = loc.Split('.');
            return new KeyValuePair<string, string>(arr[0], arr[1]);
        }

        private void GotoNext(string next)
        {
            var nextLoc = ParseLocation(next);

            
            if(string.IsNullOrEmpty(nextLoc.Key) || nextLoc.Key == "this" || nextLoc.Key == CurrentSceneName)
            {
                if (nextLoc.Value == "default")
                    PresentNewFrame(CurrentScene.Default);
                else
                    PresentNewFrame(nextLoc.Value);
            }            
            else if(nextLoc.Key == "meta")
            {
                //probably the only one carried over from Garlic Gang or Katana
                if (nextLoc.Value == "return")
                {
                    CloseDialogue();
                }                
            }
            else if (nextLoc.Key == "shop")
            {
                var container = GameState.Instance.ContainerState[nextLoc.Value];
                ContainerModal.PushModal(GameState.Instance.PlayerRpgState.Inventory, container, true, null); //TODO we could add in "return from shop" with not _too_ much difficulty
                CloseDialogue();
            }
            else if (nextLoc.Key == "scene")
            {
                CloseDialogue();

                //this has never been tested and I would not expect it to work in practise

                var sceneController = BaseSceneController.Current;

                if (sceneController != null)
                {
                    //extract additional data if possible
                    string spawnPoint = string.Empty;
                    var arr = next.Split('.');
                    if (arr.Length >= 3)
                        spawnPoint = arr[2];

                    //clean exit
                    WorldUtils.ChangeScene(nextLoc.Value, spawnPoint, Vector3.zero, Vector3.zero);
                }
                else
                {
                    SharedUtils.ChangeScene(nextLoc.Value);
                }

            }
            else if (nextLoc.Key == "script")
            {
                CloseDialogue();

                Scripting.ScriptingModule.Call(nextLoc.Value, new Scripting.ScriptExecutionContext() { Caller = this }, null);
            }
            else
            {
                LoadScene(nextLoc.Key); //this loads a dialogue scene, not a Unity scene (it's confusing right?)
                if (nextLoc.Value == "default")
                    PresentNewFrame(CurrentScene.Default);
                else
                    PresentNewFrame(nextLoc.Value);
            }

        }        

        public void CloseDialogue()
        {
            AbortWaitAndAdvance();
            ScriptingModule.CallNamedHooked("DialogueOnClose", this);
            CurrentDialogue = null;            
            LockPauseModule.UnpauseGame(this.gameObject);
            AudioPlayer.Instance.ClearMusic(MusicSlot.Cinematic);
            if (CameraController)
                Destroy(CameraController.gameObject);
            UnhideAllObjects();
            CurrentTarget = null;
            Destroy(this.gameObject);
            if(CurrentCallback != null)
            {
                try
                {
                    CurrentCallback();
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    CurrentCallback = null;
                }
            }
        }

        private void TryCallScript(string script, Frame currentFrameObject = null)
        {
            if (currentFrameObject == null)
                currentFrameObject = CurrentFrameObject;

            if (!string.IsNullOrEmpty(script))
            {
                try
                {
                    ScriptingModule.Call(script, new ScriptExecutionContext() { Activator = gameObject, Caller = this }, currentFrameObject);
                }
                catch(Exception e)
                {
                    Debug.LogError($"[DialogueController] Failed to execute script \"{script}\" ({e.GetType().Name})");
                    if (ConfigState.Instance.UseVerboseLogging)
                        Debug.LogException(e);
                }
            }
        }

        private void HideObjects(IEnumerable<string> objects)
        {
            foreach (var obj in objects)
            {
                try
                {
                    var go = WorldUtils.FindObjectByTID(ResolveSpecialObjectName(obj));
                    go.SetActive(false);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DialogueController] Failed to hide object \"{obj}\" ({e.GetType().Name})");
                    if (ConfigState.Instance.UseVerboseLogging)
                        Debug.LogException(e);
                }
            }
        }

        private void UnhideObjects(IEnumerable<string> objects)
        {
            foreach(var obj in objects)
            {
                try
                {
                    var go = WorldUtils.FindObjectByTID(ResolveSpecialObjectName(obj));
                    go.SetActive(true);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DialogueController] Failed to unhide object {obj} ({e.GetType().Name})");
                    if (ConfigState.Instance.UseVerboseLogging)
                        Debug.LogException(e);
                }
            }
        }

        private string ResolveSpecialObjectName(string objectName)
        {
            if (objectName.StartsWith("#")) //interpret this literally
                return objectName.TrimStart('#');

            //resolve Player and Target
            if (objectName.Equals("Player", StringComparison.OrdinalIgnoreCase))
                return WorldUtils.GetPlayerObject().name;

            if (objectName.Equals("Target", StringComparison.OrdinalIgnoreCase))
                return CurrentTarget;

            return objectName;
        }

        private void UnhideAllObjects()
        {
            UnhideObjects(HiddenObjects);
            HiddenObjects.Clear();
        }

        private void AbortWaitAndAdvance()
        {
            if (WaitAndAdvanceCoroutine != null)
            {
                StopCoroutine(WaitAndAdvanceCoroutine);
                WaitAndAdvanceCoroutine = null;
            }
        }

        private void StartWaitAndAdvance(float timeToWait)
        {
            AbortWaitAndAdvance();
            WaitAndAdvanceCoroutine = StartCoroutine(CoWaitAndAdvance(timeToWait));
        }

        private IEnumerator CoWaitAndAdvance(float timeToWait)
        {
            yield return new WaitForSecondsEx(timeToWait, lowestPauseState: PauseLockType.AllowCutscene);

            yield return null;

            WaitAndAdvanceCoroutine = null;
            OnChoiceButtonClick(0);
            
        }

        private string GetDefaultTraceSpeaker(Frame f)
        {
            switch (f.Options.TraceDefaultSpeaker)
            {
                case TraceDefaultSpeaker.None:
                    return "";
                case TraceDefaultSpeaker.PlayerLookup:
                    throw new NotImplementedException();
                case TraceDefaultSpeaker.PlayerName:
                    return GameState.Instance.PlayerRpgState.DisplayName;
                default:
                    throw new NotImplementedException();
            }
        }


    }
}