using CommonCore.Async;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CommonCore.XSMP
{
    public class XSMPServerWrapper : IDisposable
    {
        private CancellationTokenSource CurrentTokenSource;
        private Task CurrentTask;
        private Process ServerProcess;

        public XSMPServerWrapper()
        {
            CurrentTokenSource = new CancellationTokenSource();
            CurrentTask = Task.Run(() => StartServer(CurrentTokenSource.Token));
            AsyncUtils.RunWithExceptionHandling(async () => await CurrentTask);
        }

        private void StartServer(CancellationToken token)
        {            
            token.ThrowIfCancellationRequested();
            bool serverAlreadyRunning = CheckIfProcessExists();
            if(serverAlreadyRunning)
            {
                UnityEngine.Debug.Log("[XSMP] XSMP server process is already running!");
                return;
            }

            string serverPath = GetServerPath();

            token.ThrowIfCancellationRequested();

            ServerProcess = new Process();
            ServerProcess.StartInfo.FileName = serverPath;
            ServerProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(serverPath);
            ServerProcess.StartInfo.CreateNoWindow = true; //?

            token.ThrowIfCancellationRequested();

            ServerProcess.Start();
        }

        private bool CheckIfProcessExists()
        {
            var processes = Process.GetProcessesByName("xsmp");
            if (processes.Length > 0)
                return true;

            return false;
        }

        private string GetServerPath()
        {
            return XSMPModule.Config.ServerPath;
        }

        public void Dispose()
        {
            try
            {
                if (CurrentTask != null && !CurrentTask.IsCompleted)
                {
                    CurrentTokenSource.Cancel();
                    CurrentTask.Wait();
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError("[XSMP] Server wrapper task could not be aborted!");
                UnityEngine.Debug.LogException(e);
            }

            try
            {
                if (ServerProcess != null && !ServerProcess.HasExited)
                {
                    ServerProcess.CloseMainWindow();
                    ServerProcess.WaitForExit(1000);
                    if (!ServerProcess.HasExited)
                    {
                        ServerProcess.Kill();
                        UnityEngine.Debug.LogWarning("[XSMP] Server process force killed!");
                    }
                    else
                    {
                        UnityEngine.Debug.Log("[XSMP] Server process exited successfully!");
                    }
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError("[XSMP] Server process could not be exited");
                UnityEngine.Debug.LogException(e);
            }
        }
    }
}