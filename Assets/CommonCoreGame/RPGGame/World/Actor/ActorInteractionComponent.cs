using CommonCore.DebugLog;
using CommonCore.Messaging;
using CommonCore.ObjectActions;
using CommonCore.RpgGame.Dialogue;
using CommonCore.RpgGame.Rpg;
using CommonCore.RpgGame.State;
using CommonCore.RpgGame.UI;
using CommonCore.Scripting;
using CommonCore.State;
using CommonCore.UI;
using CommonCore.World;
using System;
using UnityEngine;

namespace CommonCore.RpgGame.World
{
    /// <summary>
    /// Handles the interaction logic of an Actor
    /// </summary>
    /// <remarks>Do not conflate this with <see cref="ActorInteractableComponent"/> which handles the actual on-interact event</remarks>
    [RequireComponent(typeof(ActorController))]
    public class ActorInteractionComponent : MonoBehaviour
    {
        [SerializeField]
        private ActorController ActorController;

        [Header("Interaction")] //TODO visibility
        public ActorInteractionType Interaction;
        public string InteractionTarget;
        public ActionSpecial InteractionSpecial;

        [Header("Alternate Interaction")]
        public EditorConditional AlternateCondition;
        public ActorInteractionType AltInteraction;
        public string AltInteractionTarget;
        public ActionSpecial AltInteractionSpecial;

        [Header("Dead Interaction"), Tooltip("You need to disable this if you want corpse items to work")]
        public bool UseDeadAction = true;
        public ActorInteractionType DeadInteraction;
        public string DeadInteractionTarget;
        public ActionSpecial DeadInteractionSpecial;

        [Header("Other")]
        public bool InteractionDisabledByHit; //not sure why this is public
        public string TooltipOverride; //TODO move this back into ActorController?
        public SerializableContainerModel CorpseItems;


        public ContainerModel CorpseContainer; //TODO encapsulate this

        private void Start()
        {
            FindComponents();
        }

        public void Init()
        {
            FindComponents();

            if (CorpseContainer == null && CorpseItems?.Items != null && CorpseItems.Items.Length > 0)
            {
                CorpseContainer = SerializableContainerModel.MakeContainerModel(CorpseItems);
            }
        }

        private void FindComponents()
        {
            if (ActorController == null)
                ActorController = GetComponent<ActorController>();

            if (ActorController == null)
                Debug.LogError($"{nameof(ActorInteractionComponent)} on {name} is missing ActorController!");

        }

        public string Tooltip => string.IsNullOrEmpty(TooltipOverride) ? name : TooltipOverride;

        public void OnInteract(ActionInvokerData data)
        {
            if (InteractionDisabledByHit)
                return;

            if (UseDeadAction && ActorController.CurrentAiState == ActorAiState.Dead)
            {
                if (DeadInteraction != ActorInteractionType.None)
                {
                    ExecuteInteraction(DeadInteraction, DeadInteractionTarget, DeadInteractionSpecial, data);
                }
            }
            else if (CorpseContainer != null && ActorController.CurrentAiState == ActorAiState.Dead && data.Activator is PlayerController)
            {
                ContainerModal.PushModal(GameState.Instance.PlayerRpgState.Inventory, CorpseContainer, false, null);
            }
            else
            {
                if (AltInteraction != ActorInteractionType.None && AlternateCondition.Parse().Evaluate())
                {
                    ExecuteInteraction(AltInteraction, AltInteractionTarget, AltInteractionSpecial, data);
                }
                else
                {
                    ExecuteInteraction(Interaction, InteractionTarget, InteractionSpecial, data);
                }
            }
        }

        private void ExecuteInteraction(ActorInteractionType type, string target, ActionSpecial special, ActionInvokerData data)
        {
            switch (type)
            {
                case ActorInteractionType.None:
                    break; //do nothing
                case ActorInteractionType.Special:
                    special.Execute(data);
                    break;
                case ActorInteractionType.AmbientMonologue:
                    string msg = DialogueModule.GetMonologue(target).GetLineRandom(); //VERY inefficient, will fix later
                    //QdmsMessageBus.Instance.PushBroadcast(new HUDPushMessage(msg));//also a very temporary display
                    QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage(msg, 5.0f, true, -1));
                    //and we need to rework Monologue and implement an audio manager before we can do speech
                    break;
                case ActorInteractionType.Dialogue:
                    DialogueInitiator.InitiateDialogue(target, true, null);
                    break;
                case ActorInteractionType.Script:
                    ScriptingModule.Call(target, new ScriptExecutionContext() { Caller = this, Activator = data.Activator.gameObject }, new object[] { });
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

    }
}