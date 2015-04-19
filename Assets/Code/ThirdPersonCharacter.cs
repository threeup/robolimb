using UnityEngine;

namespace UnityStandardAssets.Characters.ThirdPerson
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(Animator))]
	public class ThirdPersonCharacter : MonoBehaviour
	{
		public LayerMask characterMask;

		[SerializeField] float movingTurnSpeed = 360;
		[SerializeField] float stationaryTurnSpeed = 180;
		[SerializeField] float doJumpPower = 12f;
		[Range(1f, 4f)][SerializeField] float gravMultiplier = 2f;
		[SerializeField] float runCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
		[SerializeField] float moveVecSpeedMultiplier = 1f;
		[SerializeField] float animSpeedMultiplier = 1f;
		[SerializeField] float groundCheckDistance = 0.1f;

		ActorBody thisActorBody;


		Rigidbody thisRigidbody;
		Animator thisAnimator;
		bool isGrounded;
		float groundCheckDistanceDefault;
		const float k_Half = 0.5f;
		float turnAmount;
		float forwardAmount;
		Vector3 groundNormal;
		float capsuleHeight;
		Vector3 capsuleCenter;
		CapsuleCollider thisCapsule;
		bool isCrouching;


		void Start()
		{
			thisActorBody = GetComponent<ActorBody>();
			thisAnimator = GetComponent<Animator>();
			thisRigidbody = GetComponent<Rigidbody>();
			thisCapsule = GetComponent<CapsuleCollider>();
			capsuleHeight = thisCapsule.height;
			capsuleCenter = thisCapsule.center;

			thisRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
			groundCheckDistanceDefault = groundCheckDistance;
		}


		public void Move(Vector3 move, bool crouch, bool jump)
		{

			// convert the world relative moveInput vector into a local-relative
			// turn amount and forward amount required to head in the desired
			// direction.
			if (move.magnitude > 1f) move.Normalize();
			move = transform.InverseTransformDirection(move);
			CheckGroundStatus();
			move = Vector3.ProjectOnPlane(move, groundNormal);

			if( !thisActorBody.canWalkNormal )
			{
				if( thisActorBody.canWalkHop )
				{
					move *= 0.3f;		
				}
				else if( thisActorBody.canWalkKneel )
				{
					move *= 0.2f;		
				}
				else
				{
					move *= 0.05f;
				}
			}

			turnAmount = Mathf.Atan2(move.x, move.z);
			forwardAmount = move.z;


			ApplyExtraTurnRotation();

			// control and velocity handling is different when grounded and airborne:
			if (isGrounded)
			{
				HandleGroundedMovement(crouch, jump);
			}
			else
			{
				HandleAirborneMovement();
			}

			ScaleCapsuleForCrouching(crouch);
			PreventStandingInLowHeadroom();

			// send input and other state parameters to the animator
			UpdateAnimator(move);
		}


		void ScaleCapsuleForCrouching(bool crouch)
		{
			if (isGrounded && crouch)
			{
				if (isCrouching) return;
				thisCapsule.height = thisCapsule.height / 2f;
				thisCapsule.center = thisCapsule.center / 2f;
				isCrouching = true;
			}
			else
			{
				Ray crouchRay = new Ray(thisRigidbody.position + Vector3.up * thisCapsule.radius * k_Half, Vector3.up);
				float crouchRayLength = capsuleHeight - thisCapsule.radius * k_Half;
				if (Physics.SphereCast(crouchRay, thisCapsule.radius * k_Half, crouchRayLength, characterMask))
				{
					isCrouching = true;
					return;
				}
				thisCapsule.height = capsuleHeight;
				thisCapsule.center = capsuleCenter;
				isCrouching = false;
			}
		}

		void PreventStandingInLowHeadroom()
		{
			// prevent standing up in crouch-only zones
			if (!isCrouching)
			{
				Ray crouchRay = new Ray(thisRigidbody.position + Vector3.up * thisCapsule.radius * k_Half, Vector3.up);
				float crouchRayLength = capsuleHeight - thisCapsule.radius * k_Half;
				if (Physics.SphereCast(crouchRay, thisCapsule.radius * k_Half, crouchRayLength, characterMask))
				{
					isCrouching = true;
				}
			}
		}


		void UpdateAnimator(Vector3 move)
		{
			// update the animator parameters
			thisAnimator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
			thisAnimator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
			thisAnimator.SetBool("Crouch", isCrouching);
			thisAnimator.SetBool("OnGround", isGrounded);
			if (!isGrounded)
			{
				thisAnimator.SetFloat("Jump", thisRigidbody.velocity.y);
			}

			// calculate which leg is behind, so as to leave that leg trailing in the jump animation
			// (This code is reliant on the specific run cycle offset in our animations,
			// and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
			float runCycle =
				Mathf.Repeat(
					thisAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime + runCycleLegOffset, 1);
			float jumpLeg = (runCycle < k_Half ? 1 : -1) * forwardAmount;
			if (isGrounded)
			{
				thisAnimator.SetFloat("JumpLeg", jumpLeg);
			}

			// the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
			// which affects the movement speed because of the root motion.
			if (isGrounded && move.magnitude > 0)
			{
				thisAnimator.speed = animSpeedMultiplier;
			}
			else
			{
				// don't use that while airborne
				thisAnimator.speed = 1;
			}
		}


		void HandleAirborneMovement()
		{
			// apply extra gravity from multiplier:
			Vector3 extraGravityForce = (Physics.gravity * gravMultiplier) - Physics.gravity;
			thisRigidbody.AddForce(extraGravityForce);

			groundCheckDistance = thisRigidbody.velocity.y < 0 ? groundCheckDistanceDefault : 0.01f;
		}


		void HandleGroundedMovement(bool crouch, bool jump)
		{
			// check whether conditions are right to allow a jump:
			if (jump && !crouch && thisAnimator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
			{
				// jump!
				thisRigidbody.velocity = new Vector3(thisRigidbody.velocity.x, doJumpPower, thisRigidbody.velocity.z);
				isGrounded = false;
				thisAnimator.applyRootMotion = false;
				groundCheckDistance = 0.1f;
			}
		}

		void ApplyExtraTurnRotation()
		{
			// help the character turn faster (this is in addition to root rotation in the animation)
			float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
			transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
		}


		public void OnAnimatorMove()
		{
			// we implement this function to override the default root motion.
			// this allows us to modify the positional speed before it's applied.
			if (isGrounded && Time.deltaTime > 0)
			{
				Vector3 v = (thisAnimator.deltaPosition * moveVecSpeedMultiplier) / Time.deltaTime;

				// we preserve the existing y part of the current velocity.
				v.y = thisRigidbody.velocity.y;
				thisRigidbody.velocity = v;
			}
		}


		void CheckGroundStatus()
		{
			RaycastHit hitInfo;
#if UNITY_EDITOR
			// helper to visualise the ground check ray in the scene view
			Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * groundCheckDistance));
#endif
			// 0.1f is a small offset to start the ray from inside the character
			// it is also good to note that the transform position in the sample assets is at the base of the character
			if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, groundCheckDistance, characterMask))
			{
				groundNormal = hitInfo.normal;
				isGrounded = true;
				thisAnimator.applyRootMotion = true;
			}
			else
			{
				isGrounded = false;
				groundNormal = Vector3.up;
				thisAnimator.applyRootMotion = false;
			}
		}
	}
}
