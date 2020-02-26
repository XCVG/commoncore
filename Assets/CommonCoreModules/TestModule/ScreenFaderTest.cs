using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.Scripting;
using CommonCore.Async;
using System.Threading.Tasks;

namespace CommonCore.TestModule
{

    public class ScreenFaderTest
    {

        [Command(alias = "Run", className = "ScreenFaderTest", useClassName = true)]
        public static void TestScreenFader()
        {
            AsyncUtils.RunWithExceptionHandling(async () =>
            {
                ScreenFader.FadeTo(Color.white, 5.0f, true, true, true);

                await Task.Delay(6000);
                AsyncUtils.ThrowIfEditorStopped();

                SharedUtils.ChangeScene("TestScene");

                await Task.Delay(5000);
                AsyncUtils.ThrowIfEditorStopped();

                ScreenFader.FadeFrom(null, 1.0f, false, true, true);

                await Task.Delay(5000);
                AsyncUtils.ThrowIfEditorStopped();

                ScreenFader.Crossfade(Color.blue, Color.red, 5.0f, false, false, false);

                await Task.Delay(3000);
                AsyncUtils.ThrowIfEditorStopped();

                ScreenFader.ClearFade();

                await Task.Delay(5000);
                AsyncUtils.ThrowIfEditorStopped();

                ScreenFader.Crossfade(Color.blue, Color.red, 5.0f, false, false, false);

                await Task.Delay(1000);
                AsyncUtils.ThrowIfEditorStopped();

                SharedUtils.ChangeScene("TestScene");

                await Task.Delay(5000);
                AsyncUtils.ThrowIfEditorStopped();

                ScreenFader.FadeTo(Color.black, 1.0f, true, false, true);

                await Task.Delay(1500);
                AsyncUtils.ThrowIfEditorStopped();

                SharedUtils.EndGame();

            });
        }
    }
}