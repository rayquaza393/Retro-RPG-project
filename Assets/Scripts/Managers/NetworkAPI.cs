using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

public class NetworkAPI
{
    private static NetworkAPI _instance;
    public static NetworkAPI Instance => _instance ??= new NetworkAPI();

    private TcpClient client;
    private NetworkStream stream;
    private StreamReader reader;
    private StreamWriter writer;

    private Thread receiveThread;
    private readonly Queue<string> incomingMessages = new();
    private readonly Dictionary<string, Action<Dictionary<string, object>>> eventTable = new();

    public string ServerIP { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 9000;

    private bool connected = false;

    // Session Info
    public int AccountId { get; private set; } = -1;
    public string Username { get; private set; } = "";

    public string CurrentZone { get; private set; } = "";
    public string CurrentRoom { get; private set; } = "";

    // Internal message counter (for debug/monitoring)
    private int messageCount = 0;

    private NetworkAPI() { }

    public void Connect()
    {
        try
        {
            client = new TcpClient(ServerIP, Port);
            stream = client.GetStream();
            writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            reader = new StreamReader(stream, Encoding.UTF8);
            connected = true;

            Log("[NetworkAPI] Connected to server.");

            receiveThread = new Thread(ReceiveLoop)
            {
                IsBackground = true
            };
            receiveThread.Start();
        }
        catch (Exception ex)
        {
            LogError("[NetworkAPI] Connection failed: " + ex.Message);
            Emit("ConnectionError", new Dictionary<string, object> { { "message", ex.Message } });
        }
    }

    private void ReceiveLoop()
    {
        try
        {
            while (client != null && client.Connected)
            {
                string line = reader.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    messageCount++;
                    lock (incomingMessages)
                        incomingMessages.Enqueue(line);

                    Log($"[NetworkAPI] << Received #{messageCount}: {line}");
                }
            }
        }
        catch (Exception ex)
        {
            LogWarning("[NetworkAPI] Receive thread ended: " + ex.Message);
            OnConnectionLost("Receive loop stopped: " + ex.Message);
        }
    }

    public void ProcessMessages()
    {
        lock (incomingMessages)
        {
            while (incomingMessages.Count > 0)
            {
                string msg = incomingMessages.Dequeue();
                HandleMessage(msg);
            }
        }
    }

    public void Send(string type, Dictionary<string, object> data)
    {
        if (writer == null || !connected)
        {
            LogError("[NetworkAPI] Tried to send without a valid connection.");
            Emit("ConnectionError", new Dictionary<string, object> {
                { "message", "Client not connected." }
            });
            return;
        }

        // Debug - show outbound payload contents
        Log($"[NetworkAPI] Preparing to send: type='{type}'");
        foreach (var kv in data)
        {
            Log($"  ↳ key: {kv.Key}, value: {kv.Value}");
        }

        var payload = new Dictionary<string, object>(data);

        if (payload.ContainsKey("type"))
        {
            LogWarning($"[NetworkAPI] Payload already included 'type': {payload["type"]}. Overwriting with '{type}'.");
        }

        payload["type"] = type;

        try
        {
            string json = JsonConvert.SerializeObject(payload);

            Log($"[NetworkAPI] >> Sending JSON: {json}");
            writer.WriteLine(json);
        }
        catch (Exception ex)
        {
            LogError($"[NetworkAPI] Failed to serialize/send payload: {ex.Message}");
        }
    }

    public void On(string type, Action<Dictionary<string, object>> callback)
    {
        eventTable[type] = callback;
    }

    private void Emit(string type, Dictionary<string, object> data)
    {
        if (eventTable.TryGetValue(type, out var callback))
            callback(data);
    }

    private void HandleMessage(string json)
    {
        try
        {
            var parsed = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            if (parsed != null && parsed.ContainsKey("type"))
            {
                string type = parsed["type"].ToString();
                Log($"[NetworkAPI] Dispatching type: {type}");
                Emit(type, parsed);
            }
            else
            {
                LogWarning("[NetworkAPI] Message missing 'type': " + json);
            }
        }
        catch (Exception ex)
        {
            LogError("[NetworkAPI] Failed to parse message: " + ex.Message);
        }
    }

    public void Disconnect()
    {
        try { receiveThread?.Abort(); } catch { }

        writer?.Close();
        reader?.Close();
        stream?.Close();
        client?.Close();

        writer = null;
        reader = null;
        stream = null;
        client = null;

        connected = false;

        Log("[NetworkAPI] Disconnected.");
    }

    public void SetSession(int id, string username)
    {
        AccountId = id;
        Username = username;
    }

    public void SetZone(string zone, string room)
    {
        CurrentZone = zone;
        CurrentRoom = room;
    }

    public void ClearSession()
    {
        AccountId = -1;
        Username = "";
        CurrentZone = "";
        CurrentRoom = "";
    }

    public void OnConnectionLost(string reason)
    {
        LogWarning($"[NetworkAPI] Connection lost: {reason}");
        ClearSession();
        Emit("ConnectionLost", new Dictionary<string, object> {
            { "reason", reason }
        });
    }

    // Logging utilities
    private void Log(string msg) => Console.WriteLine(msg);
    private void LogWarning(string msg) => Console.WriteLine("[WARN] " + msg);
    private void LogError(string msg) => Console.WriteLine("[ERROR] " + msg);
}
