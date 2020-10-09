using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Parkour
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] BoolVariable enableMove;
        [SerializeField] BoolVariable enableLook;
        [SerializeField] Vector2Variable moveInput;
        [SerializeField] Vector2Variable lookInput;
        [SerializeField] FloatVariable sensitivity;
        [SerializeField] float moveSpeed;
        [SerializeField] float acceleration;
        [SerializeField] float airAcceleration;
        [SerializeField] float brakeSpeed;
        [SerializeField] float jumpForce;
        [SerializeField] Vector2 direction;
        [SerializeField] LayerMask groundedMask;
        [SerializeField] float groundedOffset;
        [SerializeField] float groundedLength;
        [SerializeField] float groundedRadius;
        [SerializeField] float wallOffset;
        [SerializeField] float wallLength;
        [SerializeField] float wallSpeed;
        [SerializeField] float wallCooldown;
        [SerializeField] float wallVerticalBrake;
        [SerializeField] BoolPairEvent wallRideEvent;

        Rigidbody rb;
        int currentJumps;
        int maximumJumps = 2;
        public Vector3 previousWall;
        public Vector3 currentWall;
        float verticalVelocity;

        public bool IsMoving { get; private set; }
        public bool IsGrounded { get; private set; }
        public Vector3 Velocity { get; private set; }
        public bool IsWallRunning { get; private set; }

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
            if (enableMove.Value && !IsWallRunning)
            {
                Move();
            }
            
            if (!IsGrounded && !IsWallRunning)
            {
                verticalVelocity += Physics.gravity.y * Time.deltaTime;
            }

            RaycastHit hit;
            if (Physics.SphereCast(transform.position + new Vector3(0, groundedOffset, 0), groundedRadius, Vector3.down, out hit, groundedLength, groundedMask))
            {
                IsGrounded = true;
                ResetJumps();
            }
            else
            {
                IsGrounded = false;
            }

            if (!IsGrounded)
            {
                CheckWall();
            }

            rb.velocity = new Vector3(Velocity.x, verticalVelocity, Velocity.z);
        }

        void Move()
        {
            if (moveInput.Value.magnitude > 0)
            {
                IsMoving = true;

                float tempAcceleration = IsGrounded ? acceleration : airAcceleration;
                Velocity += transform.TransformVector(new Vector3(moveInput.Value.x * tempAcceleration * Time.deltaTime, 0, moveInput.Value.y * tempAcceleration * Time.deltaTime));
                if (IsGrounded && !IsWallRunning)
                {
                    Velocity = Vector3.ClampMagnitude(Velocity, moveSpeed);
                }                
            }
            else
            {
                IsMoving = false;

                if (IsGrounded)
                {
                    if (Velocity.magnitude > 0.1f)
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
            transform.Rotate(Vector3.up, lookInput.Value.x * sensitivity.Value * 0.01f);
        }

        public void Jump(bool value)
        {
            //Only call when jump is pressed down
            if (!IsWallRunning && value && currentJumps < maximumJumps)
            {
                if (!IsGrounded)
                {
                    //Velocity = Vector3.zero;
                    //Velocity += transform.TransformVector(new Vector3(moveInput.Value.x * jumpForce, 0, moveInput.Value.y * jumpForce));
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
            else if (IsWallRunning && value && currentJumps < maximumJumps)
            {
                Vector3 direction = (Vector3.up + currentWall).normalized;
                DetachFromWall();

                Velocity += direction * jumpForce;
                verticalVelocity = jumpForce;

                currentJumps++;
            }
        }

        void ResetJumps()
        {
            currentJumps = 0;
        }

        //True is wall on right, false is wall on left
        void AttachToWall(Vector3 normal, bool direction)
        {
            IsWallRunning = true;
            currentWall = normal;
            ResetJumps();
            //playerCamera.WallRide(true, !direction);
            BoolPair pair = new BoolPair();
            pair.Item1 = true;
            pair.Item2 = !direction;
            wallRideEvent.Raise(pair);
            Velocity -= normal * Vector3.Dot(normal, Velocity);
        }

        void CheckWall()
        {
            RaycastHit rightHit;
            RaycastHit leftHit;
            bool rightCheck = Physics.Raycast(transform.position + new Vector3(wallOffset, 1, 0), transform.right, out rightHit, wallLength, groundedMask);
            bool leftCheck = Physics.Raycast(transform.position - new Vector3(wallOffset, -1, 0), -transform.right, out leftHit, wallLength, groundedMask);

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

                if (IsWallRunning)
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
            else if (IsWallRunning)
            {
                DetachFromWall();
            }
        }

        void DetachFromWall()
        {
            IsWallRunning = false;
            previousWall = currentWall;
            currentWall = Vector3.zero;
            //playerCamera.WallRide(false, false);
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
    }
}