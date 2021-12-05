using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WaveLoader;

namespace CommonCore.ResourceManagement
{

    public interface IResourceImporter
    {
        bool CanLoadSync { get; }
        bool CanLoadResource(string path, ResourceLoadContext context);
        Type GetResourceType(string path, ResourceLoadContext context);

        object LoadResource(string path, Type target, ResourceLoadContext context);
        Task<object> LoadResourceAsync(string path, Type target, ResourceLoadContext context);
        //for now, only files
        //(because if we use byte[] here then we also have to handle that for CanLoadResource etc)
       // object LoadResource(byte[] data, Type target, ResourceLoadContext context);
       // Task<object> LoadResourceAsync(byte[] data, Type target, ResourceLoadContext context);

    }

    public class ResourceLoadContext //be careful with lifetime- should only exist for the length of a single resource load operation!
    {
        public string TargetPath; //the path we wish to load into (if known)
        public ResourceLoader ResourceLoader;
        public ResourceManager ResourceManager;

        public bool AttemptingSyncLoad;

        public Type ResourceType;

        public IResourceImporter ResourceImporter;
        public object ResourceImporterData;
        
    }

    public abstract class ResourceLoadException : Exception
    {
        public ResourceLoadException(ResourceLoadContext context, Exception innerException) : base(string.Empty, innerException)
        {
            Context = context;
        }

        protected ResourceLoadContext Context { get; }
    }

    public class NoImporterFoundException : ResourceLoadException
    {
        public NoImporterFoundException(ResourceLoadContext context) : base(context, null)
        {

        }

        public override string Message => $"Failed to find an importer for \"{Context.TargetPath}\" ({Context.ResourceType})";
    }

    public class ImporterFailedException : ResourceLoadException
    {
        public ImporterFailedException(ResourceLoadContext context, Exception innerException) : base(context, innerException)
        {
        }

        public override string Message => $"Importer {Context.ResourceImporter?.GetType()?.Name} failed to import resource \"{Context.TargetPath}\" ({InnerException?.GetType()?.Name})";
    }


}