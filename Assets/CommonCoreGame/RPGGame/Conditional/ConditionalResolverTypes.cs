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