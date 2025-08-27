using UnityEngine;
using Sfs2X;

public class DebugOverlay : MonoBehaviour
{
    public SmartFox sfs;
    public bool topLeft = true;
    float dt;

    void Update() { dt += (Time.unscaledDeltaTime - dt) * 0.1f; }

    void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 12 };
        var rect = new Rect(topLeft ? 10 : Screen.width - 210, 10, 200, 80);
        int fps = (int)(1f / Mathf.Max(dt, 0.0001f));
        string room = (sfs != null && sfs.LastJoinedRoom != null) ? sfs.LastJoinedRoom.Name : "-";
        int users = (sfs != null && sfs.LastJoinedRoom != null) ? sfs.LastJoinedRoom.UserList.Count : 0;

        GUI.Box(rect, $"FPS: {fps}\nConn: {(sfs != null && sfs.IsConnected ? "Yes" : "No")}\nRoom: {room}\nUsers: {users}", style);
    }
}
