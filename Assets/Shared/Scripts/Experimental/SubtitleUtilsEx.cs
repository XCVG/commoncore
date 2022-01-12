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

        [Obsolete]
        public static IEnumerator ShowDialogueSubtitle(string text, Color color, float holdTime) => ShowDialogueSubtitle(text, color, holdTime, SubtitleWaitTime);
        [Obsolete]
        public static IEnumerator ShowDialogueSubtitle(string text, Color color, float holdTime, float pauseTime) => ShowDialogueSubtitle(null, text, color, holdTime, pauseTime);
        [Obsolete]
        public static IEnumerator ShowDialogueSubtitle(ActorController actor, string text, Color color, float holdTime) => ShowDialogueSubtitle(actor, text, color, holdTime, SubtitleWaitTime);
        [Obsolete]
        public static IEnumerator ShowDialogueSubtitle(ActorController actor, string text, Color color, float holdTime, float pauseTime) => ShowDialogueSubtitle(actor, text, null, color, holdTime, SubtitleWaitTime);

        public static IEnumerator ShowDialogueSubtitle(ActorController actor, string text, string speaker, Color? color, float holdTime, float? pauseTime) => ShowDialogueSubtitle(actor, text, speaker, color.HasValue ? "#" + ColorUtility.ToHtmlStringRGBA(color.Value) : null, holdTime, null);

        [Obsolete]
        public static IEnumerator ShowDialogueSubtitle(string text, string color, float holdTime) => ShowDialogueSubtitle(text, color, holdTime, SubtitleWaitTime);

        [Obsolete]
        public static IEnumerator ShowDialogueSubtitle(string text, string color, float holdTime, float pauseTime) => ShowDialogueSubtitle(null, text, null, color, holdTime, pauseTime);

        [Obsolete]
        public static IEnumerator ShowDialogueSubtitle(ActorController actor, string text, string color, float holdTime) => ShowDialogueSubtitle(actor, text, null, color, holdTime, SubtitleWaitTime);

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