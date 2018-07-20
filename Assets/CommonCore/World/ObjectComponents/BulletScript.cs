using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{
    public class BulletScript : MonoBehaviour
    {
        public ActorHitInfo HitInfo;
        public float StayTime = 0;

        void Start()
        {
            gameObject.layer = 9; //9=bullet

            if (StayTime > 0)
                Destroy(this.gameObject, StayTime);
        }

        void OnCollisionEnter(Collision collision)
        {
            var ac = collision.gameObject.GetComponent<ActorController>();
            if(ac != null)
            {
                ac.TakeDamage(HitInfo);
                Destroy(this.gameObject);
                return;
            }

            var ahc = collision.gameObject.GetComponent<ActorHitboxComponent>();
            if(ahc != null)
            {
                //we'll let the other component handle the collision...

                return; //...but we won't destroy this one 
            }

            Destroy(this.gameObject);
        }
    }
}