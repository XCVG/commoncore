using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.UI
{

    /// <summary>
    /// Asset representing a UI theme
    /// </summary>
    [CreateAssetMenu(fileName = "New UI Theme", menuName = "CCScriptableObjects/UIThemeAsset")]
    public class UIThemeAsset : ScriptableObject
    {
        [Header("Text")]
        public Font HeadingFont;
        public Font BodyFont;
        public Font MonospaceFont;

        public Color TextColor;
        public Color TextContrastingColor;

        [Header("Panels")]
        public Sprite Panel;
        public Sprite PanelContrasting;

        public Sprite Frame;
        public Sprite FrameContrasting;

        public Sprite Button;
        public Sprite ButtonContrasting;

        [Header("Scrollbar")]
        public Sprite ScrollbarFrame;
        public Sprite ScrollbarHandle;

        [Header("Slider")]
        public Sprite Slider;
        public Sprite SliderHandle;
        public Sprite SliderFill;

        [Header("Bar")]
        public Sprite Bar;
        public Sprite BarFill;

        [Header("Radio Buttons")]
        public Sprite RadioButton;
        public Sprite RadioButtonCheck;

        [Header("Toggle Buttons")]
        public Sprite ToggleButton;
        public Sprite ToggleButtonCheck;

        [Header("Combo Box")]
        public Sprite ComboBox;
        public Sprite ComboBoxList;
        public Sprite ComboBoxArrow;

        [Header("Input Field")]
        public Sprite InputField;
    }
}