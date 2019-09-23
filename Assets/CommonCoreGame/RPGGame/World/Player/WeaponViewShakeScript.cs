using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.LockPause;

namespace CommonCore.RpgGame.World
{

    /// <summary>
    /// Script that handles tilting/shaking the player's view with weapon recoil
    /// </summary>
    /// <remarks>
    /// Attach to a transform node before the first-person camera to use
    /// </remarks>
    public class WeaponViewShakeScript : MonoBehaviour
    {
        private const float Threshold = 0.1f;


        private Quaternion? Target = null;
        private Quaternion Original = Quaternion.identity;
        private float TimeToNext = 0;
        private float TimeToZero = 0;

        private float Elapsed = 0;

        private void LateUpdate()
        {
            if (LockPauseModule.IsPaused())
                return;

            if(Target != null)
            {
                Elapsed += Time.deltaTime;

                if (Elapsed >= TimeToNext)
                {
                    //change to next
                    if(transform.localEulerAngles.magnitude < Threshold) //we're at zero, so end
                    {
                        Elapsed = 0;
                        transform.localEulerAngles = Vector3.zero;
                        Original = transform.localRotation;
                        Target = null;                        
                    }
                    else //we're at target, so try to return to zero
                    {
                        Elapsed = 0;
                        TimeToNext = TimeToZero;
                        Original = transform.localRotation;
                        Target = Quaternion.identity;
                    }
                }
                else
                {
                    //lerp toward target
                    float ratio = Elapsed / TimeToNext;
                    Quaternion rotation = Quaternion.Slerp(Original, Target.Value, ratio);
                    transform.localRotation = rotation;
                }
            }
        }

        public void Shake(Vector3 targetEulerAngles, float time, float violence)
        {
            //Debug.Log($"{targetEulerAngles.ToString()}, {time}s, {violence}");

            //violence is pretty much the ratio of time-to-target versus time-back-to-zero

            TimeToZero = time * violence;
            TimeToNext = time - TimeToZero;

            Original = Quaternion.identity; //?
            Elapsed = 0;
            Target = Quaternion.Euler(targetEulerAngles);
            
        }
    }
}