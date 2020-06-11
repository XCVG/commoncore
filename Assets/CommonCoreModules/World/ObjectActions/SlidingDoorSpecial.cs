using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.ObjectActions
{

    /// <summary>
    /// Action Special for horizontally or vertically sliding doors
    /// </summary>
    /// <remarks>
    /// <para>Do not put this script on the moving part, put it on the fixed part and put the moving part in DoorTransform</para>
    /// </remarks>
    public class SlidingDoorSpecial : MovingDoorSpecial
    {
        [SerializeField, Header("Sliding Door Parameters")]
        private float MoveSpeed = 5.0f;
        [SerializeField]
        private Vector3 MoveVector = Vector3.up;
        [SerializeField]
        private float MoveDisplacement = 2.0f;
        
        [SerializeField]
        private Transform DoorTransform = null;
        [SerializeField, Tooltip("Must be set for BlockedAction to work")]
        private Collider DoorCollider = null;

        private float CurrentDisplacement;

        private void Start()
        {
            if(BlockedAction != MovingDoorBlockedAction.Continue && DoorCollider == null)
            {
                Debug.LogWarning($"{GetType().Name} on {gameObject.name} has a block action set but has no collider set (will never detect the door as blocked)");
            }
        }

        protected override IEnumerator CoOpenDoor()
        {
            PlayOpenSound();

            while (CurrentDisplacement < MoveDisplacement)
            {
                float remainingDisplacement = MoveDisplacement - CurrentDisplacement;
                Vector3 scaledMoveVector = MoveVector.normalized * Mathf.Min(remainingDisplacement, MoveSpeed * Time.deltaTime);
                DoorTransform.Translate(transform.TransformVector(scaledMoveVector), Space.World);
                CurrentDisplacement += scaledMoveVector.magnitude;

                yield return null;
            }
        }

        protected override IEnumerator CoCloseDoor()
        {
            PlayCloseSound();

            while (CurrentDisplacement > 0)
            {
                Vector3 scaledMoveVector = -MoveVector.normalized * Mathf.Min(CurrentDisplacement, MoveSpeed * Time.deltaTime);                
                if(BlockedAction != MovingDoorBlockedAction.Continue && DoorCollider != null && CurrentDisplacement > 0.05f)
                {
                    //detect and handle blocked
                    if(Physics.BoxCast(DoorCollider.bounds.center, DoorCollider.bounds.extents, Vector3.down, transform.rotation, 0.04f))
                    {
                        if(BlockedAction == MovingDoorBlockedAction.Reverse)
                        {
                            DoorOpen = true;
                            yield return CoOpenDoor(); //a bit buggy since this can jam a door open when it normally wouldn't be jammed open
                            yield break;
                        }
                        else
                        {
                            yield return null;
                            continue;
                        }
                    }
                }
                DoorTransform.Translate(transform.TransformVector(scaledMoveVector), Space.World);
                CurrentDisplacement -= scaledMoveVector.magnitude;

                yield return null;
            }
        }
    }
}