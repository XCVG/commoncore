using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CommonCore.State;
using CommonCore.StringSub;
using CommonCore.LockPause;
using CommonCore.RpgGame.UI;
using CommonCore.World;

namespace CommonCore.RpgGame.Dialogue
{

    public class DialogueController : MonoBehaviour
    {
        public static string CurrentDialogue { get; set; }
        public static DialogueFinishedDelegate CurrentCallback { get; set; }
        //public static bool AutoPauseGame { get; set; }

        public Text TextTitle;
        public Text TextMain;
        //public Button[] ButtonsChoice;
        public GameObject ButtonPrefab;
        public Button ButtonContinue;
        public ScrollRect ScrollChoice;
        public RectTransform ScrollChoiceContent;

        public AudioSource VoiceAudioSource;

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

            var loc = ParseLocation(CurrentDialogue); //TODO we actually need to parse it "backwards" here...

            if(loc.Key == null)
            {
                //use default
                LoadScene(loc.Value);
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
            PresentNewFrame(CurrentSceneFrames[s]);
        }
        
        private void PresentNewFrame(Frame f) //args?
        {
            //present audio
            if (VoiceAudioSource.isPlaying)
                VoiceAudioSource.Stop();
            string voicePath = string.Format("DialogueVoice/{0}/{1}", CurrentSceneName, CurrentFrameName);
            var voiceClip = CoreUtils.LoadResource<AudioClip>(voicePath);
            if(voiceClip != null)
            {
                VoiceAudioSource.clip = voiceClip;
                VoiceAudioSource.Play();
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
                    //will need to be redone to effectively deal with conditionals
                    ChoiceNode cn = cf.Choices[i];
                    bool showChoice = true;
                    if(cn.ShowCondition != null)
                    {
                        showChoice = cn.ShowCondition.Evaluate();
                    }
                    if(cn.HideCondition != null && showChoice)
                    {
                        showChoice = !cn.HideCondition.Evaluate();
                    }

                    if(showChoice)
                    {
                        GameObject choiceGO = Instantiate<GameObject>(ButtonPrefab, ScrollChoiceContent);
                        Button b = choiceGO.GetComponent<Button>();
                        b.gameObject.SetActive(true);
                        b.transform.Find("Text").GetComponent<Text>().text = Sub.Macro(cn.Text);
                        int idx = i;
                        b.onClick.AddListener(delegate { OnChoiceButtonClick(idx); });
                    }
                    
                }
            }
            else // if(f is TextFrame)
            {
                ScrollChoice.gameObject.SetActive(false);

                Button b = ButtonContinue;
                b.gameObject.SetActive(true);
                b.transform.Find("Text").GetComponent<Text>().text = "Continue..."; //TODO nextText support
            }

            CurrentFrameObject = f;
        }

        public void OnChoiceButtonClick(int idx)
        {
            string choice = null;
            if(CurrentFrameObject is ChoiceFrame)
            {
                var cf = (ChoiceFrame)CurrentFrameObject;

                if(cf.Choices[idx].NextConditional != null)
                {
                    choice = cf.Choices[idx].EvaluateConditional();
                    
                    if(choice == null)
                        choice = cf.Choices[idx].Next;
                }
                else
                {
                    choice = cf.Choices[idx].Next;
                } 

                //exec microscripts
                if(cf.Choices[idx].NextMicroscript != null)
                {
                    cf.Choices[idx].EvaluateMicroscript();
                }
            }
            else
            {
                if (CurrentFrameObject.NextConditional != null && CurrentFrameObject.NextConditional.Length > 0)
                    choice = CurrentFrameObject.EvaluateConditional();
                else
                    choice = CurrentFrameObject.Next;

                if(CurrentFrameObject.NextMicroscript != null)
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
                return new KeyValuePair<string, string>(null, loc);

            var arr = loc.Split('.');
            return new KeyValuePair<string, string>(arr[0], arr[1]);
        }

        private void GotoNext(string next)
        {
            var nextLoc = ParseLocation(next);

            if(string.IsNullOrEmpty(nextLoc.Key) || nextLoc.Key == "this" || nextLoc.Key == CurrentSceneName)
            {
                PresentNewFrame(nextLoc.Value);
            }
            else if(nextLoc.Key == "meta")
            {
                //probably the only one carried over from Garlic Gang or Katana
                if(nextLoc.Value == "return")
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
                PresentNewFrame(nextLoc.Value);
            }

        }

        private void CloseDialogue()
        {
            CurrentDialogue = null;
            LockPauseModule.UnpauseGame(this.gameObject);
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