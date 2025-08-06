using UnityEngine;

namespace Feedback
{
	public class CharacterFeedback: MonoBehaviour
	{
		private const string GROUNDED = "Grounded";
		private const string EDGE_FALL = "EdgeFall";
		private const string FREE_FALL = "FreeFall";
		private const string JUMP = "Jump";
		private const string SPEED_FRONT = "SpeedFront";
		private const string SPEED_SIDE = "SpeedSide";
		private const string MOTION_SPEED = "MotionSpeed";

		private Animator _animator;

		// animation IDs
		private int _animIDGrounded;
		private int _animIDEdgeFall;
		private int _animIDFreeFall;
		private int _animIDJump;
		private int _animIDSpeedFront;
		private int _animIDSpeedSide;
		private int _animIDMotionSpeed;

		private void Awake() => _animator = GetComponent<Animator>();

        private void Start() => AssignAnimationIDs();

        private void AssignAnimationIDs()
		{
			_animIDGrounded = Animator.StringToHash(GROUNDED);
			_animIDEdgeFall = Animator.StringToHash(EDGE_FALL);
			_animIDFreeFall = Animator.StringToHash(FREE_FALL);
			_animIDJump = Animator.StringToHash(JUMP);
			_animIDSpeedFront = Animator.StringToHash(SPEED_FRONT);
			_animIDMotionSpeed = Animator.StringToHash(MOTION_SPEED);
			_animIDSpeedSide = Animator.StringToHash(SPEED_SIDE);
		}

		public void Grounded(bool grounded) => _animator.SetBool(_animIDGrounded, grounded);

		public void EdgeFall(bool edgeFall) => _animator.SetBool(_animIDEdgeFall, edgeFall);

		public void FreeFall(bool freeFall) => _animator.SetBool(_animIDFreeFall, freeFall);

		public void Jump() => _animator.SetTrigger(_animIDJump);

		public void DirectionalSpeed(float speedFront, float speedSide)
        {
			_animator.SetFloat(_animIDSpeedFront, speedFront);
			_animator.SetFloat(_animIDSpeedSide, speedSide);
		} 

		public void MotionSpeed(float motionSpeed) => _animator.SetFloat(_animIDMotionSpeed, motionSpeed);
    }
}