using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TPC
{
    [RequireComponent(typeof(CharacterGravity))]
    [RequireComponent(typeof(CharacterManager))]
    [AddComponentMenu("Third Person Controller/Character Edge Fall")]
    [DisallowMultipleComponent()]
    public class CharacterEdgeFall : CharacterExtension
	{
        #region Inspector
        [Space(10)]
        public CharacterScriptableEdgeFall configScriptable;

        [Space(10)]
        public UnityEvent<bool> SetEdgeFall;
        #endregion

        #region Internal attributes and properties
        private Vector3 FloorDetectorCenter => transform.position + new Vector3(0, FloorRadius, 0);

        private List<Vector3> _floorDirections;
        #endregion

        #region Methods
        protected override bool CheckConditions()
        {
            _floorDirections = new List<Vector3>();
            bool ret = IsGrounded && !Physics.Raycast(FloorDetectorCenter, Vector3.down, FloorRadius * 2);
            if (!ret)
                SetEdgeFall?.Invoke(false);
            return ret;
        }

        protected override Type[] RecoverIncompatible() => incompatibleExtensionTypes;

        protected override void Action() => IsFalling();

        private void IsFalling()
        {
            float smallestAngle = 180;
            Vector3 smallestAnglePosition = Vector3.zero;
            RaycastHit[] hits = Physics.SphereCastAll(FloorDetectorCenter, FloorRadius, Vector3.down, FloorRadius);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject.transform.IsChildOf(transform))
                    break;
                Vector3 direction = new Vector3(hit.point.x - transform.position.x, 0, hit.point.z - transform.position.z);
                _floorDirections.Add(direction);
                float angle = Vector3.Angle(Vector3.down, direction);
                if (smallestAngle > angle)
                {
                    smallestAnglePosition = hit.point;
                    smallestAngle = angle;
                }
            }
            if (smallestAngle > configScriptable.SlideAngle && smallestAngle < 180)
            {
                NewVelocity = -(smallestAnglePosition - transform.position).normalized * configScriptable.SlideSpeed + new Vector3(0, NewVelocity.y, 0);
                SetEdgeFall?.Invoke(true);
            } else
                SetEdgeFall?.Invoke(false);
        }

        private void OnDrawGizmos()
        {
            if (thirdPersonPlayer == null || !IsGrounded || _floorDirections == null)
                return;
                        
            foreach (Vector3 floorDirection in _floorDirections)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, transform.position + floorDirection * 2);
                Gizmos.color = Color.blue;
                Vector3 vector = Quaternion.Euler(0, -90, 0) * floorDirection;
                Gizmos.DrawLine(transform.position, transform.position + vector);
            }
        }
        #endregion
    }
}