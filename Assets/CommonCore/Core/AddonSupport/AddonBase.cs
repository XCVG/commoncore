using System;
using System.Threading.Tasks;

namespace CommonCore
{
    public class AddonBase
    {
        public string LocalResourcePath { get; protected set; }
        protected AddonLoadContext InitialLoadContext { get; set; }

        public virtual async Task LoadAddonAsync(AddonLoadContext context)
        {
            InitialLoadContext = context;
            LocalResourcePath = context.MountPath;
            context.AddonManager.RegisterDefaultVideoPaths(context);
            await context.AddonManager.LoadAssembliesAsync(context);
            await context.AddonManager.LoadResourcesFromPathAsync(context);
            context.AddonManager.RegisterLoadedScenes(context);
            RunOnLoadedCallback(context);
        }

        protected void RunOnLoadedCallback(AddonLoadContext context)
        {
            context.OnLoadedCallback(new AddonLoadData(context.LoadedAssemblies, context.LoadedResources, context.VideoPaths));
        }
    }
}