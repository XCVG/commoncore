using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonCore;
using CommonCore.UI;
using CommonCore.State;
using CommonCore.LockPause;
using CommonCore.DebugLog;
using CommonCore.RpgGame.Rpg;
using CommonCore.RpgGame.UI;

namespace GameUI
{
    

    /*
     * Controller for levelling up modal window
     * This is really game-specific and ought to be moved out of CommonCore
     * It's also very hacky and I know it
     * We will likely do something about this as soon as Balmora
     *  -Probably going to move this into game-specific scripting and provide a generic (but non-tree) version
     */
    public class LevelUpModal : BaseMenuController
    {
        //Unity objects
        public Text HeaderText;
        public Button ConfirmButton;
        public Text PointsText;

        [Header("Buttons")]
        public Button MeleeButton;
        public Button AlacrityButton;
        public Button PrecisionButton;
        public Button BrawnButton;

        public Button ArcheryButton;
        public Button DrawButton;
        public Button SteadyButton;

        public Button GunsButton;
        public Button AccuracyButton;
        public Button RapidityButton;

        public Button AthleticsButton;
        public Button FurtiveButton;
        public Button FleetButton;

        public Button MagicButton;
        public Button ForceButton;
        public Button ElementalButton;
        public Button DarkButton;

        public Button SocialButton;
        public Button ExchangeButton;
        public Button LeverageButton;

        public Button SecurityButton;
        public Button MechanismsButton;
        public Button ComputersButton;

        //self-explanatory
        public LevelUpModalCallback Callback;

        //private state
        private StatsSet NewStats;
        private int PotentialPoints;
        private int NewLevel;
        
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
            for (int i=0, tLevel = player.Level; i < levels; i++, tLevel++)
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

            //write skill points
            PaintSkills();

            //enable buttons if allowed (point or confirm)
            SetButtons(PotentialPoints > 0);
            ConfirmButton.interactable = (PotentialPoints == 0);

            //I forgot
        }

        private void PaintSkills()
        {
            MeleeButton.GetComponentInChildren<Text>().text = string.Format("Melee ({0})", NewStats.Skills[(int)SkillType.Melee]);
            AlacrityButton.GetComponentInChildren<Text>().text = string.Format("Alacrity\n({0})", NewStats.Skills[(int)SkillType.MeleeAlacrity]);
            PrecisionButton.GetComponentInChildren<Text>().text = string.Format("Precision\n({0})", NewStats.Skills[(int)SkillType.MeleePrecision]);
            BrawnButton.GetComponentInChildren<Text>().text = string.Format("Brawn\n({0})", NewStats.Skills[(int)SkillType.MeleeBrawn]);

            ArcheryButton.GetComponentInChildren<Text>().text = string.Format("Archery ({0})", NewStats.Skills[(int)SkillType.Archery]);
            DrawButton.GetComponentInChildren<Text>().text = string.Format("Draw\n({0})", NewStats.Skills[(int)SkillType.ArcheryDraw]);
            SteadyButton.GetComponentInChildren<Text>().text = string.Format("Steady\n({0})", NewStats.Skills[(int)SkillType.ArcherySteady]);

            GunsButton.GetComponentInChildren<Text>().text = string.Format("Guns ({0})", NewStats.Skills[(int)SkillType.Guns]);
            AccuracyButton.GetComponentInChildren<Text>().text = string.Format("Accuracy\n({0})", NewStats.Skills[(int)SkillType.GunsAccuracy]);
            RapidityButton.GetComponentInChildren<Text>().text = string.Format("Rapidity\n({0})", NewStats.Skills[(int)SkillType.GunsRapidity]);

            AthleticsButton.GetComponentInChildren<Text>().text = string.Format("Athletics ({0})", NewStats.Skills[(int)SkillType.Athletics]);
            FurtiveButton.GetComponentInChildren<Text>().text = string.Format("Furtiveness\n({0})", NewStats.Skills[(int)SkillType.AthleticsFurtive]);
            FleetButton.GetComponentInChildren<Text>().text = string.Format("Fleetness\n({0})", NewStats.Skills[(int)SkillType.AthleticsFleet]);

            MagicButton.GetComponentInChildren<Text>().text = string.Format("Magic ({0})", NewStats.Skills[(int)SkillType.Magic]);
            ForceButton.GetComponentInChildren<Text>().text = string.Format("Force\n({0})", NewStats.Skills[(int)SkillType.MagicForce]);
            ElementalButton.GetComponentInChildren<Text>().text = string.Format("Elemental\n({0})", NewStats.Skills[(int)SkillType.MagicElemental]);
            DarkButton.GetComponentInChildren<Text>().text = string.Format("Dark\n({0})", NewStats.Skills[(int)SkillType.MagicDark]);

            SocialButton.GetComponentInChildren<Text>().text = string.Format("Social ({0})", NewStats.Skills[(int)SkillType.Social]);
            ExchangeButton.GetComponentInChildren<Text>().text = string.Format("Exchange\n({0})", NewStats.Skills[(int)SkillType.SocialExchange]);
            LeverageButton.GetComponentInChildren<Text>().text = string.Format("Leverage\n({0})", NewStats.Skills[(int)SkillType.SocialLeverage]);

            SecurityButton.GetComponentInChildren<Text>().text = string.Format("Security ({0})", NewStats.Skills[(int)SkillType.Security]);
            MechanismsButton.GetComponentInChildren<Text>().text = string.Format("Mechanisms\n({0})", NewStats.Skills[(int)SkillType.SecurityMechanisms]);
            ComputersButton.GetComponentInChildren<Text>().text = string.Format("Computers\n({0})", NewStats.Skills[(int)SkillType.SecurityComputers]);
        }

        private void SetButtons(bool active)
        {
            MeleeButton.interactable = active;
            AlacrityButton.interactable = active;
            PrecisionButton.interactable = active;
            BrawnButton.interactable = active;

            ArcheryButton.interactable = active;
            DrawButton.interactable = active;
            SteadyButton.interactable = active;

            GunsButton.interactable = active;
            AccuracyButton.interactable = active;
            RapidityButton.interactable = active;

            AthleticsButton.interactable = active;
            FurtiveButton.interactable = active;
            FleetButton.interactable = active;

            MagicButton.interactable = active;
            ForceButton.interactable = active;
            ElementalButton.interactable = active;
            DarkButton.interactable = active;

            SocialButton.interactable = active;
            ExchangeButton.interactable = active;
            LeverageButton.interactable = active;

            SecurityButton.interactable = active;
            MechanismsButton.interactable = active;
            ComputersButton.interactable = active;
        }

        public void OnClickAssignPoints(string key)
        {
            if (PotentialPoints <= 0)
            {
                CDebug.LogEx("Tried to level skills with no PP!", LogLevel.Warning, this);
                return;
            }
                

            //assign 12 points, distributed

            switch (key)
            {
                case "Melee":
                    NewStats.Skills[(int)SkillType.Melee] += 3;
                    NewStats.Skills[(int)SkillType.MeleeAlacrity] += 3;
                    NewStats.Skills[(int)SkillType.MeleePrecision] += 3;
                    NewStats.Skills[(int)SkillType.MeleeBrawn] += 3;
                    break;
                case "Alacrity":
                    NewStats.Skills[(int)SkillType.Melee] += 3;
                    NewStats.Skills[(int)SkillType.MeleeAlacrity] += 7;
                    NewStats.Skills[(int)SkillType.MeleePrecision] += 1;
                    NewStats.Skills[(int)SkillType.MeleeBrawn] += 1;
                    break;
                case "Precision":
                    NewStats.Skills[(int)SkillType.Melee] += 3;
                    NewStats.Skills[(int)SkillType.MeleeAlacrity] += 1;
                    NewStats.Skills[(int)SkillType.MeleePrecision] += 7;
                    NewStats.Skills[(int)SkillType.MeleeBrawn] += 1;
                    break;
                case "Brawn":
                    NewStats.Skills[(int)SkillType.Melee] += 3;
                    NewStats.Skills[(int)SkillType.MeleeAlacrity] += 1;
                    NewStats.Skills[(int)SkillType.MeleePrecision] += 1;
                    NewStats.Skills[(int)SkillType.MeleeBrawn] += 7;
                    break;

                case "Archery":
                    NewStats.Skills[(int)SkillType.Archery] += 4;
                    NewStats.Skills[(int)SkillType.ArcheryDraw] += 4;
                    NewStats.Skills[(int)SkillType.ArcherySteady] += 4;
                    break;
                case "Draw":
                    NewStats.Skills[(int)SkillType.Archery] += 4;
                    NewStats.Skills[(int)SkillType.ArcheryDraw] += 7;
                    NewStats.Skills[(int)SkillType.ArcherySteady] += 1;
                    break;
                case "Steady":
                    NewStats.Skills[(int)SkillType.Archery] += 4;
                    NewStats.Skills[(int)SkillType.ArcheryDraw] += 1;
                    NewStats.Skills[(int)SkillType.ArcherySteady] += 7;
                    break;

                case "Guns":
                    NewStats.Skills[(int)SkillType.Guns] += 4;
                    NewStats.Skills[(int)SkillType.GunsAccuracy] += 4;
                    NewStats.Skills[(int)SkillType.GunsRapidity] += 4;
                    break;
                case "Accuracy":
                    NewStats.Skills[(int)SkillType.Guns] += 4;
                    NewStats.Skills[(int)SkillType.GunsAccuracy] += 7;
                    NewStats.Skills[(int)SkillType.GunsRapidity] += 1;
                    break;
                case "Rapidity":
                    NewStats.Skills[(int)SkillType.Guns] += 4;
                    NewStats.Skills[(int)SkillType.GunsAccuracy] += 1;
                    NewStats.Skills[(int)SkillType.GunsRapidity] += 7;
                    break;

                case "Athletics":
                    NewStats.Skills[(int)SkillType.Athletics] += 4;
                    NewStats.Skills[(int)SkillType.AthleticsFurtive] += 4;
                    NewStats.Skills[(int)SkillType.AthleticsFleet] += 4;
                    break;
                case "Furtive":
                    NewStats.Skills[(int)SkillType.Athletics] += 4;
                    NewStats.Skills[(int)SkillType.AthleticsFurtive] += 7;
                    NewStats.Skills[(int)SkillType.AthleticsFleet] += 1;
                    break;
                case "Fleet":
                    NewStats.Skills[(int)SkillType.Athletics] += 4;
                    NewStats.Skills[(int)SkillType.AthleticsFurtive] += 1;
                    NewStats.Skills[(int)SkillType.AthleticsFleet] += 7;
                    break;

                case "Magic":
                    NewStats.Skills[(int)SkillType.Magic] += 3;
                    NewStats.Skills[(int)SkillType.MagicForce] += 3;
                    NewStats.Skills[(int)SkillType.MagicElemental] += 3;
                    NewStats.Skills[(int)SkillType.MagicDark] += 3;
                    break;
                case "Force":
                    NewStats.Skills[(int)SkillType.Magic] += 3;
                    NewStats.Skills[(int)SkillType.MagicForce] += 7;
                    NewStats.Skills[(int)SkillType.MagicElemental] += 1;
                    NewStats.Skills[(int)SkillType.MagicDark] += 1;
                    break;
                case "Elemental":
                    NewStats.Skills[(int)SkillType.Magic] += 3;
                    NewStats.Skills[(int)SkillType.MagicForce] += 1;
                    NewStats.Skills[(int)SkillType.MagicElemental] += 7;
                    NewStats.Skills[(int)SkillType.MagicDark] += 1;
                    break;
                case "Dark":
                    NewStats.Skills[(int)SkillType.Magic] += 3;
                    NewStats.Skills[(int)SkillType.MagicForce] += 1;
                    NewStats.Skills[(int)SkillType.MagicElemental] += 1;
                    NewStats.Skills[(int)SkillType.MagicDark] += 7;
                    break;

                case "Social":
                    NewStats.Skills[(int)SkillType.Social] += 4;
                    NewStats.Skills[(int)SkillType.SocialExchange] += 4;
                    NewStats.Skills[(int)SkillType.SocialLeverage] += 4;
                    break;
                case "Exchange":
                    NewStats.Skills[(int)SkillType.Social] += 4;
                    NewStats.Skills[(int)SkillType.SocialExchange] += 7;
                    NewStats.Skills[(int)SkillType.SocialLeverage] += 1;
                    break;
                case "Leverage":
                    NewStats.Skills[(int)SkillType.Social] += 4;
                    NewStats.Skills[(int)SkillType.SocialExchange] += 1;
                    NewStats.Skills[(int)SkillType.SocialLeverage] += 7;
                    break;

                case "Security":
                    NewStats.Skills[(int)SkillType.Security] += 4;
                    NewStats.Skills[(int)SkillType.SecurityMechanisms] += 4;
                    NewStats.Skills[(int)SkillType.SecurityComputers] += 4;
                    break;
                case "Mechanisms":
                    NewStats.Skills[(int)SkillType.Security] += 4;
                    NewStats.Skills[(int)SkillType.SecurityMechanisms] += 7;
                    NewStats.Skills[(int)SkillType.SecurityComputers] += 1;
                    break;
                case "Computers":
                    NewStats.Skills[(int)SkillType.Security] += 4;
                    NewStats.Skills[(int)SkillType.SecurityMechanisms] += 1;
                    NewStats.Skills[(int)SkillType.SecurityComputers] += 7;
                    break;

                default:
                    CDebug.LogEx("Couldn't assign points for " + key, LogLevel.Error, this);
                    return;
            }

            PotentialPoints--;
            PaintAll();
        }

        public void OnClickCancel()
        {
            Destroy(this.gameObject);
        }

        public void OnClickContinue()
        {
            if(PotentialPoints == 0)
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


        
    }
}