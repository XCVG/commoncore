using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Vnx
{

    public class VnOptions
    {
        public bool ClearStage { get; set; }
        public bool PlayAdvanceBeep { get; set; } = true;
        public string PostPresentScript { get; set; }
        public bool TypeOn { get; set; } = true;
    }

    public class VnActor
    {
        public string Name { get; set; }
        public string Image { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public bool Flip { get; set; }
        public bool Fade { get; set; }
        public bool Visible { get; set; } = true;

    }

    public class VnConfig
    {
        public bool AllowFade { get; set; } = true;
        public bool EnableAdvanceBeep { get; set; } = true;
        public float TypeOnSpeed { get; set; } = 1f;
    }

}