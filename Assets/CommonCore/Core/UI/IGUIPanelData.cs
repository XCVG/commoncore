using System;
using UnityEngine;

namespace CommonCore.UI
{
    public struct IGUIPanelData
    {
        public string NiceName;
        public int Priority;
        public Func<Transform, GameObject> Builder;

        [Obsolete]
        internal IGUIPanelData(int priority, string niceName, GameObject prefab)
        {
            Priority = priority;
            NiceName = niceName;
            Builder = (t) => GameObject.Instantiate(prefab, t);
        }

        internal IGUIPanelData(int priority, string niceName, Func<Transform, GameObject> builder)
        {
            Priority = priority;
            NiceName = niceName;
            Builder = builder;
        }
    }
}