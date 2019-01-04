using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonCore.LockPause;
using CommonCore.Rpg;
using CommonCore.State;
using CommonCore.StringSub;

namespace CommonCore.UI
{
    public delegate void LevelUpModalCallback();

    /*
     * Controller for levelling up modal window
     * The awkward naming is because the game-specific one was built first
     * It turned out a lot less elegant than I was hoping :(
     */
    public class DefaultLevelUpModal : BaseMenuController
    {
        private const string ItemPrefab = "IGUI_SkillItemTemplate";
        private const string SubList = "IGUI_STATS";
        private const string DescriptionList = "IGUI_DESCRIPTION";

        //Unity objects
        public Text HeaderText;
        public Button ConfirmButton;
        public Text PointsText;
        public GameObject LevelItemPrefab;
        public RectTransform ScrollContent;

        public GameObject DetailPanel;
        public Text DetailTitle;
        public Image DetailImage;
        public Text DetailDescription;
        public Text DetailLevel;
        public Button LevelUpButton;

        //self-explanatory
        public LevelUpModalCallback Callback;

        //private state
        private StatsSet NewStats;
        private int PotentialPoints;
        private int NewLevel;
        private int SelectedSkill;
        private Button[] SkillButtons;

        public override void Start()
        {
            base.Start();
            LockPauseModule.PauseGame(PauseLockType.AllowMenu, this);

            CalculateValues();
            PaintAll();
        }

        public override void OnDisable()
        {
            LockPauseModule.UnpauseGame(this);
        }

        private void CalculateValues()
        {
            var player = GameState.Instance.PlayerRpgState;
            int levels = RpgValues.LevelsForExperience(player);
            for (int i = 0, tLevel = player.Level; i < levels; i++, tLevel++)
            {
                PotentialPoints += RpgValues.PotentialPointsForLevel(tLevel, player);
            }

            NewLevel = player.Level + levels;

            //copy stats set
            NewStats = new StatsSet(player.BaseStats);
        }

        private void PaintAll()
        {
            //paint header
            HeaderText.text = string.Format("Welcome to level {0}", NewLevel);

            //write potential points
            PointsText.text = string.Format("Potential Points: {0}", PotentialPoints);

            //write skill list
            PaintSkills();

            //enable buttons if available
            ConfirmButton.interactable = (PotentialPoints == 0);

        }

        private void PaintSkills()
        {
            //double-check the prefab
            if(LevelItemPrefab == null)
            {
                LevelItemPrefab = Resources.Load<GameObject>("UI/" + ItemPrefab);
            }

            SelectedSkill = -1;

            List<Button> allButtons = new List<Button>();
            //the fact that we're using an enum actually helps us *a lot* as long as the end-user doesn't do something criminally stupid
            foreach (int value in Enum.GetValues(typeof(SkillType)))
            {
                string name = Enum.GetName(typeof(SkillType), value);

                GameObject skillGO = Instantiate<GameObject>(LevelItemPrefab, ScrollContent);
                skillGO.GetComponentInChildren<Text>().text = string.Format("{0} [{1}]", Sub.Replace(name, SubList), NewStats.Skills[value]);
                Button b = skillGO.GetComponent<Button>();
                int lexI = value;
                b.onClick.AddListener(delegate { OnSkillSelected(lexI, b); }); //scoping is weird here
                allButtons.Add(b);
            }
            SkillButtons = allButtons.ToArray();
        }

        private void UpdateValues()
        {
            //update buttons
            for(int i = 0; i < SkillButtons.Length; i++)
            {
                string name = Enum.GetName(typeof(SkillType), i);

                Button b = SkillButtons[i];
                b.GetComponentInChildren<Text>().text = string.Format("{0} [{1}]", Sub.Replace(name, SubList), NewStats.Skills[i]);
            }

            //update details
            DetailLevel.text = string.Format("{0}->{1}", NewStats.Skills[SelectedSkill], NewStats.Skills[SelectedSkill] + RpgValues.SkillGainForPoints(1));

            //if we're out of PP, set buttons
            LevelUpButton.interactable = (PotentialPoints > 0);
            ConfirmButton.interactable = (PotentialPoints == 0);

            PointsText.text = string.Format("Potential Points: {0}", PotentialPoints);
        }

        public void OnSkillSelected(int i, Button b)
        {
            //Debug.Log(Enum.GetName(typeof(SkillType), i));

            //TODO set selected index, paint detail, enable buttons if valid
            var skillName = Enum.GetName(typeof(SkillType), i);
            SelectedSkill = i;
            DetailPanel.SetActive(true);
            DetailTitle.text = Sub.Replace(skillName, SubList);
            DetailDescription.text = Sub.Exists(skillName, DescriptionList) ? Sub.Replace(skillName, DescriptionList) : string.Empty;
            DetailLevel.text = string.Format("{0}->{1}", NewStats.Skills[i], NewStats.Skills[i]+RpgValues.SkillGainForPoints(1));
            LevelUpButton.interactable = (PotentialPoints > 0);
        }

        //on actual level handler
        public void OnClickLevelUp()
        {
            NewStats.Skills[SelectedSkill] = NewStats.Skills[SelectedSkill] + RpgValues.SkillGainForPoints(1);
            PotentialPoints--;

            UpdateValues();
        }

        
        //these ones are fine
        public void OnClickCancel()
        {
            Destroy(this.gameObject);
        }

        public void OnClickContinue()
        {
            if (PotentialPoints == 0)
            {
                GameState.Instance.PlayerRpgState.Experience = RpgValues.XPAfterMaxLevel(GameState.Instance.PlayerRpgState);
                GameState.Instance.PlayerRpgState.Level = NewLevel;
                GameState.Instance.PlayerRpgState.BaseStats.Skills = NewStats.Skills; //assign points
                GameState.Instance.PlayerRpgState.UpdateStats();
                Destroy(this.gameObject);
                Callback.Invoke();
            }
            else
            {
                //display a error
                Modal.PushMessageModal("You must assign all Potential Points", "Information", null, null);
            }
        }

        //modal push stuff (really ought to move this)

        private const string DefaultPrefab = "UI/IGUI_DefaultLevelUp";
        private const string AltPrefab = "UI/IGUI_LevelUp";
        
        public static void PushModal(LevelUpModalCallback callback)
        {
            if(CoreParams.UseCustomLeveling)
            {
                var go = Instantiate<GameObject>(Resources.Load<GameObject>(AltPrefab), CoreUtils.GetWorldRoot());
                var modal = go.GetComponent<GameUI.LevelUpModal>();
                modal.Callback = callback;
                if (IngameMenuController.Current != null)
                    go.transform.SetParent(IngameMenuController.Current.EphemeralRoot.transform, false);
            }
            else
            {
                var go = Instantiate<GameObject>(Resources.Load<GameObject>(DefaultPrefab), CoreUtils.GetWorldRoot());
                var modal = go.GetComponent<DefaultLevelUpModal>();
                modal.Callback = callback;
                if (IngameMenuController.Current != null)
                    go.transform.SetParent(IngameMenuController.Current.EphemeralRoot.transform, false);
            }
        }

    }
}