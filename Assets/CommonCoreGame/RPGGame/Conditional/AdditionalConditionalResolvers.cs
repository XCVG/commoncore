using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.RpgGame.State;
using CommonCore.State;
using System;

namespace CommonCore.RpgGame.State
{

    //Additional semi-experimental conditional/microscript resolvers for PlayerFlags and SessionFlags

    public class PlayerFlagConditionalResolver : ConditionalResolver
    {
        public PlayerFlagConditionalResolver(Conditional conditional) : base(conditional)
        {
            if (Conditional.RawData.ContainsKey("playerFlag") || Conditional.RawData.ContainsKey("noPlayerFlag"))
                CanResolve = true;
        }

        public override bool Resolve()
        {
            if(Conditional.RawData.ContainsKey("playerFlag"))
            {
                var pFlag = Conditional.RawData.Value<string>("playerFlag");
                return GameState.Instance.PlayerFlags.Contains(pFlag);
            }
            else
            {
                var pFlag = Conditional.RawData.Value<string>("noPlayerFlag");
                return !GameState.Instance.PlayerFlags.Contains(pFlag);
            }
        }
    }

    public class SessionFlagConditionalResolver : ConditionalResolver
    {
        public SessionFlagConditionalResolver(Conditional conditional) : base(conditional)
        {
            if (Conditional.RawData.ContainsKey("sessionFlag") || Conditional.RawData.ContainsKey("noSessionFlag"))
                CanResolve = true;
        }

        public override bool Resolve()
        {
            if (Conditional.RawData.ContainsKey("sessionFlag"))
            {
                var sFlag = Conditional.RawData.Value<string>("sessionFlag");
                return MetaState.Instance.SessionFlags.Contains(sFlag);
            }
            else
            {
                var sFlag = Conditional.RawData.Value<string>("noSessionFlag");
                return !MetaState.Instance.SessionFlags.Contains(sFlag);
            }
        }
    }

    public class PlayerFlagMicroscriptResolver : MicroscriptResolver
    {
        public PlayerFlagMicroscriptResolver(MicroscriptNode microscript) : base(microscript)
        {
            if (Microscript.RawData.ContainsKey("playerFlag"))
                CanResolve = true;
        }

        public override void Resolve()
        {
            var pFlag = Microscript.RawData.Value<string>("playerFlag");

            switch (Microscript.Action)
            {
                case MicroscriptAction.Set:
                    var value = Convert.ToBoolean(Microscript.Value);
                    if (value)
                        GameState.Instance.PlayerFlags.Add(pFlag);
                    else
                        GameState.Instance.PlayerFlags.Remove(pFlag);
                    break;
                case MicroscriptAction.Toggle:
                    if (GameState.Instance.PlayerFlags.ContainsSpecific(pFlag))
                        GameState.Instance.PlayerFlags.Remove(pFlag);
                    else
                        GameState.Instance.PlayerFlags.Add(pFlag);
                    break;
                default:
                    throw new NotSupportedException($"{GetType().Name} does not support action \"{Microscript.Action}\"");
            }
        }
    }

}

