using CommonCore.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.World
{
    /// <summary>
    /// Script for a basic bullet that works with ActorHitboxComponent and ITakeDamage
    /// </summary>
    public class BulletScript : MonoBehaviour
    {
        private const int BulletLayer = 9; //9=bullet, TODO find a better way of doing this

        public ActorHitInfo HitInfo;
        public float StayTime = 0;

        void Start()
        {
            gameObject.layer = BulletLayer; 

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

            var otherController = collision.gameObject.GetComponent<BaseController>();
            if(otherController == null)
                otherController = collision.gameObject.GetComponentInParent<BaseController>();
            if (otherController != null && otherController is ITakeDamage itd)
            {
                if (otherController == HitInfo.Originator) //no friendly fire for now
                    return;

                HitInfo.HitCoords = transform.position;
                itd.TakeDamage(HitInfo);
            }

            Destroy(this.gameObject);
        }
    }
}