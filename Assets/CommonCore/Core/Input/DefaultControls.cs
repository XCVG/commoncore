using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Input
{
    /// <summary>
    /// A simple list of default controls
    /// </summary>
    /// <remarks>
    /// <para>It's up to you to prevent collisions, especially if you use AdditionalAxes/AdditionalButtons</para>
    /// </remarks>
    public static class DefaultControls
    {
        //the intent was at one point to do redirection here, before we realized it was a TERRIBLE idea
        //so now we have a weirdly designed kinda-legacy class

        [ControlIsAxis]
        public static readonly string MoveX = "Horizontal";
        [ControlIsAxis]
        public static readonly string MoveY = "Vertical";
        [ControlIsAxis]
        public static readonly string LookX = "LookX";
        [ControlIsAxis]
        public static readonly string LookY = "LookY";
        public static readonly string Jump = "Jump";
        public static readonly string Sprint = "Run";
        public static readonly string Crouch = "Crouch";

        public static readonly string Fire = "Fire1";
        public static readonly string AltFire = "Fire2";
        public static readonly string Zoom = "Fire3";
        public static readonly string Reload = "Reload";
        public static readonly string Use = "Use";
        public static readonly string AltUse = "Use2";
        public static readonly string Offhand1 = "Offhand1";
        public static readonly string Offhand2 = "Offhand2";

        public static readonly string OpenMenu = "OpenMenu";
        public static readonly string OpenFastMenu = "OpenFastMenu";
        public static readonly string ChangeView = "ChangeView";

        [ControlIsAxis]
        public static readonly string NavigateX = "NavigateX";
        [ControlIsAxis]
        public static readonly string NavigateY = "NavigateY";
        public static readonly string Confirm = "Confirm";
        public static readonly string Cancel = "Cancel";   
    }

    /// <summary>
    /// Indicates that this control should be treated as an axis rather than a button.
    /// </summary>
    /// <remarks>
    /// What to do with this is up to the input backend.
    /// </remarks>
    public class ControlIsAxisAttribute : Attribute
    {

    }
}