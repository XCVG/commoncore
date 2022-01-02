using CommonCore.Config;
using CommonCore.DelayedEvents;
using CommonCore.Scripting;
using CommonCore.State;
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Linq;

namespace CommonCore.RpgGame.State
{

    public abstract class ConditionalResolver
    {
        protected Conditional Conditional;

        public ConditionalResolver(Conditional conditional)
        {
            Conditional = conditional;
        }

        public virtual bool CanResolve { get; protected set; }

        public abstract bool Resolve();

        
    }

    public abstract class MicroscriptResolver
    {
        protected MicroscriptNode Microscript;

        public MicroscriptResolver(MicroscriptNode microscript)
        {
            Microscript = microscript;
        }

        public virtual bool CanResolve { get; protected set; }

        public abstract void Resolve();
    }

}