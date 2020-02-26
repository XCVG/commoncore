using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonCore;
using CommonCore.Async;
using System.Threading.Tasks;

namespace AsyncTestScene
{

    public class AsyncTestScript : MonoBehaviour
    {
        public Text TestText;

        public void HandleClickButtonTestCoroutine()
        {
            StartCoroutine(new WaitForTask(TestTask()));
        }

        private async Task TestTask()
        {
            TestText.text = "Beginning test Task... \n";

            for (int i = 0; i < 5; i++)
            {
                TestText.text = TestText.text + " " + i;
                await Task.Delay(1000);
            }

            TestText.text = TestText.text + "\n ...done!";
        }

        public async void HandleClickButtonTestAsync()
        {
            //await coroutine
            await TestCoroutine().AsTask();
        }

        private IEnumerator TestCoroutine()
        {
            TestText.text = "Beginning test coroutine... \n";

            for(int i = 0; i < 5; i++)
            {
                TestText.text = TestText.text + " " + i;
                yield return new WaitForSeconds(1);
            }

            TestText.text = TestText.text + "\n ...done!";
        }

        public async void HandleClickButtonTestDelayTask()
        {
            AsyncUtils.RunWithExceptionHandling(async () =>
            {
                TestText.text = "Before delays";

                await SkippableWait.DelayScaled(5f);

                TestText.text = "Done gametime delay";

                await SkippableWait.DelayRealtime(5f);

                TestText.text = "Done realtime delay";

                await Task.Delay(5000);

                TestText.text = "Done task.delay";

            });
        }

        public void HandleClickButtonTestDelay()
        {
            StartCoroutine(TestDelayCoroutine());
        }

        private IEnumerator TestDelayCoroutine()
        {
            TestText.text = "Before delays";

            yield return SkippableWait.WaitForSeconds(5f);

            TestText.text = "After gametime wait";

            yield return SkippableWait.WaitForSecondsRealtime(5f);

            TestText.text = "After realtime wait";

        }
    }
}