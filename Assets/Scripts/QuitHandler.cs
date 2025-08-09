using UnityEngine;

public class QuitHandler : MonoBehaviour
{
    public void QuitServer()
    {
        Debug.Log("Quitting server...");
        Application.Quit();
    }
}
