using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (ThirdPersonCharacter))]
    public class ThirdPersonUserControl : MonoBehaviour
    {
        public bool isAI = false;
        public Actor thisActor;
        private ThirdPersonCharacter thisCharacter; // A reference to the ThirdPersonCharacter on the object
        private Transform thisCam;                  // A reference to the main camera in the scenes transform
        private Vector3 thisCamForward;             // The current forward direction of the camera
        private Vector3 moveVec;
        private bool doJump;          
        private bool doCrouch;        
        private bool doCycle;         
        private bool doThrow;         

        
        private void Start()
        {
            // get the transform of the main camera
            if (Camera.main != null)
            {
                thisCam = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.");
                // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            }

            // get the third person character ( this should never be null due to require component )
            thisCharacter = GetComponent<ThirdPersonCharacter>();
        }

        public void Spawn(bool isPC)
        {
            this.isAI = !isPC;
            NavMeshAgent naver = GetComponent<NavMeshAgent>();
            AICharacterControl aier = GetComponent<AICharacterControl>();
            if( isAI )
            {
                naver.enabled = true;                
                aier.enabled = true;
                this.enabled = false;
            }
            else
            {
                naver.enabled = false;                
                aier.enabled = false;
                this.enabled = true;
            }
        }


        private void Update()
        {
            if (!doJump)
            {
                doJump = CrossPlatformInputManager.GetButtonDown("Jump");
            }
            bool shouldThrow = CrossPlatformInputManager.GetButton("Fire1");
            bool shouldCycle = CrossPlatformInputManager.GetButton("Fire2") || Input.GetKey(KeyCode.LeftControl);
            if( doThrow != shouldThrow)
            {
                doThrow = shouldThrow;
            }
            doCycle = shouldCycle;
            thisActor.Throwing(doThrow, doCycle);
        }


        // Fixed update is called in sync with physics
        private void FixedUpdate()
        {
            // read inputs
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");
            doCrouch = Input.GetKey(KeyCode.C);

            // calculate move direction to pass to character
            if (thisCam != null)
            {
                // calculate camera relative direction to move:
                thisCamForward = Vector3.Scale(thisCam.forward, new Vector3(1, 0, 1)).normalized;
                moveVec = v*thisCamForward + h*thisCam.right;
            }
            else
            {
                // we use world-relative directions in the case of no main camera
                moveVec = v*Vector3.forward + h*Vector3.right;
            }

            // pass all parameters to the character control script

            float speed = thisActor.body.GetSpeed();
            thisCharacter.Move(moveVec, speed, doCrouch, doJump);
            doJump = false;
        }
    }
}
