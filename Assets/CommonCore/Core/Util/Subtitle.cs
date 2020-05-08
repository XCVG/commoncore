using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CommonCore.Messaging;
using CommonCore.UI;

namespace CommonCore
{

    /// <summary>
    /// Convenience methods for subtitles
    /// </summary>
    public static class Subtitle
    {
        public static void Show(string text, float holdTime)
        {
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage(text, holdTime));
        }

        public static void Show(string text, float holdTime, bool useSubstitution)
        {
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage(text, holdTime, useSubstitution, 0));
        }

        public static void Show(string text, float holdTime, int priority)
        {
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage(text, holdTime, true, priority));
        }

        public static void Show(string text, float holdTime, bool useSubstitution, int priority)
        {
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage(text, holdTime, useSubstitution, priority));
        }

        public static void Clear()
        {
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage(string.Empty, 0, false, int.MaxValue));
        }

        public static void Clear(int priority)
        {
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage(string.Empty, 0, false, priority));
        }
    }
}