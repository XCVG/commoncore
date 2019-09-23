using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CommonCore.State;
using CommonCore.UI;
using CommonCore.World;
using CommonCore.RpgGame.State;
using CommonCore.RpgGame.World;

namespace CommonCore.RpgGame.UI
{

    public class MapPanelController : PanelController
    {
        public Text TitleText;
        public Text MarkerText;
        public RawImage MapImage;
        public RectTransform OverlayContainer;

        //TODO caching/optimization
        private Sprite PlayerIcon
        {
            get
            {
                return CoreUtils.LoadResource<Sprite>("UI/MapMarkers/player");
            }
        }

        private Sprite KnownIcon
        {
            get
            {
                return CoreUtils.LoadResource<Sprite>("UI/MapMarkers/known");
            }
        }

        private Sprite VisitedIcon
        {
            get
            {
                return CoreUtils.LoadResource<Sprite>("UI/MapMarkers/visited");
            }
        }

        public override void SignalPaint()
        {
            ClearPanel();

            //grab map name from string tables
            string sceneName = SceneManager.GetActiveScene().name;
            string realMapName = StringSub.Sub.Replace(sceneName, "MAPS");
            TitleText.text = realMapName;

            //get cartographer
            Cartographer cartographer = Cartographer.Current;
            if (cartographer == null)
            {
                Debug.LogWarning("No cartographer exists!");
                ClearPanel();
                return;
            }

            //push map
            MapImage.texture = cartographer.MapTexture;

            //scale image
            if(MapImage.texture != null)
            {
                float ratio = (float)MapImage.texture.height / (float)MapImage.texture.width;
                float newHeight = ratio * MapImage.rectTransform.rect.width;
                MapImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
                MapImage.rectTransform.ForceUpdateRectTransforms();
            }

            //draw player if applicable
            if(cartographer.ShowPlayer)
            {
                DrawIcon(WorldUtils.GetPlayerObject().transform.position, PlayerIcon, cartographer, GameState.Instance?.PlayerRpgState?.DisplayName);
            }

            DrawMarkers(cartographer);
        }

        private void ClearPanel()
        {
            TitleText.text = string.Empty;
            MarkerText.text = string.Empty;
            MapImage.texture = null;
            SceneUtils.DestroyAllChildren(OverlayContainer);
        }

        private void ClearMarkerText()
        {
            MarkerText.text = string.Empty;
        }
        
        private void SetMarkerText(string text)
        {
            MarkerText.text = text;
        }

        private void DrawIcon(Vector3 worldPos, Sprite icon, Cartographer c, string mouseOverText)
        {
            GameObject go = DrawIcon(c.NormalizeWorldPosition(worldPos), icon);

            if(!string.IsNullOrEmpty(mouseOverText))
            {
                var et = go.AddComponent<EventTrigger>();

                EventTrigger.Entry enterEntry = new EventTrigger.Entry();
                enterEntry.eventID = EventTriggerType.PointerEnter;
                enterEntry.callback.AddListener((data) => SetMarkerText(mouseOverText));
                et.triggers.Add(enterEntry);

                EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                exitEntry.eventID = EventTriggerType.PointerExit;
                exitEntry.callback.AddListener((data) => ClearMarkerText());
                et.triggers.Add(exitEntry);
            }
        }

        private GameObject DrawIcon(Vector2 normalizedPos, Sprite icon)
        {
            GameObject go = new GameObject("Icon", typeof(RectTransform));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(OverlayContainer);

            go.AddComponent<CanvasRenderer>();
            Image image = go.AddComponent<Image>();
            image.sprite = icon;

            float posX = normalizedPos.x * (MapImage.rectTransform.rect.width / 2f);
            float posY = normalizedPos.y * (MapImage.rectTransform.rect.height / 2f);
            rt.anchoredPosition = new Vector2(posX, posY);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 16f);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 16f);

            return go;
        }

        private void DrawMarkers(Cartographer c)
        {
            if (c == null || c.Markers == null || c.Markers.Length < 1)
                return;

            Dictionary<string, MapMarkerState> markersData = GameState.Instance.MapMarkers;

            foreach(EditorMapMarker emm in c.Markers)
            {
                if (string.IsNullOrEmpty(emm.Id) && !emm.ForceShow)
                    continue;

                MapMarkerState state = emm.ForceShow ? MapMarkerState.Known : markersData.GetOrDefault(emm.Id, MapMarkerState.Unknown);

                if(state != MapMarkerState.Unknown)
                    DrawIcon(emm.WorldPoint, GetMarkerSprite(emm.IconOverride, state), c, emm.NiceName);
            }

        }

        private Sprite GetMarkerSprite(string markerOverride, MapMarkerState state)
        {
            if(string.IsNullOrEmpty(markerOverride))
            {
                switch (state)
                {
                    case MapMarkerState.Known:
                        return KnownIcon;
                    case MapMarkerState.Visited:
                        return VisitedIcon;
                }

                return null;
            }

            Sprite loadedSprite = CoreUtils.LoadResource<Sprite>(string.Format("UI/MapMarkers/{0}/{1}", markerOverride, state.ToString().ToLowerInvariant()));

            if(loadedSprite == null)
            {
                switch (state)
                {
                    case MapMarkerState.Known:
                        return KnownIcon;
                    case MapMarkerState.Visited:
                        return VisitedIcon;
                }

                return null;
            }
            else
            {
                return loadedSprite;
            }

        }


    }
}