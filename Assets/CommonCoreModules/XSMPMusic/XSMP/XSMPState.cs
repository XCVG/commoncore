using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.XSMP
{


    //TODO tweak visibility
    public class XSMPState
    {
        //state
        public bool ComponentEnabled { get; set; } = false;
        public bool Playing { get; set; } = false;

        //playback
        public float Volume { get; set; } = 1f;
        public bool Repeat { get; set; } = false;
        public bool Shuffle { get; set; } = false;
        public float TrackTime { get; set; } = 0;
        public Dictionary<string, RefCountedClip> ClipCache { get; set; } = new Dictionary<string, RefCountedClip>();
        public RefCountedClip CurrentClip { get; set; } = null; //this (should) alias something in the ClipCache

        //browse
        public List<string> LastBrowsePath { get; set; } = null;
        public List<string> CurrentBrowsePath { get; set; } = null;
        public List<DataRow> BrowseData { get; set; } = null;

        //queue
        public List<SongDataRow> QueueData { get; set; } = null;
        public string CurrentPlaylist { get; set; } = null;
        public string CurrentPlaylistName { get; set; } = null;
        public int CurrentQueueIndex { get; set; } = -1;

        //shuffle handling
        public List<int> ShuffledIndices { get; set; } = null;
        public int ShuffleCurrentIndex { get; set; } = -1;

        public void ResetQueue()
        {
            QueueData = null;
            CurrentPlaylist = null;
            CurrentPlaylistName = null;
            CurrentQueueIndex = -1;
            //PlaybackQueue = null;
            ResetShuffle();
        }

        public void ResetShuffle()
        {
            ShuffledIndices = null;
            ShuffleCurrentIndex = -1;
        }

        public void SetupShuffleFromQueue()
        {
            ShuffledIndices = new List<int>();
            for (int i = 0; i < QueueData.Count; i++)
                ShuffledIndices.Add(i);

            ShuffledIndices.Shuffle();

            if(CurrentQueueIndex >= 0)
            {
                ShuffledIndices.Remove(CurrentQueueIndex);
                ShuffledIndices.Insert(0, CurrentQueueIndex);
            }
        }

        public Playlist CreatePlaylistFromQueue()
        {
            Playlist playlist = new Playlist();
            playlist.NiceName = CurrentPlaylistName;

            foreach(var row in QueueData)
            {
                playlist.Songs.Add(row.Id);
            }

            return playlist;
        }
    }
}