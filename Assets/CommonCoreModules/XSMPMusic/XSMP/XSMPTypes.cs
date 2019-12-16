using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CommonCore.XSMP
{
    public enum ServerStatus
    {
        Offline, NotReady, Ready
    }

    public enum HttpVerb
    {
        GET, POST, PUT, DELETE
    }

    internal struct RestRequest
    {
        public HttpMethod Verb;
        public string Path;
        public IDictionary<string, string> Params;
        public string Body;

        public RestRequest(string path, HttpMethod verb)
        {
            Path = path;
            Verb = verb;
            Params = null;
            Body = null;
        }

        public RestRequest(string path, HttpMethod verb, string body, params KeyValuePair<string, string>[] prms)
        {
            Path = path;
            Verb = verb;
            Body = body;

            if (prms != null && prms.Length > 0)
            {
                Params = new Dictionary<string, string>();
                foreach (var param in prms)
                    Params.Add(param.Key, param.Value);
            }
            else
            {
                Params = null;
            }
        }

        public string MakeQueryStringFromParams()
        {
            if (Params == null || Params.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder(255);
            sb.Append("?");
            
            foreach(var key in Params.Keys)
            {
                var value = Params[key];
                sb.AppendFormat("{0}={1}&", key, Uri.EscapeDataString(value));
            }

            if(sb.Length > 0 && sb[sb.Length-1] == '&')
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }
    }

    internal struct RestResponse
    {
        public int StatusCode;
        public string Body;

        public RestResponse(int statusCode, string body)
        {
            StatusCode = statusCode;
            Body = body;
        }
    }

    internal class RestRequestFailedException : Exception
    {
        public override string Message => $"The REST request failed with status code {StatusCode}";

        public int StatusCode { get; private set; } = -1;

        public RestRequestFailedException(int statusCode)
        {
            StatusCode = statusCode;
        }
    }

    public class RefCountedClip //I think we can actually eliminate reference counting if we never delete currentclip and are careful about when we delete clips
    {
        public AudioClip Clip { get; private set; }
        //public int RefCount { get; private set; }
        public DateTime CreationTime { get; private set; } //because I just don't want to fucking deal with Unity's thread-unsafe time class bullshit
        public DateTime LastUsedTime { get; private set; }

        public RefCountedClip(AudioClip clip)
        {
            Clip = clip;
            //RefCount = 0;
            CreationTime = DateTime.Now;
        }

        public void AddRef()
        {
            //RefCount++;
            LastUsedTime = DateTime.Now;
        }

        public void ReleaseRef()
        {
            //RefCount--;
        }
    }

    //data row model objects

    /// <summary>
    /// Base ViewModel class for data rows
    /// </summary>
    public abstract class DataRow
    {
        //TODO visibility?

        /// <summary>
        /// The target browse path if this data row is selected (may be null)
        /// </summary>
        public virtual IReadOnlyList<string> TargetBrowsePath {get; protected set;}

        /// <summary>
        /// The title of this data row
        /// </summary>
        public string Title { get; protected set; }

        /// <summary>
        /// Backing ID; eg a song hash or cname
        /// </summary>
        public string Id { get; protected set; }

    }

    /// <summary>
    /// ViewModel class for a header/navigation data row
    /// </summary>
    public class HeaderDataRow : DataRow
    {
        //we don't actually need any extra data I think

        public HeaderDataRow(string title, IEnumerable<string> target)
        {
            Id = null;
            Title = title;            
            TargetBrowsePath = target?.ToImmutableArray() ?? null;
        }

        //we can't exactly parse this one lol
    }

    /// <summary>
    /// ViewModel class for a folder/subfolder data row
    /// </summary>
    public class FolderDataRow : DataRow
    {
        public string Path { get; private set; }

        public FolderDataRow(string name, string path)
        {
            Title = name;
            Path = path;
            TargetBrowsePath = new string[] { "folder", path };
        }
    }

    /// <summary>
    /// ViewModel class for a song
    /// </summary>
    public class SongDataRow : DataRow
    {
        public IReadOnlyList<string> Artists { get; private set; }
        public string Album { get; private set; }
        public string AlbumId { get; private set; }
        public string AlbumArtistId { get; private set; }
        public string Genre { get; private set; }        
        public int Track { get; private set; }
        public int Set { get; private set; }
        public float Length { get; private set; }

        public SongDataRow(string id, string title, IEnumerable<string> artists, string album, string albumId, string albumArtistId, string genre, int track, int set, float length)
        {
            Id = id;
            Title = title;
            Artists = artists.ToImmutableArray();
            Album = album;
            AlbumId = albumId;
            AlbumArtistId = albumArtistId;
            Genre = genre;
            Track = track;
            Set = set;
            Length = length;
            TargetBrowsePath = null;
        }

        public static SongDataRow Parse(JObject obj)
        {
            string id = obj.Value<string>("Hash");
            string title = obj.Value<string>("Title");
            int track = obj.Value<int>("Track");
            int set = obj.Value<int>("Set");

            string genre = string.Empty;
            if (!obj["Genre"].IsNullOrEmpty())
                genre = obj.Value<string>("Genre");

            //album handling
            string album = string.Empty;
            string albumId = null;
            string albumArtistId = null;
            if (!obj["Album"].IsNullOrEmpty())
            {
                album = obj["Album"].Value<string>("Title");
                albumId = obj["Album"].Value<string>("Name");
                albumArtistId = obj["Album"]["Artist"].Value<string>("Name");
            }

            //artists handling
            List<string> artists = new List<string>();
            if(!obj["Artists"].IsNullOrEmpty())
            {
                var artistsJArray = obj["Artists"] as JArray;
                foreach(var artistJToken in artistsJArray)
                {
                    artists.Add(artistJToken.Value<string>("NiceName"));
                }
            }

            float length = obj["Length"].ToObject<float>();

            return new SongDataRow(id, title, artists, album, albumId, albumArtistId, genre, track, set, length);
        }
    }

    public class AlbumDataRow : DataRow
    {
        public string Artist { get; private set; } //artist nice name
        public string ArtistId { get; private set; }
        
        public AlbumDataRow(string id, string title, string artistId, string artist)
        {
            Id = id;
            Title = title;
            ArtistId = artistId;
            Artist = artist;
            TargetBrowsePath = ImmutableArray.Create("album", id);
        }

        public static AlbumDataRow Parse(JObject obj)
        {
            string albumId = obj.Value<string>("Name");
            string artistId = obj["Artist"].Value<string>("Name");
            string id = artistId + "_" + albumId;

            string title = obj.Value<string>("Title");

            string artist = obj["Artist"].Value<string>("NiceName");

            return new AlbumDataRow(id, title, artistId, artist);
        }
    }

    public class ArtistDataRow : DataRow
    {
        //we may add support for number of songs someday

        public ArtistDataRow(string id, string name)
        {
            Id = id;
            Title = name;
            TargetBrowsePath = ImmutableArray.Create("artist", id);
        }

        public static ArtistDataRow Parse(JObject obj)
        {
            string id = obj.Value<string>("Name");
            string name = obj.Value<string>("NiceName");

            return new ArtistDataRow(id, name);
        }
    }

    /// <summary>
    /// ViewModel class for a row representing a playlist (not a row in a playlist)
    /// </summary>
    public class PlaylistDataRow : DataRow
    {
        //we may add support for number of songs someday

        public PlaylistDataRow(string id, string name)
        {
            Id = id;
            Title = name;
            TargetBrowsePath = ImmutableArray.Create("playlist", id);
        }

        public static PlaylistDataRow Parse(JObject obj)
        {
            string id = obj.Value<string>("Name");
            string name = obj.Value<string>("NiceName");

            return new PlaylistDataRow(id, name);
        }
    }
}