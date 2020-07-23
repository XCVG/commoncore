using System.Threading.Tasks;

namespace CommonCore.AsyncLoadTestModule
{
    /// <summary>
    /// Dummy module for testing async loads (can be loaded asynchronously or synchronously)
    /// </summary>
    public class AsyncLoadTestModule : CCAsyncModule
    {
        public override bool CanLoadSynchronously => true;

        public override async Task LoadAsync()
        {
            Log("Wait...");
            await Task.Delay(10);
            Log("...done");
        }

        public override void Load()
        {
            Log("No wait, load synchronously!");
        }
    }
}