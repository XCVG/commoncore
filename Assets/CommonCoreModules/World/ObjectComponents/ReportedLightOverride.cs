using CommonCore.Config;
using CommonCore.Messaging;
using UnityEngine;

namespace CommonCore.World
{
    /// <summary>
    /// Overrides the ambient light used by light reporting
    /// </summary>
    public class ReportedLightOverride : MonoBehaviour
    {
        [Tooltip("If set, will override in probed mode, if not, only in calculated mode")]
        public bool OverrideProbed;
        public Color ReportedLight;

        public static ReportedLightOverride Current
        {
            get
            {
                if (_Current == null)
                    _Current = CoreUtils.GetWorldRoot().GetComponent<ReportedLightOverride>();

                return _Current;
            }
        }

        private static ReportedLightOverride _Current;

        private void OnDestroy()
        {
            _Current = null;
        }
    }
}