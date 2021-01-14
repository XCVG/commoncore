using CommonCore.World;
using System;
using UnityEngine;

namespace CommonCore.RpgGame.World
{
    /// <summary>
    /// World/Actor related console commands specific to RpgGame
    /// </summary>
    public class RpgWorldConsoleCommands
    {

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
            bool wasLocked = ac.AnimationComponent.LockAnimState;
            if (wasLocked)
                ac.AnimationComponent.LockAnimState = false;
            ac.AnimationComponent.SetAnimation((ActorAnimState)Enum.Parse(typeof(ActorAnimState), newState));
            ac.AnimationComponent.LockAnimState = wasLocked || lockState;
        }

        [Command]
        static void Kill()
        {
            var ac = WorldConsoleCommands.SelectedObject.GetComponent<ActorController>();
            //ac.Health = 0;
            ac.Kill();
        }

        [Command]
        static void Resurrect()
        {
            var ac = WorldConsoleCommands.SelectedObject.GetComponent<ActorController>();
            ac.Raise();
        }

    }
}
