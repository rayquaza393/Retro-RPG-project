using System.Collections.Generic;
using UnityEngine;
using Sfs2X;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;

public class SFSPositionBroadcaster : MonoBehaviour
{
    public float sendRateHz = 10f;
    float t; SmartFox sfs;
    void Start()
    {
        var gm = FindObjectOfType<SmartFoxServer.Unity.Examples.GlobalManager>();
        sfs = gm ? gm.CreateSfsClient() : null;
        if (sfs == null) Debug.LogWarning("[SFS] Broadcaster: no SmartFox client.");
    }
    void Update()
    {
        if (sfs == null || !sfs.IsConnected) return;
        t += Time.deltaTime;
        if (t >= 1f / Mathf.Max(1f, sendRateHz))
        {
            t = 0f;
            var p = transform.position; var ry = transform.eulerAngles.y;
            var vars = new List<UserVariable>{
                new SFSUserVariable("px",(double)p.x),
                new SFSUserVariable("py",(double)p.y),
                new SFSUserVariable("pz",(double)p.z),
                new SFSUserVariable("ry",(double)ry)
            };
            sfs.Send(new SetUserVariablesRequest(vars));
        }
    }
}
