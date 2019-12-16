using CommonCore.Config;
using CommonCore.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.XSMP
{

    public class XSMPConfigPanelController : ConfigSubpanelController
    {
        [SerializeField]
        private Toggle XSMPEnableToggle;

        public override void PaintValues()
        {
            XSMPEnableToggle.isOn = ConfigState.Instance.CustomConfigFlags.Contains("XSMPEnabled");
        }

        public override void UpdateValues()
        {
            if (XSMPEnableToggle.isOn && !ConfigState.Instance.CustomConfigFlags.Contains("XSMPEnabled"))
                ConfigState.Instance.CustomConfigFlags.Add("XSMPEnabled");
            else if (!XSMPEnableToggle.isOn && ConfigState.Instance.CustomConfigFlags.Contains("XSMPEnabled"))
                ConfigState.Instance.CustomConfigFlags.Remove("XSMPEnabled");

        }
    }
}