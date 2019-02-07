using UnityEngine;
using System.Collections;

//General purpose light blinking script
//Attach it to any light and watch the magic happen!

namespace CommonCore.ObjectActions
{

    public class LightBlinkScript : MonoBehaviour
    {

        // Maximum intensity to flicker to (default: 1.0/full)
        public float maxIntensity = 1.0f;
        // Minimum intensity to flicker to (default: 0.0/none)
        public float minIntensity = 0.0f;
        // Chance to flicker each tick (default: 0.5/50%)
        public float flickerChance = 0.5f;
        // Minimum frames between flickers (default: 0/every frame)
        public int flickerDelay = 0;
        // Randomize causes the light to flicker to a random intensity (default: false/off)
        public bool randomize = false;

        private Light myLight;
        private int framesSinceLast;

        void Start()
        {
            myLight = gameObject.GetComponent<Light>();
            if (myLight == null)
            {
                Debug.LogWarning(string.Format("LightBlinkScript {0} attached to GameObject {1}({2}) has no light attached!", this.GetInstanceID(), this.gameObject.name, this.gameObject.GetInstanceID()));
            }

            framesSinceLast = 0;
        }

        void FixedUpdate()
        {

            //if flickerDelay > 0, see if we need to execute this frame
            //a bit awkward because we're optimizing for the case when you're NOT using this
            if (flickerDelay > 0)
            {
                framesSinceLast++;


                if (framesSinceLast < flickerDelay)
                    return; //it's not time to flicker yet

                //reset framesSinceLast before execution
                framesSinceLast = 0;
            }

            //if random < flickerChance...        
            if (Random.Range(0.0f, 1.0f) < flickerChance)
            {
                //if randomize is enabled, flicker to a random intensity between min and max
                if (randomize)
                {
                    myLight.intensity = Random.Range(minIntensity, maxIntensity);
                }
                else
                {
                    //otherwise, flicker to min or max (whichever this one isn't)
                    if (myLight.intensity > minIntensity)
                    {
                        myLight.intensity = minIntensity;
                    }
                    else
                    {
                        myLight.intensity = maxIntensity;
                    }
                }

            }

        }
    }
}
