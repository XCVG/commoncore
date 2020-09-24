using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    /// <summary>
    /// Light blink/flash script
    /// </summary>
    public class LightFlashScript : MonoBehaviour
    {
        [SerializeField]
        private Light Light = null;

        [SerializeField]
        private float FadeInTime = 0.1f;
        [SerializeField]
        private float HoldTime = 0.2f;
        [SerializeField]
        private float FadeOutTime = 0.1f;
        [SerializeField]
        private float MinIntensity = 0;
        [SerializeField]
        private float MaxIntensity = 1;
        [SerializeField]
        private bool Repeat = false;

        private FlashState State = FlashState.FadeIn;
        private float TimeInState = 0;

        private void Start()
        {
            if (Light == null)
                Light = GetComponent<Light>();

            if (Light == null)
                Light = GetComponentInChildren<Light>();
        }

        private void Update()
        {
            TimeInState += Time.deltaTime;

            switch (State)
            {
                case FlashState.FadeIn:
                    {
                        float ratio = TimeInState / FadeInTime;

                        Light.intensity = Mathf.Lerp(MinIntensity, MaxIntensity, ratio);

                        if (TimeInState >= FadeInTime)
                        {
                            State = FlashState.Hold;
                            TimeInState = 0;
                        }
                    }
                    break;
                case FlashState.Hold:
                    {
                        if (TimeInState >= HoldTime)
                        {
                            State = FlashState.FadeOut;
                            TimeInState = 0;
                        }
                    }
                    break;
                case FlashState.FadeOut:
                    {
                        float ratio = TimeInState / FadeOutTime;

                        Light.intensity = Mathf.Lerp(MinIntensity, MaxIntensity, 1f - ratio);

                        if (TimeInState >= FadeOutTime)
                        {
                            if (Repeat)
                            {
                                State = FlashState.FadeIn;
                            }
                            else
                            {
                                Light.intensity = 0;
                                //Light.enabled = false;
                                enabled = false;
                            }
                        }
                    }
                    break;
            }
        }

        private enum FlashState
        {
            FadeIn, Hold, FadeOut
        }
    }
}