using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.BasicConsole
{

    /// <summary>
    /// Controller for the basic console implementation
    /// </summary>
    public class BasicConsoleController : MonoBehaviour
    {
        private struct IncomingConsoleMessage
        {
            public string Message;
            public LogType Type;
            public object Originator;
        }

        [SerializeField]
        private GameObject CanvasObject = null;

        //we'll optimize these later
        private ConcurrentQueue<IncomingConsoleMessage> IncomingMessages = new ConcurrentQueue<IncomingConsoleMessage>();
        private List<ConsoleMessage> Messages = new List<ConsoleMessage>();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Application.logMessageReceivedThreaded += HandleLog;
        }

        private void Start()
        {
            CanvasObject.SetActive(false);
        }

        private void OnDestroy()
        {
            Application.logMessageReceivedThreaded -= HandleLog;
        }

        private void Update()
        {
            if(UnityEngine.Input.GetKeyDown(KeyCode.BackQuote) || UnityEngine.Input.GetKeyDown(KeyCode.Tilde))
            {
                CanvasObject.SetActive(!CanvasObject.activeSelf);
            }

            //dequeue messages and push them into the message list
            while(!IncomingMessages.IsEmpty)
            {
                if(IncomingMessages.TryDequeue(out IncomingConsoleMessage incomingMessage))
                {
                    Messages.Add(new ConsoleMessage(incomingMessage.Message, incomingMessage.Type));
                }
            }
        }



        //WIP capture debug logs and explicit messages
        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            IncomingMessages.Enqueue(new IncomingConsoleMessage() { Message = logString, Type = type, Originator = null });
        }

        internal void HandleExplicitLog(string logString, LogType type)
        {
            HandleLog(logString, string.Empty, type);
        }


        //TODO handling input of commands

        //TODO the actual scrollview (will probably push that into a separate class)
    }
}