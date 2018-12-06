using CommonCore.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UITestScene
{

    public class UITestSceneController : MonoBehaviour
    {

        public Text TestText;
        public TextAnimation TextAnim;

        public void OnClickStart()
        {
            TextAnim = TextAnimation.TypeOn(TestText, "This is a test string!", 3.0f);
        }

        public void OnClickAbort()
        {
            TextAnim.Abort();
        }

        public void OnClickComplete()
        {
            TextAnim.Complete();
        }
    }
}