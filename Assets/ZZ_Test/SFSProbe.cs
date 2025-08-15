using UnityEngine;
using Sfs2X;

public class SFSProbe : MonoBehaviour
{
    void Start()
    {
        var s = new SmartFox(true);
        Debug.Log("SFS OK: " + s.Version);
        Debug.Log(s.Version);
    }
}
