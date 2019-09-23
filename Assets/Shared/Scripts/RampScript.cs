using CommonCore.RpgGame.World;
using CommonCore.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Test script that flings a player
/// </summary>
public class RampScript : MonoBehaviour
{
    [SerializeField]
    private Vector3 Velocity = new Vector3(0f, 2f, 4f);
    [SerializeField]
    private Vector3 Displacement = new Vector3(0, 0.2f, 0);

    //BROKEN AS FUCK TODO FIGURE OUT HOW TO MAKE PLAYER ACTIVATE TRIGGERS

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("welp");

        PlayerController pc = null;

        var ahc = other.gameObject.GetComponent<ActorHitboxComponent>();
        if (ahc != null)
            pc = ahc.ParentController as PlayerController;
        else if (other.gameObject.GetComponent<BaseController>() is PlayerController playerController)
            pc = playerController;

        if (pc != null)
        {
            DoPush(pc);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnTriggerEnter(collision.collider);
        
    }

    private void DoPush(PlayerController pc)
    {
        Quaternion quatFacing = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up);


        pc.MovementComponent.Push(quatFacing * Velocity, quatFacing * Displacement);
    }
}
