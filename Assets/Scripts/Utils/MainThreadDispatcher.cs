using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Queues actions to run on the main Unity thread. Safe to use from any thread.
/// </summary>
public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> actions = new Queue<Action>();
    private static MainThreadDispatcher instance;

    public static void Enqueue(Action action)
    {
        if (action == null) return;

        lock (actions)
        {
            actions.Enqueue(action);
        }
    }

    private void Update()
    {
        lock (actions)
        {
            while (actions.Count > 0)
            {
                actions.Dequeue()?.Invoke();
            }
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (instance != null) return;

        var go = new GameObject("MainThreadDispatcher");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<MainThreadDispatcher>();
    }
}
