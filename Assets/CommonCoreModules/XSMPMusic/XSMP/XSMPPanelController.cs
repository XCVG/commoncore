using CommonCore.Async;
using CommonCore.Audio;
using CommonCore.UI;
using CommonCore.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CommonCore.XSMP
{
    /// <summary>
    /// Controller for the XSMP music player panel. No doubt a hilarious mess by the time you see it.
    /// </summary>
    public class XSMPPanelController : PanelController
    {
        private const float StatusUpdateInterval = 1.0f;
        private readonly string[] LibraryRoots = new string[] { "Artist", "Album", "Folder", "Playlist" };

        private XSMPState XSMPState => XSMPModule.Instance.State;

        [SerializeField, Header("Panel Elements")]
        private CanvasGroup PanelGroup = null; //will use this to disable interaction 
        [SerializeField]
        private Text StatusText = null;
        [SerializeField]
        private RectTransform LibraryPanelContent = null;
        [SerializeField]
        private RectTransform BrowsePanelContent = null;
        [SerializeField]
        private RectTransform QueuePanelContent = null;

        [SerializeField, Header("Queue Controls")]
        private Button TransferButton = null;
        [SerializeField]
        private Button AddAllButton = null;
        [SerializeField]
        private Button AddButton = null;
        [SerializeField]
        private Button RemoveButton = null;
        [SerializeField]
        private Button MoveUpButton = null;
        [SerializeField]
        private Button MoveDownButton = null;
        [SerializeField]
        private Button ClearQueueButton = null;
        [SerializeField]
        private Button SaveQueueButton = null;
        [SerializeField]
        private InputField QueueNameInputField = null;

        [SerializeField, Header("Transport Controls")]
        private Button TransportPlayButton = null;
        [SerializeField]
        private Button TransportBackButton = null;
        [SerializeField]
        private Button TransportForwardButton = null;
        [SerializeField]
        private Slider TransportSeekBar = null;
        [SerializeField]
        private Button TransportRepeatButton = null;
        [SerializeField]
        private Button TransportShuffleButton = null;

        [SerializeField, Header("Prefabs")]
        private GameObject LibraryItemPrefab = null;
        [SerializeField]
        private GameObject BrowseItemPrefab = null;
        [SerializeField]
        private GameObject BrowseItemHeaderPrefab = null;
        [SerializeField]
        private GameObject BrowseItemSongPrefab = null;
        [SerializeField]
        private GameObject QueueItemPrefab = null;

        //vars
        private QdmsMessageInterface MessageInterface;
        private Task CurrentTask;
        private CancellationTokenSource CurrentTokenSource;

        private bool FatalErrorEncountered = false;

        private float TimeSinceLastStatusUpdate = 0;

        [SerializeField, Header("Debug")] //for debug only
        private RectTransform SelectedItem = null;
        [SerializeField]
        private bool PlaylistNameModified = false;
        [SerializeField]
        private bool Seeking = false;


        //TODO we'll probably move some of this to XSMPModule or a third class

        private void Awake()
        {
            MessageInterface = new QdmsMessageInterface(gameObject);
            MessageInterface.SubscribeReceiver(HandleMessageReceived);
        }

        public override void SignalInitialPaint()
        {
            base.SignalInitialPaint();

            if (XSMPModule.Instance == null || !XSMPModule.Instance.Enabled)
            {
                FatalErrorEncountered = true;
                DisableUI();
            }
            else
            {


                //TODO refresh if BrowseData=null and BrowsePath is valid

                //TODO refresh queue data if QueueData=null and PlaybackQueue is non-empty

                EnableUI();
            }
        }

        public override void SignalPaint()
        {
            base.SignalPaint();

            if (FatalErrorEncountered)
                return;

            HandleItemDeselected(null); //clear selection (we may handle this more gracefully in the future)

            PaintStatusIndicator();
            PaintLibraryPanel();
            PaintBrowsePanel();
            PaintQueueControls();
            PaintQueuePanel();
            PaintTransportControls();
        }

        public override void SignalUnpaint()
        {
            base.SignalUnpaint();

            CurrentTokenSource?.Cancel();

            
        }

        private void Update()
        {
            TimeSinceLastStatusUpdate += Time.unscaledDeltaTime;

            if(TimeSinceLastStatusUpdate > StatusUpdateInterval) //TODO will probably switch this to messaging
            {
                TimeSinceLastStatusUpdate = 0;

                PaintStatusIndicator();
                PaintTransportControls(); //for seekbar mostly
            }

        }

        private void HandleMessageReceived(QdmsMessage message)
        {
            if(message is QdmsFlagMessage flagMessage)
            {
                switch(flagMessage.Flag)
                {
                    case "XSMPStateChanged":
                        Seeking = false; //HACK unbreak the seek bar
                        SignalPaint(); //repaint the UI
                        break;
                }
            }
        }

        //painting can be synchronous but things that request server data can't be

        private void PaintStatusIndicator()
        {
            string status = XSMPModule.Instance.Status.ToString();
            StatusText.text = $"Status: {status}";
        }        

        private void PaintLibraryPanel()
        {
            LibraryPanelContent.DestroyAllChildren();

            foreach(string root in LibraryRoots)
            {
                var itemGO = Instantiate<GameObject>(LibraryItemPrefab, LibraryPanelContent);
                itemGO.GetComponent<BackingFieldReference>().Id = root;
                itemGO.GetComponentInChildren<Text>().text = root;

                var button = itemGO.GetComponent<Button>();
                button.onClick.AddListener(() => Navigate(new string[] { root.ToLower(CultureInfo.InvariantCulture) }));
            }
        }

        private void PaintBrowsePanel()
        {
            BrowsePanelContent.DestroyAllChildren();

            if(XSMPState.BrowseData != null && XSMPState.BrowseData.Count > 0)
            {
                foreach(var row in XSMPState.BrowseData)
                {
                    PaintBrowseDataRow(row);
                }
            }
            else
            {
                //paint "no data" placeholder
                var itemGO = Instantiate<GameObject>(BrowseItemPrefab, BrowsePanelContent);
                itemGO.GetComponentInChildren<Text>().text = "No Data";
            }
        }

        private void PaintBrowseDataRow(DataRow row)
        {
            GameObject itemGO = null;

            //create different row items and fill them based on row type
            if(row is HeaderDataRow)
            {
                itemGO = Instantiate<GameObject>(BrowseItemHeaderPrefab, BrowsePanelContent);
                itemGO.GetComponentInChildren<Text>().text = row.Title;
            }
            else if(row is SongDataRow songRow)
            {
                //TODO discriminate between showing album songs and showing playlist songs

                itemGO = Instantiate<GameObject>(BrowseItemSongPrefab, BrowsePanelContent);
                itemGO.transform.Find("Track").GetComponent<Text>().text = songRow.Track.ToString();
                itemGO.transform.Find("Title").GetComponent<Text>().text = songRow.Title;
                itemGO.transform.Find("Length").GetComponent<Text>().text = TimeSpan.FromSeconds(songRow.Length).ToString(@"m\:ss");
                itemGO.transform.Find("Artist").GetComponent<Text>().text = (songRow.Artists != null && songRow.Artists.Count > 0) ? songRow.Artists[0] : "MISSING";
                itemGO.transform.Find("Extra").GetComponent<Text>().text = songRow.Album;

                //add select handling
                var button = itemGO.GetComponent<Button>();
                button.onClick.AddListener(delegate () { HandleItemSelected((RectTransform)itemGO.transform); });
                
            }
            else if(row is AlbumDataRow || row is ArtistDataRow || row is PlaylistDataRow || row is FolderDataRow)
            {
                //all these use a fairly generic handler
                itemGO = Instantiate<GameObject>(BrowseItemPrefab, BrowsePanelContent);
                itemGO.GetComponentInChildren<Text>().text = row.Title;
            }
            else
            {
                throw new NotImplementedException();
            }

            BackingFieldReference itemBFR = itemGO.GetComponent<BackingFieldReference>();
            itemBFR.Id = row.Id;
            itemBFR.Value = row;

            //attach navigation handler
            if(row.TargetBrowsePath != null && row.TargetBrowsePath.Count > 0)
            {
                var button = itemGO.GetComponent<Button>();
                button.onClick.AddListener(() => Navigate(row.TargetBrowsePath));
            }

            //attach select/deselect handlers
            //var itemTrigger = itemGO.GetComponent<EventTrigger>();
            //var itemTransform = (RectTransform)itemGO.transform;
            //itemTrigger.AddListener(EventTriggerType.Select, d => HandleItemSelected(itemTransform)); //doesn't work, something something buttons
            //itemTrigger.AddListener(EventTriggerType.Deselect, d => HandleItemDeselected(itemTransform));
        }

        private void PaintQueueControls()
        {
            //WIP set interactibility of queue controls

            //"transfer" button: a valid playlist is opened in browse
            if (XSMPState.CurrentBrowsePath != null && XSMPState.CurrentBrowsePath.Count == 2 && XSMPState.CurrentBrowsePath[0] == "playlist")
                TransferButton.interactable = true;
            else
                TransferButton.interactable = false;

            //"add all" button: a non-empty album, artist, or playlist is opened in browse, or a folder with songs is opened in browse
            if (XSMPState.CurrentBrowsePath != null && XSMPState.CurrentBrowsePath.Count == 2
                && XSMPState.BrowseData != null && XSMPState.BrowseData.Count > 0 && XSMPModule.HasSongData(XSMPState.BrowseData) &&
                (XSMPState.CurrentBrowsePath[0] == "playlist" || XSMPState.CurrentBrowsePath[0] == "album" || XSMPState.CurrentBrowsePath[0] == "artist" || XSMPState.CurrentBrowsePath[0] == "folder"))
            {
                AddAllButton.interactable = true;
            }
            else
                AddAllButton.interactable = false;

            //"add one" button: a song in the browse panel is selected
            if (SelectedItem != null && SelectedItem.parent == BrowsePanelContent && SelectedItem.GetComponent<BackingFieldReference>().Value is SongDataRow)
                AddButton.interactable = true;
            else
                AddButton.interactable = false;

            //"remove", "move up", "move down" button: a song in the queue panel is selected
            if (SelectedItem != null && SelectedItem.parent == QueuePanelContent)
            {
                RemoveButton.interactable = true;
                MoveUpButton.interactable = true;
                MoveDownButton.interactable = true;
            }
            else
            {
                RemoveButton.interactable = false;
                MoveUpButton.interactable = false;
                MoveDownButton.interactable = false;
            }

            //name input field: a (non-empty) queue exists
            if (XSMPState.QueueData != null && XSMPState.QueueData.Count > 0)
                QueueNameInputField.interactable = true;
            else
                QueueNameInputField.interactable = false;

            //name input field: put a name in it
            if(!string.IsNullOrEmpty(XSMPState.CurrentPlaylistName) && !PlaylistNameModified)
            {
                QueueNameInputField.text = XSMPState.CurrentPlaylistName;
            }

            //save button: a (non-empty) queue exists and (it is a playlist OR the name field is non-empty)
            if (XSMPState.QueueData != null && XSMPState.QueueData.Count > 0 && (!string.IsNullOrEmpty(XSMPState.CurrentPlaylist) || !string.IsNullOrEmpty(QueueNameInputField.text)))
                SaveQueueButton.interactable = true;
            else
                SaveQueueButton.interactable = false;

            //clear button: a (non-empty) queue exists OR a playlist is open
            if (!string.IsNullOrEmpty(XSMPState.CurrentPlaylist) || (XSMPState.QueueData != null && XSMPState.QueueData.Count > 0))
                ClearQueueButton.interactable = true;
            else
                ClearQueueButton.interactable = false;
        }

        private void PaintQueuePanel()
        {
            QueuePanelContent.DestroyAllChildren();

            var queue = XSMPModule.Instance.State.QueueData;
            if(queue != null && queue.Count > 0)
            {
                for(int i = 0; i < queue.Count; i++)
                {
                    PaintQueueDataRow(queue[i], i);
                }
            }
        }

        private void PaintQueueDataRow(DataRow row, int index)
        {
            var songRow = row as SongDataRow;
            GameObject itemGO = Instantiate(QueueItemPrefab, QueuePanelContent);

            BackingFieldReference itemBFR = itemGO.GetComponent<BackingFieldReference>();
            itemBFR.Id = row.Id;
            itemBFR.Value = row;
            itemBFR.Index = index;

            itemGO.transform.Find("Title").GetComponent<Text>().text = songRow.Title;
            itemGO.transform.Find("Length").GetComponent<Text>().text = TimeSpan.FromSeconds(songRow.Length).ToString(@"m\:ss");

            var button = itemGO.GetComponent<Button>();
            button.onClick.AddListener(delegate () { HandleItemSelected((RectTransform)itemGO.transform); HandleQueueItemSelected(index); });

            //preselect
            if (XSMPState.CurrentQueueIndex >= 0 && index == XSMPState.CurrentQueueIndex)
                HandleItemSelected((RectTransform)itemGO.transform);
        }

        private void PaintTransportControls()
        {
            bool hasSongSelected = (XSMPState.CurrentQueueIndex >= 0 && XSMPState.QueueData.Count > 0);

            TransportPlayButton.interactable = hasSongSelected;
            if (XSMPState.Playing)
                TransportPlayButton.GetComponentInChildren<Text>().text = "||";
            else
                TransportPlayButton.GetComponentInChildren<Text>().text = ">";

            if(hasSongSelected && XSMPState.Playing)
            {
                if (!XSMPState.Shuffle)
                {
                    TransportBackButton.interactable = XSMPState.CurrentQueueIndex > 0 || XSMPState.Repeat;
                    TransportForwardButton.interactable = XSMPState.CurrentQueueIndex < (XSMPState.QueueData.Count - 1) || XSMPState.Repeat;
                }
                else
                {                    
                    TransportBackButton.interactable = XSMPState.ShuffleCurrentIndex > 0 || XSMPState.Repeat;
                    TransportForwardButton.interactable = XSMPState.ShuffleCurrentIndex < (XSMPState.ShuffledIndices.Count - 1) || XSMPState.Repeat;
                }
            }
            else
            {
                TransportBackButton.interactable = false;
                TransportForwardButton.interactable = false;
            }

            //WIP paint seek bar etc
            if(hasSongSelected && XSMPState.CurrentClip != null && XSMPState.CurrentClip.Clip != null)
            {
                TransportSeekBar.interactable = true;
                if (!Seeking)
                {
                    float lengthRatio = XSMPState.TrackTime / XSMPState.CurrentClip.Clip.length;
                    //Debug.Log(lengthRatio);
                    TransportSeekBar.value = lengthRatio;
                }
            }
            else
            {
                TransportSeekBar.interactable = false;
            }

            TransportRepeatButton.GetComponentInChildren<Text>().text = XSMPState.Repeat ? "Repeat On" : "Repeat Off";
            TransportShuffleButton.GetComponentInChildren<Text>().text = XSMPState.Shuffle ? "Shuffle On" : "Shuffle Off";
        }

        //playback handling


        private void StartPlayback()
        {
            DisableUIAndRun((t) => XSMPModule.Instance.StartPlayback(t), true);  
        }

        private void PausePlayback()
        {
            //set playback state, stop 
            DisableUIAndRun((t) => XSMPModule.Instance.PausePlayback(t), true);
        }

        private void StopPlayback()
        {
            DisableUIAndRun((t) => XSMPModule.Instance.StopPlayback(t), true);
        }

        private void ChangePlaybackTrack()
        {
            Seeking = false; //HACK unbreak the seek bar
            DisableUIAndRun((t) => XSMPModule.Instance.ChangePlaybackTrack(t), true);
        }

        //utility method to unfuck later
        private async void DisableUIAndRun(Func<CancellationToken, Task> action, bool repaintAfter = true)
        {
            try
            {
                DisableUI();

                CurrentTokenSource = new CancellationTokenSource();
                CurrentTask = action(CurrentTokenSource.Token);
                await CurrentTask;
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                CurrentTask = null;
                EnableUI();
                SignalPaint();
            }

            
        }

        //navigation

        //navigate to new browse path
        private void Navigate(IReadOnlyList<string> newBrowsePath)
        {
            //throw if task running, or errored
            if (CurrentTask != null && !CurrentTask.IsCompleted)
                throw new InvalidOperationException();

            if (FatalErrorEncountered)
                throw new InvalidOperationException();

            //clear data and add previous browse path as row
            DisableUI();
            XSMPState.BrowseData?.Clear();

            //if they're the same, it's a refresh and/or doesn't matter
            if(XSMPState.CurrentBrowsePath != null && !newBrowsePath.SequenceEqual(XSMPState.CurrentBrowsePath))
                XSMPState.LastBrowsePath = XSMPState.CurrentBrowsePath;

            //set browse path
            XSMPState.CurrentBrowsePath = new List<string>(newBrowsePath);

            //async magic
            CurrentTokenSource = new CancellationTokenSource();
            CurrentTask = NavigateAsync(CurrentTokenSource.Token);
            AsyncUtils.RunWithExceptionHandling(async () => await CurrentTask); //TODO replace this with custom exception handling instead of bodging it into NavigateAsync
        }

        private async Task NavigateAsync(CancellationToken token)
        {
            try
            {
                if (XSMPState.CurrentBrowsePath.Count == 1)
                {
                    //root folders basically
                    switch (XSMPState.CurrentBrowsePath[0])
                    {
                        case "folder":
                            XSMPState.BrowseData = await Task.Run(() => XSMPModule.Instance.GetRootFolders(token));
                            break;
                        case "album":
                            XSMPState.BrowseData = await Task.Run(() => XSMPModule.Instance.GetAlbums(token));
                            break;
                        case "artist":
                            XSMPState.BrowseData = await Task.Run(() => XSMPModule.Instance.GetArtists(token));
                            break;
                        case "playlist":
                            XSMPState.BrowseData = await Task.Run(() => XSMPModule.Instance.GetPlaylists(token));
                            break;
                    }
                }
                else
                {
                    switch (XSMPState.CurrentBrowsePath[0])
                    {
                        case "folder":
                            XSMPState.BrowseData = await Task.Run(() => XSMPModule.Instance.GetFolderContents(XSMPState.CurrentBrowsePath[1], token));
                            {
                                //break up the browse path
                                string[] pathSegments = XSMPState.CurrentBrowsePath[1].Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                                if(pathSegments.Length > 1)
                                {
                                    string lastPath = string.Join("/", new ArraySegment<string>(pathSegments, 0, pathSegments.Length - 1));
                                    Debug.Log(lastPath);
                                    XSMPState.BrowseData.Insert(0, new HeaderDataRow("..", new string[] { "folder",  lastPath}));
                                }
                                else
                                    XSMPState.BrowseData.Insert(0, new HeaderDataRow("..", new string[] { "folder" }));                                   
                            }

                            break;
                        case "album":
                            XSMPState.BrowseData = await Task.Run(() => XSMPModule.Instance.GetAlbumSongs(XSMPState.CurrentBrowsePath[1], token));

                            //when we add the previous browse path row we will need to be careful; examine LastBrowsePath to see how we got here
                            if (XSMPState.LastBrowsePath[0] == "artist")
                                XSMPState.BrowseData.Insert(0, new HeaderDataRow("<-Artist Albums", new string[] { "artist", XSMPState.LastBrowsePath[1] }));
                            else if(XSMPState.LastBrowsePath[0] == "album")
                                XSMPState.BrowseData.Insert(0, new HeaderDataRow("<-Albums", new string[] { "album" }));

                            break;
                        case "artist":                            
                            XSMPState.BrowseData = await Task.Run(() => XSMPModule.Instance.GetArtistAlbums(XSMPState.CurrentBrowsePath[1], token));
                            XSMPState.BrowseData.Insert(0, new HeaderDataRow("<-Artists", new string[] { "artist" })); //I think this is always correct
                            break;
                        case "playlist":
                            XSMPState.BrowseData = await Task.Run(() => XSMPModule.Instance.GetPlaylist(XSMPState.CurrentBrowsePath[1], token));
                            XSMPState.BrowseData.Insert(0, new HeaderDataRow("<-Playlists", new string[] { "playlist" }));
                            break;
                    }
                    
                }
            }
            catch(Exception e)
            {
                AsyncUtils.ThrowIfEditorStopped();

                if (e is TaskCanceledException || e is OperationCanceledException)
                {
                    Debug.Log("[XSMP] Navigation operation cancelled");

                    XSMPState.BrowseData.Clear();
                    //TODO clear browse path?
                }
                else
                    throw; //TODO handle this instead of fucking the UI
            }

            //refresh and unlock
            EnableUI();
            SignalPaint();

        }

        //enable/disable UI

        private void DisableUI()
        {
            PanelGroup.interactable = false;
        }

        private void EnableUI()
        {
            PanelGroup.interactable = true;
        }

        //handlers

        //selection logic is totally fucked up because I used buttons when I shouldn't have and also because Unity's UI is crap

        public void HandleItemSelected(RectTransform item)
        {
            //Debug.Log("Selected " + item.name);

            if (SelectedItem != null) //deselect last
                SelectedItem.GetComponent<Graphic>().color = Color.white;

            //color selected
            item.GetComponent<Graphic>().color = new Color(0.5f, 0.5f, 1.0f, 1.0f);

            SelectedItem = item;
            PaintQueueControls();
            PaintTransportControls();
        }

        public void HandleItemDeselected(RectTransform item)
        {
            //Debug.Log("Deselected " + item.name);

            if (SelectedItem != null) //deselect last
                SelectedItem.GetComponent<Graphic>().color = Color.white;

            SelectedItem = null;
            PaintQueueControls();
            PaintTransportControls();
        }

        public void HandleQueueItemSelected(int index)
        {
            bool changed = XSMPState.CurrentQueueIndex != index;

            XSMPState.CurrentQueueIndex = index;

            if (changed && XSMPState.Playing)
            {
                ChangePlaybackTrack();

                if(XSMPState.Shuffle)
                    XSMPState.ShuffleCurrentIndex = XSMPState.ShuffledIndices.IndexOf(XSMPState.CurrentQueueIndex);
            }
        }

        public void HandleRefreshDataButtonClicked()
        {
            Navigate(XSMPState.CurrentBrowsePath);

            //TODO also refresh playlist (?)
        }

        public void HandleRefreshLibraryButtonClicked()
        {
            //throw if task running, or errored
            if (CurrentTask != null && !CurrentTask.IsCompleted)
                throw new InvalidOperationException();

            if (FatalErrorEncountered)
                throw new InvalidOperationException();

            DisableUI();

            //clear data and browse path            
            XSMPState.BrowseData?.Clear();
            XSMPState.CurrentBrowsePath = new List<string>();

            //TODO also clear playlist?

            CurrentTokenSource = new CancellationTokenSource();
            CurrentTask = refreshLibraryAsync(CurrentTokenSource.Token);
            AsyncUtils.RunWithExceptionHandling(async() => await CurrentTask);

            async Task refreshLibraryAsync(CancellationToken token)
            {
                try
                {
                    await Task.Run(() => XSMPModule.Instance.RefreshLibrary(token));
                }
                catch (Exception e)
                {
                    AsyncUtils.ThrowIfEditorStopped();                    

                    if (e is TaskCanceledException || e is OperationCanceledException)
                    {
                        Debug.Log("[XSMP] Async operation cancelled");
                    }
                    else
                        throw;
                }
                finally
                {
                    EnableUI();
                    SignalPaint();
                }
                
            }
        }

        public async void HandleTransferButtonClicked()
        {
            try
            {
                DisableUI();
                CurrentTokenSource = new CancellationTokenSource();

                //check browse path
                if (XSMPState.CurrentBrowsePath == null || XSMPState.CurrentBrowsePath.Count != 2 || XSMPState.CurrentBrowsePath[0] != "playlist"
                    || XSMPState.BrowseData == null || XSMPState.BrowseData.Count == 0)
                    throw new InvalidOperationException();

                //clear playlist if it exists
                XSMPState.ResetQueue();

                //stop playback
                CurrentTask = XSMPModule.Instance.StopPlayback(CurrentTokenSource.Token);
                await CurrentTask;
                CurrentTask = null;

                //load playlist metadata
                Playlist playlist = null;
                CurrentTask = Task.Run(async () => {
                    playlist = await XSMPModule.Instance.GetPlaylistRaw(XSMPState.CurrentBrowsePath[1], CurrentTokenSource.Token);
                });
                await CurrentTask;
                CurrentTask = null;

                //set playlist metadata
                XSMPState.CurrentPlaylist = XSMPState.CurrentBrowsePath[1];
                XSMPState.CurrentPlaylistName = playlist.NiceName;
                XSMPState.CurrentQueueIndex = 0;

                //setup shuffle if enabled
                if(XSMPState.Shuffle)
                {
                    XSMPState.SetupShuffleFromQueue();
                    XSMPState.ShuffleCurrentIndex = 0;
                }

                //load playlist items
                //could thread this if we wanted to
                XSMPState.QueueData = new List<SongDataRow>();
                foreach (var dataRow in XSMPState.BrowseData)
                {
                    if(dataRow is SongDataRow songDataRow)
                    {
                        XSMPState.QueueData.Add(songDataRow);
                    }
                }

            }
            catch(Exception e)
            {
                XSMPState.ResetQueue();

                Debug.LogException(e);
            }
            finally
            {
                EnableUI();
                SignalPaint();
            }
        }

        public void HandleAddAllButtonClicked()
        {
            //check browse path
            if (XSMPState.CurrentBrowsePath == null || XSMPState.CurrentBrowsePath.Count != 2 || XSMPState.BrowseData == null || XSMPState.BrowseData.Count == 0)
                throw new InvalidOperationException();

            //ensure queue exists
            if (XSMPState.QueueData == null)
                XSMPState.QueueData = new List<SongDataRow>();

            //insert browse items to queue
            foreach (var dataRow in XSMPState.BrowseData)
            {
                if (dataRow is SongDataRow songDataRow)
                {
                    XSMPState.QueueData.Add(songDataRow);
                }
            }

            //shuffle handling is pretty much "give up"
            if (XSMPState.Shuffle)
            {
                XSMPState.SetupShuffleFromQueue();
                XSMPState.ShuffleCurrentIndex = 0;
            }

            //repaint
            SignalPaint();
        }

        public void HandleAddButtonClicked()
        {
            //check selected item
            if (SelectedItem == null || SelectedItem.parent != BrowsePanelContent)
                throw new InvalidOperationException();
            var backingField = SelectedItem.GetComponent<BackingFieldReference>();
            if (backingField == null || !(backingField.Value is SongDataRow))
                throw new InvalidOperationException();

            //ensure queue exists
            if (XSMPState.QueueData == null)
                XSMPState.QueueData = new List<SongDataRow>();

            //insert selected item to queue
            var songDataRow = backingField.Value as SongDataRow;
            XSMPState.QueueData.Add(songDataRow);

            //insert another index to shuffle indices
            if(XSMPState.Shuffle)
            {
                XSMPState.ShuffledIndices.Add(XSMPState.QueueData.Count - 1);
            }

            //repaint
            SignalPaint();
        }

        public void HandleRemoveButtonClicked()
        {
            //check selected item
            if (SelectedItem == null || SelectedItem.parent != QueuePanelContent)
                throw new InvalidOperationException();
            var backingField = SelectedItem.GetComponent<BackingFieldReference>();
            if (backingField == null || !(backingField.Value is SongDataRow))
                throw new InvalidOperationException();

            //remove selected item from queue
            XSMPState.QueueData.RemoveAt(backingField.Index);
            
            //remove the index from the shuffle indices
            if(XSMPState.Shuffle)
            {
                int index = XSMPState.ShuffledIndices.IndexOf(XSMPState.QueueData.Count);
                XSMPState.ShuffledIndices.RemoveAt(index);
            }

            //repaint
            SignalPaint();
        }

        public void HandleMoveUpButtonClicked()
        {
            HandleMoveButtonClicked(-1); //wait what? think about the way lists are indexed...
        }

        public void HandleMoveDownButtonClicked()
        {
            HandleMoveButtonClicked(1);
        }

        private void HandleMoveButtonClicked(int delta)
        {
            //do some checks
            if (Math.Abs(delta) > 1)
                throw new NotImplementedException(); //not implemented... yet

            if (SelectedItem == null || SelectedItem.parent != QueuePanelContent)
                throw new InvalidOperationException();

            var backingField = SelectedItem.GetComponent<BackingFieldReference>();
            if (backingField == null || !(backingField.Value is SongDataRow))
                throw new InvalidOperationException();

            var targetIndex = backingField.Index;
            if (targetIndex == 0 && delta == -1)
                return; //nop
            else if (targetIndex == (XSMPState.QueueData.Count - 1) && delta == 1)
                return; //also nop

            //reorder the list (will break if abs(delta) > 1)
            XSMPState.QueueData.Swap(targetIndex, targetIndex + delta);

            //perform some sleight of hand with queue index
            XSMPState.CurrentQueueIndex += delta;

            //once again our shuffle strategy is "just give up"
            if(XSMPState.Shuffle)
            {
                XSMPState.SetupShuffleFromQueue();
                XSMPState.ShuffleCurrentIndex = 0;
            }

            //repaint the list
            PaintQueuePanel();
        }

        public void HandleQueueNameFieldUpdated()
        {
            PaintQueueControls(); //refresh this
        }

        public async void HandleSaveQueueButtonClicked()
        {
            try
            {
                DisableUI();
                CurrentTokenSource = new CancellationTokenSource();

                //check a few things: input field must be non-empty

                //if we have a backing playlist, save over top of it
                if(!string.IsNullOrEmpty(XSMPState.CurrentPlaylist))
                {
                    //set playlist name
                    XSMPState.CurrentPlaylistName = QueueNameInputField.text;

                    //make playlist
                    var playlist = XSMPState.CreatePlaylistFromQueue();

                    //put playlist
                    CurrentTask = Task.Run(async () => {
                        await XSMPModule.Instance.PutPlaylistRaw(XSMPState.CurrentPlaylist, playlist, CurrentTokenSource.Token);
                    });
                    await CurrentTask;
                    CurrentTask = null;

                    //display a message
                    CurrentTask = Modal.PushMessageModalAsync($"Saved playlist as {XSMPState.CurrentPlaylist}", "Playlist Updated", false, CurrentTokenSource.Token);
                    await CurrentTask;
                    CurrentTask = null;

                }
                else //otherwise save a new playlist
                {
                    //set playlist name
                    XSMPState.CurrentPlaylistName = QueueNameInputField.text;

                    //get playlist cname
                    CurrentTask = Task.Run(async () =>
                    {
                        XSMPState.CurrentPlaylist = await XSMPModule.Instance.GetPlaylistUniqueName(XSMPState.CurrentPlaylistName, CurrentTokenSource.Token);
                    });
                    await CurrentTask;
                    CurrentTask = null;

                    Debug.Log(XSMPState.CurrentPlaylist);

                    //make playlist
                    var playlist = XSMPState.CreatePlaylistFromQueue();

                    //put playlist
                    CurrentTask = Task.Run(async () =>
                    {
                        await XSMPModule.Instance.PutPlaylistRaw(XSMPState.CurrentPlaylist, playlist, CurrentTokenSource.Token);
                    });
                    await CurrentTask;
                    CurrentTask = null;

                    //display a message
                    CurrentTask = Modal.PushMessageModalAsync($"Saved playlist as {XSMPState.CurrentPlaylist}", "Playlist Saved", false, CurrentTokenSource.Token);
                    await CurrentTask;
                    CurrentTask = null;
                }

                //also update the browse panel if necessary
                if(XSMPState.CurrentBrowsePath != null && XSMPState.CurrentBrowsePath.Count > 0 && XSMPState.CurrentBrowsePath[0] == "playlist")
                {
                    XSMPState.BrowseData?.Clear();

                    CurrentTask = NavigateAsync(CurrentTokenSource.Token);
                    await CurrentTask;
                    CurrentTask = null;
                }
                
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                EnableUI();
                SignalPaint();
            }
        }

        public void HandleClearQueueButtonClicked()
        {
            //is it really that simple?

            XSMPState.ResetQueue();

            QueueNameInputField.text = string.Empty;

            //clearing the queue implies stopping playback
            StopPlayback();

            SignalPaint();
        }

        public void HandleTransportPlayButtonClicked()
        {

            if (!XSMPState.Playing)
            {
                if (XSMPState.Shuffle)
                {
                    if (XSMPState.ShuffledIndices == null || XSMPState.ShuffledIndices.Count == 0)
                        XSMPState.SetupShuffleFromQueue();

                    if (XSMPState.ShuffleCurrentIndex == -1)
                        XSMPState.ShuffleCurrentIndex = 0;
                }

                StartPlayback();
            }
            else
                PausePlayback(); //TODO stop vs pause distinction?
        }

        public void HandleTransportBackButtonClicked()
        {
            if(XSMPState.Shuffle)
            {
                if (XSMPState.ShuffleCurrentIndex == 0)
                {
                    if (XSMPState.Repeat)
                        XSMPState.ShuffleCurrentIndex = XSMPState.ShuffledIndices.Count;
                    else
                        return;
                }

                XSMPState.ShuffleCurrentIndex--;
                XSMPState.CurrentQueueIndex = XSMPState.ShuffledIndices[XSMPState.ShuffleCurrentIndex];
            }
            else
            {
                if (XSMPState.CurrentQueueIndex == 0)
                {
                    if (XSMPState.Repeat)
                        XSMPState.CurrentQueueIndex = XSMPState.QueueData.Count;
                    else
                        return;
                }

                XSMPState.CurrentQueueIndex--;
            }


            ChangePlaybackTrack();
        }

        public void HandleTransportForwardButtonClicked()
        {
            if (XSMPState.Shuffle)
            {
                XSMPState.ShuffleCurrentIndex++;
                if(XSMPState.ShuffleCurrentIndex >= XSMPState.ShuffledIndices.Count)
                {
                    if (XSMPState.Repeat)
                        XSMPState.ShuffleCurrentIndex = 0;
                    else
                        return;
                }
                XSMPState.CurrentQueueIndex = XSMPState.ShuffledIndices[XSMPState.ShuffleCurrentIndex];
            }
            else
            {
                XSMPState.CurrentQueueIndex++;
                if (XSMPState.CurrentQueueIndex >= XSMPState.QueueData.Count)
                {
                    if (XSMPState.Repeat)
                        XSMPState.CurrentQueueIndex = 0;
                    else
                        XSMPState.CurrentQueueIndex--; //undo our fuckup
                        return;
                }
            }

            ChangePlaybackTrack();
        }

        public void HandleTransportRepeatButtonClicked()
        {
            XSMPState.Repeat = !XSMPState.Repeat;

            PaintTransportControls();
        }

        public void HandleTransportShuffleButtonClicked()
        {
            XSMPState.Shuffle = !XSMPState.Shuffle;

            //enable/disable shuffle?
            if(XSMPState.Shuffle)
            {
                XSMPState.SetupShuffleFromQueue();
                XSMPState.ShuffleCurrentIndex = 0;
            }
            else
            {
                XSMPState.ResetShuffle();
            }

            PaintTransportControls();
        }

        //these only kinda work

        public void HandleTransportSliderMoveStart()
        {
            Seeking = true;
        }

        public void HandleTransportSliderMoveEnd()
        {
            Seeking = false;

            if (XSMPState.Playing && XSMPState.CurrentClip != null) //are these the correct conditions?
            {

                XSMPState.TrackTime = TransportSeekBar.value * XSMPState.CurrentClip.Clip.length;

                AudioPlayer.Instance.SeekMusic(MusicSlot.User, XSMPState.TrackTime);
            }
        }
    }
}