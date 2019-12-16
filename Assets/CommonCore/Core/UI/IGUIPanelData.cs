using UnityEngine;

namespace CommonCore.UI
{
    public struct IGUIPanelData
    {
        public string NiceName;
        public int Priority;
        public GameObject Prefab;

        internal IGUIPanelData(int priority, string niceName, GameObject prefab)
        {
            Priority = priority;
            NiceName = niceName;
            Prefab = prefab;
        }
    }
}