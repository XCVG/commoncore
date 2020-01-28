using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.Messaging;
using CommonCore.DebugLog;

namespace CommonCore.LockPause
{
    public enum InputLockType
    {
        All, GameOnly, MoveOnly
    }
    
    public enum PauseLockType
    {
        All, AllowMenu
    }

    public class InputLock
    {
        public readonly InputLockType Type;
        public readonly object Owner;

        public InputLock(InputLockType type, object owner)
        {
            Type = type;
            Owner = owner;
        }

        public override string ToString()
        {
            return string.Format("{0} [{2}:{1}]", Type.ToString(), Owner.ToString(), Owner.GetType().Name);
        }
    }

    public class PauseLock
    {
        public readonly PauseLockType Type;
        public readonly object Owner;

        public PauseLock(PauseLockType type, object owner)
        {
            Type = type;
            Owner = owner;
        }

        public override string ToString()
        {
            return string.Format("{0} [{2}:{1}]", Type.ToString(), Owner.ToString(), Owner.GetType().Name);
        }

    }

    public partial class LockPauseModule : CCModule
    {

        private static LockPauseModule Instance;

        private List<InputLock> InputLocks;
        private List<PauseLock> PauseLocks;

        private InputLockType? InputLockState;
        private PauseLockType? PauseLockState;

        public LockPauseModule() : base()
        {
            Instance = this;
            InputLocks = new List<InputLock>();
            PauseLocks = new List<PauseLock>();

            Log("Pause module started!");
        }

        public override void OnSceneUnloaded()
        {
            //clear all locks
            ClearAll();
        }

        public override void Dispose()
        {
            Instance = null;
        }

        private void ClearAll()
        {
            EnableMouseCapture = false;
            InputLocks.Clear();
            PauseLocks.Clear();

            DoUnlock();
            DoUnpause();
            DoUncapture();
        }

        private void AddInputLock(InputLock iLock)
        {
            if(InputLockState == null || iLock.Type < InputLockState.Value)
            {
                ApplyInputLock(iLock.Type);
            }

            InputLocks.Add(iLock);
        }

        private void RemoveInputLock(object iLockObject)
        {
            InputLocks.RemoveAll(iLock => iLock.Owner == iLockObject);

            if (InputLocks.Count > 0)
                CleanInputLocks();
            else
                DoUnlock();
        }

        private void RemoveInputLock(InputLock iLock)
        {
            InputLocks.Remove(iLock);

            if (InputLocks.Count > 0)
                CleanInputLocks();
            else
                DoUnlock();
        }

        private void CleanInputLocks()
        {
            //remove "dead" locks
            int lowestLockState = int.MaxValue;
            for(int i = InputLocks.Count-1; i >= 0; i--)
            {
                InputLock iLock = InputLocks[i];
                if(IsOwnerAlive(iLock.Owner))
                {
                    //try lowest lock
                    if ((int)iLock.Type < lowestLockState)
                        lowestLockState = (int)iLock.Type;
                }
                else
                {
                    //remove it
                    InputLocks.RemoveAt(i);
                }
            }

            //drop to new lowest lock state
            if(lowestLockState < int.MaxValue)
            {
                ApplyInputLock((InputLockType)lowestLockState);
            }
            else
            {
                DoUnlock();
            }
        }

        private void ApplyInputLock(InputLockType newState)
        {
            if (QdmsMessageBus.Instance != null) //it's possible for this to run after the message bus is destroyed
                QdmsMessageBus.Instance.PushBroadcast(new InputLockMessage(newState));

            InputLockState = newState;            
        }

        private void AddPauseLock(PauseLock pLock)
        {
            if (PauseLockState == null || pLock.Type < PauseLockState.Value)
            {
                ApplyPauseLock(pLock.Type);
            }

            PauseLocks.Add(pLock);
        }

        private void RemovePauseLock(object pLockObject)
        {
            PauseLocks.RemoveAll(pLock => pLock.Owner == pLockObject);

            if (PauseLocks.Count > 0)
                CleanPauseLocks();
            else
                DoUnpause();
        }

        private void RemovePauseLock(PauseLock pLock)
        {
            PauseLocks.Remove(pLock);

            if (PauseLocks.Count > 0)
                CleanPauseLocks();
            else
                DoUnpause();
        }

        private void CleanPauseLocks()
        {
            //remove "dead" locks
            int lowestLockState = int.MaxValue;
            for (int i = PauseLocks.Count - 1; i >= 0; i--)
            {
                PauseLock pLock = PauseLocks[i];
                if (IsOwnerAlive(pLock.Owner))
                {
                    //try lowest lock
                    if ((int)pLock.Type < lowestLockState)
                        lowestLockState = (int)pLock.Type;
                }
                else
                {
                    //remove it
                    PauseLocks.RemoveAt(i);
                }
            }

            //drop to new lowest lock state
            if (lowestLockState < int.MaxValue)
            {
                ApplyPauseLock((PauseLockType)lowestLockState);
            }
            else
            {
                DoUnpause();
            }
        }

        private void ApplyPauseLock(PauseLockType newState)
        {
            //stop time (not all that failsafe, it turns out)
            Time.timeScale = 0;
            AudioListener.pause = true;

            //uncapture mouse
            DoUncapture();

            //send message and set state
            if(QdmsMessageBus.Instance != null) //it's possible for this to run after the message bus is destroyed
                QdmsMessageBus.Instance.PushBroadcast(new PauseLockMessage(newState));

            PauseLockState = newState;
        }

        private bool IsOwnerAlive(object owner)
        {
            if (owner is UnityEngine.Object && (UnityEngine.Object)owner == null) //fuck you, Unity
                return false;

            if (owner == null)
                return false;

            if (owner is WeakReference && !((WeakReference)owner).IsAlive)
                return false;

            return true;
        }

        private void DoUnlock()
        {
            InputLockState = null;

            QdmsMessageBus.Instance.PushBroadcast(new InputLockMessage(null));
        }

        private void DoUnpause()
        {
            PauseLockState = null;
            Time.timeScale = 1.0f;

            //recapture mouse (if applicable)
            if (EnableMouseCapture)
                DoCapture();

            AudioListener.pause = false;
            QdmsMessageBus.Instance.PushBroadcast(new PauseLockMessage(null));
        }

        private void DoUncapture()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void DoCapture()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private bool EnableMouseCapture;

        public static bool CaptureMouse
        {
            get
            {
                return Instance.EnableMouseCapture;
            }
            set
            {
                Instance.EnableMouseCapture = value;
                if (Instance.EnableMouseCapture && !IsPaused())
                    Instance.DoCapture();
                else
                    Instance.DoUncapture();
            }
        }

        public static InputLock LockControls(InputLockType type, object token)
        {
            var iLock = new InputLock(type, token);
            Instance.AddInputLock(iLock);
            return iLock;
        }

        public static void UnlockControls(object token)
        {
            if (token is InputLock)
                Instance.RemoveInputLock((InputLock)token);
            else
                Instance.RemoveInputLock(token);
        }

        public static bool IsInputLocked()
        {
            return (Instance.InputLockState != null && Instance.InputLockState.Value <= InputLockType.GameOnly);
        }

        public static InputLockType? GetInputLockState()
        {
            return Instance.InputLockState;
        }

        public static PauseLock PauseGame(object token)
        {
            return PauseGame(PauseLockType.AllowMenu, token);
        }

        public static PauseLock PauseGame(PauseLockType type, object token)
        {
            var pLock = new PauseLock(type, token);
            Instance.AddPauseLock(pLock);
            return pLock;
        }

        public static void UnpauseGame(object token)
        {
            if (token is PauseLock)
                Instance.RemovePauseLock((PauseLock)token);
            else
                Instance.RemovePauseLock(token);
        }

        public static bool IsPaused()
        {
            return (Instance.PauseLockState != null);
        }
    
        public static PauseLockType? GetPauseLockState()
        {
            return Instance.PauseLockState;
        }

        public static void ForceClearLocks()
        {
            CDebug.LogEx("Forced unlock and unpause!", LogLevel.Warning, Instance);
            Instance.ClearAll();
        }

        public static void ForceCleanLocks()
        {
            Instance.CleanInputLocks();
            Instance.CleanPauseLocks();
        }

    }

    //need to find a way to remove dependencies or something

    public class InputLockMessage : QdmsMessage
    {
        public readonly InputLockType? NewLockType;

        public InputLockMessage(InputLockType? lockType) : base()
        {
            NewLockType = lockType;
        }
    }

    public class PauseLockMessage : QdmsMessage
    {
        public readonly PauseLockType? NewLockType;

        public PauseLockMessage(PauseLockType? lockType) : base()
        {
            NewLockType = lockType;
        }
    }
}