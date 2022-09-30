using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using CommonCore.RpgGame.Rpg;
using CommonCore.State;
using CommonCore.StringSub;
using CommonCore.UI;
using System.Linq;
using PseudoExtensibleEnum;

namespace CommonCore.RpgGame.UI
{

    public class CharacterPanelController : PanelController
    {
        private const string SubList = "RPG_AV";

        public Text StatsText;
        public Text ConditionsText;
        public Text SkillsText;

        public override void SignalPaint()
        {
            PaintStats();
            PaintConditions();
            PaintSkills();

            CallPostRepaintHooks();
        }

        private void PaintStats()
        {
            StringBuilder statsSB = new StringBuilder(); //really can't guess here

            var player = GameState.Instance.PlayerRpgState;
            StatsSet baseStats = player.BaseStats;
            StatsSet derivedStats = player.DerivedStats;

            //level and XP
            statsSB.AppendFormat("Level {0} ({1}/{2})\n\n", player.Level, player.Experience, RpgValues.XPToNext(player.Level));

            //base statistics
            foreach (int value in PxEnum.GetValues(typeof(StatType)))
            {
                string name = PxEnum.GetName(typeof(StatType), value);
                if (GameParams.HideStats.Contains(name))
                    continue;
                int baseValue = baseStats.Stats.GetOrDefault(value);
                int derivedValue = derivedStats.Stats.GetOrDefault(value);
                statsSB.AppendFormat("{0}: {1} [{2}]\n", Sub.Replace(name, SubList), baseValue, derivedValue);
            }

            statsSB.AppendLine();

            //damage resistance and threshold
            foreach(int value in PxEnum.GetValues(typeof(DamageType)))
            {
                string name = PxEnum.GetName(typeof(DamageType), value);
                float baseDR = baseStats.DamageResistance.GetOrDefault(value);
                float baseDT = baseStats.DamageThreshold.GetOrDefault(value);
                float derivedDR = derivedStats.DamageResistance.GetOrDefault(value);
                float derivedDT = derivedStats.DamageThreshold.GetOrDefault(value);
                statsSB.AppendFormat("{0}: DR({1:f1} [{2:f1}]) | DT({3:f1} [{4:f1}])\n", name.Substring(0,Math.Min(4, name.Length)), baseDR, derivedDR, baseDT, derivedDT);
            }

            statsSB.AppendLine();

            //max health
            statsSB.AppendFormat("Max Health: {0:f1} [{1:f1}]", baseStats.MaxHealth, derivedStats.MaxHealth);

            StatsText.text = statsSB.ToString();
        }

        private void PaintConditions()
        {
            var conditions = GameState.Instance.PlayerRpgState.AllConditions.ToList();
            StringBuilder conditionsSB = new StringBuilder(conditions.Count * 16);
            foreach(var c in conditions)
            {
                conditionsSB.AppendLine(c.NiceName);
            }

            ConditionsText.text = conditionsSB.ToString();
        }

        private void PaintSkills()
        {
            var player = GameState.Instance.PlayerRpgState;
            StatsSet baseStats = player.BaseStats;
            StatsSet derivedStats = player.DerivedStats;

            StringBuilder skillsSB = new StringBuilder(baseStats.Skills.Keys.Count * 16);

            foreach(int value in PxEnum.GetValues(typeof(SkillType)))
            {
                string name = PxEnum.GetName(typeof(SkillType), value);
                if (GameParams.HideSkills.Contains(name))
                    continue;

                int baseSkill = baseStats.Skills.GetOrDefault(value);
                int derivedSkill = derivedStats.Skills.GetOrDefault(value);
                skillsSB.AppendFormat("{0}: {1} [{2}]\n", Sub.Replace(name, SubList), baseSkill, derivedSkill);
            }

            SkillsText.text = skillsSB.ToString();
        }


    }
}