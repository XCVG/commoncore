using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    }
}