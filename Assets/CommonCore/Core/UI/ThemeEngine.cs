using CommonCore.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.UI
{
    
    /// <summary>
    /// Very basic UI theming engine
    /// </summary>
    public static class ThemeEngine
    {

        //TODO add some try/catch

        public static void ApplyThemeToAll(Transform root, UIThemeAsset theme)
        {
            Queue<Transform> elements = new Queue<Transform>();
            ApplyThemeToElement(root, theme, elements);
            while(elements.Count > 0)
            {
                ApplyThemeToElement(elements.Dequeue(), theme, elements);
            }
        }

        public static void ApplyThemeToElement(Transform element, UIThemeAsset theme, Queue<Transform> elements)
        {
            try
            {
                //handle non-themable/ignore element
                var nonThemableComponent = element.GetComponent<NonThemableElement>();
                if (nonThemableComponent != null)
                {
                    if (!nonThemableComponent.IgnoreChildren)
                    {
                        foreach (Transform child in element)
                        {
                            elements.Enqueue(child);
                        }
                    }

                    return;
                }

                //determine element class and color class
                DetermineElement(element, out var elementClass, out var elementColorClass, out var applyToChildren);

                //apply theme
                ApplyThemeToElement(element, elementClass, elementColorClass, theme);

                //append children if applicable
                if (applyToChildren)
                {
                    foreach (Transform child in element)
                    {
                        elements.Enqueue(child);
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogError($"[ThemeEngine] Error in outer ApplyThemeToElement \"{(element.Ref()?.name ?? "null")}\" ({e.GetType().Name})");
                if (ConfigState.Instance.UseVerboseLogging)
                    Debug.LogException(e);
            }
        }

        public static void ApplyThemeToElement(Transform element, UIThemeAsset theme)
        {
            try
            {
                if (element.GetComponent<NonThemableElement>())
                    return;

                //determine element class and color class
                DetermineElement(element, out var elementClass, out var elementColorClass, out var applyToChildren);

                //apply theme
                ApplyThemeToElement(element, elementClass, elementColorClass, theme);

                //done!
            }
            catch (Exception e)
            {
                Debug.LogError($"[ThemeEngine] Error in outer ApplyThemeToElement \"{(element.Ref()?.name ?? "null")}\" ({e.GetType().Name})");
                if (ConfigState.Instance.UseVerboseLogging)
                    Debug.LogException(e);
            }
        }

        private static void DetermineElement(Transform element, out ElementClass elementClass, out ElementColorClass elementColorClass, out bool applyToChildren)
        {
            var themableComponent = element.GetComponent<ThemableElement>();
            if (themableComponent != null)
            {
                if (themableComponent.OverrideClass)
                    elementClass = themableComponent.ClassOverride;
                else
                    elementClass = DetermineElementClass(element);

                if (themableComponent.ElementColor == ThemableElement.ThemableElementColor.Auto)
                    elementColorClass = DetermineElementColorClass(element, elementClass);
                else if (themableComponent.ElementColor == ThemableElement.ThemableElementColor.None)
                    elementColorClass = ElementColorClass.None;
                else
                    elementColorClass = themableComponent.ElementColor == ThemableElement.ThemableElementColor.Contrasting ? ElementColorClass.Contrasting : ElementColorClass.Normal;

                if (themableComponent.ThemeChildren == ThemableElement.ThemableElementChildOption.Auto)
                    applyToChildren = DetermineApplyToChildren(element, elementClass);
                else
                    applyToChildren = themableComponent.ThemeChildren == ThemableElement.ThemableElementChildOption.Apply;
            }
            else
            {
                elementClass = DetermineElementClass(element);
                elementColorClass = DetermineElementColorClass(element, elementClass);
                applyToChildren = DetermineApplyToChildren(element, elementClass);
            }
        }        

        private static ElementClass DetermineElementClass(Transform element)
        {
            var comboBox = element.GetComponent<Dropdown>();
            if(comboBox != null)
            {
                //combo box
                return ElementClass.ComboBox;
            }

            var inputField = element.GetComponent<InputField>();
            if(inputField != null)
            {
                //input field
                return ElementClass.InputField;
            }

            var slider = element.GetComponent<Slider>();
            if(slider != null)
            {
                //slider or bar
                if (slider.interactable)
                    return ElementClass.Slider;
                return ElementClass.Bar;
            }

            var button = element.GetComponent<Button>();
            if(button != null)
            {
                //button
                return ElementClass.Button;
            }

            var toggle = element.GetComponent<Toggle>();
            if(toggle != null)
            {
                //radio button or toggle button
                if (toggle.group != null)
                    return ElementClass.RadioButton;
                return ElementClass.ToggleButton;
            }

            var text = element.GetComponent<Text>();
            if(text != null)
            {
                //must be a text type
                if (text.font != null && (text.fontStyle == FontStyle.Bold || text.font.name.IndexOf("bold", StringComparison.OrdinalIgnoreCase) >= 0) && text.fontSize > 20)
                    return ElementClass.HeadingText;
                return ElementClass.BodyText;
            }

            var image = element.GetComponent<Image>();
            if(image != null && image.sprite != null)
            {
                if (image.sprite.name.IndexOf("frame", StringComparison.OrdinalIgnoreCase) >= 0)
                    return ElementClass.Frame;
                else if (image.sprite.name.IndexOf("panel", StringComparison.OrdinalIgnoreCase) >= 0 || image.sprite.name.Equals("background", StringComparison.OrdinalIgnoreCase))
                    return ElementClass.Panel;
            }

            //containers have no graphics or behaviour, just an empty transform
            var graphic = element.GetComponent<Graphic>();
            var behaviour = element.GetComponent<Behaviour>();
            if (graphic == null && behaviour == null)
            {
                return ElementClass.Container;
            }

            //special case for toggle groups
            var toggleGroup = element.GetComponent<ToggleGroup>();
            if (toggleGroup != null)
                return ElementClass.Container;

            return ElementClass.Unknown;
        }

        private static ElementColorClass DetermineElementColorClass(Transform element, ElementClass elementClass)
        {
            if(elementClass == ElementClass.Button || elementClass == ElementClass.Frame || elementClass == ElementClass.Panel)
            {
                var image = element.GetComponent<Image>();
                if (image != null && image.sprite != null && image.sprite.name.IndexOf("dark", StringComparison.OrdinalIgnoreCase) >= 0)
                    return ElementColorClass.Contrasting;
            }

            if(elementClass == ElementClass.BodyText || elementClass == ElementClass.HeadingText || elementClass == ElementClass.MonospaceText)
            {
                var text = element.GetComponent<Text>();
                var color = text.color;
                if (color.grayscale >= 0.75f) //look for white text
                    return ElementColorClass.Contrasting;
            }

            return ElementColorClass.Normal;
        }

        private static bool DetermineApplyToChildren(Transform element, ElementClass elementClass)
        {
            switch (elementClass)
            {
                case ElementClass.Unknown:
                case ElementClass.Container:
                case ElementClass.Panel:
                case ElementClass.Frame:
                    return true;
                case ElementClass.HeadingText:
                case ElementClass.BodyText:
                case ElementClass.MonospaceText:
                case ElementClass.Button:
                case ElementClass.Slider:
                case ElementClass.Bar:
                case ElementClass.RadioButton:
                case ElementClass.ToggleButton:
                case ElementClass.ComboBox:
                case ElementClass.InputField:
                    return false;
                default:
                    if (ConfigState.Instance.UseVerboseLogging && !ConfigState.Instance.SuppressThemeWarnings)
                        Debug.LogWarning($"[ThemeEngine] Unknown element class {elementClass}");
                    return true;
            }
        }

        private static void ApplyThemeToElement(Transform element, ElementClass elementClass, ElementColorClass elementColorClass, UIThemeAsset theme)
        {
            try
            {
                switch (elementClass)
                {
                    case ElementClass.HeadingText:
                    case ElementClass.BodyText:
                    case ElementClass.MonospaceText:
                        {
                            var text = element.GetComponent<Text>();

                            switch (elementClass)
                            {
                                case ElementClass.HeadingText:
                                    text.font = theme.HeadingFont;
                                    break;
                                case ElementClass.BodyText:
                                    text.font = theme.BodyFont;
                                    break;
                                case ElementClass.MonospaceText:
                                    text.font = theme.MonospaceFont;
                                    break;
                            }

                            switch (elementColorClass)
                            {
                                case ElementColorClass.Normal:
                                    text.color = theme.TextColor;
                                    break;
                                case ElementColorClass.Contrasting:
                                    text.color = theme.TextContrastingColor;
                                    break;
                            }

                        }
                        break;
                    case ElementClass.Panel:
                        {
                            var image = element.GetComponent<Image>();

                            switch (elementColorClass)
                            {
                                case ElementColorClass.Normal:
                                    image.sprite = theme.Panel;
                                    break;
                                case ElementColorClass.Contrasting:
                                    image.sprite = theme.PanelContrasting;
                                    break;
                            }
                        }
                        break;
                    case ElementClass.Frame:
                        {
                            var image = element.GetComponent<Image>();

                            switch (elementColorClass)
                            {
                                case ElementColorClass.Normal:
                                    image.sprite = theme.Frame;
                                    break;
                                case ElementColorClass.Contrasting:
                                    image.sprite = theme.FrameContrasting;
                                    break;
                            }
                        }
                        break;
                    case ElementClass.Button:
                        {
                            //button is a little more complicated because we have to style the children also
                            var button = element.GetComponent<Button>();

                            Color textColor = default;
                            switch (elementColorClass)
                            {
                                case ElementColorClass.Normal:
                                    if (button.image != null)
                                        button.image.sprite = theme.Button;
                                    textColor = theme.TextColor;
                                    break;
                                case ElementColorClass.Contrasting:
                                    if (button.image != null)
                                        button.image.sprite = theme.ButtonContrasting;
                                    textColor = theme.TextContrastingColor;
                                    break;
                            }

                            var texts = element.GetComponentsInChildren<Text>(true);
                            foreach (Text text in texts)
                            {
                                text.font = theme.BodyFont;
                                text.color = textColor;
                            }
                        }
                        break;
                    case ElementClass.Slider:
                        {
                            var slider = element.GetComponent<Slider>();
                            var backgroundElement = element.Find("Background").Ref() ?? element.Find("background");
                            if (backgroundElement != null)
                            {
                                var backgroundImage = backgroundElement.GetComponent<Image>();
                                if (backgroundImage != null)
                                    backgroundImage.sprite = theme.Slider;
                            }

                            var fill = slider.fillRect.Ref()?.GetComponentInChildren<Image>();
                            if (fill != null)
                                fill.sprite = theme.SliderFill;

                            var handle = slider.handleRect.Ref()?.GetComponentInChildren<Image>();
                            if (handle != null)
                                handle.sprite = theme.SliderHandle;
                        }
                        break;
                    case ElementClass.Bar:
                        {
                            var slider = element.GetComponent<Slider>();

                            if (element.childCount > 0)
                            {
                                var backgroundElement = element.Find("Background").Ref() ?? element.Find("background").Ref() ?? element.GetChild(0);
                                if (backgroundElement != null)
                                {
                                    var backgroundImage = backgroundElement.GetComponent<Image>();
                                    if (backgroundImage != null)
                                        backgroundImage.sprite = theme.Bar;
                                }
                            }

                            var fill = slider.fillRect.Ref()?.GetComponentInChildren<Image>();
                            if (fill != null)
                                fill.sprite = theme.BarFill;
                        }
                        break;
                    case ElementClass.RadioButton:
                        {
                            var toggle = element.GetComponent<Toggle>();
                            if (toggle.targetGraphic.Ref() is Image targetGraphicImage)
                            {
                                targetGraphicImage.sprite = theme.RadioButton;
                            }
                            else
                            {
                                var backgroundElement = element.Find("Background").Ref() ?? element.Find("background");
                                if (backgroundElement != null)
                                {
                                    var backgroundImage = backgroundElement.GetComponent<Image>();
                                    if (backgroundImage != null)
                                        backgroundImage.sprite = theme.RadioButton;
                                }
                            }

                            if (toggle.graphic != null)
                            {
                                if (toggle.graphic is Image graphicImage)
                                {
                                    graphicImage.sprite = theme.RadioButtonCheck;
                                }
                            }
                        }
                        break;
                    case ElementClass.ToggleButton:
                        {
                            var toggle = element.GetComponent<Toggle>();
                            if (toggle.targetGraphic.Ref() is Image targetGraphicImage)
                            {
                                targetGraphicImage.sprite = theme.ToggleButton;
                            }
                            else
                            {
                                var backgroundElement = element.Find("Background").Ref() ?? element.Find("background");
                                if (backgroundElement != null)
                                {
                                    var backgroundImage = backgroundElement.GetComponent<Image>();
                                    if (backgroundImage != null)
                                        backgroundImage.sprite = theme.ToggleButton;
                                }
                            }

                            if (toggle.graphic != null)
                            {
                                if (toggle.graphic is Image graphicImage)
                                {
                                    graphicImage.sprite = theme.ToggleButtonCheck;
                                }
                            }

                            var labelElement = element.Find("Label").Ref() ?? element.Find("label");
                            if(labelElement != null)
                            {
                                var labelText = labelElement.GetComponentInChildren<Text>();
                                if (labelText != null)
                                    labelText.font = theme.BodyFont;
                            }
                        }
                        break;
                    case ElementClass.ComboBox:
                        {
                            var dropdown = element.GetComponent<Dropdown>();

                            if (dropdown.captionText)
                                dropdown.captionText.font = theme.BodyFont;

                            if (dropdown.itemText)
                                dropdown.itemText.font = theme.BodyFont;

                            var backgroundImage = element.GetComponent<Image>();
                            if (backgroundImage != null)
                            {
                                backgroundImage.sprite = theme.ComboBox;
                            }

                            var templateElement = dropdown.template;
                            if (templateElement)
                            {
                                var templateImage = templateElement.GetComponent<Image>();
                                if (templateImage)
                                    templateImage.sprite = theme.ComboBoxList;
                            }

                            var arrowElement = element.Find("Arrow").Ref() ?? element.Find("arrow");
                            if (arrowElement != null)
                            {
                                var arrowImage = arrowElement.GetComponent<Image>();
                                if (arrowImage != null)
                                    arrowImage.sprite = theme.ComboBoxArrow;

                                var arrowText = arrowElement.GetComponent<Text>();
                                if (arrowText != null)
                                    arrowText.font = theme.MonospaceFont;
                            }
                        }
                        break;
                    case ElementClass.InputField:
                        {
                            var inputfield = element.GetComponent<InputField>();

                            var backgroundImage = element.GetComponent<Image>();
                            if (backgroundImage != null)
                                backgroundImage.sprite = theme.InputField;

                            if (inputfield.placeholder.Ref() is Text inputfieldPlaceholderText)
                                inputfieldPlaceholderText.font = theme.BodyFont;

                            if (inputfield.textComponent)
                                inputfield.textComponent.font = theme.BodyFont;
                        }
                        break;
                    case ElementClass.Container:
                        //nop
                        break;
                    default:
                        if (ConfigState.Instance.UseVerboseLogging && !ConfigState.Instance.SuppressThemeWarnings)
                            Debug.LogWarning($"[ThemeEngine] Failed to apply theme to element \"{element.name}\" (unknown class)");
                        break;
                }
            }
            catch(Exception e)
            {
                Debug.LogError($"[ThemeEngine] Error applying theme to element \"{(element.Ref()?.name ?? "null")}\" ({e.GetType().Name})");
                if(ConfigState.Instance.UseVerboseLogging)
                    Debug.LogException(e);
            }

        }



    }

    [Serializable]
    public enum ElementClass
    {
        Unknown,
        Container,
        HeadingText,
        BodyText,
        MonospaceText,
        Panel,
        Frame,
        Button,
        Slider,
        Bar,
        RadioButton,
        ToggleButton,
        ComboBox,
        InputField
    }

    [Serializable]
    public enum ElementColorClass
    {
        Normal,
        Contrasting,
        None
    }
}