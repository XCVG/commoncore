using CommonCore.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace CommonCore.UI
{
    public static class HUDPushMessageDefaultTags
    {
        public const string StatusUpdate = "StatusUpdate";
    }

    /// <summary>
    /// Message signalling some text to push to the onscreen log
    /// </summary>
    public class HUDPushMessage : QdmsMessage
    {
        public readonly string Contents;
        public readonly ImmutableHashSet<string> Tags;

        public HUDPushMessage(string contents) : base()
        {
            Contents = contents;
            Tags = ImmutableHashSet.Create<string>(StringComparer.OrdinalIgnoreCase);
        }

        public HUDPushMessage(string contents, IEnumerable<string> tags) : base()
        {
            Contents = contents;
            Tags = tags?.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase) ?? ImmutableHashSet.Create<string>(StringComparer.OrdinalIgnoreCase);            
        }

        public HUDPushMessage(string contents, params string[] tags) : this(contents, (IEnumerable<string>)tags)
        {

        }
    }

    /// <summary>
    /// Message signalling to clear the onscreen log
    /// </summary>
    public class HUDClearMessage : QdmsMessage
    {
        public HUDClearMessage() : base()
        {

        }
    }

    /// <summary>
    /// Message signaling a subtitle to display on screen
    /// </summary>
    public class SubtitleMessage : QdmsMessage
    {
        public readonly string Contents;
        public readonly float HoldTime;
        public readonly bool UseSubstitution;
        public readonly int Priority;

        public SubtitleMessage(string contents, float holdTime, bool useSubstitution, int priority) : base()
        {
            Contents = contents;
            HoldTime = holdTime;
            UseSubstitution = useSubstitution;
            Priority = priority;
        }

        public SubtitleMessage(string contents, float holdTime) : this(contents, holdTime, true, 0)
        {

        }


    }
}
