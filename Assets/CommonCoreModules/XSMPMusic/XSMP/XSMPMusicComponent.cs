using CommonCore.Audio;
using CommonCore.Async;
using CommonCore.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CommonCore.DebugLog;

namespace CommonCore.XSMP
{
    public class XSMPMusicComponent : UserMusicComponent
    {
        private XSMPState XSMPState => XSMPModule.Instance.State;


        public override bool Enabled
        {
            get
            {
                //throw new System.NotImplementedException();
                return XSMPState.ComponentEnabled;
            }
            set
            {
                //throw new System.NotImplementedException();
                XSMPState.ComponentEnabled = value;
                if(value)
                {
                    //attach music, even if we don't have any
                    AudioClip clip = XSMPState.CurrentClip?.Clip;
                    XSMPState.CurrentClip?.AddRef();

                    AudioPlayer.Instance.SetMusic(clip, MusicSlot.User, XSMPState.Volume, false, true);
                    if (XSMPState.Playing)
                        AudioPlayer.Instance.StartMusic(MusicSlot.User);
                    

                }
                else
                {
                    //set playing state to false
                    XSMPState.Playing = false;
                }

                QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("XSMPStateChanged"));
            }
        }

        public override GameObject PanelPrefab => CoreUtils.LoadResource<GameObject>("Modules/XSMPMusic/IGUI_XSMPPanel");

        public override string NiceName => "XSMP";

        public override void ReportClipReleased(AudioClip clip)
        {
            if (XSMPState.CurrentClip != null && XSMPState.CurrentClip.Clip == clip)
            {
                Debug.Log("Released clip " + clip.name); //I expect a lot of bugs
                XSMPState.CurrentClip?.ReleaseRef();
            }

            //TODO trigger a cache clean?
            //XSMPModule.Instance.TrimMediaCache();
        }

        public override void ReportTime(float time)
        {
            XSMPState.TrackTime = time;
        }

        public override void SignalTrackEnded()
        {
            //throw new System.NotImplementedException();
            //swap to next track if possible, stop playback if not
            if(XSMPState.Shuffle && (XSMPState.ShuffleCurrentIndex < XSMPState.ShuffledIndices.Count - 1 || XSMPState.Repeat))
            {
                XSMPState.TrackTime = 0;

                XSMPState.ShuffleCurrentIndex++;
                if (XSMPState.ShuffleCurrentIndex >= XSMPState.ShuffledIndices.Count)
                    XSMPState.ShuffleCurrentIndex = 0;

                int nextIndex = XSMPState.ShuffledIndices[XSMPState.ShuffleCurrentIndex];

                AsyncUtils.RunWithExceptionHandling(async () => {
                    XSMPState.CurrentQueueIndex = nextIndex;
                    //XSMPState.TrackTime = 0;
                    await XSMPModule.Instance.ChangePlaybackTrack(default); //should probably be smarter about the CancellationToken
                    QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("XSMPStateChanged"));
                });
            }
            else if(XSMPState.CurrentQueueIndex < XSMPState.QueueData.Count - 1 || XSMPState.Repeat)
            {
                XSMPState.TrackTime = 0;
                AsyncUtils.RunWithExceptionHandling(async () => {                    
                    XSMPState.CurrentQueueIndex++;
                    if (XSMPState.CurrentQueueIndex >= XSMPState.QueueData.Count)
                        XSMPState.CurrentQueueIndex = 0;
                    //XSMPState.TrackTime = 0;
                    await XSMPModule.Instance.ChangePlaybackTrack(default); //should probably be smarter about the CancellationToken
                    QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("XSMPStateChanged"));
                });
            }
            else
            {
                AsyncUtils.RunWithExceptionHandling(async () => {
                    await XSMPModule.Instance.StopPlayback(default);
                    QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("XSMPStateChanged"));
                });
            }
        }

        public override void SignalAudioRestarted()
        {
            //invalidate cache and reload current clip

            //stop/unload music from AudioPlayer
            if(Enabled && XSMPState.Playing)
            {
                AudioPlayer.Instance.StopMusic(MusicSlot.User);
                AudioPlayer.Instance.SetMusicClip(null, MusicSlot.User);
            }

            //clear cache entirely
            XSMPState.CurrentClip = null;
            foreach(var rClip in XSMPState.ClipCache.Values)
            {
                UnityEngine.Object.Destroy(rClip.Clip);
            }
            XSMPState.ClipCache.Clear();

            //reload/start track
            AsyncUtils.RunWithExceptionHandling(async () =>
            {
                if (Enabled)
                {
                    float trackTime = XSMPState.TrackTime;
                    //load track
                    await XSMPModule.Instance.ChangePlaybackTrack(default);

                    if (XSMPState.Playing)
                    {
                        //start track
                        AudioPlayer.Instance.StartMusic(MusicSlot.User);
                        XSMPState.TrackTime = trackTime;
                        AudioPlayer.Instance.SeekMusic(MusicSlot.User, XSMPState.TrackTime);
                    }
                }
            });
        }

    }
}