using UnityEngine;
using System.Collections.Generic;
using Sfs2X;
using Sfs2X.Requests;
using Sfs2X.Entities.Variables;

namespace SmartFoxServer.Unity.Examples
{
    public class LocalNetworkSender : MonoBehaviour
    {
        public float sendRate = 10f;          // Hz
        public float posThreshold = 0.02f;    // meters
        public float yawThreshold = 0.5f;     // degrees

        private SmartFox sfs;
        private float timer;
        private Vector3 lastPos;
        private float lastYaw;

        // Call this once right after you spawn the local player
        public void Init(SmartFox client)
        {
            sfs = client;
        }

        void Awake()
        {
            lastPos = transform.position;
            lastYaw = transform.eulerAngles.y;
        }

        void Update()
        {
            if (sfs == null || !sfs.IsConnected) return;

            timer += Time.deltaTime;
            if (timer < 1f / sendRate) return;
            timer = 0f;

            var p = transform.position;
            var y = transform.eulerAngles.y;

            if ((p - lastPos).sqrMagnitude > posThreshold * posThreshold ||
                Mathf.Abs(Mathf.DeltaAngle(y, lastYaw)) > yawThreshold)
            {
                lastPos = p;
                lastYaw = y;

                var vars = new List<UserVariable>
                {
                    new SFSUserVariable("px", (double)p.x),
                    new SFSUserVariable("py", (double)p.y),
                    new SFSUserVariable("pz", (double)p.z),
                    new SFSUserVariable("ry", (double)y)
                };
                sfs.Send(new SetUserVariablesRequest(vars));
            }
        }
    }
}
