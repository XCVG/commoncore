using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.Scripting;
using System;
using UnityEngine.Rendering;

namespace CommonCore
{

    /// <summary>
    /// Handles distributing camera events to 
    /// </summary>
    public static class CameraEventsManager
    {
        private const int MinDeadHandlersToCull = 10;

        private static List<Action<Camera>> OnPreCullHandlers = new List<Action<Camera>>();
        private static int DeadOnPreCullHandlers;

        [CCScript, CCScriptHook(AllowExplicitCalls = false, Hook = ScriptHook.AfterAddonsLoaded)]
        private static void Setup()
        {
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                RenderPipelineManager.beginCameraRendering += ((srp, cam) => ExecuteOnPreCull(cam));
            }
            
        }

        [CCScript, CCScriptHook(AllowExplicitCalls = false, Hook = ScriptHook.OnSceneUnload)]
        private static void HandleSceneExit()
        {
            ClearLists();
        }

        public static void ExecuteOnPreCull(Camera camera)
        {
            foreach(var handler in OnPreCullHandlers)
            {
                if(handler?.Target.Ref() == null)
                {
                    DeadOnPreCullHandlers++;
                }
                else
                {
                    handler(camera);
                }
            }

            if (DeadOnPreCullHandlers > MinDeadHandlersToCull && DeadOnPreCullHandlers > OnPreCullHandlers.Count)
                CleanOnPreCullList();
        }

        public static void RegisterOnPreCull(Action<Camera> action)
        {
            OnPreCullHandlers.Add(action);
        }

        private static void CleanOnPreCullList()
        {
            for(int i = OnPreCullHandlers.Count - 1; i >= 0; i--)
            {
                var handler = OnPreCullHandlers[i];
                if (handler?.Target?.Ref() == null)
                    OnPreCullHandlers.RemoveAt(i);
            }
        }

        //empty all lists of delegates
        private static void ClearLists()
        {
            OnPreCullHandlers.Clear();
            DeadOnPreCullHandlers = 0;
        }

        //garbage-collect nulls out of lists
        private static void CleanLists()
        {
            CleanOnPreCullList();
        }

        
    }

}