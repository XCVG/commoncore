using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.ObjectActions;
using CommonCore.State;
using CommonCore.DebugLog;
using CommonCore.Dialogue;
using CommonCore.Messaging;

namespace CommonCore.World
{
    public class ActorController : BaseController
    {
        public string CharacterModelIdOverride;

        [Header("Components")]
        public CharacterController CharController;
        public Animator AnimController;
        public ActorInteractableComponent InteractComponent;

        [Header("Interaction")]
        public InteractionType Interaction;
        public string InteractionTarget;
        public ActionSpecial InteractionSpecial;

        public EditorConditional AlternateCondition;
        public InteractionType AltInteraction;
        public string AltInteractionTarget;
        public ActionSpecial AltInteractionSpecial;

        public string TooltipOverride;

        public override void Start()
        {
            base.Start();

            if (CharController == null)
                CharController = GetComponent<CharacterController>();

            if (AnimController == null)
                AnimController = GetComponent<Animator>();

            if (InteractComponent == null)
                InteractComponent = GetComponent<ActorInteractableComponent>();

            if (InteractComponent == null)
                InteractComponent = GetComponentInChildren<ActorInteractableComponent>();

            InteractComponent.ControllerOnInteractDelegate = OnInteract;
            if (!string.IsNullOrEmpty(TooltipOverride))
                InteractComponent.Tooltip = TooltipOverride;
            else
                InteractComponent.Tooltip = name;

        }

        public void OnInteract(ActionInvokerData data)
        {
            if(AltInteraction != InteractionType.None && AlternateCondition.Parse().Evaluate())
            {
                ExecuteInteraction(AltInteraction, AltInteractionTarget, AltInteractionSpecial, data);
            }
            else
            {
                ExecuteInteraction(Interaction, InteractionTarget, InteractionSpecial, data);
            }

        }

        protected void ExecuteInteraction(InteractionType type, string target, ActionSpecial special, ActionInvokerData data)
        {
            switch (type)
            {
                case InteractionType.None:
                    throw new InvalidOperationException();
                case InteractionType.Special:
                    special.Execute(data);
                    break;
                case InteractionType.AmbientMonologue:
                    string msg = DialogueModule.GetMonologue(target).GetLineRandom(); //VERY inefficient, will fix later
                    QdmsMessageBus.Instance.PushBroadcast(new HUDPushMessage(msg));//also a very temporary display
                    //and we need to rework Monologue and implement an audio manager before we can do speech
                    break;
                case InteractionType.Dialogue:
                    DialogueInitiator.InitiateDialogue(target, true, null);
                    break;
                case InteractionType.Script:
                    throw new NotImplementedException(); //we will have explicit support, soon
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

    }

    //TODO move these types
    public enum InteractionType
    {
        None, Special, AmbientMonologue, Dialogue, Script
    }

    

}