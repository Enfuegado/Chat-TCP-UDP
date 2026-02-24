using System;
using System.Collections.Concurrent;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly ConcurrentQueue<Action> actions = new();

    public static void Enqueue(Action action)
    {
        actions.Enqueue(action);
    }

    void Update()
    {
        while (actions.TryDequeue(out var action))
        {
            action.Invoke();
        }
    }
}