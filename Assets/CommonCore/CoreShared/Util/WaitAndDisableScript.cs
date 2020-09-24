using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Util
{

    /// <summary>
    /// Waits and then disables or destroys the target gameobject
    /// </summary>
    public class WaitAndDisableScript : MonoBehaviour
    {
        [SerializeField, Tooltip("If not set, will disable/destroy the attached gameobject")]
        private GameObject Target = null;

        [SerializeField]
        private float Delay = 5f;
        [SerializeField]
        private bool Realtime = false;
        [SerializeField]
        private bool DestroyTarget = false;

        private Coroutine CurrentCoroutine = null;

        private void Start()
        {
            if (Delay == 0)
                HandleDisable();
            else
                CurrentCoroutine = StartCoroutine(CoWaitAndDisable());
        }

        private IEnumerator CoWaitAndDisable()
        {
            if (Realtime)
                yield return new WaitForSecondsRealtime(Delay);
            else
                yield return new WaitForSeconds(Delay);

            HandleDisable();
        }

        private void HandleDisable()
        {
            GameObject target = Target;
            if (target == null)
                target = gameObject;

            if (DestroyTarget)
                Destroy(target);
            else
                target.SetActive(false);
        }
    }
}