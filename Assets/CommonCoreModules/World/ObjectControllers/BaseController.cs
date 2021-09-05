using CommonCore.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{
    public abstract class BaseController : MonoBehaviour
    {
        //a gross hack to prevent overwriting
        //I call it the "work around unity" design pattern
        public string EditorFormID;
        public string FormID { get; private set; }
        [SerializeField]
        protected string[] EntityTags;

        [Tooltip("May be deferred to subclass and have no effect")]
        public bool HandleRestorableExtraData = true;

        public int HitMaterial = 0;

        protected virtual bool DeferRestorableExtraDataToSubclass => false;
        protected virtual bool DeferComponentInitToSubclass => false;

        protected bool Initialized;

        public virtual HashSet<string> Tags
        {
            get
            {
                if(_Tags == null)
                {
                    _Tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    if (EntityTags != null && EntityTags.Length > 0)
                        _Tags.UnionWith(EntityTags);
                }

                return _Tags;
            }
        }
        protected HashSet<string> _Tags;

        public virtual void Awake()
        {
            FormID = EditorFormID;
            
            if(FormID == name)
            {
                Debug.Log("FID: " + FormID + " TID: " + name);
                Debug.LogWarning("TID is the same as FID (did you forget to assign TID?)");
            }

        }

        // Use this for initialization
        public virtual void Start()
        {
            //set Unity tag
            //if (tag == null || tag == "Untagged")
            //    tag = "CCEntity";

            TryExecuteOnComponents(component => component.BeforeInit(this));

            if (!DeferComponentInitToSubclass)
            {
                TryExecuteOnComponents(component => component.Init(this));
                Initialized = true;
            }
        }

        public virtual void OnEnable()
        {
            TryExecuteOnComponents(component => component.Activate(!Initialized));
        }

        public virtual void OnDisable()
        {
            TryExecuteOnComponents(component => component.Deactivate());
        }

        public virtual void OnDestroy()
        {
            TryExecuteOnComponents(component => component.BeforeDestroy());
        }

        // Update is called once per frame
        public virtual void Update()
        {

        }

        //commit/restore methods
        //probably should have used properties but oh well
        public virtual Dictionary<string, object> CommitEntityData()
        {
            var data = new Dictionary<string, object>();

            if(HandleRestorableExtraData && !DeferRestorableExtraDataToSubclass)
            {
                foreach (IHaveRestorableExtraData component in GetComponentsInChildren<IHaveRestorableExtraData>(true))
                    component.CommitExtraData(data);
            }

            return data;
        }        
        public virtual void RestoreEntityData(Dictionary<string, object> data)
        {
            if (HandleRestorableExtraData && !DeferRestorableExtraDataToSubclass)
            {
                foreach (IHaveRestorableExtraData component in GetComponentsInChildren<IHaveRestorableExtraData>(true))
                    component.RestoreExtraData(data);
            }

            return;
        }

        public virtual bool GetVisibility()
        {
            return true;
        }
        public virtual void SetVisibility(bool visible)
        {
            return;
        }

        //TODO move to utility class?
        protected void TryExecuteOnComponents(Action<IReceiveEntityEvents> function)
        {
            foreach (var component in GetComponentsInChildren<IReceiveEntityEvents>(true)) //TODO should we run on deactivated components?
            {
                try
                {
                    function(component);
                }
                catch(Exception e)
                {
                    Debug.LogError($"Failed to execute action on component {component} ({e.GetType().Name})");
                    if (ConfigState.Instance.UseVerboseLogging)
                        Debug.LogException(e);
                }
            }
        }

    }
}