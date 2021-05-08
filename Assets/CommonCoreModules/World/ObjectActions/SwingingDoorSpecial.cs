using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.ObjectActions
{

    /// <summary>
    /// Action Special for rotating/swinging doors
    /// </summary>
    /// <remarks>
    /// <para>Do not put this script on the moving part, put it on the fixed part and put the moving part in DoorTransform</para>
    /// </remarks>
    public class SwingingDoorSpecial : MovingDoorSpecial
    {
        [SerializeField, Header("Swinging Door Parameters")]
        private float RotateSpeed = 180f;
        [SerializeField]
        private Vector3 RotateAxis = Vector3.up;
        [SerializeField]
        private float RotateDisplacement = 90.0f;

        [SerializeField]
        private Transform DoorTransform = null;
        [SerializeField]
        private Transform PivotTransform = null;
        [SerializeField]
        private Transform MirrorDoorTransform = null;
        [SerializeField]
        private Transform MirrorPivotTransform = null;
        [SerializeField]
        private Collider DoorCollider = null;

        //TODO "rotate away from activator" logic?

        private float CurrentDisplacement;

        protected override void Start()
        {
            if (BlockedAction != MovingDoorBlockedAction.Continue)
            {
                Debug.LogWarning($"{GetType().Name} on {gameObject.name} has a block action set but this is not yet supported");
            }

            if(PivotTransform != null && MirrorDoorTransform != null && MirrorPivotTransform == null)
            {
                Debug.LogWarning($"{GetType().Name} on {gameObject.name} has a pivot and mirror door set, but no pivot set (will result in undefined behaviour)");
            }

            base.Start();
        }

        protected override IEnumerator CoOpenDoor()
        {
            PlayOpenSound();

            float absRotateDisplacement = Mathf.Abs(RotateDisplacement);
            while (Mathf.Abs(CurrentDisplacement) < absRotateDisplacement)
            {
                float remaining = absRotateDisplacement - Mathf.Abs(CurrentDisplacement);
                int direction = Math.Sign(RotateDisplacement);
                float rotateMagnitude = Mathf.Min(Mathf.Abs(RotateSpeed) * Time.deltaTime, remaining);
                float rotate = direction * rotateMagnitude;
                RotateDoors(rotate);
                CurrentDisplacement += rotate;

                yield return null;
            }
        }

        protected override IEnumerator CoCloseDoor()
        {
            PlayCloseSound();

            while (Mathf.Abs(CurrentDisplacement) > 0)
            {
                float remaining = Mathf.Abs(CurrentDisplacement);
                int direction = -Math.Sign(RotateDisplacement);
                float rotateMagnitude = Mathf.Min(Mathf.Abs(RotateSpeed) * Time.deltaTime, remaining);
                float rotate = direction * rotateMagnitude;
                /* //doesn't work for swinging doors
                if (BlockedAction != MovingDoorBlockedAction.Continue && DoorCollider != null && Mathf.Abs(CurrentDisplacement) > 0.05f)
                {
                    //detect and handle blocked
                    if (Physics.BoxCast(DoorCollider.bounds.center, DoorCollider.bounds.extents, DoorCollider.transform.forward, DoorCollider.transform.rotation, 0.04f))
                    {
                        if (BlockedAction == MovingDoorBlockedAction.Reverse)
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
                */
                RotateDoors(rotate);
                CurrentDisplacement += rotate;

                yield return null;
            }
        }

        private void RotateDoors(float rotate)
        {
            if (PivotTransform != null)
            {
                DoorTransform.RotateAround(PivotTransform.position, RotateAxis, rotate);
                if (MirrorDoorTransform != null)
                    MirrorDoorTransform.RotateAround(MirrorPivotTransform.position, RotateAxis, -rotate);
            }
            else
            {
                DoorTransform.Rotate(RotateAxis, rotate, Space.World);
                if (MirrorDoorTransform != null)
                    MirrorDoorTransform.Rotate(RotateAxis, -rotate, Space.World);
            }
        }

        protected override void SetDoorOpen()
        {
            if (Mathf.Approximately(CurrentDisplacement, RotateDisplacement))
                return;

            RotateDoors(RotateDisplacement);

            CurrentDisplacement = RotateDisplacement;
        }

        protected override void SetDoorClosed()
        {
            if (Mathf.Approximately(CurrentDisplacement, 0))
                return;

            RotateDoors(-CurrentDisplacement);

            CurrentDisplacement = 0;
        }
    }
}