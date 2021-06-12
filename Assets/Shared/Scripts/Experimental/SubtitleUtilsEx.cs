using CommonCore.LockPause;
using CommonCore.RpgGame.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Experimental
{

    /// <summary>
    /// Experimental subtitle convenience methods ported from Mother Earth
    /// </summary>
    public static class SubtitleUtilsEx
    {
        public const float SubtitleWaitTime = 0.5f;

        public static IEnumerator ShowDialogueSubtitle(string text, string color, float holdTime) => ShowDialogueSubtitle(text, color, holdTime, SubtitleWaitTime);

        public static IEnumerator ShowDialogueSubtitle(string text, string color, float holdTime, float pauseTime) => ShowDialogueSubtitle(null, text, color, holdTime, pauseTime);

        public static IEnumerator ShowDialogueSubtitle(ActorController actor, string text, string color, float holdTime) => ShowDialogueSubtitle(actor, text, color, holdTime, SubtitleWaitTime);

        public static IEnumerator ShowDialogueSubtitle(ActorController actor, string text, string color, float holdTime, float pauseTime)
        {
            bool useActor = (actor != null && actor.CurrentAiState == ActorAiState.ScriptedAction && actor.AnimationComponent != null);

            if (useActor)
                actor.AnimationComponent.SetAnimation(ActorAnimState.Talking);
            Subtitle.Show($"<color={color}>{text}</color>", holdTime);
            yield return new WaitForSecondsEx(holdTime, true, PauseLockType.AllowCutscene, false);

            if (useActor)
                actor.AnimationComponent.SetAnimation(ActorAnimState.Idle);
            Subtitle.Clear();
            yield return new WaitForSecondsEx(pauseTime, false, PauseLockType.AllowCutscene, false);

        }
    }
}