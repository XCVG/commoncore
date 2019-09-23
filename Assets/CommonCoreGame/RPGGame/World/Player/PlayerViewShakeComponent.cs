using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.World
{

    /// <summary>
    /// Component that handles player view shaking
    /// </summary>
    /// <remarks>Currently unused; it didn't work out for what we wanted it for</remarks>
    public class PlayerViewShakeComponent : MonoBehaviour
    {
        private const float Threshold = 0.001f;

        private int CurrentShakePriority = -1; //-1 indicates "not shaking"
        private Coroutine CurrentShakeCoroutine = null;

        //TODO add support for a "tilt" style shake, because panning doesn't look right for weapons

        //uses the player's message loop

        public void RequestShake(float intensity, Vector3 direction, float time, int priority, bool overrideCurrent)
        {
            if (priority < CurrentShakePriority || (priority == CurrentShakePriority && !overrideCurrent))
                return;

            if (CurrentShakeCoroutine != null)
            {
                StopCoroutine(CurrentShakeCoroutine);
                CurrentShakeCoroutine = null;
            }

            CurrentShakePriority = priority;
            CurrentShakeCoroutine = StartCoroutine(ShakeCoroutine(intensity, direction, time));
        }

        public void CancelShake()
        {            
            StopCoroutine(CurrentShakeCoroutine);
            CurrentShakeCoroutine = null;
            CurrentShakePriority = -1;
            transform.localPosition = Vector3.zero;
        }

        private IEnumerator ShakeCoroutine(float intensity, Vector3 direction, float time)
        {

            if (direction == Vector3.zero)
            {
                Debug.LogError("Random shaking is not supported yet!");
            }
            else
            {
                Vector3 target = intensity * direction; //"jerk" in the direction of the target

                //TODO play with this distribution a little
                float timeToTarget = time * 0.33f;
                float timeToZero = time * 0.66f;

                //move to target
                for(float elapsed = 0; elapsed < timeToTarget; elapsed += Time.deltaTime)
                {
                    float ratio = Mathf.Min(elapsed / timeToTarget, 1f);
                    transform.localPosition = target * ratio;

                    yield return null;
                }

                //yield return null;

                //move back to center
                Vector3 vecTargetToCenter = -target;
                for(float elapsed = 0; elapsed < timeToZero; elapsed += Time.deltaTime)
                {
                    float ratio = Mathf.Min(elapsed / timeToTarget, 1f);
                    transform.localPosition = target + vecTargetToCenter * ratio;

                    yield return null;
                }

                //yield return null;

                transform.localPosition = Vector3.zero;
                
            }

            yield return null;

            CurrentShakePriority = -1;
            CurrentShakeCoroutine = null;

        }
    }
}