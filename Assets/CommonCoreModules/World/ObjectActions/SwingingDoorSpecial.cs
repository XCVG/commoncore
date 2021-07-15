using CommonCore.World;
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
        [SerializeField]
        private bool OpenAwayFromActivator = false;

        private bool ReverseDisplacement = false;
        private float CurrentDisplacement;

        private float EffectiveRotateDisplacement => RotateDisplacement * (ReverseDisplacement ? -1 : 1);

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

        public override void Execute(ActionInvokerData data)
        {
            if (Locked || (!AllowInvokeWhenDisabled && !isActiveAndEnabled))
                return;

            ReverseIfNeeded(data.Activator);

            ToggleDoor();
            SaveState();

            if (!Repeatable)
                Locked = true;
        }

        private void ReverseIfNeeded(BaseController activator)
        {
            if(OpenAwayFromActivator && activator != null && !DoorOpen)
            {
                //reverse direction if needed

                //first find if we rotate away from the forward vector by default, using logic which is probably correct but completely heinous
                int defaultRotationSign = Mathf.RoundToInt(Mathf.Sign(PivotTransform.position.x) * Mathf.Sign(RotateAxis.y) * Mathf.Sign(RotateDisplacement));
                //Debug.Log(defaultRotationSign);

                bool defaultRotatesAway = defaultRotationSign > 0;

                //then we decide if we're behind or in front of the door using the magic of vectors
                float dot = Vector2.Dot((activator.transform.position - transform.position).GetFlatVector().normalized, transform.forward.GetFlatVector().normalized);

                bool behindDoor = dot < 0;
                //Debug.Log("behind door: " + behindDoor);

                ReverseDisplacement = (defaultRotatesAway && behindDoor) || (!defaultRotatesAway && !behindDoor); //could have been done with XOR or XNOR?
            }

        }

        protected override IEnumerator CoOpenDoor()
        {
            PlayOpenSound();

            float absRotateDisplacement = Mathf.Abs(RotateDisplacement);
            while (Mathf.Abs(CurrentDisplacement) < absRotateDisplacement)
            {
                float remaining = absRotateDisplacement - Mathf.Abs(CurrentDisplacement);
                int direction = Math.Sign(EffectiveRotateDisplacement);
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
                int direction = -Math.Sign(EffectiveRotateDisplacement);
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
            if (Mathf.Approximately(CurrentDisplacement, EffectiveRotateDisplacement))
                return;

            RotateDoors(EffectiveRotateDisplacement);

            CurrentDisplacement = RotateDisplacement;
        }

        protected override void SetDoorClosed()
        {
            if (Mathf.Approximately(CurrentDisplacement, 0))
                return;

            RotateDoors(-CurrentDisplacement);

            CurrentDisplacement = 0;
        }

        protected override void RestoreState()
        {
            if (!PersistState)
                return;

            if (BaseSceneController.Current.LocalStore.TryGetValue(LocalStorePersistKey + "_ReverseDisplacement", out object reversedObj) && reversedObj is bool reversed)
            {
                ReverseDisplacement = reversed;
            }

            base.RestoreState();
        }

        protected override void SaveState()
        {
            if (!PersistState)
                return;

            BaseSceneController.Current.LocalStore[LocalStorePersistKey + "_ReverseDisplacement"] = ReverseDisplacement;

            base.SaveState();
        }
    }
}