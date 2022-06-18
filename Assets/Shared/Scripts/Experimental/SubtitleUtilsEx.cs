using CommonCore.LockPause;
using CommonCore.RpgGame.World;
using System;
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
        public const string NoSpeakerPlaceholder = "\n";
        public const string SpeakerDelimiter = "\n";

        public static IEnumerator ShowDialogueSubtitle(ActorController actor, string text, string speaker, Color? color, float holdTime, float? pauseTime) => ShowDialogueSubtitle(actor, text, speaker, color.HasValue ? "#" + ColorUtility.ToHtmlStringRGBA(color.Value) : null, holdTime, null);

        public static IEnumerator ShowDialogueSubtitle(ActorController actor, string text, string speaker, string color, float holdTime, float? pauseTime)
        {
            bool useActor = (actor != null && actor.CurrentAiState == ActorAiState.ScriptedAction && actor.AnimationComponent != null);

            if (useActor)
                actor.AnimationComponent.SetAnimation(ActorAnimState.Talking);

            string speakerText = string.IsNullOrEmpty(speaker) ? NoSpeakerPlaceholder : $"{speaker}{SpeakerDelimiter}";

            Subtitle.Show(string.IsNullOrEmpty(color) ? text : $"{speakerText}<color={color}>{text}</color>", holdTime);
            yield return new WaitForSecondsEx(holdTime, true, PauseLockType.AllowCutscene, false);

            if (useActor)
                actor.AnimationComponent.SetAnimation(ActorAnimState.Idle);
            Subtitle.Clear();
            yield return new WaitForSecondsEx(pauseTime ?? SubtitleWaitTime, false, PauseLockType.AllowCutscene, false);

        }
    }
}