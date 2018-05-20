using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Messaging
{

    public class QdmsMessageComponent : MonoBehaviour
    {
        public QdmsMessageInterface MessageInterface;

        void Start()
        {
            MessageInterface = new QdmsMessageInterface();
        }

        private void LateUpdate()
        {
            if (!MessageInterface.Valid)
                MessageInterface = new QdmsMessageInterface();
        }

    }
}