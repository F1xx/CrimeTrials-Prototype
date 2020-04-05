using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class CrouchMovement : MonoBehaviour
{
    public bool IsCrouching = false;//Exposed for debugging.
    public bool IsSprinting = false;//Exposed for debugging.
    public float MaximumGroundSpeedWhileCrouching = 2.5f;
    public float SlideLerpMovementSpeedDecayTime = 4.0f;

    PlayerController Controller;
    Player ThePlayer;
    Rigidbody PhysicsBody;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        //Not sure why this fails in Start so whatever just going to leave it here.
        Controller = GetComponent<Player>().Controller;
        PhysicsBody = GetComponent<Rigidbody>();
        ThePlayer = GetComponent<Player>();

        IsCrouching = Controller.ToggleCrouch();
        IsSprinting = Controller.IsSprinting();

        //If crouching is toggled then clamp movement speed and shrink the character model.
        if  (IsCrouching && ThePlayer.m_MovementState != Player.MovementState.InAir && ThePlayer.m_MovementState != Player.MovementState.WallRunning)
        {
            //switch to the crouch model. This runs only once per crouch session.
            if (ThePlayer.m_MovementState != Player.MovementState.Crouch)
            {
                gameObject.GetComponent<Collider>().transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                gameObject.GetComponent<Renderer>().transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                gameObject.transform.position -= new Vector3(0.0f, 0.49f, 0.0f);
            }

            ApplyCrouchingVelocity();
        }
        else     
        {
            //we exited crouch. Go back to ground state.
            if (ThePlayer.m_MovementState == Player.MovementState.Crouch)
            {
                RaycastHit safetyCheck;
                //Physics.Raycast(transform.position, Vector3.up, out safetyCheck, 1.5f);
                Physics.BoxCast(transform.position, transform.localScale, Vector3.up, out safetyCheck, transform.rotation, 1.5f);

                //we can safely exit crouch without bonking something.
                if (safetyCheck.collider == null)
                {

                    ThePlayer.SetMovementState(Player.MovementState.OnGround);

                    //Scale for standing
                    gameObject.GetComponent<Collider>().transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    gameObject.GetComponent<Renderer>().transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                }
                //we're going to hit something so instead we're going to just move as normal.
                else
                {
                    ApplyCrouchingVelocity();
                }
            }
        }
    }

    void ApplyCrouchingVelocity()
    {
        Vector3 playerVelocity = ThePlayer.m_Velocity;
        Vector3 movementDir = Controller.GetMoveInput();

        //ground movement START
        if (movementDir.sqrMagnitude > MathUtils.CompareEpsilon)
        {
            Vector3 localVel = ThePlayer.m_Velocity - ThePlayer.GroundVelocity;
            Vector3 moveAccel = ThePlayer.CalcMoveAccel(movementDir);
            Vector3 groundTangent = moveAccel - Vector3.Project(moveAccel, ThePlayer.GroundNormal);
            groundTangent.Normalize();

            moveAccel = groundTangent;

            Vector3 velAlongMoveDir = Vector3.Project(localVel, moveAccel);

            if (Vector3.Dot(velAlongMoveDir, moveAccel) > 0.0f)
            {
                localVel = MathUtils.LerpTo(ThePlayer.OnGroundStopEaseSpeed, localVel, velAlongMoveDir, Time.fixedDeltaTime);
            }
            else
            {
                localVel = MathUtils.LerpTo(ThePlayer.OnGroundStopEaseSpeed, localVel, Vector3.zero, Time.fixedDeltaTime);
            }

            moveAccel *= ThePlayer.OnGroundMoveAccel;

            localVel += moveAccel * Time.fixedDeltaTime;

            localVel = Vector3.ClampMagnitude(localVel, ThePlayer.OnGroundMaxSpeed);
            ThePlayer.m_Velocity = localVel + ThePlayer.GroundVelocity;
        }
        else
        {
            ThePlayer.UpdateStopping(ThePlayer.OnGroundStopEaseSpeed);
        }
        //ground movement END

        //update state to crouching
        ThePlayer.SetMovementState(Player.MovementState.Crouch);

        //check current magnitude
        float velocityMag = Mathf.Sqrt((playerVelocity.x * playerVelocity.x) + (playerVelocity.z * playerVelocity.z));

        //is the player sprinting while attempting a crouch, in that case we're going to perform a slide instead.
        //sliding is effectively the same as crouching except that when you slide you retain some of your momentum instead of 
        //immediately losing it with a normal crouch.

        //our target magnitude
        float targetMagnitude = MaximumGroundSpeedWhileCrouching;

        //if the player is sprinting then we'll ease into the intended magnitude instead of immediately.
        if (IsSprinting)
        {
            targetMagnitude = MathUtils.LerpToEaseOut(velocityMag, MaximumGroundSpeedWhileCrouching, Time.fixedDeltaTime);
        }

        Vector2 newSpeed = new Vector2(ThePlayer.m_Velocity.x, ThePlayer.m_Velocity.z);
        newSpeed = Vector2.ClampMagnitude(newSpeed, targetMagnitude);

        //set the adjusted velocity accordingly.
        ThePlayer.m_Velocity.x = newSpeed.x;
        ThePlayer.m_Velocity.z = newSpeed.y;

        //set our velocity and apply it.
        PhysicsBody.velocity = ThePlayer.m_Velocity;
        ThePlayer.ApplyVelocity(ThePlayer.m_Velocity);
    }
}
