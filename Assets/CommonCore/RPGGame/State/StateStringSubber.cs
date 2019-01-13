using System;
using System.Collections;
using System.Collections.Generic;
using CommonCore.State;
using CommonCore.StringSub;

namespace CommonCore.RpgGame.State
{

    /// <summary>
    /// StringSubber for GameState patterns
    /// </summary>
    public class StateStringSubber : IStringSubber
    {
        public IEnumerable<string> MatchPatterns { get; } = new string[] { "av", "inv", "cpf", "cpv", "cqs" };

        public string Substitute(string[] sequenceParts)
        {
            string result = null;

            switch (sequenceParts[0])
            {
                case "av":
                    result = GameState.Instance.PlayerRpgState.GetAV<object>(sequenceParts[1]).ToString();
                    break;
                case "inv":
                    result = GameState.Instance.PlayerRpgState.Inventory.CountItem(sequenceParts[1]).ToString();
                    break;
                case "cpf":
                    result = GameState.Instance.CampaignState.HasFlag(sequenceParts[1]).ToString();
                    break;
                case "cpv":
                    result = GameState.Instance.CampaignState.GetVar(sequenceParts[1]);
                    break;
                case "cqs":
                    result = GameState.Instance.CampaignState.GetQuestStage(sequenceParts[1]).ToString();
                    break;
                default:
                    throw new ArgumentException();
            }

            return result;
        }

    }
}