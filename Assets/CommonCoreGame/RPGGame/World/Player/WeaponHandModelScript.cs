using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.World
{
    public class WeaponHandModelScript : MonoBehaviour
    {
        [Header("Components")]
        public GameObject HandsObject;
        public Animator HandsAnimator;
        public Transform HandBonePoint;

        private void Start()
        {

        }

        public void SetVisibility(bool visible)
        {
            HandsObject.SetActive(visible);
        }

        public void SetState(ViewModelState newState, WeaponViewModelScript weapon, ViewModelHandednessState handednessState, float timeScale) //TODO extend this for 2hand/1hand/ADS
        {
            string stateName = null;
            float stateDuration = -1; //sentinel value for "use animation value"
            if(weapon != null)
            {
                (stateName, stateDuration) = weapon.GetHandAnimation(newState, handednessState); //grab anim params from model if possible
            }
            else
            {
                stateName = GetAnimForState(newState, handednessState);
            }

            HandsAnimator.Play(stateName);
            if (stateDuration > 0)
                HandsAnimator.speed = (1f / stateDuration) * (1f / timeScale); //we assume animations have a duration of 1 second... TODO make it actually scale considering animation length
            else
                HandsAnimator.speed = 1 * (1f / timeScale);

        }

        /// <summary>
        /// Gets the default animations for a state
        /// </summary>
        private string GetAnimForState(ViewModelState state, ViewModelHandednessState handednessState)
        {
            //TODO handle this properly

            //return state.ToString(); //for now

            return WeaponViewModelScript.HandsHidden; //if we have no ViewModel, we should not have hands either
        }

        public (Vector3 position, Quaternion rotation) HandBoneTransform => (HandBonePoint.position, HandBonePoint.rotation);

    }
}