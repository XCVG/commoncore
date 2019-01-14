using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SickDev.CommandSystem;
using Newtonsoft.Json;
using CommonCore.State;
using CommonCore.RpgGame.World;
using CommonCore.World;

namespace CommonCore.RpgGame.World
{
    /// <summary>
    /// World/Actor related console commands specific to RpgGame
    /// </summary>
    public class RpgWorldConsoleCommands
    {

        [Command]
        static void Noclip()
        {
            var player = RpgWorldUtils.GetPlayerController();

            player.Clipping = !(player.Clipping);
        }

        [Command]
        static void WarpEx(string scene, bool hideloading)
        {
            RpgWorldUtils.ChangeScene(scene, null, Vector3.zero, Vector3.zero, hideloading, null);
        }

        [Command]
        static void WarpEx(string scene, bool hideloading, string overrideobject)
        {
            RpgWorldUtils.ChangeScene(scene, null, Vector3.zero, Vector3.zero, hideloading, overrideobject);
        }

        //***** ACTOR MANIPULATION
        [Command]
        static void SetAiState(string newState, bool lockState)
        {
            var ac = WorldConsoleCommands.SelectedObject.GetComponent<ActorController>();
            bool wasLocked = ac.LockAiState;
            if (wasLocked)
                ac.LockAiState = false;
            ac.EnterState((ActorAiState)Enum.Parse(typeof(ActorAiState), newState));
            ac.LockAiState = wasLocked || lockState;
        }

        [Command]
        static void SetAnimState(string newState, bool lockState)
        {
            var ac = WorldConsoleCommands.SelectedObject.GetComponent<ActorController>();
            bool wasLocked = ac.LockAnimState;
            if (wasLocked)
                ac.LockAnimState = false;
            ac.SetAnimation((ActorAnimState)Enum.Parse(typeof(ActorAnimState), newState));
            ac.LockAnimState = wasLocked || lockState;
        }

        [Command]
        static void Kill()
        {
            var ac = WorldConsoleCommands.SelectedObject.GetComponent<ActorController>();
            ac.Health = 0;
        }

        [Command]
        static void Resurrect()
        {
            var ac = WorldConsoleCommands.SelectedObject.GetComponent<ActorController>();
            ac.gameObject.SetActive(true);
            ac.Health = ac.MaxHealth;
            ac.EnterState(ac.BaseAiState);
        }

    }
}
