using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{

    public abstract class InteractableComponent : MonoBehaviour
    {
        public bool PlayerCanActivate;
        public bool NpcCanActivate;
        public string Tooltip;

        public GameObject TooltipObject;
        public bool TooltipFacesPlayer;
        public Sprite TooltipSprite;
        Coroutine TooltipCoroutine;

        public virtual void Start()
        {
            if (TooltipSprite == null)
                TooltipSprite = Resources.Load<Sprite>("TouchUI/TUI_UseGeneric");
        }

        public abstract void OnActivate(GameObject activator);

        public virtual void OnLook(GameObject activator)
        {
            if (!isActiveAndEnabled)
                return;

            //show tooltip object
            if (TooltipObject != null)
            {
                TooltipObject.SetActive(true);
                if (TooltipFacesPlayer)
                {
                    Vector3 pointer = (activator.transform.position - TooltipObject.transform.position);
                    TooltipObject.transform.forward = pointer.normalized;
                }
                if (TooltipCoroutine != null)
                    StopCoroutine(TooltipCoroutine);
                TooltipCoroutine = StartCoroutine(WaitForTooltipCoroutine());
            }
        }

        protected bool CheckEligibility(GameObject activator)
        {
            if (PlayerCanActivate)
            {
                if (activator.GetComponent<PlayerController>())
                    return true;
            }
            if (NpcCanActivate)
            {
                if (activator.GetComponent<ActorController>())
                    return true;
            }
            return false;
        }

        protected IEnumerator WaitForTooltipCoroutine()
        {
            yield return new WaitForSeconds(1f);
            if (TooltipObject != null)
                TooltipObject.SetActive(false);
        }
    }
}