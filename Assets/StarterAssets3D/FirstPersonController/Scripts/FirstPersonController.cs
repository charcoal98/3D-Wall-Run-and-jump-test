using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		AudioSource audioSource;
		public AudioClip walk;
		public AudioClip jump;
		public static FirstPersonController instance;



		[Header("Player")]
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		
		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;



		//Custom Movement Vars
		public Vector3 acceleration;
		private Vector3 prevVelo;
		private Vector3 inputDirection;

		//Velocity Limits for the BHOP
		[Tooltip("Max Velocity of the player in air")]
    	public float max_velocity_ground;
		[Tooltip("Max Velocity of the player on the ground")]
    	public float max_velocity_air;

    	//accelerate speed
		[Tooltip("Air acceleration of the character in m/s")]
    	public float air_accelerate;
		[Tooltip("Ground acceleration of the character in m/s")]
    	public float ground_accelerate;

    	//friction for surface
		[Tooltip("Friction for the player while on the ground")]
    	public float friction;

		//horizontal velocity needed because magnitude calculations of player speed should not include gravity since the player has no controll of it
		public Vector3 horVelo;



		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

		

	
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		private PlayerInput _playerInput;
#endif
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		private const float _threshold = 0.01f;

		private bool IsCurrentDeviceMouse
		{
			get
			{
				#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
				return _playerInput.currentControlScheme == "KeyboardMouse";
				#else
				return false;
				#endif
			}
		}

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}

		}

		private void Start()
		{

			//audio
			audioSource = GetComponent<AudioSource>();
	
			//acceleration = Vector3.zero;
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
			_playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
			
		}

		private void Update()
		{	GroundedCheck();
			JumpAndGravity();
			Move();
		}

		private void LateUpdate()
		{
			CameraRotation();
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

		private void CameraRotation()
		{
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				
				_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;

				/*Disabled horizontal mouse look because it in playtesting seemed to distract players from straifing. 
				Players naturally try to look at the platforms they were trying to land on while holding forward instead of straifing
				which meant that they were still adding to their forward momentum and were not moving. See Ascii Art Below ↓ */
				_rotationVelocity = 0;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

//			Why You Need To Let Go of W While Straifing
//                              ↑                                     
//                              /                                     
//     ↑             ↗         |                                   ↑  
//     |            /          /                                   /  
//     |     +     /    =     |                                   |   
//     |          /           /                                   /   
// x=0, y=1   x=1, y=1       |      Which gets trunkated to x=0.5, y=1
//                           /                                        
//                       x=1, y=2       
//This was intentional to mimic the way momentum feels in the source engine                              

		
		
		private void Move()
		{
			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			
			if (_input.move != Vector2.zero)
			{
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			} else {
				inputDirection = Vector3.zero;
			}

			// move the player
			
			horVelo = new Vector3 (_controller.velocity.x,0,_controller.velocity.z);
			prevVelo = acceleration; //uses the calculated acceleration instead of the character controllers velocity so you don't instantly lose all speed when you clip a platform
			
			if (horVelo.magnitude == 0){
				prevVelo = Vector3.zero;
			}

			if (Grounded)
			{
				acceleration = MoveGround(inputDirection, prevVelo);
			}
			else
			{
				acceleration = MoveAir(inputDirection, prevVelo);
			}

			_controller.Move(acceleration * Time.deltaTime + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
		}

		//thanks to Adrian Biagioli for the code  https://adrianb.io/2015/02/14/bunnyhop.html This is the code that defines how momentum is handled
		//please click the link vector math is hard to explain with out diagrams 
    
    	public Vector3 Accelerate(Vector3 accelDir, Vector3 prevVelocity, float accelerate, float max_velocity)
    	{
    	    float projVel = Vector3.Dot(prevVelocity, accelDir);
    	    float accelVel = accelerate * Time.deltaTime;

    	    if(projVel + accelVel > max_velocity){
    	        accelVel = max_velocity - projVel;
			}
    	    return prevVelocity + accelDir * accelVel;
    	}

    	private Vector3 MoveGround(Vector3 accelDir, Vector3 prevVelocity)
    	{
    	    // Apply Friction
    	    float speed = prevVelocity.magnitude;
    	    if (speed != 0) // To avoid divide by zero errors
    	    {
				//StartCoroutine(PlayWalkSound(ground_accelerate));
    	        float drop = speed * friction * Time.deltaTime;
    	        prevVelocity *= Mathf.Max(speed - drop, 0) / speed; // Scale the velocity based on friction.
    	    }

    	    // ground_accelerate and max_velocity_ground are server-defined inputDirection variables
    	    return Accelerate(accelDir, prevVelocity, ground_accelerate, max_velocity_ground);
    	}

    	private Vector3 MoveAir(Vector3 accelDir, Vector3 prevVelocity)
    	{
    	    // air_accelerate and max_velocity_air are server-defined inputDirection variables
    	    return Accelerate(accelDir, prevVelocity, air_accelerate, max_velocity_air);
    	}



		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -0f;
				}

				// Jump
				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					PlayJumpSound();
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
					
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				
			}
			if(!_input.jump){
				_input.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}

		public void PlayJumpSound()
		{
			audioSource.PlayOneShot(jump);
		}
	}
}