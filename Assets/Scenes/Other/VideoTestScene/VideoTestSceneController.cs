using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using CommonCore;
using CommonCore.Video;

namespace VideoTestScene
{

    public class VideoTestSceneController : MonoBehaviour
    {
        public Text StatusText;
        public VideoPlayer VideoPlayer;

        private void Start()
        {
            StartCoroutine(CoDoVideoTest());
        }

        private IEnumerator CoDoVideoTest()
        {
            StatusText.text = "Video player setting up";

            VideoUtils.SetupVideoPlayer(VideoPlayer, "test");

            StatusText.text = "Video player starting";

            yield return VideoUtils.WaitForPlayback(VideoPlayer);

            StatusText.text = "Video player done";
        }
    }
}