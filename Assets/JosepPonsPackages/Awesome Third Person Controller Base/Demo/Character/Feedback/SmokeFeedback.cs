using UnityEngine;

namespace Feedback
{
	public class SmokeFeedback : MonoBehaviour
	{
		public ParticleSystem[] smokeParticles;

		private float _previousSpeed, _previousSpeedX;
		private bool _isGrounded = true;
		public void PlaySmoke()
		{
			foreach (ParticleSystem particle in smokeParticles)
				particle.Play();
		}

		public void Grounded(bool grounded)
		{
			if (!_isGrounded && grounded)
				PlaySmoke();
			_isGrounded = grounded;
		}

		public void Jump() => PlaySmoke();

		public void DirectionalSpeed(float speedX, float speedY)
		{
			float speed = new Vector2(speedX, speedY).magnitude;

			if (speed > 4 && _previousSpeed < 4 || _previousSpeedX * speedX < 0)
				PlaySmoke();
			_previousSpeed = speed;
			_previousSpeedX = speedX;
		}
	}
}