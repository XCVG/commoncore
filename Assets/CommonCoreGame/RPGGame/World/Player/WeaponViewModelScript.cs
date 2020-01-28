using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.World
{
    public enum ViewModelState
    {
        Idle, Raise, Lower, Block, Reload, Charge, Fire, Recock //(we may remove or defer Charge and Recock though we kinda need them for bows and bolt guns respectively)
    }

    public abstract class WeaponViewModelScript : MonoBehaviour
    {

        protected abstract void Start();

        protected abstract void Update();

        public abstract void SetVisibility(bool visible);

        public abstract void SetState(ViewModelState newState, ViewModelHandednessState handedness, float timeScale);

        public abstract (string, float) GetHandAnimation(ViewModelState newState, ViewModelHandednessState handedness);

    }
}