
namespace CommonCore.State
{
    //base class for Intents
    //Intents are created by one scene and executed on the next or on the loading screen
    public abstract class Intent
    {
        public virtual void LoadingExecute() { }

        public virtual void PreloadExecute() { }

        public virtual void PostloadExecute() { }
    }
}
