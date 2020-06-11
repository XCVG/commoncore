using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using CommonCore.RpgGame.Rpg;
using CommonCore.State;
using CommonCore.StringSub;

namespace CommonCore.RpgGame.State
{

    /// <summary>
    /// StringSubber for GameState patterns
    /// </summary>
    public class StateStringSubber : IStringSubber
    {
        public IEnumerable<string> MatchPatterns { get; } = new string[] { "av", "inv", "invname", "cpf", "cpv", "cqs", "cqname", "player" };

        public string Substitute(string[] sequenceParts)
        {
            string result = null;

            switch (sequenceParts[0])
            {
                case "av":
                    result = GameState.Instance.PlayerRpgState.GetAV(sequenceParts[1]).ToString();
                    break;
                case "inv":
                    result = GameState.Instance.PlayerRpgState.Inventory.CountItem(sequenceParts[1]).ToString();
                    break;
                case "invname":
                    {
                        var invModel = InventoryModel.GetModel(sequenceParts[1]);
                        result = (invModel != null ? InventoryModel.GetNiceName(invModel) : "?MODEL?") ?? "?NAME?";
                    }
                    break;
                case "cpf":
                    result = GameState.Instance.CampaignState.HasFlag(sequenceParts[1]).ToString();
                    break;
                case "cpv":
                    result = GameState.Instance.CampaignState.GetVar<string>(sequenceParts[1]);
                    break;
                case "cqs":
                    result = GameState.Instance.CampaignState.GetQuestStage(sequenceParts[1]).ToString();
                    break;
                case "cqname":
                    result = QuestModel.GetNiceName(sequenceParts[1]);
                    break;
                case "player":
                    result = GetPlayerAlias(sequenceParts[1]);
                    break;
                default:
                    throw new ArgumentException();
            }

            return result;
        }

        /// <summary>
        /// Gets aliased "player" strings (pronouns and a few other things)
        /// </summary>
        private string GetPlayerAlias(string alias)
        {
            //check the case
            StringCase aliasCase = StringCase.Unspecified;

            if (char.IsLower(alias[0]))
                aliasCase = StringCase.LowerCase;
            else if (char.IsLower(alias[1]))
                aliasCase = StringCase.TitleCase;
            else
                aliasCase = StringCase.UpperCase;
            if (alias.EndsWith("|keepcase", StringComparison.OrdinalIgnoreCase))
            {
                aliasCase = StringCase.Unspecified;
                alias = alias.Substring(0, alias.IndexOf('|'));
            }

            CharacterModel player = GameState.Instance.PlayerRpgState;

            string uncasedResult = null;
            if (alias.Equals("name", StringComparison.OrdinalIgnoreCase))
            {
                uncasedResult = player.DisplayName;
            }
            else if (alias.Equals("shortname", StringComparison.OrdinalIgnoreCase))
            {
                uncasedResult = player.DisplayName; //will likely return something different later
            }
            else if (alias.Equals("race", StringComparison.OrdinalIgnoreCase))
            {
                uncasedResult = "Human";
            }
            else
            {
                //it's a lookup!

                //figure out which list we need
                string listName;                
                switch (player.Gender)
                {
                    case Sex.Undefined:
                        listName = "ALIAS_NOGENDER";
                        break;
                    case Sex.Female:
                        listName = "ALIAS_FEMALE";
                        break;
                    case Sex.Male:
                        listName = "ALIAS_MALE";
                        break;
                    case Sex.Other:
                        listName = "ALIAS_NEUTRAL";
                        break;
                    default:
                        throw new NotSupportedException($"Unknown gender \"{player.Gender}\"");
                }

                uncasedResult = Sub.Replace(alias, listName, true);
            }

            //conform the casing
            string casedResult = null;
            switch (aliasCase)
            {
                case StringCase.LowerCase:
                    casedResult = uncasedResult.ToLower(CultureInfo.InvariantCulture);
                    break;
                case StringCase.UpperCase:
                    casedResult = uncasedResult.ToUpper(CultureInfo.InvariantCulture);
                    break;
                case StringCase.TitleCase:
                    casedResult = uncasedResult.ToTitleCase();
                    break;
                default:
                    casedResult = uncasedResult;
                    break;
            }

            return casedResult;
        }

    }
}