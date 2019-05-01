using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.BasicConsole
{

    /// <summary>
    /// Controller for the basic console implementation
    /// </summary>
    public class BasicConsoleController : MonoBehaviour
    {
        //TODO split this so that the majority is attached to Canvas and only runs if the console is visible

        private struct IncomingConsoleMessage
        {
            public string Message;
            public LogType Type;
            public object Originator;
        }

        [SerializeField]
        private GameObject CanvasObject = null;
        [SerializeField]
        private GameObject ScrollPanel = null;
        [SerializeField]
        private GameObject ScrollItemTemplate = null;

        private int CurrentBottomLine = 0;
        private bool AutoScroll = true;

        private bool PaintLinesEmergencyAbort = false;

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

            ScrollItemTemplate.SetActive(false);
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
            bool receivedMessages = false;
            while(!IncomingMessages.IsEmpty)
            {
                if(IncomingMessages.TryDequeue(out IncomingConsoleMessage incomingMessage))
                {
                    Messages.Add(new ConsoleMessage(incomingMessage.Message, incomingMessage.Type));
                    receivedMessages = true;
                }
            }

            if (receivedMessages && AutoScroll)
            {
                CurrentBottomLine = GetShownLines() - 1; //will absolutely explode if we have expanded messages
                PaintMessages();                
            }
        }

        //WIP handle the actual display

        //TODO NEXT handle scrolling

        /// <summary>
        /// Repaint the scroll view with messages.
        /// </summary>
        /// <remarks>
        /// Slow.
        /// </remarks>
        private void PaintMessages()
        {
            try
            {
                //emergency abort to prevent a meltdown from this dumping debug log and then receiving them and trying to process them, overflowing in an infinite loop
                if (PaintLinesEmergencyAbort)
                    return;

                //don't fail
                if (Messages.Count <= 0)
                    return;

                //clear the window
                ClearScrollWindow();

                //locate actual top
                float lineHeight = (ScrollItemTemplate.transform as RectTransform).rect.height;
                int numLines = (int)((ScrollPanel.transform as RectTransform).rect.height / lineHeight);
                int topLineIndex = Math.Max(CurrentBottomLine - numLines + 1, 0); //I'm not sure where the off-by-one actually is XD

                for (int lineNum = 0; lineNum < numLines; lineNum++)
                {
                    var (message, offset) = GetLineByNumber(topLineIndex + lineNum);
                    string text = message.Lines[offset];

                    //handle the actual drawing
                    float yOffset = lineNum * lineHeight;
                    var lineObject = Instantiate(ScrollItemTemplate, ScrollPanel.transform);
                    var lineObjectRectTransform = lineObject.transform as RectTransform;
                    lineObjectRectTransform.anchoredPosition = new Vector2(0, -yOffset); //we draw from top to bottom!
                    lineObject.GetComponentInChildren<Text>().text = text;
                    lineObject.SetActive(true);
                }
                
            }
            catch(Exception e)
            {
                Debug.LogError("BasicConsoleController failed because: ");
                Debug.LogException(e);
                PaintLinesEmergencyAbort = true;
            }
        }

        /// <summary>
        /// Gets the actual line of text at a line number
        /// </summary>
        private (ConsoleMessage message, int offset) GetLineByNumber(int lineNumber)
        {
            //watch for off-by-ones!

            int messageIndex = 0, lineIndex = 0, offset = 0;
            while(lineIndex < lineNumber)
            {
                var message = Messages[messageIndex];
                if (message.Expanded)
                {
                    int remainingLines = lineNumber - lineIndex;
                    if (message.ShownLines > remainingLines)
                    {
                        offset = remainingLines;
                        break;
                    }
                    else
                    {
                        lineIndex += message.ShownLines;
                    }
                }
                else
                {
                    lineIndex++;
                }

                messageIndex++;                
            }

            return (Messages[messageIndex], offset);
            
        }

        /// <summary>
        /// Gets the number of shown lines in all messages
        /// </summary>
        private int GetShownLines()
        {
            int lines = 0;
            foreach(ConsoleMessage message in Messages)
            {
                lines += message.ShownLines;
            }
            return lines;
        }

        /// <summary>
        /// Gets the total number of all lines in all messages
        /// </summary>
        private int GetAllLines()
        {
            int lines = 0;
            foreach(ConsoleMessage message in Messages)
            {
                lines += message.Lines.Length;
            }
            return lines;
        }

        private void ClearScrollWindow()
        {
            foreach(Transform t in ScrollPanel.transform)
            {
                if (t.gameObject != ScrollItemTemplate)
                    Destroy(t.gameObject);
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

    }
}