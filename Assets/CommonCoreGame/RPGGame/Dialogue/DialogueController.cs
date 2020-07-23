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

namespace CommonCore.RpgGame.Dialogue
{

    public class DialogueController : MonoBehaviour
    {
        public static string CurrentDialogue { get; set; }
        public static DialogueFinishedDelegate CurrentCallback { get; set; }
        //public static bool AutoPauseGame { get; set; }

        public Text TextTitle;
        public Text TextMain;
        public Image BackgroundImage;
        public Image FaceImage;
        //public Button[] ButtonsChoice;
        public GameObject ButtonPrefab;
        public Button ButtonContinue;
        public ScrollRect ScrollChoice;
        public RectTransform ScrollChoiceContent;

        public AudioSource VoiceAudioSource;
        public GameObject CameraPrefab;
        public DialogueCameraController CameraController;

        private string CurrentFrameName;
        private string CurrentSceneName;
        private DialogueScene CurrentScene;
        private Dictionary<string, Frame> CurrentSceneFrames { get { return CurrentScene.Frames; } }

        //private string CurrentFrameName;
        private Frame CurrentFrameObject;

        void Awake()
        {
            name = "DialogueSystem";
        }

        void Start()
        {
            //GameState.Instance.CurrentDialogue = "intro.intro1";

            //if (AutoPauseGame)
            //    LockPauseModule.PauseGame(this.gameObject);

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
            CurrentScene = DialogueModule.GetDialogue(scene);
            CurrentSceneName = scene;
        }

        private void PresentNewFrame(string s)
        {
            CurrentFrameName = s;
            if (!CurrentSceneFrames.ContainsKey(s))
                Debug.LogError($"[Dialogue] Can't find frame \"{s}\"");
            PresentNewFrame(CurrentSceneFrames[s]);
        }
        
        private void PresentNewFrame(Frame f)
        {
            //special handling for blank frames
            if(f is BlankFrame)
            {
                CurrentFrameObject = f;
                OnChoiceButtonClick(0);
                return;
            }

            //present music
            if (!string.IsNullOrEmpty(f.Music))
            {
                if(!(AudioPlayer.Instance.IsMusicSetToPlay(MusicSlot.Cinematic) && AudioPlayer.Instance.GetMusicName(MusicSlot.Cinematic) == f.Music))
                {
                    AudioPlayer.Instance.SetMusic(f.Music, MusicSlot.Cinematic, 1.0f, true, false);
                    AudioPlayer.Instance.StartMusic(MusicSlot.Cinematic);
                }                
            }
            else if(f.Music != null) //null = no change, empty = no music
            {
                AudioPlayer.Instance.ClearMusic(MusicSlot.Cinematic);
            }

            //present audio
            if (VoiceAudioSource.isPlaying)
                VoiceAudioSource.Stop();
            var voiceClip = CCBase.GetModule<AudioModule>().GetSound($"{CurrentSceneName}/{CurrentFrameName}", SoundType.Voice, !GameParams.DialogueVerboseLogging); //GetModule<T> is now preferred
            if (voiceClip != null)
            {
                VoiceAudioSource.clip = voiceClip;
                VoiceAudioSource.Play();
            }

            //present background
            BackgroundImage.sprite = null;
            BackgroundImage.gameObject.SetActive(false);
            if(!string.IsNullOrEmpty(f.Background))
            {
                var sprite = CoreUtils.LoadResource<Sprite>("Dialogue/bg/" + f.Background);
                if(sprite != null)
                {
                    BackgroundImage.sprite = sprite;
                    BackgroundImage.gameObject.SetActive(true);
                }
                else
                {
                    if(GameParams.DialogueVerboseLogging)
                        CDebug.LogEx($"Couldn't find face sprite Dialogue/bg/{f.Background}", LogLevel.Verbose, this);
                }
            }

            //present image
            FaceImage.sprite = null;
            FaceImage.gameObject.SetActive(false);
            if(!string.IsNullOrEmpty(f.Image))
            {
                //attempt to present image
                var sprite = CoreUtils.LoadResource<Sprite>("Dialogue/char/" + f.Image);
                if(sprite != null)
                {
                    //Debug.Log(sprite.name);

                    float spriteX = sprite.texture.width * (100f / sprite.pixelsPerUnit);
                    float spriteY = sprite.texture.height * (100f / sprite.pixelsPerUnit);

                    switch (f.ImagePosition)
                    {
                        case FrameImagePosition.Fill:
                            FaceImage.rectTransform.localPosition = Vector3.zero;
                            FaceImage.rectTransform.sizeDelta = FaceImage.canvas.pixelRect.size;
                            break;
                        case FrameImagePosition.Character:
                            FaceImage.rectTransform.localPosition = new Vector3(0, 100, 0);
                            FaceImage.rectTransform.sizeDelta = new Vector2(spriteX, spriteY);
                            break;
                        default:
                            //center, no scale
                            FaceImage.rectTransform.localPosition = Vector3.zero;
                            FaceImage.rectTransform.sizeDelta = new Vector2(spriteX, spriteY);
                            break;
                    }

                    FaceImage.sprite = sprite;
                    FaceImage.gameObject.SetActive(true);
                }
                else
                {
                    if(GameParams.DialogueVerboseLogging)
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
            catch(Exception e)
            {
                Debug.LogError($"Failed to point camera ({f.CameraDirection})");
                Debug.LogException(e);
            }

            //present text
            TextTitle.text = Sub.Macro(f.NameText);
            TextMain.text = Sub.Macro(f.Text);

            //clear buttons
            foreach (Transform t in ScrollChoiceContent)
            {
                Destroy(t.gameObject);
            }
            ScrollChoiceContent.DetachChildren();

            //present buttons
            if (f is ChoiceFrame)
            {
                ScrollChoice.gameObject.SetActive(true);
                ButtonContinue.gameObject.SetActive(false);

                ChoiceFrame cf = (ChoiceFrame)f;
                for (int i = 0; i < cf.Choices.Length; i++)
                {
                    ChoiceNode cn = cf.Choices[i];

                    string prependText = string.Empty;
                    bool showChoice = true;
                    bool lockChoice = false;

                    if(cn.ShowCondition != null)
                    {
                        showChoice = cn.ShowCondition.Evaluate();
                    }
                    if(cn.HideCondition != null && showChoice)
                    {
                        showChoice = !cn.HideCondition.Evaluate();
                    }

                    //skill checks
                    if(cn.SkillCheck != null)
                    {
                        bool isPossible = cn.SkillCheck.CheckIfPossible();

                        if (!GameParams.ShowImpossibleSkillChecks && !isPossible)
                            showChoice = false;

                        if (!GameParams.AttemptImpossibleSkillChecks && !isPossible)
                            lockChoice = true;

                        string passValue = cn.SkillCheck.CheckType == SkillCheckType.Soft ? $"{(int)(cn.SkillCheck.GetApproximatePassChance() * 100)}%" : cn.SkillCheck.Value.ToString();

                        prependText = $"[{Sub.Replace(cn.SkillCheck.Target, "RPG_AV")} {passValue}] ";
                    }

                    if(showChoice)
                    {
                        GameObject choiceGO = Instantiate<GameObject>(ButtonPrefab, ScrollChoiceContent);
                        Button b = choiceGO.GetComponent<Button>();
                        b.interactable = !lockChoice;
                        b.gameObject.SetActive(true);
                        b.transform.Find("Text").GetComponent<Text>().text = prependText + Sub.Macro(cn.Text);
                        int idx = i;
                        b.onClick.AddListener(delegate { OnChoiceButtonClick(idx); });
                    }
                    
                }
            }
            else // if(f is TextFrame)
            {
                ScrollChoice.gameObject.SetActive(false);

                string nextText = string.IsNullOrEmpty(f.NextText) ? Sub.Replace("DefaultNextText", "IGUI_DIALOGUE", false) : f.NextText;

                Button b = ButtonContinue;
                b.gameObject.SetActive(true);
                b.transform.Find("Text").GetComponent<Text>().text = nextText;
            }

            CurrentFrameObject = f;
        }

        public void OnChoiceButtonClick(int idx)
        {
            string choice = null;
            if (CurrentFrameObject is ChoiceFrame)
            {
                var cf = (ChoiceFrame)CurrentFrameObject;

                if (cf.Choices[idx].SkillCheck != null)
                {
                    choice = cf.Choices[idx].SkillCheck.EvaluateSkillCheck();
                }
                else if (cf.Choices[idx].NextConditional != null)
                {
                    choice = cf.Choices[idx].EvaluateConditional();

                    if (choice == null)
                        choice = cf.Choices[idx].Next;
                }
                else
                {
                    choice = cf.Choices[idx].Next;
                }

                //exec microscripts
                if (cf.Choices[idx].NextMicroscript != null)
                {
                    cf.Choices[idx].EvaluateMicroscript();
                }
                if (GameParams.DialogueAlwaysExecuteFrameMicroscript && cf.NextMicroscript != null)
                {
                    cf.EvaluateMicroscript();
                }
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
                ContainerModal.PushModal(GameState.Instance.PlayerRpgState.Inventory, container, true, null);
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
                    Debug.LogWarning("DialogueController forced scene exit!");
                    SceneManager.LoadScene(nextLoc.Value); //BAD BAD BAD forced exit
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

        private void CloseDialogue()
        {
            CurrentDialogue = null;
            LockPauseModule.UnpauseGame(this.gameObject);
            AudioPlayer.Instance.ClearMusic(MusicSlot.Cinematic);
            if (CameraController)
                Destroy(CameraController.gameObject);
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

        
    }
}