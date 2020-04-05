using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class WallRunningMovement : MonoBehaviour
{
    public float DistanceFromWallRequiredToMount = 0.3f;
    public float WallRunningMovementSpeed = 10.0f;
    public bool IsCurrentlyWallRunning = false;//exposed for debugging
    public float MaxWallRunningDuration = 6.0f;
    public float CurrentRunningDuration;

    public float WallRunningCooldown = 2.0f;//this is per wall
    public float CurrentRunningCooldown = 0.0f;
    GameObject LastWallWeRanOn;//for determining cooldowns.

    Player ThePlayer;
    public bool IsRight = false;
    public bool IsFront = false;

    public bool IsWallRunQueued = false;
    Vector3 LastPositionFrame;

    //public float MaxJumpPreventionLimit = 0.1f;
    //public float CurrentJumpPreventionLimit = 0.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(ThePlayer.transform.rotation.eulerAngles.y);
        //update timers
        //these handle flow control of bouncing between walls and limiting how long the player can run along a wall

        if (Input.GetKeyDown(KeyCode.Q))
        {
            IsWallRunQueued = true;
        }

        if (IsCurrentlyWallRunning)
        {
            CurrentRunningDuration += Time.deltaTime; 

            //We ran out of wall-running time. Time to get off.
            if (CurrentRunningDuration >= MaxWallRunningDuration)
            {
                SwitchWallRunning(false);
            }
        }
        else
        //adjust the wall running cooldown
        {
            if (CurrentRunningCooldown > 0.0f)
            {
                CurrentRunningCooldown -= Time.deltaTime;
                Mathf.Clamp(CurrentRunningCooldown, 0.0f, WallRunningCooldown);
            }
        }
    }

    private void FixedUpdate()
    {
        ThePlayer = GetComponent<Player>();

        //Didnt queue a wallrun and not wallrunning? get out.
        if (IsWallRunQueued == false && IsCurrentlyWallRunning == false)
        {
            return;
        }

        //if we're wall-running and didnt actually move last frame that means we hit a wall and can cancel out.
        if (IsCurrentlyWallRunning)
        {
            Vector3 CurrentPositionFrame = transform.position;

            if (CurrentPositionFrame == LastPositionFrame)
            {
                SwitchWallRunning(false);
                return;
            }

            LastPositionFrame = CurrentPositionFrame;
        }

        //We're now processing a request so we can safely false out the queue.
        IsWallRunQueued = false;

        //can't wall run on ground
        if (ThePlayer.m_MovementState == Player.MovementState.OnGround)
        {
            return;
        }

        //Check for mounting a wall.
        RaycastHit leftRay = PerformWallCheck(false);
        RaycastHit rightRay = PerformWallCheck(true);

        float leftWallDistance = float.MaxValue;
        float rightWallDistance = float.MaxValue;

        //if left wall is viable get the distance from it.
        if (IsViableWall(leftRay))
        {
            leftWallDistance = leftRay.distance;
        }

        //if right wall is viable get the distance from it.
        if (IsViableWall(rightRay))
        {
            rightWallDistance = rightRay.distance;
        }

        RaycastHit closestWall;

        //Is left wall close enough and closer?
        if (leftWallDistance < rightWallDistance && leftWallDistance <= DistanceFromWallRequiredToMount)
        {
            closestWall = leftRay;
            IsRight = false;
        }
        //Is the right wall close enough and closer?
        else if (rightWallDistance < leftWallDistance && rightWallDistance <= DistanceFromWallRequiredToMount)
        {
            closestWall = rightRay;
            IsRight = true;
        }
        //Neither are close enough.
        else
        {
            SwitchWallRunning(false);
            return;
        }

        //are we on cooldown for this particular wall?
        if (CurrentRunningCooldown > 0.0f && closestWall.collider.gameObject.name == LastWallWeRanOn.name)
        {
            SwitchWallRunning(false);
            return;
        }

        //we have a wall to mount. While wall running we're considered in air.
        if (IsCurrentlyWallRunning == false)
        {
            SwitchWallRunning(true);
            LastWallWeRanOn = closestWall.collider.gameObject;
        }

        if (ThePlayer.m_JumpUsed == true)
        {
            SwitchWallRunning(false);
            ThePlayer.ActivateJump();
            
            Vector3 direction = ThePlayer.Controller.GetLookInput();
            ThePlayer.m_Velocity = direction * WallRunningMovementSpeed;
            ThePlayer.ApplyVelocity(ThePlayer.m_Velocity);
        }
        else if (ThePlayer.m_MovementState == Player.MovementState.WallRunning)
        {
            Vector3 direction = GetDirectionVectorAlongWall(closestWall);

            //Vector3 rotation = ThePlayer.transform.rotation.eulerAngles;
            //rotation.y = -rotation.y;

            //rotation.x = 0.0f;
            //transform.GetChild(2).Rotate(rotation, Time.fixedDeltaTime * 120);  

            Debug.DrawLine(closestWall.point, closestWall.point + direction * 5.0f, Color.black);


            Vector3 localVel = Vector3.ClampMagnitude(direction * WallRunningMovementSpeed, ThePlayer.OnGroundMaxSpeed);
            ThePlayer.m_Velocity = localVel;
            ThePlayer.ApplyVelocity(ThePlayer.m_Velocity);
        }
    }

    void SwitchWallRunning(bool Enabled)
    {
        if (Enabled == IsCurrentlyWallRunning)
            return;

        IsCurrentlyWallRunning = Enabled;

        if (Enabled)
        {
            LastPositionFrame = transform.position;
            CurrentRunningDuration = 0.0f;
            ThePlayer.SetMovementState(Player.MovementState.WallRunning);
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            ThePlayer.m_DoubleJumpUsed = false;
            ThePlayer.m_JumpUsed = false;
            CurrentRunningCooldown = 0.0f;
        }
        else
        {
            CurrentRunningDuration = 0.0f;
            CurrentRunningCooldown = WallRunningCooldown;
            ThePlayer.SetMovementState(Player.MovementState.InAir);
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            ThePlayer.m_JumpUsed = true;
            //Quaternion cameraRot = ThePlayer.transform.GetChild(2).rotation;
            //ThePlayer.transform.rotation = cameraRot;
        }
    }

    RaycastHit PerformWallCheck(bool IsRight)
    {
        RaycastHit ray;

        //direction switch
        Vector3 direction;
        if (IsRight)
            direction = transform.right;
        else
            direction = -transform.right;

        Debug.DrawLine(transform.position, transform.position + direction * DistanceFromWallRequiredToMount, Color.magenta);

        Physics.Raycast(transform.position, direction, out ray, DistanceFromWallRequiredToMount);
        return ray;
    }

    bool IsViableWall(RaycastHit HitInfo)
    {
        //nothing hit. not a wall
        if (HitInfo.collider == null)
            return false;

        //collider isnt perpendicular to the ground then it isnt a wall.
        if (Vector3.Dot(HitInfo.normal, Vector3.up) != 0)
            return false;

        //should be a wall
        return true;
    }

    Vector3 GetDirectionVectorAlongWall(in RaycastHit closestWall)
    {
        float currentRotationY = ThePlayer.transform.rotation.eulerAngles.y;

        if (currentRotationY >= 0.0f && currentRotationY < 180.0f)
        {
            IsFront = true;
        }
        else
        {
            IsFront = false;
        }

        if (IsFront)
        {
            if (IsRight)
            {
                return Vector3.Cross(Vector3.up, closestWall.normal);
            }
            else
            {
                return -Vector3.Cross(Vector3.up, closestWall.normal);
            }
        }
        else
        {
            if (IsRight == false)
            {
                return -Vector3.Cross(Vector3.up, closestWall.normal);
            }
            else
            {
                return Vector3.Cross(Vector3.up, closestWall.normal);
            }
        }
    }
}
