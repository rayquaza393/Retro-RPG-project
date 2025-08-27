using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _instance;

    public static NetworkManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("NetworkManager");
                _instance = obj.AddComponent<NetworkManager>();
            }
            return _instance;
        }
    }

    [Header("KeepAlive")]
    public float keepAliveInterval = 15f;
    private Coroutine keepAliveCoroutine;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[NetworkManager] Initialized and persistent.");
    }

    private void Start()
    {
        keepAliveCoroutine = StartCoroutine(KeepAliveLoop());
    }

    private void Update()
    {
        NetworkAPI.Instance.ProcessMessages();
    }

    private IEnumerator KeepAliveLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(keepAliveInterval);

            if (IsConnected())
            {
                NetworkAPI.Instance.Send("KeepAlive", new Dictionary<string, object>());
#if UNITY_EDITOR
                Debug.Log("[NetworkManager] Sent keepalive packet.");
#endif
            }
        }
    }

    private bool IsConnected()
    {
        return NetworkAPI.Instance != null && NetworkAPI.Instance.AccountId != -1;
    }

    private void OnApplicationQuit()
    {
        NetworkAPI.Instance?.Disconnect();
    }
}
