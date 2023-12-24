using CommonCore;
using CommonCore.Audio;
using CommonCore.Config;
using CommonCore.Input;
using CommonCore.LockPause;
using CommonCore.RpgGame.Dialogue;
using CommonCore.Scripting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CommonCore.RpgGame.Dialogue
{
    public class VnxController : MonoBehaviour
    {
        [Header("Components")]
        public DialogueController DialogueController;
        public RectTransform ChoicePanel;
        public Button ContinueButton;
        public Text MainText;

        [Header("Stage")]
        public RectTransform Stage;

        [Header("Graphics")]
        public Sprite HalfPanel;
        public Sprite FullPanel;

        [Header("Options")]
        public string BeepSoundName = "DialogueBeep";

        private AudioPlayer AudioPlayer;

        private Dictionary<string, VnActor> Actors = new Dictionary<string, VnActor>();

        private Coroutine TypeOnCoroutine;
        private bool PanelClicked;

        //current frame data
        private Frame CurrentFrame = null;
        private bool PlayBeepOnAdvance = false;
        private string PostPresentScript = null;
        private bool UseTypeOn = false;
        private string FrameText = null;

        public void Init()
        {
            if(!GameParams.DialogueUseVnx)
            {
                enabled = false;
                return;
            }

            Stage.gameObject.SetActive(true);
            AudioPlayer = CCBase.GetModule<AudioModule>().AudioPlayer;
        }

        public void OnDialogueOpen(DialogueController dialogueController)
        {
            //DialogueController = dialogueController;
            //Debug.Log("OnDialogueOpen");
        }

        public void OnDialogueClose()
        {
            //Debug.Log("OnDialogueClose");
        }

        public void OnDialoguePresent(Frame currentFrame)
        {
            //Debug.Log($"OnDialoguePresent {GetCurrentSceneName()}.{GetCurrentFrameName()}");

            CurrentFrame = currentFrame;

            PlayBeepOnAdvance = true; //enabled by default
            PostPresentScript = null;
            UseTypeOn = true;

            UpdateDataFromFrame();

            MutateChoicePanel();

            DrawStage();

            HandleTypeOn();

            TryCallScript(PostPresentScript, currentFrame);
        }

        public void OnDialogueAdvance(KeyValuePair<string, string> nextLocation)
        {
            //Debug.Log($"OnDialogueAdvance {nextLocation.Key}.{nextLocation.Value}");

            if(PlayBeepOnAdvance && CurrentVnConfig.EnableAdvanceBeep)
            {
                AudioPlayer.PlayUISound(BeepSoundName);
            }

            AbortTypeOn();
        }

        public void OnDialoguePanelClick(BaseEventData data)
        {
            var pointerData = data as PointerEventData;
            //Debug.Log("Click! " + pointerData.pointerPress);
            if(pointerData.pointerPress == ChoicePanel.gameObject)
            {
                PanelClicked = true;
            }
        }

        private void UpdateDataFromFrame()
        {
            var frameObject = CurrentFrame.RawData as JObject;

            if(frameObject == null)
            {
                Debug.LogError("[VnxController] no frame jobject!");
                return;
            }

            //check for VnOptions, if clear is declared then clear (other options later?)
            if (!frameObject["vnOptions"].IsNullOrEmpty())
            {
                var options = CoreUtils.InterpretJson<VnOptions>(frameObject["vnOptions"]);
                if(options.ClearStage)
                {
                    Actors.Clear();
                }

                if(!options.PlayAdvanceBeep)
                {
                    PlayBeepOnAdvance = false;
                }

                if(!options.TypeOn)
                {
                    UseTypeOn = false;
                }

                PostPresentScript = options.PostPresentScript;
            }

            if (!frameObject["vnStage"].IsNullOrEmpty() && frameObject["vnStage"] is JObject vnStageObject)
            {
                foreach(var actorKvp in vnStageObject)
                {
                    if(!Actors.ContainsKey(actorKvp.Key))
                    {
                        var actor = CoreUtils.InterpretJson<VnActor>(actorKvp.Value);
                        Actors[actorKvp.Key] = actor;
                    }
                    else
                    {
                        using (var sr = actorKvp.Value.CreateReader())
                        {
                            JsonSerializer.Create(CoreParams.DefaultJsonSerializerSettings).Populate(sr, Actors[actorKvp.Key]);
                        }
                        
                    }
                    
                }
            }

            if (!frameObject["vnSpeaker"].IsNullOrEmpty() && !string.IsNullOrEmpty(frameObject["vnSpeaker"].ToString()))
            {
                string newSpeaker = frameObject["vnSpeaker"].ToString();
                foreach (var actorKvp in Actors)
                {
                    if(actorKvp.Key.Equals(newSpeaker))
                    {
                        actorKvp.Value.Fade = false;
                    }
                    else
                    {
                        actorKvp.Value.Fade = true;
                    }
                }
            }
        }

        private void MutateChoicePanel()
        {

            //set height and sprite of choice panel
            var height = CurrentFrame.Options.PanelHeight;
            var panelImage = ChoicePanel.GetComponent<Image>();
            if (height == ChoicePanelHeight.Half)
            {
                if(HalfPanel != null)
                {
                    panelImage.sprite = HalfPanel;
                    //panelImage.type = HalfPanel.
                    ChoicePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, HalfPanel.rect.height * (100 / HalfPanel.pixelsPerUnit));
                    ChoicePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, HalfPanel.rect.width * (100 / HalfPanel.pixelsPerUnit));
                }                
            }
            else if (height == ChoicePanelHeight.Full || height == ChoicePanelHeight.Default)
            {
                if(FullPanel != null)
                {
                    panelImage.sprite = FullPanel;
                    //panelImage.type = HalfPanel.
                    ChoicePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, FullPanel.rect.height * (100 / FullPanel.pixelsPerUnit));
                    ChoicePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, FullPanel.rect.width * (100 / FullPanel.pixelsPerUnit));
                }                
            }
            
        }

        private void HandleTypeOn()
        {
            //AbortTypeOn(); //safety?

            //if type on is enabled, clear test and disable button
            if(UseTypeOn && CurrentFrame is TextFrame && CurrentVnConfig.TypeOnSpeed > 0 && !string.IsNullOrEmpty(MainText.text))
            {
                ContinueButton.gameObject.SetActive(false);

                //probably grab actual text from the dialogue box because then we don't have to run stringsub again
                FrameText = MainText.text;
                MainText.text = String.Empty;

                TypeOnCoroutine = StartCoroutine(CoTypeOn());
            }
        }

        private IEnumerator CoTypeOn()
        {
            PanelClicked = false;

            bool armed = true;
            const float baseInterval = 0.05f;
            float interval = baseInterval * (1 / CurrentVnConfig.TypeOnSpeed);

            char[] characters = FrameText.Replace("<i>", "").Replace("</i>", "").Replace("<b>", "").Replace("</b>", "").ToCharArray(); //really crude hack lol

            //yield return null;
            armed = !(MappedInput.GetButton(CommonCore.Input.DefaultControls.Submit) || MappedInput.GetButton(CommonCore.Input.DefaultControls.Use) || MappedInput.GetButton(CommonCore.Input.DefaultControls.Fire));
            yield return null;
            for (int i = 0; i < characters.Length; i++)
            {
                MainText.text = MainText.text + characters[i];

                if(!armed && !(MappedInput.GetButton(CommonCore.Input.DefaultControls.Submit) || MappedInput.GetButton(CommonCore.Input.DefaultControls.Use) || MappedInput.GetButton(CommonCore.Input.DefaultControls.Fire)))
                {
                    armed = true;
                }
                
                if (armed && (MappedInput.GetButton(CommonCore.Input.DefaultControls.Submit) || MappedInput.GetButton(CommonCore.Input.DefaultControls.Use) || MappedInput.GetButton(CommonCore.Input.DefaultControls.Fire)))
                {
                    yield return null;
                }
                else
                {
                    float elapsed = 0;
                    while (elapsed < interval)
                    {
                        var pls = LockPauseModule.GetPauseLockState();
                        if (pls == null || (pls >= PauseLockType.AllowCutscene))
                        {
                            //handle skip
                            if (MappedInput.GetButtonUp(CommonCore.Input.DefaultControls.Submit) || MappedInput.GetButtonUp(CommonCore.Input.DefaultControls.Use) || MappedInput.GetButtonUp(CommonCore.Input.DefaultControls.Fire))
                            {
                                if(armed)
                                {
                                    yield return null;

                                    FinishTypeOn();
                                    TypeOnCoroutine = null;
                                    yield break;
                                }
                                else
                                {
                                    armed = true;
                                }
                            }
                        }

                        yield return null;
                        elapsed += Time.unscaledDeltaTime;
                    }
                }

                //yield return new WaitForSecondsEx(interval, false, PauseLockType.AllowCutscene, true);
            }

            yield return null;

            FinishTypeOn();
            TypeOnCoroutine = null;
        }

        private void FinishTypeOn()
        {
            MainText.text = FrameText;
            ContinueButton.gameObject.SetActive(true);
        }

        private void AbortTypeOn()
        {
            if(TypeOnCoroutine != null)
            {
                StopCoroutine(TypeOnCoroutine);
                TypeOnCoroutine = null;
            }
        }

        private void DrawStage()
        {
            ClearStage();

            foreach(var actorKvp in Actors.OrderBy(kvp => kvp.Value.Z)) //yolo linq
            {
                DrawActor(actorKvp.Key, actorKvp.Value);
            }
        }

        private void ClearStage()
        {
            foreach(Transform t in Stage)
            {
                Destroy(t.gameObject);
            }
        }

        private void DrawActor(string name, VnActor actor)
        {
            //Debug.Log("Drawing " + name + " " + actor.Visible);

            if (!actor.Visible) //"optimization"
                return;

            var sprite = CoreUtils.LoadResource<Sprite>("Dialogue/char/" + actor.Image);
            if(sprite == null)
            {
                Debug.LogWarning($"[VnxController] sprite \"{actor.Image}\" can't be found");
            }

            var actorObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

            var actorTransform = (RectTransform)actorObj.transform;
            actorTransform.SetParent(Stage);
            actorTransform.anchorMin = new Vector2(0.5f, 0);
            actorTransform.anchorMax = new Vector2(0.5f, 0);

            var actorImage = actorObj.GetComponent<Image>();

            float scale = 1f, width = 100f, height = 200f;
            if(sprite != null)
            {
                actorImage.sprite = sprite;
                actorTransform.pivot = new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height);
                scale = 100f / sprite.pixelsPerUnit;
                width = sprite.rect.width;
                height = sprite.rect.height;
            }
            else
            {
                actorTransform.pivot = new Vector2(0.5f, 0.5f);
            }
            actorTransform.anchoredPosition3D = new Vector3(actor.X, actor.Y, actor.Z);
            actorTransform.sizeDelta = new Vector2(width, height);

            if(actor.Flip)
            {
                actorTransform.localScale = new Vector3(-1, 1, 1) * scale;
            }
            else
            {
                actorTransform.localScale = Vector3.one * scale;
            }

            if(CurrentVnConfig.AllowFade && actor.Fade)
            {
                actorImage.color = new Color(0.5f, 0.5f, 0.5f, 1.0f); //TODO better fade fx
            }
            
        }

        private void TryCallScript(string script, Frame currentFrameObject)
        {
            if (!string.IsNullOrEmpty(script))
            {
                try
                {
                    ScriptingModule.Call(script, new ScriptExecutionContext() { Activator = gameObject, Caller = this }, currentFrameObject);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[VnxController] Failed to execute script \"{script}\" ({e.GetType().Name})");
                    if (ConfigState.Instance.UseVerboseLogging)
                        Debug.LogException(e);
                }
            }
        }

        //hackcessors
        private string GetCurrentSceneName()
        {
            return DialogueController.GetType().GetField("CurrentSceneName", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(DialogueController).ToString();
        }

        private string GetCurrentFrameName()
        {
            return DialogueController.GetType().GetField("CurrentFrameName", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(DialogueController).ToString();
        }

        private VnConfig CurrentVnConfig
        {
            get
            {
                ConfigState.Instance.AddCustomVarIfNotExists("VnxConfig", () => new VnConfig());
                return (VnConfig)ConfigState.Instance.CustomConfigVars["VnxConfig"];
            }
        }

    }
}


