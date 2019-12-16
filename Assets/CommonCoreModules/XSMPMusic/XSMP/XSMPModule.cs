using CommonCore.Config;
using CommonCore.Async;
using CommonCore.Audio;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using System.Net.Http;
using System.Text;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using UnityEngine.Networking;
using WaveLoader;
using CommonCore.DebugLog;
using System.Linq;

namespace CommonCore.XSMP
{

    public class XSMPModule : CCModule
    {
        internal static XSMPModule Instance { get; private set; }

        internal XSMPState State { get; private set; } = new XSMPState(); //we may move this
        public ServerStatus Status { get; private set; } = ServerStatus.Offline;

        internal static XSMPConfig Config => ConfigState.Instance.CustomConfigVars["XSMP"] as XSMPConfig;

        internal bool Enabled { get; private set; } = true;

        private XSMPServerWrapper ServerWrapper = null;
        private float LastStatusUpdateTime;

        public XSMPModule()
        {
            Instance = this;

            SetupConfig();
            ConfigModule.Instance.RegisterConfigPanel("XSMPPanel", 100, CoreUtils.LoadResource<GameObject>("Modules/XSMPMusic/XSMPConfigPanel"));

            if(!ConfigState.Instance.CustomConfigFlags.Contains("XSMPEnabled"))
            {
                Enabled = false;
                return;
            }

            if (Config.ServerAutostart)
                ServerWrapper = new XSMPServerWrapper();
            
        }

        public override void OnAllModulesLoaded()
        {
            if (!Enabled)
                return;

            //inject audio component
            var musicComponent = new XSMPMusicComponent();
            AudioPlayer.Instance.RegisterUserMusicComponent(musicComponent);

        }

        public override void OnFrameUpdate()
        {
            if (!Enabled)
                return;

            //use this to update status periodically
            if (Time.unscaledTime > LastStatusUpdateTime + Config.StatusUpdateInterval)
            {
                AsyncUtils.RunWithExceptionHandling(async() => await Task.Run(UpdateStatus));
                LastStatusUpdateTime = Time.unscaledTime;
            }
        }

        public override void Dispose()
        {
            if (ServerWrapper != null)
                ServerWrapper.Dispose();
        }

        private void SetupConfig()
        {
            if(!ConfigState.Instance.CustomConfigVars.ContainsKey("XSMP") || !(ConfigState.Instance.CustomConfigVars["XSMP"] is XSMPConfig))
            {
                ConfigState.Instance.CustomConfigVars["XSMP"] = new XSMPConfig();
                Log("Added XSMP config node to CustomConfigVars");
            }
        }

        //Playback handling methods

        public void TrimClipCache()
        {
            //Debug.Log(State.ClipCache.ToNiceString());

            int cachedClips = State.ClipCache.Count;
            if (cachedClips <= Config.MaxCachedClips)
                return;

            var clipRefsSorted = State.ClipCache.OrderBy(x => x.Value.LastUsedTime).ToArray();
            foreach(var clipRef in clipRefsSorted)
            {
                if (clipRef.Value == State.CurrentClip)
                    continue;

                if (cachedClips <= Config.MaxCachedClips)
                    break;

                UnityEngine.Object.Destroy(clipRef.Value.Clip);
                State.ClipCache.Remove(clipRef.Key);
                cachedClips--;
            }

            Debug.Log($"[XSMP] Trimmed clip cache");
        }

        public async Task StartPlayback(CancellationToken token)
        {
            //set playback state, load clip and play!
            State.Playing = true;

            if (State.CurrentClip != null)
            {
                AudioPlayer.Instance.SetMusic(State.CurrentClip.Clip, MusicSlot.User, State.Volume, false, true);
                AudioPlayer.Instance.StartMusic(MusicSlot.User, true);
                AudioPlayer.Instance.SeekMusic(MusicSlot.User, State.TrackTime);
            }
            else
            {
                State.TrackTime = 0;
                await ChangePlaybackTrack(token);
            }
        }

        public async Task PausePlayback(CancellationToken token)
        {
            State.Playing = false;
            AudioPlayer.Instance.StopMusic(MusicSlot.User);
        }

        public async Task StopPlayback(CancellationToken token)
        {
            State.Playing = false;
            AudioPlayer.Instance.StopMusic(MusicSlot.User);
            AudioPlayer.Instance.SetMusicClip(null, MusicSlot.User);

            State.CurrentClip = null;
            State.CurrentQueueIndex = -1;
            State.TrackTime = 0;
            State.ResetShuffle();
        }

        public async Task ChangePlaybackTrack(CancellationToken token)
        {
            string song = State.QueueData[State.CurrentQueueIndex].Id;
            RefCountedClip rClip = await GetSongClip(song, token);
            rClip.AddRef();

            State.CurrentClip = rClip;
            State.TrackTime = 0;

            AudioPlayer.Instance.SetMusic(State.CurrentClip.Clip, MusicSlot.User, State.Volume, false, true);
            AudioPlayer.Instance.StartMusic(MusicSlot.User, true);
            AudioPlayer.Instance.SeekMusic(MusicSlot.User, State.TrackTime);

            TrimClipCache(); //?
        }

        //API-ish methods

        /// <summary>
        /// Gets a list of playlist viewmodels
        /// </summary>
        public async Task<List<DataRow>> GetPlaylists(CancellationToken token)
        {
            //TODO resilience

            List<DataRow> data = new List<DataRow>();

            data.Add(new HeaderDataRow("Playlists", null));

            var request = new RestRequest("library/playlist", HttpMethod.Get);
            var response = await DoRestRequest(request, 100000, token);

            var jroot = JToken.Parse(response.Body);
            token.ThrowIfCancellationRequested();
            if(!jroot.IsNullOrEmpty() && !jroot["data"].IsNullOrEmpty() && !jroot["data"]["playlists"].IsNullOrEmpty())
            {
                //we have playlists!
                var jplaylists = (JArray)jroot["data"]["playlists"];
                foreach(var jplaylist in jplaylists)
                {
                    var dataRow = PlaylistDataRow.Parse((JObject)jplaylist);
                    data.Add(dataRow);
                    token.ThrowIfCancellationRequested();
                }
            }

            return data;
        }

        /// <summary>
        /// Gets a list of song viewmodels in a given playlist
        /// </summary>
        public async Task<List<DataRow>> GetPlaylist(string playlist, CancellationToken token)
        {
            List<DataRow> data = new List<DataRow>();

            var request = new RestRequest($"library/playlist/{playlist}", HttpMethod.Get);
            var response = await DoRestRequest(request, 100000, token);

            var jroot = JToken.Parse(response.Body);
            token.ThrowIfCancellationRequested();
            if (!jroot.IsNullOrEmpty() && !jroot["data"].IsNullOrEmpty())
            {
                string name = jroot["data"]["NiceName"].ToString();
                data.Add(new HeaderDataRow(name, null));

                if(!jroot["data"]["Songs"].IsNullOrEmpty())
                {
                    foreach(var jsong in jroot["data"]["Songs"])
                    {
                        string songHash = jsong.ToString();
                        var dataRow = await GetSong(songHash, token);
                        if (dataRow != null)
                            data.Add(dataRow);
                    }
                }
                    
            }

            return data;
        }

        /// <summary>
        /// Gets a playlist object from the server
        /// </summary>
        public async Task<Playlist> GetPlaylistRaw(string playlistName, CancellationToken token)
        {
            var request = new RestRequest($"library/playlist/{playlistName}", HttpMethod.Get);
            var response = await DoRestRequest(request, 100000, token);

            var jroot = JToken.Parse(response.Body);
            Playlist playlist = null;
            if (!jroot.IsNullOrEmpty() && !jroot["data"].IsNullOrEmpty())
            {
                playlist = jroot["data"].ToObject<Playlist>();
            }

            return playlist;
        }

        /// <summary>
        /// Gets the unique canonical name for a new playlist
        /// </summary>
        public async Task<string> GetPlaylistUniqueName(string playlistFullName, CancellationToken token)
        {
            var request = new RestRequest("library/playlist_unique_name", HttpMethod.Post, playlistFullName, null);
            var response = await DoRestRequest(request, 100000, token);

            var jroot = JToken.Parse(response.Body);
            return jroot["data"].ToString();

        }

        /// <summary>
        /// Puts a playlist object onto the server
        /// </summary>
        public async Task PutPlaylistRaw(string playlistName, Playlist playlistObject, CancellationToken token)
        {
            var request = new RestRequest($"library/playlist/{playlistName}", HttpMethod.Put, JsonConvert.SerializeObject(playlistObject), null);
            var response = await DoRestRequest(request, 100000, token);
        }

        /// <summary>
        /// Gets a single song viewmodel
        /// </summary>
        public async Task<SongDataRow> GetSong(string song, CancellationToken token)
        {
            var request = new RestRequest($"library/song/{song}", HttpMethod.Get);
            var response = await DoRestRequest(request, 100000, token);

            var jroot = JToken.Parse(response.Body);
            if(!jroot.IsNullOrEmpty() && !jroot["data"].IsNullOrEmpty() && !jroot["data"]["song"].IsNullOrEmpty())
            {
                var dataRow = SongDataRow.Parse((JObject)jroot["data"]["song"]);
                return dataRow;
            }

            return null;
        }

        /// <summary>
        /// Gets an AudioClip for a song
        /// </summary>
        /// <remarks>
        /// Run this from the main thread and only from the main thread!
        /// </remarks>
        public async Task<RefCountedClip> GetSongClip(string song, CancellationToken token)
        {
            if (!AsyncUtils.IsOnMainThread())
                throw new InvalidOperationException("GetSongClip must be run from the main thread. I don't make the rules, sorry.");

            if (State.ClipCache.ContainsKey(song))
                return State.ClipCache[song];

            //request the server transcode
            var request = new RestRequest($"library/song/{song}", HttpMethod.Get, null, new KeyValuePair<string, string>("transcode","wave"),
                new KeyValuePair<string, string>("return", "path"));

            RestResponse response = default;
            await Task.Run(async() => response = await DoRestRequest(request, 100000, token));

            var jroot = JToken.Parse(response.Body);
            if (jroot.IsNullOrEmpty() || jroot["data"].IsNullOrEmpty() || jroot["data"]["transcodedPath"].IsNullOrEmpty())
                return null;

            string path = jroot["data"]["transcodedPath"].ToString();

            //load the clip
            var clip = WaveLoader.WaveLoader.LoadWaveToAudioClip(path, song); //will probably stutter, we'll optimize it later
            var rClip = new RefCountedClip(clip);

            State.ClipCache.Add(song, rClip);
            
            return rClip;
            
            //TODO trigger a cache clean?
        }
                

        public async Task<List<DataRow>> GetAlbums(CancellationToken token)
        {
            List<DataRow> data = new List<DataRow>();

            data.Add(new HeaderDataRow("Albums", null));

            var request = new RestRequest("library/album", HttpMethod.Get);
            var response = await DoRestRequest(request, 100000, token);

            var jroot = JToken.Parse(response.Body);
            token.ThrowIfCancellationRequested();
            if (!jroot.IsNullOrEmpty() && !jroot["data"].IsNullOrEmpty() && !jroot["data"]["albums"].IsNullOrEmpty())
            {
                var jalbums = (JArray)jroot["data"]["albums"];
                string lastArtist = string.Empty;
                foreach (var jalbum in jalbums)
                {
                    var dataRow = AlbumDataRow.Parse((JObject)jalbum);
                    if(dataRow.ArtistId != lastArtist) //we naively assume it's grouped by artist. If it isn't, it won't break, but it'll look really weird
                    {
                        data.Add(new HeaderDataRow(dataRow.Artist, new string[] { "artist", dataRow.ArtistId }));
                        lastArtist = dataRow.ArtistId;
                    }
                    data.Add(dataRow);
                    token.ThrowIfCancellationRequested();
                }
            }

            return data;
        }

        public async Task<List<DataRow>> GetAlbumSongs(string album, CancellationToken token)
        {
            List<DataRow> data = new List<DataRow>();

            var request = new RestRequest($"library/album/{album}", HttpMethod.Get, null, new KeyValuePair<string, string>("list", "songs"));
            var response = await DoRestRequest(request, 100000, token);

            var jroot = JToken.Parse(response.Body);
            token.ThrowIfCancellationRequested();
            if (!jroot.IsNullOrEmpty() && !jroot["data"].IsNullOrEmpty() && !jroot["data"]["songs"].IsNullOrEmpty())
            {
                if(!jroot["data"]["album"].IsNullOrEmpty())
                {
                    string albumName = jroot["data"]["album"]["Title"].ToString();
                    data.Add(new HeaderDataRow(albumName, null));
                }

                var jsongs = (JArray)jroot["data"]["songs"];
                foreach (var jsong in jsongs)
                {
                    var dataRow = SongDataRow.Parse((JObject)jsong);
                    data.Add(dataRow);
                    token.ThrowIfCancellationRequested();
                }
            }

            return data;
        }

        public async Task<List<DataRow>> GetArtists(CancellationToken token)
        {
            List<DataRow> data = new List<DataRow>();

            data.Add(new HeaderDataRow("Artists", null));

            var request = new RestRequest("library/artist", HttpMethod.Get);
            var response = await DoRestRequest(request, 100000, token);

            var jroot = JToken.Parse(response.Body);
            token.ThrowIfCancellationRequested();
            if (!jroot.IsNullOrEmpty() && !jroot["data"].IsNullOrEmpty() && !jroot["data"]["artists"].IsNullOrEmpty())
            {
                var jartists = (JArray)jroot["data"]["artists"];
                foreach (var jartist in jartists)
                {
                    var dataRow = ArtistDataRow.Parse((JObject)jartist);
                    data.Add(dataRow);
                    token.ThrowIfCancellationRequested();
                }
            }

            return data;
        }

        public async Task<List<DataRow>> GetArtistAlbums(string artist, CancellationToken token)
        {
            List<DataRow> data = new List<DataRow>();

            var request = new RestRequest($"library/artist/{artist}", HttpMethod.Get, null, new KeyValuePair<string, string>("list","songs"));
            var response = await DoRestRequest(request, 100000, token);

            var jroot = JToken.Parse(response.Body);
            token.ThrowIfCancellationRequested();
            if (!jroot.IsNullOrEmpty() && !jroot["data"].IsNullOrEmpty() && !jroot["data"]["artist"].IsNullOrEmpty() && !jroot["data"]["songs"].IsNullOrEmpty())
            {
                string artistName = jroot["data"]["artist"]["NiceName"].ToString();
                data.Add(new HeaderDataRow(artistName, null));

                var jsongs = (JArray)jroot["data"]["songs"];
                string lastAlbum = string.Empty;
                foreach(var jsong in jsongs)
                {
                    var dataRow = SongDataRow.Parse((JObject)jsong);
                    if(dataRow.AlbumId != null && dataRow.AlbumId != lastAlbum)
                    {
                        data.Add(new HeaderDataRow(dataRow.Album, new string[] { "album", dataRow.AlbumArtistId + "_" + dataRow.AlbumId }));
                        lastAlbum = dataRow.AlbumId;
                    }

                    data.Add(dataRow);
                    token.ThrowIfCancellationRequested();
                }
            }

            return data;
        }

        public async Task<List<DataRow>> GetRootFolders(CancellationToken token)
        {
            List<DataRow> data = new List<DataRow>();

            data.Add(new HeaderDataRow("Folders", null));

            var request = new RestRequest("library/folder", HttpMethod.Get);
            var response = await DoRestRequest(request, 100000, token);

            var jroot = JToken.Parse(response.Body);
            token.ThrowIfCancellationRequested();
            if (!jroot.IsNullOrEmpty() && !jroot["data"].IsNullOrEmpty() && !jroot["data"]["folders"].IsNullOrEmpty())
            {
                var jfolders = (JArray)jroot["data"]["folders"];
                foreach (var jfolder in jfolders)
                {
                    string folder = jfolder.ToString();
                    var dataRow = new FolderDataRow(folder, folder);
                    data.Add(dataRow);
                    token.ThrowIfCancellationRequested();
                }
            }

            return data;
        }

        public async Task<List<DataRow>> GetFolderContents(string folderPath, CancellationToken token)
        {
            List<DataRow> data = new List<DataRow>();

            var request = new RestRequest($"library/folder/{folderPath}", HttpMethod.Get);
            var response = await DoRestRequest(request, 100000, token);

            var jroot = JToken.Parse(response.Body);
            token.ThrowIfCancellationRequested();
            if (!jroot.IsNullOrEmpty() && !jroot["data"].IsNullOrEmpty())
            {
                string basePath = jroot["data"]["path"].ToString();
                data.Add(new HeaderDataRow(basePath, null));

                if(!jroot["data"]["folders"].IsNullOrEmpty())
                {
                    foreach(var jfolder in jroot["data"]["folders"])
                    {
                        string folder = jfolder.ToString();
                        string fullPath = basePath + "/" + folder;
                        data.Add(new FolderDataRow(folder, fullPath));
                    }
                }

                if(!jroot["data"]["songs"].IsNullOrEmpty())
                {
                    foreach(var jsong in jroot["data"]["songs"])
                    {
                        var dataRow = SongDataRow.Parse((JObject)jsong);
                        data.Add(dataRow);
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Signals the server to start a library refresh
        /// </summary>
        public async Task RefreshLibrary(CancellationToken token)
        {
            await DoRestRequest(new RestRequest("meta/refresh", HttpMethod.Post), 5f, token); //um ok
        }

        //network stuff will go here

        /// <summary>
        /// Pings the server and updates status
        /// </summary>
        private async Task UpdateStatus()
        {
           
            try
            {
                var response = await DoRestRequest(new RestRequest("meta/status", HttpMethod.Get), 5f);
                //Log(response.Body);
                var jObject = JToken.Parse(response.Body);
                var status = jObject.Value<string>("status");
                //Log(status);

                ServerStatus newStatus;
                if (status == "ready")
                    newStatus = ServerStatus.Ready;
                else
                    newStatus = ServerStatus.NotReady;

                if (Status != newStatus)
                    Log($"Server changing status from {Status.ToString()} to {newStatus.ToString()}");

                Status = newStatus;

            }
            catch(Exception e) //TODO make it tell between "server not running" and "server running but returning garbage"
            {
                if(Status != ServerStatus.Offline)
                    Log($"Server disconnected ({e.GetType().Name})");
                Status = ServerStatus.Offline;
            }
        }        

        private static async Task<RestResponse> DoRestRequest(RestRequest restRequest, float timeout = 100f, CancellationToken? token = null)
        {
            string queryString = restRequest.MakeQueryStringFromParams();
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create($"http://{Config.Hostname}:{Config.Port}/{Config.Prefix}/{restRequest.Path}{queryString}");
            webRequest.Method = restRequest.Verb.ToString();
            if (!string.IsNullOrEmpty(restRequest.Body))
            {
                //WIP should probably switch this all over to use HttpClient or something
                var data = Encoding.UTF8.GetBytes(restRequest.Body);
                webRequest.ContentType = "application/json"; //I mean, assume it's json lol
                webRequest.ContentLength = data.Length;

                using (var stream = webRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            else
            {
                webRequest.ContentLength = 0;
            }            
            webRequest.Timeout = (int)(timeout * 1000);

            HttpWebResponse response;
            if (token.HasValue)
                response = (HttpWebResponse)await webRequest.GetResponseAsync(token.Value);
            else
                response = (HttpWebResponse)await webRequest.GetResponseAsync();

            if (!((int)response.StatusCode >= 200 && (int)response.StatusCode < 300))
            {
                throw new RestRequestFailedException((int)response.StatusCode);
            }

            string body = null;
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                body = reader.ReadToEnd();
            }

            return new RestResponse((int)response.StatusCode, body);

        }

        //utility methods we'll probably move

        /// <summary>
        /// Checks if a collection of data rows has actual data (ie not just header rows)
        /// </summary>
        public static bool HasData(IEnumerable<DataRow> rows)
        {
            if (rows == null)
                return false;

            foreach(var row in rows)
            {
                if (row is AlbumDataRow || row is ArtistDataRow || row is FolderDataRow || row is PlaylistDataRow || row is SongDataRow)
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Checks if a collection of data rows has at least one song data row
        /// </summary>
        public static bool HasSongData(IEnumerable<DataRow> rows)
        {
            if (rows == null)
                return false;

            foreach (var row in rows)
            {
                if (row is SongDataRow)
                    return true;
            }

            return false;
        }

    }

    //hacky, will probably move this
    internal static class WebRequestExtensions
    {
        /// <summary>
        /// Gets a response for a WebRequest asynchronously and allows the use of a CancellationToken
        /// </summary>
        /// <remarks>
        /// <para>Based on https://stackoverflow.com/questions/19211972/getresponseasync-does-not-accept-cancellationtoken </para>
        /// </remarks>
        public static async Task<HttpWebResponse> GetResponseAsync(this HttpWebRequest request, CancellationToken ct)
        {
            using (ct.Register(() => request.Abort(), useSynchronizationContext: false))
            {
                try
                {
                    var response = await request.GetResponseAsync();
                    return (HttpWebResponse)response;
                }
                catch (WebException ex)
                {
                    // WebException is thrown when request.Abort() is called,
                    // but there may be many other reasons,
                    // propagate the WebException to the caller correctly
                    if (ct.IsCancellationRequested)
                    {
                        // the WebException will be available as Exception.InnerException
                        throw new OperationCanceledException(ex.Message, ex, ct);
                    }

                    // cancellation hasn't been requested, rethrow the original WebException
                    throw;
                }
            }
        }
    }
}