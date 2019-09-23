using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Input
{
    /*
     * Default controls
     * A simple dataset of values, limits errors and could do redirection here
     */
    public static class DefaultControls
    {
        public const string MoveX = "Horizontal";
        public const string MoveY = "Vertical";
        public const string LookX = "LookX";
        public const string LookY = "LookY";
        public const string Jump = "Jump";
        public const string Sprint = "Run";
        public const string Crouch = "Crouch";

        public const string Fire = "Fire1";
        public const string AltFire = "Fire2";
        public const string Zoom = "Fire3";
        public const string Reload = "Reload";
        public const string Use = "Use";
        public const string AltUse = "Use2";
        public const string Offhand1 = "Offhand1";
        public const string Offhand2 = "Offhand2";

        public const string OpenFastMenu = "OpenFastMenu";
        public const string ChangeView = "ChangeView";
    }
}