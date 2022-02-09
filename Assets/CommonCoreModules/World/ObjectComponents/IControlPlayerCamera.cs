using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{
    public interface IControlPlayerCamera
    {
        Camera GetCamera();
        AudioListener GetAudioListener();
    }
}
