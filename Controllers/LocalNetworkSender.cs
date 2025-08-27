using UnityEngine;
//using Sfs2X;
//using Sfs2X.Entities.Data;
//using Sfs2X.Requests;
using System;


//public class LocalNetworkSender : MonoBehaviour
{
    public float sendRate = 0.05f;
    public Transform target;
    public Animator animator;

    private float lastSentTime = 0f;
    //private SmartFox sfs;

    public void Init(SmartFox smartFoxConn, Transform playerTransform, Animator playerAnimator)
    {
        sfs = smartFoxConn;
        target = playerTransform;
        animator = playerAnimator;
    }

    void Update()
    {
        if (sfs == null || !sfs.IsConnected || target == null)
            return;

        if (Time.time - lastSentTime >= sendRate)
        {
            SendPose();
            lastSentTime = Time.time;
        }
    }

    void SendPose()
    {
        var data = new SFSObject();

        Vector3 pos = target.position;
        float yaw = target.eulerAngles.y;
        float speed = animator != null ? animator.GetFloat("Speed") : 0f;

        data.PutFloat("x", pos.x);
        data.PutFloat("z", pos.z);
        data.PutFloat("yaw", yaw);
        data.PutFloat("speed", speed);
        data.PutLong("t", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

        sfs.Send(new ExtensionRequest("player.pos", data));
    }
}
