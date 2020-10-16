using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommonCore.AddonSupport
{
    /// <summary>
    /// Proxy component that creates and configures a SceneController of your choosing
    /// </summary>
    public class SceneControllerProxy : MonoBehaviour
    {
        public string SceneControllerClass;

        [Header("Proxied Fields (base scene controller)"), ProxyField]
        public bool AutosaveOnEnter = false;
        [ProxyField]
        public bool AutosaveOnExit = true;

        [ProxyField]
        public bool AutoRestore = true;
        [ProxyField]
        public bool AutoCommit = true;

        [ProxyField]
        public bool AutoinitUi = true;
        [ProxyField]
        public bool AutoinitHud = true;
        [ProxyField]
        public bool AutoinitState = true;

        [ProxyField]
        public string HudOverride = null;

        [Header("Proxied Fields (world scene controller)"), ProxyField(LinkToTypes = new string [] {"WorldSceneController"})]
        public bool AllowQuicksave = true;
        [ProxyField(LinkToTypes = new string[] { "WorldSceneController" })]
        public bool AutoGameover = true;
        [ProxyField(LinkToTypes = new string[] { "WorldSceneController" })]
        public string SetMusic;
        [ProxyField(LinkToTypes = new string[] { "WorldSceneController" })]
        public Bounds WorldBounds = new Bounds(Vector3.zero, new Vector3(2000f, 2000f, 1000f));

        [Header("Other Proxied Fields"), ProxyExtensionData]
        public List<ProxyExtensionTuple> ExtraFields;

        private void Awake()
        {
            try
            {
                Debug.Log($"Creating a scene controller of type {SceneControllerClass}");

                gameObject.SetActive(false);

                Type controllerType = CCBase.BaseGameTypes
                    .Where(t => t.Name == SceneControllerClass)
                    //.Where(t => t.Name.EndsWith(SceneControllerClass))
                    .Single();
                MonoBehaviour controller = (MonoBehaviour)gameObject.AddComponent(controllerType);

                ProxyUtils.SetProxyFields(this, controller); //this call does 90% of the magic

                //fires awake, probably
                gameObject.SetActive(true);
                
                //it will also fire OnEnable and OnDisable
                //that _shouldn't_ be a problem but if we need to we can re-root the world
            }
            catch(Exception e)
            {
                Debug.LogError($"Error setting up proxied scene controller {e.GetType().Name}");
                Debug.LogException(e);
            }
            finally
            {
                gameObject.SetActive(true);
            }

        }
    }
}