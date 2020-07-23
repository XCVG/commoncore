using System.Threading.Tasks;

namespace CommonCore.AsyncOnlyLoadTestModule
{
    /// <summary>
    /// Dummy module for testing async loads (can ONLY be loaded asynchronously)
    /// </summary>
    public class AsyncOnlyLoadTestModule : CCAsyncModule
    {
        public override bool CanLoadSynchronously => false;

        public override async Task LoadAsync()
        {
            Log("Wait...");
            await Task.Delay(10);
            Log("...done");
        }

    }
}