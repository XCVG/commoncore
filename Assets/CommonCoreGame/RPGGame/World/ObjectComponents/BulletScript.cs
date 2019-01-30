using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.World
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
            var ahc = collision.gameObject.GetComponent<ActorHitboxComponent>();
            if (ahc != null)
            {
                //we'll let the other component handle the collision...

                return; //...but we won't destroy this one 
            }

            var ac = collision.gameObject.GetComponent<ActorController>();
            if(ac != null)
            {
                if (ac == HitInfo.Originator) //no friendly fire for now
                    return;

                HitInfo.HitCoords = transform.position;
                ac.TakeDamage(HitInfo);
                Destroy(this.gameObject);
                return;
            }

            var pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc == null)
                pc = collision.gameObject.GetComponentInParent<PlayerController>();
            if(pc != null)
            {
                if (pc == HitInfo.Originator)
                    return;

                HitInfo.HitCoords = transform.position;
                pc.TakeDamage(HitInfo);
                Destroy(this.gameObject);
                return;
            }            

            Destroy(this.gameObject);
        }
    }
}