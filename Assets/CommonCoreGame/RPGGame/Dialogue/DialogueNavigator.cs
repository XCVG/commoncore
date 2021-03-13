using CommonCore.Input;
using CommonCore.LockPause;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CommonCore.RpgGame.Dialogue
{

    /// <summary>
    /// Controller that handles keyboard/controller navigation in a dialogue system
    /// </summary>
    /// <remarks>
    /// <para>Like everything in Dialogue 1.5, it's hacky as fuck</para>
    /// </remarks>
    public class DialogueNavigator : MonoBehaviour
    {
        //TODO handle scroll view?!

        private int SelectedButton = -1;
        private List<Button> CurrentButtons = new List<Button>();

        private EventSystem EventSystem
        {
            get
            {
                if (EventSystem.current == null)
                    Instantiate(CoreUtils.LoadResource<GameObject>("UI/DefaultEventSystem"));
                return EventSystem.current;
            }
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            if (!((LockPauseModule.GetPauseLockState() ?? PauseLockType.AllowCutscene) > PauseLockType.AllowMenu)) //handle pauses
                return;

            if (LockPauseModule.GetInputLockState() == InputLockType.All)
                return;            

            //this probably won't work, but it's supposed to reselect after a return from the menu
            if(EventSystem.currentSelectedGameObject == null || !EventSystem.currentSelectedGameObject.activeInHierarchy)
            {
                SelectButton(SelectedButton);
            }            

            //update our view of SelectedButton
            if(EventSystem.currentSelectedGameObject != CurrentButtons[SelectedButton].gameObject)
            {
                int newIndex = CurrentButtons.IndexOf(EventSystem.currentSelectedGameObject.GetComponent<Button>());
                if(newIndex >= 0)
                    SelectedButton = newIndex;

                //Debug.Log($"selected {SelectedButton}");
            }
        }

        public void ClearButtons()
        {
            //clear buttons and selection
            CurrentButtons.Clear();
            EventSystem.SetSelectedGameObject(null);
            SelectedButton = -1;
        }

        public void AttachButtons(IEnumerable<Button> buttons)
        {
            //set up buttons
            ClearButtons();

            //add buttons
            CurrentButtons.AddRange(buttons);

            //setup nav (will this work?)
            if (CurrentButtons.Count > 1)
            {
                for (int i = 0; i < CurrentButtons.Count; i++)
                {
                    var button = CurrentButtons[i];
                    var nav = button.navigation;
                    nav.mode = Navigation.Mode.Explicit;

                    //TODO allow cycling?
                    if (i > 0)
                        nav.selectOnUp = CurrentButtons[i - 1];

                    if (i < CurrentButtons.Count - 1)
                        nav.selectOnDown = CurrentButtons[i + 1];

                    button.navigation = nav;
                }
            }

            //select first button
            SelectedButton = 0;
            if(CurrentButtons.Count > 0)
                SelectButton(0);
        }

        private void SelectButton(int index)
        {
            var button = CurrentButtons[index];
            button.Select();
            button.OnSelect(new BaseEventData(EventSystem));
        }
    }
}