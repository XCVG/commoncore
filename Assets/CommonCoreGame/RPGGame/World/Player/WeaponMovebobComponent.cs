using CommonCore.Config;
using CommonCore.LockPause;
using CommonCore.RpgGame.Rpg;
using CommonCore.State;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.World
{

    /// <summary>
    /// Component that handles weapon move bobbing. Attach to weapon bob point
    /// </summary>
    public class WeaponMovebobComponent : MonoBehaviour
    {

        private const float Threshold = 0.0001f;

        [SerializeField, Header("Components")]
        private PlayerMovementComponent MovementComponent = null;
        [SerializeField]
        private PlayerWeaponComponent WeaponComponent = null;

        [SerializeField, Header("Bob Options")]
        private float YDisplacement = 0.1f;
        [SerializeField]
        private float YJitter = 0.005f;
        [SerializeField]
        private float XDisplacement = 0.1f;
        [SerializeField]
        private float XJitter = 0.005f;
        [SerializeField]
        private float BaseVelocity = 0.5f;
        [SerializeField]
        private float VelocityFactor = 0.1f;
        [SerializeField]
        private float ADSDisplacementMultiplier = 0.1f;
        [SerializeField]
        private float ADSVelocityMultiplier = 0.1f;
        
        private Vector3 TargetPosition = Vector3.zero;
        private int TargetLastXSign = -1;
        private bool WasInADS = false;
        private bool StopAndCenter = false;

        private void Start()
        {
            if (MovementComponent == null)
                MovementComponent = GetComponentInParent<PlayerMovementComponent>();

            if (MovementComponent == null)
            {
                Debug.LogError($"{nameof(WeaponMovebobComponent)} on {name} can't find {nameof(PlayerMovementComponent)}");
                enabled = false;
                return;
            }

            if (WeaponComponent == null)
                WeaponComponent = GetComponentInParent<PlayerWeaponComponent>();

            if (WeaponComponent == null)
            {
                Debug.LogError($"{nameof(WeaponMovebobComponent)} on {name} can't find {nameof(PlayerWeaponComponent)}");
                enabled = false;
                return;
            }

            WasInADS = WeaponComponent.IsADS;
        }

        private void LateUpdate()
        {
            if (LockPauseModule.IsPaused())
                return;

            if (!ConfigState.Instance.GetGameplayConfig().BobEffects || !WeaponUseMovebob) //cancel viewbob if disabled
            {
                TargetPosition = Vector3.zero;
                transform.localPosition = TargetPosition;
                return;
            }

            bool isADS = WeaponComponent.IsADS;
            if (isADS && !WasInADS) //ADS hack
            {
                transform.localPosition = Vector3.zero;
                TargetPosition = Vector3.zero;
                WasInADS = true;
            }

            if(!isADS && WasInADS)
            {
                //reset state but don't change anything!
                WasInADS = false;
            }

            bool movementStopped = !(MovementComponent.IsMoving || (MovementComponent.IsOnSlope && MovementComponent.Velocity.sqrMagnitude > 0)) ||
                !MovementComponent.IsGrounded;

            if ((TargetPosition - transform.localPosition).magnitude < Threshold)
            {
                if (movementStopped)
                {
                    if(TargetLastXSign == -1 || TargetLastXSign == 1)
                        TargetLastXSign *= -2; //WTF is this for?
                    TargetPosition = Vector3.zero;
                    StopAndCenter = true;
                }
                //more complex selection logic
                else if (Mathf.Approximately(TargetPosition.y, 0))
                {
                    //move up and to one side
                    //TargetLastXSign *= -1;
                    TargetLastXSign = Math.Sign(TargetLastXSign);
                    TargetPosition = new Vector3(TargetLastXSign * (XDisplacement + UnityEngine.Random.Range(-XJitter, XJitter)),
                        YDisplacement + UnityEngine.Random.Range(-YJitter, YJitter),
                        0);
                    if (isADS)
                        TargetPosition *= ADSDisplacementMultiplier;
                    StopAndCenter = false;
                }
                else
                {
                    //return to center
                    TargetLastXSign *= -1;
                    TargetPosition = Vector3.zero;
                    StopAndCenter = false;
                }

            }

            //animate toward target (same as player movebob I think)
            Vector3 vecToTarget = TargetPosition - transform.localPosition;
            float distToTarget = vecToTarget.magnitude; //this is recalculated because the above code block may change TargetPosition
            if (distToTarget > Threshold)
            {
                //get extra velocity from movement speed
                float extraVelocity = VelocityFactor * MovementComponent.Velocity.GetFlatVector().magnitude;
                Vector3 dirToTarget = vecToTarget.normalized;
                float moveDist = Mathf.Min(distToTarget, (BaseVelocity + extraVelocity) * Time.deltaTime * (isADS ? ADSVelocityMultiplier : 1));
                transform.Translate(moveDist * dirToTarget, Space.Self);
                if(StopAndCenter && (TargetPosition - transform.localPosition).magnitude <= Threshold)
                {
                    transform.localPosition = Vector3.zero;
                }
            }
            else if (StopAndCenter)
            {
                transform.localPosition = Vector3.zero;
                //Debug.Log(TargetPosition);
            }

        }

        //wtf
        private bool WeaponUseMovebob => !GameState.Instance.PlayerRpgState.Equipped.GetOrDefault((int)EquipSlot.RightWeapon, null)?.ItemModel.CheckFlag(ItemFlag.WeaponNoMovebob) ?? true;
    }
}