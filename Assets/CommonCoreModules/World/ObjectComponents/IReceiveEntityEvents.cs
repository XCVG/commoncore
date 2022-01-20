using System.Collections.Generic;

namespace CommonCore.World
{
    /// <summary>
    /// Interface representing a component on an object that receives entity event calls
    /// </summary>
    public interface IReceiveEntityEvents
    {

        void BeforeInit(BaseController controller);

        void Init(BaseController controller);

        void Activate(bool firstActivation);

        void Deactivate();

        void BeforeDestroy(); //needed?

    }

    /// <summary>
    /// Interface representing a component on an object that receives destroyable entity event calls
    /// </summary>
    public interface IReceiveDamageableEntityEvents : IReceiveEntityEvents
    {
        void DamageTaken(ActorHitInfo data);

        void Killed();
    }    
}