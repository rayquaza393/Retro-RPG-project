using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginSceneController_RRPG : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text statusText;
    public TMP_Text errorText;  // NEW: Separate error display
    public string nextScene = "World01";

    private void Start()
    {
        NetworkAPI.Instance.Connect();

        // Register event listeners
        NetworkAPI.Instance.On("login.success", OnLoginSuccess);
        NetworkAPI.Instance.On("login.failed", OnLoginFailed);
        NetworkAPI.Instance.On("room.join.success", OnRoomJoinSuccess);
        NetworkAPI.Instance.On("room.join.failed", OnRoomJoinFailed);
        NetworkAPI.Instance.On("ConnectionError", OnConnectionError);
    }

    public void OnLoginButtonClick()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("Missing username or password.");
            return;
        }

        UpdateStatus("[LOGIN STATUS] Sending login request...");

        var loginData = new Dictionary<string, object> {
            { "username", username },
            { "password", password },
            { "type", "PlayerLogin" }
        };

        NetworkAPI.Instance.Send("PlayerLogin", loginData);
    }

    private void OnLoginSuccess(Dictionary<string, object> data)
    {
        int accountId = Convert.ToInt32(data["accountId"]);
        string username = data["username"].ToString();
        string zone = data["zone"].ToString();
        string room = data["room"].ToString();

        NetworkAPI.Instance.SetSession(accountId, username);
        NetworkAPI.Instance.SetZone(zone, room);

        UpdateStatus("[LOGIN STATUS] Login successful. Joining room...");

        var roomJoinData = new Dictionary<string, object> {
            { "room", room }
        };
        NetworkAPI.Instance.Send("player.joinRoom", roomJoinData);
    }

    private void OnLoginFailed(Dictionary<string, object> data)
    {
        string reason = data.ContainsKey("reason") ? data["reason"].ToString() : "Unknown error";
        ShowError($"Login failed: {reason}");
    }

    private void OnRoomJoinSuccess(Dictionary<string, object> data)
    {
        string room = data["room"].ToString();
        UpdateStatus($"[ROOM JOIN] Joined room: {room}. Loading world...");

        SceneManager.LoadScene(nextScene);
    }

    private void OnRoomJoinFailed(Dictionary<string, object> data)
    {
        string reason = data.ContainsKey("reason") ? data["reason"].ToString() : "Unknown";
        ShowError($"Room join failed: {reason}");
    }

    private void OnConnectionError(Dictionary<string, object> data)
    {
        string msg = data.ContainsKey("message") ? data["message"].ToString() : "Connection lost.";
        ShowError($"[CONNECTION ERROR] {msg}");
    }

    private void UpdateStatus(string message)
    {
        Debug.Log(message);

        if (errorText != null)
            errorText.text = ""; // Clear error when updating status

        if (statusText != null)
            statusText.text = message;
    }

    private void ShowError(string message)
    {
        Debug.LogWarning(message);

        if (statusText != null)
            statusText.text = ""; // Clear status when showing error

        if (errorText != null)
            errorText.text = message;
    }
}
