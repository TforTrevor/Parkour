using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Parkour
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("SO Variables")]
        [SerializeField] BoolVariable enableMove;
        [SerializeField] BoolVariable enableLook;
        [SerializeField] Vector2Variable moveInput;
        [SerializeField] Vector2Variable lookInput;
        [SerializeField] FloatVariable sensitivity;
        [Header("Walk")]
        [SerializeField] float walkSpeed;
        [SerializeField] float sprintSpeed;
        [SerializeField] float crouchSpeed;
        [SerializeField] float walkAcceleration;
        [SerializeField] float sprintAcceleration;
        [SerializeField] float crouchAcceleration;
        [SerializeField] float airAcceleration;
        [SerializeField] float brakeSpeed;
        [SerializeField] float jumpForce;
        [Header("Ground Check")]
        [SerializeField] LayerMask groundedMask;
        [SerializeField] float groundedOffset;
        [SerializeField] float groundedLength;
        [SerializeField] float projectLength;
        [Header("Wall Running")]
        [SerializeField] float wallOffset;
        [SerializeField] float wallLength;
        [SerializeField] float wallSpeed;
        [SerializeField] float wallCooldown;
        [SerializeField] float wallVerticalBrake;
        [SerializeField] float wallJumpForce;
        [SerializeField] BoolPairEvent wallRideEvent;
        [Header("Crouch")]
        [SerializeField] float normalHeight;
        [SerializeField] float crouchHeight;
        [SerializeField] CapsuleCollider capsuleCollider;
        [SerializeField] BoolEvent crouchEvent;
        [SerializeField] float slideBrake;
        [SerializeField] float slideThreshold;
        [SerializeField] float slideSpeed;
        [SerializeField] float slideAccleration;

        Rigidbody rb;
        int currentJumps;
        int maximumJumps = 2;
        Vector3 previousWall;
        Vector3 currentWall;
        float verticalVelocity;

        public MovementState MoveState { get; private set; }
        public bool IsMoving { get; private set; }
        public bool IsCrouching { get; private set; }
        public Vector3 Velocity { get; private set; }

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        void Update()
        {
            if (enableLook.Value)
            {
                Rotate();
            }
        }

        void FixedUpdate()
        {
            if (IsCrouching && Velocity.magnitude >= slideThreshold && MoveState != MovementState.Airborne)
            {
                MoveState = MovementState.Sliding;
            }

            if (MoveState == MovementState.Sliding)
            {
                Slide();
            }

            if (enableMove.Value && (MoveState == MovementState.Default || MoveState == MovementState.Sprinting || MoveState == MovementState.Airborne))
            {
                Move();
            }
            
            if (MoveState == MovementState.Airborne)
            {
                verticalVelocity += Physics.gravity.y * Time.deltaTime;
            }

            if (MoveState != MovementState.WallRunning)
            {
                RaycastHit groundHit;
                RaycastHit projectHit;

                bool groundedCheck = Physics.Raycast(transform.position + new Vector3(0, groundedOffset, 0), Vector3.down, out groundHit, groundedLength, groundedMask);
                bool projectCheck = Physics.Raycast(transform.position + new Vector3(0, groundedOffset, 0), Vector3.down, out projectHit, projectLength, groundedMask);
                //Landed on ground
                if (MoveState == MovementState.Airborne && groundedCheck)
                {
                    MoveState = MovementState.Default;
                    transform.position = groundHit.point;
                    verticalVelocity = 0;
                    previousWall = Vector3.zero;
                    ResetJumps();
                }
                //if (!groundedCheck && !projectCheck)
                if (!groundedCheck && (verticalVelocity > 0 ? true : !projectCheck))
                {
                    MoveState = MovementState.Airborne;
                    Debug.Log("Airborne");
                }

                if (MoveState != MovementState.Airborne && projectCheck && verticalVelocity <= 0)
                {
                    transform.position = new Vector3(transform.position.x, projectHit.point.y, transform.position.z);
                    Velocity = Vector3.ProjectOnPlane(Velocity, projectHit.normal);
                }
            }

            if (MoveState == MovementState.Airborne || MoveState == MovementState.WallRunning)
            {
                CheckWall();
            }

            rb.velocity = new Vector3(Velocity.x, verticalVelocity + Velocity.y, Velocity.z);
        }

        void Slide()
        {
            if (Velocity.magnitude > 2f)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + new Vector3(0, groundedOffset, 0), Vector3.down, out hit, projectLength, groundedMask))
                {
                    float slope = Vector3.Dot(hit.normal, new Vector3(Velocity.x, 0, Velocity.z));
                    if (slope > 0)
                    {
                        //Sliding downhill
                        float multiplier = 1 - Vector3.Dot(Vector3.up, hit.normal);
                        Velocity += transform.forward * slideAccleration * multiplier * Time.deltaTime;
                        Velocity = Vector3.ClampMagnitude(Velocity, slideSpeed);
                    }
                    else
                    {
                        //Sliding uphill or on flat ground
                        Velocity -= Velocity.normalized * slideBrake * (1 - slope) * Time.deltaTime;
                    }

                    //Velocity = Vector3.ProjectOnPlane(Velocity, hit.normal);
                }
            }
            else
            {
                MoveState = MovementState.Default;
            }
        }

        void Move()
        {
            if (moveInput.Value.magnitude > 0)
            {
                IsMoving = true;

                float tempSpeed = 0;
                float tempAcceleration = 0;
                switch (MoveState)
                {
                    case MovementState.Default:
                        tempAcceleration = walkAcceleration;
                        tempSpeed = walkSpeed;
                        break;
                    case MovementState.Airborne:
                        tempAcceleration = airAcceleration;
                        break;
                    case MovementState.Sprinting:
                        tempAcceleration = sprintAcceleration;
                        tempSpeed = sprintSpeed;
                        break;
                }
                if (IsCrouching)
                {
                    tempAcceleration = crouchAcceleration;
                    tempSpeed = crouchSpeed;
                }

                Vector3 inputVector = new Vector3(moveInput.Value.x, 0, moveInput.Value.y).normalized;

                Velocity += transform.TransformVector(inputVector * tempAcceleration * Time.deltaTime);
                if (MoveState != MovementState.Airborne)
                {
                    Velocity = Vector3.ClampMagnitude(Velocity, tempSpeed);
                }

                RaycastHit hit;
                if (Physics.Raycast(transform.position + new Vector3(0, groundedOffset, 0), Vector3.down, out hit, projectLength, groundedMask))
                {
                    //Velocity = Vector3.ProjectOnPlane(Velocity, hit.normal);
                }
            }
            else
            {
                IsMoving = false;

                if (MoveState == MovementState.Sprinting)
                {
                    MoveState = MovementState.Default;
                }

                if (MoveState == MovementState.Default)
                {
                    if (Velocity.magnitude > 0.25f)
                    {
                        Velocity -= Velocity.normalized * brakeSpeed * Time.deltaTime;
                    }
                    else
                    {
                        Velocity = Vector3.zero;
                    }
                }
            }
        }

        void Rotate()
        {
            transform.Rotate(Vector3.up, lookInput.Value.x * sensitivity.Value * 0.01f * Time.timeScale);
        }

        public void Jump(bool value)
        {
            //Only call when jump is pressed down
            if (value && currentJumps < maximumJumps)
            {
                if (MoveState != MovementState.WallRunning)
                {
                    if (MoveState == MovementState.Airborne)
                    {
                        Vector3 direction = transform.TransformVector(new Vector3(moveInput.Value.x, 0, moveInput.Value.y).normalized);
                        if (direction == Vector3.zero)
                        {
                            direction = transform.forward;
                        }
                        Velocity = direction * Velocity.magnitude;
                    }

                    verticalVelocity = jumpForce;

                    currentJumps++;
                }
                else if (MoveState == MovementState.WallRunning)
                {
                    Vector3 direction = (Vector3.up + currentWall).normalized;
                    DetachFromWall();

                    Velocity += direction * wallJumpForce;
                    verticalVelocity = wallJumpForce;

                    currentJumps++;
                }
            }
        }

        void ResetJumps()
        {
            currentJumps = 0;
        }

        //True is wall on right, false is wall on left
        void AttachToWall(Vector3 normal, bool direction)
        {
            MoveState = MovementState.WallRunning;
            currentWall = normal;
            ResetJumps();

            BoolPair pair = new BoolPair();
            pair.Item1 = true;
            pair.Item2 = !direction;
            wallRideEvent.Raise(pair);
            Velocity -= normal * Vector3.Dot(normal, Velocity);
        }

        void CheckWall()
        {
            if (!IsCrouching)
            {
                RaycastHit rightHit;
                RaycastHit leftHit;
                bool rightCheck = Physics.Raycast(transform.position + new Vector3(wallOffset, normalHeight / 2, 0), transform.right, out rightHit, wallLength, groundedMask);
                bool leftCheck = Physics.Raycast(transform.position + new Vector3(-wallOffset, normalHeight / 2, 0), -transform.right, out leftHit, wallLength, groundedMask);

                RaycastHit workingHit = new RaycastHit();
                bool workingCheck = false;
                bool workingDir = false;
                if (rightCheck)
                {
                    workingHit = rightHit;
                    workingCheck = rightCheck;
                    workingDir = true;
                }
                else if (leftCheck)
                {
                    workingHit = leftHit;
                    workingCheck = leftCheck;
                    workingDir = false;
                }
                if (workingCheck)
                {
                    if (workingHit.normal != currentWall && workingHit.normal != previousWall)
                    {
                        AttachToWall(workingHit.normal, workingDir);
                    }

                    if (MoveState == MovementState.WallRunning)
                    {
                        Vector3 forward = Vector3.Cross(Vector3.up, workingHit.normal);
                        forward *= Mathf.Sign(Vector3.Dot(forward, transform.forward));

                        Velocity += forward * airAcceleration * Time.deltaTime;
                        Velocity = Vector3.ClampMagnitude(Velocity, wallSpeed);

                        verticalVelocity -= Mathf.Sign(verticalVelocity) * wallVerticalBrake * Time.deltaTime;
                        if (Mathf.Abs(verticalVelocity) < 0.1f)
                        {
                            verticalVelocity = 0;
                        }
                    }
                }
                else if (MoveState == MovementState.WallRunning)
                {
                    DetachFromWall();
                }
            }
            else
            {
                DetachFromWall();
            }            
        }

        void DetachFromWall()
        {
            MoveState = MovementState.Airborne;
            previousWall = currentWall;
            currentWall = Vector3.zero;

            BoolPair pair = new BoolPair();
            pair.Item1 = false;
            pair.Item2 = false;
            wallRideEvent.Raise(pair);

            Timing.RunCoroutine(WallCooldown());
        }

        IEnumerator<float> WallCooldown()
        {
            yield return Timing.WaitForSeconds(wallCooldown);
            previousWall = Vector3.zero;
        }

        public void Crouch(bool value)
        {
            IsCrouching = value;
            if (IsCrouching)
            {
                capsuleCollider.height = crouchHeight;
                capsuleCollider.center = new Vector3(0, crouchHeight / 2, 0);
                if (MoveState == MovementState.Sprinting)
                {
                    MoveState = MovementState.Default;
                }
            }
            else
            {
                capsuleCollider.height = normalHeight;
                capsuleCollider.center = new Vector3(0, normalHeight / 2, 0);
                if (MoveState == MovementState.Sliding)
                {
                    MoveState = MovementState.Default;
                }
            }
            crouchEvent.Raise(IsCrouching);
        }

        public void ToggleSprint(bool value)
        {
            if (MoveState == MovementState.Default)
            {
                if (value)
                {
                    MoveState = MovementState.Sprinting;
                }
                else
                {
                    MoveState = MovementState.Default;
                }
            }            
        }
    }

    public enum MovementState
    {
        Default,
        Airborne,
        WallRunning,
        Grappling,
        Climbing,
        //Crouching,
        Sprinting,
        Sliding
    }
}