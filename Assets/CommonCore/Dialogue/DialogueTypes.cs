using System;
using System.Collections.Generic;
using CommonCore.State;

namespace CommonCore.Dialogue
{
    internal class DialogueScene
    {
        public Dictionary<string, Frame> Frames { get; private set; }
        public string Default;
        public string Music;

        public DialogueScene(Dictionary<string, Frame> frames, string defaultFrame, string music)
        {
            Frames = frames;
            Default = defaultFrame;
            Music = music;
        }
    }

    internal enum FrameType
    {
        ChoiceFrame, TextFrame
    }

    internal class ChoiceNode
    {
        public readonly string Text;
        public readonly string Next;
        public readonly MicroscriptNode[] NextMicroscript;
        public readonly ConditionNode[] NextConditional;

        public readonly Conditional ShowCondition;
        public readonly Conditional HideCondition;

        public ChoiceNode(string next, string text)
        {
            Text = text;
            Next = next;
        }

        public ChoiceNode(string next, string text, Conditional showCondition, Conditional hideCondition, MicroscriptNode[] nextMicroscript, ConditionNode[] nextConditional)
            : this(next, text)
        {
            ShowCondition = showCondition;
            HideCondition = hideCondition;
            NextMicroscript = nextMicroscript;
            NextConditional = nextConditional;
        }

        public string EvaluateConditional()
        {
            for (int i = NextConditional.Length - 1; i >= 0; i--)
            {
                var nc = NextConditional[i];
                if (nc.Evaluate())
                    return nc.Next;
            }
            return null;
        }

        public void EvaluateMicroscript()
        {
            if (NextMicroscript == null || NextMicroscript.Length < 1)
                return;

            foreach (MicroscriptNode mn in NextMicroscript)
            {
                mn.Execute();
            }
        }

    }

    internal class Frame
    {
        public readonly string Background;
        public readonly string Image;
        public readonly string Next;
        public readonly string Music;
        public readonly string NameText;
        public readonly string Text;
        public readonly ConditionNode[] NextConditional;
        public readonly MicroscriptNode[] NextMicroscript;

        public Frame(string background, string image, string next, string music, string nameText, string text, ConditionNode[] nextConditional, MicroscriptNode[] nextMicroscript)
        {
            Background = background;
            Image = image;
            Next = next;
            Music = music;
            NameText = nameText;
            Text = text;

            if (nextConditional != null && nextConditional.Length > 0)
                NextConditional = (ConditionNode[])nextConditional.Clone();

            if (nextMicroscript != null && nextMicroscript.Length > 0)
                NextMicroscript = (MicroscriptNode[])nextMicroscript.Clone();
        }

        public string EvaluateConditional()
        {
            for(int i = NextConditional.Length-1; i >= 0; i--)
            {
                var nc = NextConditional[i];
                if (nc.Evaluate())
                    return nc.Next;
            }
            return null;
        }

        public void EvaluateMicroscript()
        {
            if (NextMicroscript == null || NextMicroscript.Length < 1)
                return;

            foreach (MicroscriptNode mn in NextMicroscript)
            {
                mn.Execute();
            }
        }
    }

    internal class TextFrame : Frame
    {
        public TextFrame(string background, string image, string next, string music, string nameText, string text, ConditionNode[] nextConditional, MicroscriptNode[] nextMicroscript)
            : base(background, image, next, music, nameText, text, nextConditional, nextMicroscript)
        {
            
        }
    }

    internal class ChoiceFrame: Frame
    {
        public readonly ChoiceNode[] Choices;

        public ChoiceFrame(string background, string image, string next, string music, string nameText, string text, ChoiceNode[] choices, ConditionNode[] nextConditional, MicroscriptNode[] nextMicroscript)
            : base(background, image, next, music, nameText, text, nextConditional, nextMicroscript)
        {
            Choices = (ChoiceNode[])choices.Clone();
        }
    }

    internal class ConditionNode
    {
        public readonly string Next;
        public readonly Conditional[] Conditions;

        public ConditionNode(string next, Conditional[] conditions)
        {
            Next = next;
            Conditions = conditions;
        }

        public bool Evaluate()
        {
            if (Conditions == null || Conditions.Length == 0) //odd, but in the spec
                return true;

            foreach(Conditional c in Conditions)
            {
                if (!c.Evaluate())
                    return false;
            }
            return true;
        }
    }

}