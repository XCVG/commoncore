using CommonCore.Console;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CommonCore.UI
{

    /// <summary>
    /// Console commands for UI theming
    /// </summary>
    public static class UIConsoleCommands
    {
        //list themes
        [Command(alias = "PrintThemeList", className = "UI", useClassName = true)]
        private static void PrintThemeList()
        {
            ConsoleModule.WriteLine(CCBase.GetModule<UIModule>().GetThemesList());
        }

        //get current theme
        [Command(alias = "GetCurrentTheme", className = "UI", useClassName = true)]
        private static void GetCurrentTheme()
        {
            ConsoleModule.WriteLine(CCBase.GetModule<UIModule>().CurrentTheme.name);
        }

        //set current theme
        [Command(alias = "SetCurrentTheme", className = "UI", useClassName = true)]
        private static void SetCurrentTheme(string theme)
        {
            var uiModule = CCBase.GetModule<UIModule>();
            var uiTheme = uiModule.GetThemeByName(theme);

            if(uiTheme == null)
            {
                ConsoleModule.WriteLine($"Couldn't find theme \"{theme}\"");
                return;
            }

            uiModule.CurrentTheme = uiTheme;

            ConsoleModule.WriteLine($"Set theme to \"{theme}\". You will need to reload the UI to apply changes (usually can be done with Reload)");
        }



    }
}