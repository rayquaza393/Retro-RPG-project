using UnityEngine;
using Sfs2X;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;

namespace SmartFoxServer.Unity.Examples
{
    public class LocalNetworkSender : MonoBehaviour
    {
        [Header("Net Send")]
        public float sendRate = 10f;
        public float posThreshold = 0.02f;
        public float yawThreshold = 0.5f;
        public bool sendImmediatelyOnInit = true;

        private SmartFox sfs;
        private float timer;
        private Vector3 lastPos;
        private float lastYaw;
        private bool didInitialSend;

        public void Init(SmartFox client)
        {
            sfs = client;
            lastPos = transform.position;
            lastYaw = transform.eulerAngles.y;
            timer = 0f;
            didInitialSend = !sendImmediatelyOnInit ? true : false; // if true, we'll skip the forced send
        }

        void Awake()
        {
            lastPos = transform.position;
            lastYaw = transform.eulerAngles.y;
        }

        void Update()
        {
            if (sfs == null || !sfs.IsConnected) return;

            // Force one send right after spawn
            if (!didInitialSend)
            {
                SendNow();
                didInitialSend = true;
                return;
            }

            timer += Time.deltaTime;
            if (timer < 1f / sendRate) return;
            timer = 0f;

            Vector3 p = transform.position;
            float yaw = transform.eulerAngles.y;

            bool movedEnough = (p - lastPos).sqrMagnitude > posThreshold * posThreshold;
            bool turnedEnough = Mathf.Abs(Mathf.DeltaAngle(yaw, lastYaw)) > yawThreshold;
            if (!movedEnough && !turnedEnough) return;

            lastPos = p; lastYaw = yaw;
            Send(p, yaw);
        }

        private void SendNow()
        {
            Vector3 p = transform.position;
            float yaw = transform.eulerAngles.y;
            lastPos = p; lastYaw = yaw;
            Send(p, yaw);
        }

        private void Send(Vector3 p, float yaw)
        {
            var obj = new SFSObject();
            obj.PutFloat("x", p.x);
            obj.PutFloat("z", p.z);
            obj.PutFloat("yaw", yaw);
            // Zone-level request (no room param)
            sfs.Send(new ExtensionRequest("player.pos", obj));
        }
    }
}
