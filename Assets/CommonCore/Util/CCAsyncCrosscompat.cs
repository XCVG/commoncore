/*
*            DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
*                    Version 2, December 2004
*
* Copyright (C) 2018 Chris Leclair <chris@xcvgsystems.com>
*
* Everyone is permitted to copy and distribute verbatim or modified
* copies of this license document, and changing it is allowed as long
* as the name is changed.
*
*            DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
*   TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
*
*   0. You just DO WHAT THE FUCK YOU WANT TO.
 */

using System.Collections;
using System.Threading.Tasks;

//no idea if this will work at all lol
public class WaitForTask : IEnumerator
{
    private Task MyTask;

    public WaitForTask(Task task)
    {
        MyTask = task;
    }

    public object Current
    {
        get
        {
            return null;
        }
    }

    public bool MoveNext()
    {
        if (MyTask.IsFaulted)
            throw MyTask.Exception;

        return !MyTask.IsCompleted;
    }

    public void Reset()
    {
        throw new System.NotImplementedException();
    }
}

public static class CCAsyncCrosscompat
{
    public static Task AsTask(this IEnumerator coroutine)
    {
        return new Task(() =>
        {
            while (coroutine.MoveNext())
                Task.Yield();
        });
    }
}
