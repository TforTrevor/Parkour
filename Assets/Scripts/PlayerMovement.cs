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
        Vector3 previousWall;
        Vector3 currentWall;
        float verticalVelocity;

        public MovementState MoveState { get; private set; }
        public bool IsMoving { get; private set; }
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
            if (enableMove.Value && MoveState != MovementState.WallRunning)
            {
                Move();
            }
            
            if (MoveState == MovementState.Airborne)
            {
                verticalVelocity += Physics.gravity.y * Time.deltaTime;
            }

            if (MoveState != MovementState.WallRunning)
            {
                RaycastHit hit;
                if (Physics.SphereCast(transform.position + new Vector3(0, groundedOffset, 0), groundedRadius, Vector3.down, out hit, groundedLength, groundedMask))
                {
                    MoveState = MovementState.Grounded;
                    ResetJumps();
                }
                else
                {
                    MoveState = MovementState.Airborne;
                }
            }            

            if (MoveState == MovementState.Airborne || MoveState == MovementState.WallRunning)
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

                float tempAcceleration = MoveState == MovementState.Grounded ? acceleration : airAcceleration;
                Velocity += transform.TransformVector(new Vector3(moveInput.Value.x * tempAcceleration * Time.deltaTime, 0, moveInput.Value.y * tempAcceleration * Time.deltaTime));
                if (MoveState == MovementState.Grounded)
                {
                    Velocity = Vector3.ClampMagnitude(Velocity, moveSpeed);
                }                
            }
            else
            {
                IsMoving = false;

                if (MoveState == MovementState.Grounded)
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

                    Velocity += direction * jumpForce;
                    verticalVelocity = jumpForce;

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
    }

    public enum MovementState
    {
        Grounded,
        Airborne,
        WallRunning,
        Grappling,
        Climbing
    }
}