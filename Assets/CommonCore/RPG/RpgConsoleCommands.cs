using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SickDev.CommandSystem;
using System;
using CommonCore.State;

namespace CommonCore.Rpg
{
    public static class RpgConsoleCommands
    {

        //***** Player Manipulation

        [Command(className = "Player")]
        static void GetAV(string av)
        {
            try
            {
                var value = GameState.Instance.PlayerRpgState.GetAV<object>(av);
                DevConsole.singleton.Log(string.Format("{0} : {1}", av, value));
            }
            catch (Exception e)
            {
                DevConsole.singleton.LogError(e.ToString());
            }
        }

        [Command(className = "Player")]
        static void SetAV(string av, string value)
        {
            object convertedValue = CCBaseUtil.StringToNumericAuto(value);
            if (convertedValue == null)
                convertedValue = value;

            //we *should* now have the correct type in the box
            //DevConsole.singleton.Log(convertedValue.GetType().Name);
            try
            {
                if(convertedValue.GetType() == typeof(float))
                    GameState.Instance.PlayerRpgState.SetAV(av, (float)convertedValue);
                else if (convertedValue.GetType() == typeof(int))
                    GameState.Instance.PlayerRpgState.SetAV(av, (int)convertedValue);
                else if (convertedValue.GetType() == typeof(string))
                    GameState.Instance.PlayerRpgState.SetAV(av, (string)convertedValue);
                else
                    GameState.Instance.PlayerRpgState.SetAV(av, convertedValue);
            }
            catch(Exception e)
            {
                DevConsole.singleton.LogError(e.ToString());
            }
        }

        [Command(className = "Player")]
        static void ModAV(string av, string value)
        {
            object convertedValue = CCBaseUtil.StringToNumericAuto(value);
            if (convertedValue == null)
                convertedValue = value;

            //we *should* now have the correct type in the box
            //DevConsole.singleton.Log(convertedValue.GetType().Name);
            try
            {
                if (convertedValue.GetType() == typeof(float))
                    GameState.Instance.PlayerRpgState.ModAV(av, (float)convertedValue);
                else if (convertedValue.GetType() == typeof(int))
                    GameState.Instance.PlayerRpgState.ModAV(av, (int)convertedValue);
                else if (convertedValue.GetType() == typeof(string))
                    GameState.Instance.PlayerRpgState.ModAV(av, (string)convertedValue);
                else
                    GameState.Instance.PlayerRpgState.ModAV(av, convertedValue);
            }
            catch (Exception e)
            {
                DevConsole.singleton.LogError(e.ToString());
            }
        }

    }
}