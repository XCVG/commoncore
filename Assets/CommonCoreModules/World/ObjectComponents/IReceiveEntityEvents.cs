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

        //no save/load, that's handled by another interface

        //TODO CONCEPTUAL entity-related things like "onkilled"?

    }

    /// <summary>
    /// Interface representing a component on an object that receives destroyable entity event calls
    /// </summary>
    public interface IReceiveDamageableEntityEvents : IReceiveEntityEvents
    {
        void DamageTaken(ActorHitInfo data);

        void Killed();
    }

    /// <summary>
    /// Interface representing a component on an object that receives actor entity event calls
    /// </summary>
    public interface IReceiveActorEntityEvents : IReceiveDamageableEntityEvents
    {

    }
}