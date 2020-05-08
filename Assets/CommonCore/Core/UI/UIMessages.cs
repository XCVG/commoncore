using CommonCore.Messaging;

namespace CommonCore.UI
{
    /// <summary>
    /// Message signalling some text to push to the onscreen log
    /// </summary>
    public class HUDPushMessage : QdmsMessage
    {
        public readonly string Contents;

        public HUDPushMessage(string contents) : base()
        {
            Contents = contents;
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
