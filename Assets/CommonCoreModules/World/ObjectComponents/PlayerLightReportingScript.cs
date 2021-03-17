using CommonCore.Config;
using CommonCore.Messaging;
using UnityEngine;

namespace CommonCore.World
{

    /// <summary>
    /// Light reporting script
    /// </summary>
    public class PlayerLightReportingScript : MonoBehaviour, IReportLight
    {
        //public for now, should probably SerializeField this stuff
        public PlayerLightReportingType ReportingType;

        public bool Clamp = false;

        [Header("Calculated Mode")]
        public int CalculateInterval = 15;
        public float CalculatedAmbientWeight = 1.0f;
        public float CalculatedDirectionalWeight = 1.0f;
        public float CalculatedBias = 1.5f;

        [Header("Probed Mode")]
        public int ProbeInterval = 1;
        public float ProbeBias = 4f;

        [Header("Probed Mode Components")]
        public Camera ProbeCamera;
        public GameObject ProbeTargetObject;
        public RenderTexture ProbeRenderTexture;

        //[field: SerializeField]
        public Color Light { get; private set; } = Color.white;

        private int FramesSinceLast = 0;
        private QdmsMessageInterface MessageInterface;

        private void Awake()
        {
            MessageInterface = new QdmsMessageInterface(gameObject);
            MessageInterface.KeepMessagesInQueue = false;
            MessageInterface.SubscribeReceiver(HandleMessageReceived);
        }

        private void Start()
        {
            ReportingType = ConfigState.Instance.PlayerLightReporting;
            //Debug.Log("Light reporting mode: " + ReportingType);

            //should we disable if ReportingType is None?
            //if (ReportingType == PlayerLightReportingType.None)
            //    enabled = false;
            SetupProbe();
            SetProbedComponentsState();
            FramesSinceLast = short.MaxValue;
        }

        private void HandleMessageReceived(QdmsMessage message)
        {
            if (message is QdmsFlagMessage flagMessage && flagMessage.Flag == "ConfigChanged")
            {
                ReportingType = ConfigState.Instance.PlayerLightReporting;
                //Debug.Log("Light reporting mode: " + ReportingType);
                SetProbedComponentsState();
                FramesSinceLast = short.MaxValue;

                if (ReportingType == PlayerLightReportingType.None)
                    Light = Color.white;
            }
        }

        private void Update()
        {
            FramesSinceLast++;

            switch (ReportingType)
            {
                case PlayerLightReportingType.Calculated:
                    if(FramesSinceLast >= CalculateInterval)
                    {
                        UpdateCalculated();
                        FramesSinceLast = 0;
                    }
                    break;
                case PlayerLightReportingType.Probed:
                    if (FramesSinceLast >= ProbeInterval)
                    {
                        UpdateProbed();
                        FramesSinceLast = 0;
                    }
                    break;
            }
        }

        private void SetupProbe()
        {
            if (ProbeCamera == null)
                ProbeCamera = GetComponentInChildren<Camera>();

            if(ProbeRenderTexture == null)
            {
                ProbeRenderTexture = new RenderTexture(ProbeCamera.targetTexture);
                //ProbeRenderTexture = ProbeCamera.targetTexture;
            }

            ProbeCamera.targetTexture = ProbeRenderTexture;

            ProbeTargetObject.GetComponent<Renderer>().receiveShadows = true;
            
        }

        private void SetProbedComponentsState()
        {
            if (ReportingType != PlayerLightReportingType.Probed)
            {
                ProbeCamera.enabled = false;
                ProbeTargetObject.SetActive(false);
            }
            else
            {
                ProbeCamera.enabled = true;
                ProbeTargetObject.SetActive(true);
            }
        }

        private void UpdateCalculated()
        {
            Color ambientLight, directionalLight, light;

            switch (RenderSettings.ambientMode)
            {               
                case UnityEngine.Rendering.AmbientMode.Trilight:
                    ambientLight = (RenderSettings.ambientSkyColor + RenderSettings.ambientEquatorColor * 0.5f + RenderSettings.ambientGroundColor * 0.5f) / 2f;
                    break;
                case UnityEngine.Rendering.AmbientMode.Flat:
                    ambientLight = RenderSettings.ambientLight;
                    break;
                case UnityEngine.Rendering.AmbientMode.Skybox:
                case UnityEngine.Rendering.AmbientMode.Custom:
                default:
                    ambientLight = RenderSettings.ambientIntensity * Color.white; //TODO better solution for skyboxen?
                    break;
            }

            var sun = RenderSettings.sun;
            if(sun != null)
            {
                directionalLight = sun.color * sun.intensity;

                light = (ambientLight * CalculatedAmbientWeight + directionalLight * CalculatedDirectionalWeight) * (1f / (CalculatedAmbientWeight + CalculatedDirectionalWeight));
            }
            else
            {
                light = ambientLight;
            }

            light *= CalculatedBias;

            if (Clamp)
            {
                light = new Color(Mathf.Min(Light.r, 1.0f), Mathf.Min(Light.g, 1.0f), Mathf.Min(Light.b, 1.0f), 1.0f);
            }
            else
            {
                light.a = 1.0f;
            }

            Light = light;

            //Debug.Log(Light);
        }

        private void UpdateProbed()
        {
            //dunno how much of this is necessary but we'll optimize it later
            //based on a youtube video: https://www.youtube.com/watch?v=NYysvuyivc4

            RenderTexture tempRT = RenderTexture.GetTemporary(ProbeRenderTexture.width, ProbeRenderTexture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            Graphics.Blit(ProbeRenderTexture, tempRT);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tempRT;

            Texture2D tempTex = new Texture2D(ProbeRenderTexture.width, ProbeRenderTexture.height,TextureFormat.RGBA32, false, true);
            tempTex.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
            tempTex.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tempRT);

            Color[] colors = tempTex.GetPixels();
            Vector4 colorTotal = Vector4.zero;
            for(int i = 0; i < colors.Length; i++)
            {
                colorTotal += (Vector4)colors[i];
            }

            //Debug.Log(colors.ToNiceString());

            Color light = (Color)(colorTotal / colors.Length);

            light *= ProbeBias;

            if (Clamp)
            {
                light = new Color(Mathf.Min(Light.r, 1.0f), Mathf.Min(Light.g, 1.0f), Mathf.Min(Light.b, 1.0f), 1.0f);
            }
            else
            {
                light.a = 1.0f;
            }

            Light = light;

            Destroy(tempTex);

            //Debug.Log(light);
        }
    }
}