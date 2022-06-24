using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

namespace CommonCore.Video
{

    /// <summary>
    /// Utility methods for video playback
    /// </summary>
    public static class VideoUtils
    {
        /// <summary>
        /// Sets up a video player for playback
        /// </summary>
        public static void SetupVideoPlayer(VideoPlayer videoPlayer, string videoName) => SetupVideoPlayer(videoPlayer, videoName, false, 1.0f);

        /// <summary>
        /// Sets up a video player for playback
        /// </summary>
        public static void SetupVideoPlayer(VideoPlayer videoPlayer, string videoName, bool loop, float volume)
        {
            var module = CCBase.GetModule<VideoModule>();
            var videoPath = module.GetPathForVideo(videoName);
#if !UNITY_WEBGL
            if (string.IsNullOrEmpty(videoPath) || !File.Exists(videoPath))
                throw new FileNotFoundException($"[{nameof(SetupVideoPlayer)}] Could not find video file", videoPath);
#endif

            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = videoPath;
            videoPlayer.isLooping = false;

            videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetDirectAudioVolume(0, 1.0f);            
        }

        /// <summary>
        /// Waits for a video player to be finished preparing
        /// </summary>
        public static IEnumerator WaitForPrepare(VideoPlayer videoPlayer)
        {
            videoPlayer.Prepare();

            while (!videoPlayer.isPrepared)
                yield return null;
        }

        /// <summary>
        /// Waits for a video player to be finished preparing, then starts playback
        /// </summary>
        public static IEnumerator WaitForPrepareThenPlay(VideoPlayer videoPlayer)
        {
            yield return WaitForPrepare(videoPlayer);
            videoPlayer.Play();
        }

        /// <summary>
        /// Waits for a video player to finish playing back, starting prepare and playback if necessary
        /// </summary>
        public static IEnumerator WaitForPlayback(VideoPlayer videoPlayer)
        {
            if (videoPlayer.isLooping)
                throw new InvalidOperationException($"[{nameof(WaitForPlayback)}] Cannot wait for playback on a loopable video!");

            if(!videoPlayer.isPrepared)
                yield return WaitForPrepare(videoPlayer);

            if(!videoPlayer.isPlaying)
                videoPlayer.Play();

            while (videoPlayer.isPlaying)
                yield return null;
        }

        /// <summary>
        /// Waits for a video player to finish playing back, starting prepare and playback if necessary. Pausable.
        /// </summary>
        public static IEnumerator WaitForPlayback(VideoPlayer videoPlayer, Func<bool> isPausedFunc)
        {
            if (videoPlayer.isLooping)
                throw new InvalidOperationException($"[{nameof(WaitForPlayback)}] Cannot wait for playback on a loopable video!");

            if (isPausedFunc == null)
                throw new ArgumentNullException(nameof(isPausedFunc), $"[{nameof(WaitForPlayback)}] A valid is-paused function is required");

            if (!videoPlayer.isPrepared)
                yield return WaitForPrepare(videoPlayer);

            videoPlayer.Play();

            while (videoPlayer.isPlaying)
            {
                if (isPausedFunc())
                {
                    videoPlayer.Pause();
                    while (isPausedFunc())
                    {
                        yield return null;
                    }
                    videoPlayer.Play();
                }

                yield return null;
            }
        }


    }
}