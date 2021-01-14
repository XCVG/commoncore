using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.State
{
    public partial class GameState
    {

        [Init(-10)]
        private void PriorityNegativeInit()
        {
            Debug.Log("Priority -10 init");
        }

        [Init(10)]
        private void Priority10Init()
        {
            Debug.Log("Priority 10 init");
        }

        [Init(20)]
        private void Priority20Init()
        {
            Debug.Log("Priority 20 init");
        }

    }
}