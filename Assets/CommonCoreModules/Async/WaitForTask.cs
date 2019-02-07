using System;
using System.Threading.Tasks;
using UnityEngine;

// Based on code from the Xbox Live Unity Plugin
// Copyright (c) Microsoft Corporation, Chris Leclair
// Licensed under the MIT license. See MSLICENSE file in the module folder for full license information.

namespace CommonCore.Async
{
    /// <summary>
    /// Suspends the coroutine execution until the Task is complete
    /// </summary>
    public class WaitForTask : CustomYieldInstruction
    {
        private bool TaskComplete;
        private bool ThrowExceptions;

        public Task Task { get; protected set; }

        /// <summary>
        /// The exception thrown by the Task on failure
        /// </summary>
        public Exception Exception { get; protected set; }

        /// <summary>
        /// Suspends the coroutine execution until the Task is complete. Will throw into the aether on failure.
        /// </summary>
        /// <param name="task">The Task to wait for</param>
        public WaitForTask(Task task) : this(task, true)
        {
        }

        /// <summary>
        /// Suspends the coroutine execution until the Task is complete.
        /// </summary>
        /// <param name="task">The Task to wait for</param>
        /// <param name="throwExceptions">Whether to throw exceptions into the aether or to swallow and save them</param>
        public WaitForTask(Task task, bool throwExceptions)
        {
            Task = task ?? throw new ArgumentNullException("task");

            ThrowExceptions = throwExceptions;

            if (task.IsCompleted)
            {
                TaskComplete = true;
                return;
            }

            // If the task is not complete yet, we need to attach a
            // continuation to mark the task as complete.
            task.ContinueWith(t => TaskComplete = true);
        }

        public override bool keepWaiting
        {
            get
            {
                if (!TaskComplete)
                {
                    return true;
                }

                // If the task has completed, but completes with an error
                // this will force the exception to be thrown so that the 
                // coroutine code will at least log it somewhere which 
                // should prevent stuff from getting lost.
                if (Task.Exception != null)
                {
                    if (ThrowExceptions)
                        throw Task.Exception;

                    //otherwise, save it for safekeeping
                    Exception = Task.Exception;
                    return false;
                }

                return false;
            }
        }

    }
    
    /// <summary>
    /// Suspends the coroutine execution until the Task is complete, and holds its result.
    /// </summary>
    public class WaitForTask<TResult> : WaitForTask
    {

        /// <summary>
        /// Suspends the coroutine execution until the Task is complete. Will throw into the aether on failure.
        /// </summary>
        /// <param name="task">The Task to wait for</param>
        public WaitForTask(Task<TResult> task) : base(task)
        {
        }

        /// <summary>
        /// Suspends the coroutine execution until the Task is complete.
        /// </summary>
        /// <param name="task">The Task to wait for</param>
        /// <param name="throwExceptions">Whether to throw exceptions into the aether or to swallow and save them</param>
        public WaitForTask(Task<TResult> task, bool throwExceptions) : base(task, throwExceptions)
        {
        }

        public new Task<TResult> Task
        {
            get => base.Task as Task<TResult>;
            protected set => base.Task = value;
        }

        /// <summary>
        /// The result of the Task execution
        /// </summary>
        public TResult Result => Task.Result;
    }
}