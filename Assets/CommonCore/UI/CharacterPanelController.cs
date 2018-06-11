using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonCore.Rpg;
using CommonCore.State;
using System.Text;

namespace CommonCore.UI
{

    public class CharacterPanelController : PanelController
    {
        public Text StatsText;
        public Text ConditionsText;
        public Text SkillsText;

        public override void SignalPaint()
        {
            PaintStats();
            PaintConditions();
            PaintSkills();
        }

        private void PaintStats()
        {
            StringBuilder statsSB = new StringBuilder(); //really can't guess here

            var player = GameState.Instance.PlayerRpgState;
            StatsSet baseStats = player.BaseStats;
            StatsSet derivedStats = player.DerivedStats;

            //base statistics
            foreach(int value in Enum.GetValues(typeof(StatType)))
            {
                string name = Enum.GetName(typeof(StatType), value);
                int baseValue = baseStats.Stats[value];
                int derivedValue = derivedStats.Stats[value];
                statsSB.AppendFormat("{0}: {1} [{2}]\n", name, baseValue, derivedValue);
            }

            statsSB.AppendLine();

            //damage resistance and threshold
            foreach(int value in Enum.GetValues(typeof(DamageType)))
            {
                string name = Enum.GetName(typeof(DamageType), value);
                float baseDR = baseStats.DamageResistance[value];
                float baseDT = baseStats.DamageThreshold[value];
                float derivedDR = derivedStats.DamageResistance[value];
                float derivedDT = derivedStats.DamageThreshold[value];
                statsSB.AppendFormat("{0}: R({1:f1} [{2:f1}]) | T({3:f1} [{4:f1}])\n", name.Substring(0,Math.Min(4, name.Length)), baseDR, derivedDR, baseDT, derivedDT);
            }

            statsSB.AppendLine();

            //max health
            statsSB.AppendFormat("Max Health: {0:f1} [{1:f1}]", baseStats.MaxHealth, derivedStats.MaxHealth);

            StatsText.text = statsSB.ToString();
        }

        private void PaintConditions()
        {
            var conditions = GameState.Instance.PlayerRpgState.Conditions;
            StringBuilder conditionsSB = new StringBuilder(conditions.Count * 16);
            foreach(Condition c in conditions)
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

            StringBuilder skillsSB = new StringBuilder(baseStats.Skills.Length * 16);

            foreach(int value in Enum.GetValues(typeof(SkillType)))
            {
                string name = Enum.GetName(typeof(SkillType), value);
                int baseSkill = baseStats.Skills[value];
                int derivedSkill = derivedStats.Skills[value];
                skillsSB.AppendFormat("{0}: {1} [{2}]\n", name, baseSkill, derivedSkill);
            }

            SkillsText.text = skillsSB.ToString();
        }


    }
}