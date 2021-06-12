using CommonCore.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.World
{

    /// <summary>
    /// Component that declares an enemy as a boss, handles messaging, health bar, etc
    /// </summary>
    /// <remarks>
    /// Right now it pretty much just reports health to the HUDController, but it may do more later
    /// </remarks>
    public class BossComponent : MonoBehaviour
    {
        [SerializeField, Header("Components")]
        private ActorController ActorController = null;
        [SerializeField]
        private ActorInteractionComponent InteractionComponent = null;

        [SerializeField, Header("Options")]
        private bool EnableMessaging = true;
        [SerializeField]
        private bool EnableHealthUpdates = true;
        [SerializeField]
        private string TargetTooltipOverride = null;

        private float LastKnownHealth;
        private bool DeathKnown;

        private void Start()
        {
            if(ActorController == null)
            {
                ActorController = GetComponent<ActorController>();
            }

            if(ActorController == null)
            {
                Debug.LogError($"{nameof(BossComponent)} on {gameObject.name} has no {nameof(ActorController)}");
                enabled = false;
                return;
            }

            if (InteractionComponent == null)
                InteractionComponent = GetComponent<ActorInteractionComponent>();

            if (EnableMessaging)
                QdmsMessageBus.Instance.PushBroadcast(new QdmsKeyValueMessage("RpgBossAwake", "Target", GetTooltip()));

            LastKnownHealth = ActorController.Health / ActorController.MaxHealth;
        }

        private void Update()
        {
            //health reporting
            float currentHealthFraction = ActorController.Health / ActorController.MaxHealth;

            if(!Mathf.Approximately(currentHealthFraction, LastKnownHealth))
            {
                //signal health change
                if(EnableMessaging && EnableHealthUpdates && !DeathKnown)
                {
                    var message = new QdmsKeyValueMessage("RpgBossHealthUpdate", new Dictionary<string, object>() { { "Target", GetTooltip()}, {"Health", currentHealthFraction} });
                    QdmsMessageBus.Instance.PushBroadcast(message);
                }
            }

            LastKnownHealth = currentHealthFraction;

            //death reporting
            if(LastKnownHealth <= 0 && !DeathKnown)
            {
                if(EnableMessaging)
                {
                    QdmsMessageBus.Instance.PushBroadcast(new QdmsKeyValueMessage("RpgBossDead", "Target", GetTooltip()));
                }

                DeathKnown = true;
            }
        }

        /// <summary>
        /// Gets the tooltip/target name to use; our override, interactioncomponent override, base name
        /// </summary>
        private string GetTooltip()
        {
            if (!string.IsNullOrEmpty(TargetTooltipOverride))
                return TargetTooltipOverride;

            if (InteractionComponent != null)
                return InteractionComponent.TooltipOverride;

            return ActorController.gameObject.name;
        }

    }
}