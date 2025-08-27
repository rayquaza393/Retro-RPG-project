using System.Collections.Generic;
using UnityEngine;

public static class RemoteEntityRegistry
{
    private static readonly Dictionary<int, RemoteEntity> entities = new Dictionary<int, RemoteEntity>();

    public static void Register(int id, RemoteEntity entity)
    {
        if (entities.ContainsKey(id))
        {
            Debug.LogWarning($"[RemoteEntityRegistry] ID {id} already registered. Overwriting.");
            entities[id] = entity;
        }
        else
        {
            entities.Add(id, entity);
        }
    }

    public static void Unregister(int id)
    {
        if (entities.ContainsKey(id))
            entities.Remove(id);
    }

    public static RemoteEntity Get(int id)
    {
        return entities.TryGetValue(id, out var entity) ? entity : null;
    }

    public static void ClearAll()
    {
        entities.Clear();
    }

    public static bool IsRegistered(int id)
    {
        return entities.ContainsKey(id);
    }

    public static IReadOnlyDictionary<int, RemoteEntity> All => entities;
}
