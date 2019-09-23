using CommonCore.LockPause;
using UnityEngine;

namespace CommonCore.RpgGame.World
{

    /// <summary>
    /// Component that handles player view bobbing. Attach to view bob point
    /// </summary>
    public class PlayerMovebobComponent : MonoBehaviour
    {
        private const float Threshold = 0.001f;

        [SerializeField, Header("Components")]
        private PlayerController PlayerController = null;
        [SerializeField]
        private PlayerMovementComponent MovementComponent = null;

        [SerializeField, Header("Bob Options")]
        private float YDisplacement = 0.1f;
        [SerializeField]
        private float YJitter = 0.025f;
        [SerializeField]
        private float BaseVelocity = 0.5f;
        [SerializeField]
        private float VelocityFactor = 0.1f;

        private Vector3 TargetPosition = Vector3.zero;

        private void Start()
        {
            if (PlayerController == null)
                PlayerController = GetComponentInParent<PlayerController>();

            if(PlayerController == null)
            {
                Debug.LogError($"{nameof(PlayerMovebobComponent)} on {name} can't find {nameof(PlayerController)}");
                enabled = false;
                return;
            }

            if (MovementComponent == null)
                MovementComponent = GetComponentInParent<PlayerMovementComponent>();

            if(MovementComponent == null)
            {
                Debug.LogError($"{nameof(PlayerMovebobComponent)} on {PlayerController.name} can't find {nameof(PlayerMovementComponent)}");
                enabled = false;
                return;
            }

        }

        private void LateUpdate()
        {
            if (LockPauseModule.IsPaused())
                return;

            //set new target

            if (!(MovementComponent.IsMoving || (MovementComponent.IsOnSlope && MovementComponent.Velocity.sqrMagnitude > 0)) ||
                !MovementComponent.IsGrounded ||
                !PlayerController.PlayerInControl)
            {
                TargetPosition = Vector3.zero;
            }
            else if ((transform.localPosition - TargetPosition).magnitude < Threshold)
            {                
                float yDirection = -Mathf.Sign(TargetPosition.y); //bob up and down
                TargetPosition = new Vector3(0, yDirection * (YDisplacement + Random.Range(-YJitter, YJitter)), 0);
            }

            //animate toward target
            Vector3 vecToTarget = TargetPosition - transform.localPosition;
            float distToTarget = vecToTarget.magnitude;
            if (distToTarget > Threshold)
            {
                //get extra velocity from movement speed
                float extraVelocity = VelocityFactor * MovementComponent.Velocity.GetFlatVector().magnitude;
                Vector3 dirToTarget = vecToTarget.normalized;
                float moveDist = Mathf.Min(distToTarget, (BaseVelocity + extraVelocity) * Time.deltaTime);
                transform.Translate(moveDist * dirToTarget, Space.Self);
            }

        }
    }
}