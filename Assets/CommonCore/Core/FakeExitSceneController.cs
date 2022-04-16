using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{

    /// <summary>
    /// Controller for the fake-exit-scene
    /// </summary>
    public class FakeExitSceneController : MonoBehaviour
    {
        public GameObject ExitedMessage;

        private void Start()
        {
            CCBase.OnApplicationQuit(); //force a fake exit
            StartCoroutine(WaitAndDisplayMessage());
        }

        private IEnumerator WaitAndDisplayMessage()
        {
            yield return null; //wait one frame...

            ExitedMessage.SetActive(true); //...then display message
        }
        
    }
}