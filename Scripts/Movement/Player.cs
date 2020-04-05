using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(AudioSource))]
public class Player : MonoBehaviour
{

    public float OnGroundMoveAccel = 20.0f;
    public float OnGroundMaxSpeed = 10.0f;
    public float OnGroundStopEaseSpeed = 10.0f;

    public float MinAllowedSurfaceAngle = 15.0f;

    public float GroundCheckStartOffsetY = 0.5f;
    public float CheckForGroundRadius = 0.5f;
    public float GroundResolutionOverlap = 0.05f;

    public GameObject FootLocationObj;
    public float VectorVisualizeScale = 2.0f;

    public float JumpStrength = 1000.0f;
    public float PlayerMass = 10.0f;
    public float SprintMult = 2.0f;

    public PlayerController Controller { get; set; }
    public Vector3 GroundVelocity { get; private set; }
    public Vector3 GroundAngularVelocity { get; private set; }
    public Vector3 GroundNormal { get; private set; }

    public Vector3 AirVelocity { get; private set; }

    [Range(1.0f, 10.0f)]
    public float Camera_Sensitivity = 5.0f;

    // Alex: Real-time changing of values to view in the inspector for camera clamping
    public float min = 0;
    public float max = 0;
    public Vector2 m_Rotation = Vector2.zero;

    // Alex: Attach camera through inspector so that its explicit which camera we're grabbing just in case we have more than one camera
    public Camera m_Camera = null;

    void Start ()
	{
        if(!SetupHumanPlayer())
        {
            return;
        }

        // Alex: If no camera attached currently, grab component camera, if no component camera; find one 
        if (!m_Camera)
        {
            m_Camera = GetComponentInChildren<Camera>();
            if (!m_Camera)
                m_Camera = GameObject.FindObjectOfType<Camera>();
        }

        m_GroundCheckMask = ~LayerMask.GetMask("Player", "IgnoreRaycast");

        m_Rigidbody = GetComponent<Rigidbody>();
        m_Rigidbody.mass = PlayerMass;
        m_Velocity = Vector3.zero;

        m_HealthComp = GetComponent<HealthComponent>();
    }
	
	// Update is called once per frame
	void Update ()
	{
        //live update some vars so you can change them in the editor and it will live effect
        //used for values that set other values and aren't used directly
        m_Rigidbody.mass = PlayerMass;

        //queue a jump for fixed update
        if (Input.GetKeyDown(KeyCode.Space) && !m_IsJumpQueued)
        {
            m_IsJumpQueued = true;
        }
        
        // Alex: Put this here as camera doesn't need to have Time.deltatime applied to its movements through fixed update
        transform.rotation = Quaternion.Euler(0, m_Camera.transform.rotation.eulerAngles.y, 0);
        HandleCamera();
    }

    void FixedUpdate()
    {
        if (m_HealthComp.IsAlive() && m_MovementState != MovementState.Disable)
        { 
            // Alex: moved HandleCamera() to Update, doesn't need to be in Fixed Update, can cause stutter
            HandleMovement();
            HandleState();
            HandleJumping();
        }
        else
        {
            GameObject temp = GameObject.Find("CanvasObj");

            if(temp)
            {
                CanvasGroup can = temp.GetComponentInChildren<CanvasGroup>();
                can.alpha += Time.deltaTime;
            }
        }
    }

    void HandleMovement()
    {
        m_Velocity = m_Rigidbody.velocity;
        UpdateGroundInfo();

        Controller.UpdateControls();
    }

    private void HandleCamera()
    {
        //Camera Look around
        if (Input.GetMouseButtonDown(1))
        {
            if (m_MouseLookEnabled)
            {
                m_MouseLookEnabled = false;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                m_MouseLookEnabled = true;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        if (m_MouseLookEnabled)
        {
            //Vector2 mouseMove = new Vector2(Camera_Sensitivity * Input.GetAxis("Mouse X"), Camera_Sensitivity * Input.GetAxis("Mouse Y"));
            //
            //var rotationLerpPct = 1.0f - Mathf.Exp((Mathf.Log(1f - 0.99f) / m_RotationLerpTime) * Time.deltaTime);
            //Vector3 cameraRotDelta = new Vector3(mouseMove.y * -1.0f, mouseMove.x, 0.0f) * m_LookSpeed;
            //Vector3 cameraRotCurrent = camera.transform.rotation.eulerAngles;
            //Vector3 cameraRotLerp = new Vector3(Mathf.Lerp(cameraRotCurrent.x, cameraRotCurrent.x + cameraRotDelta.x, rotationLerpPct),
            //                                    Mathf.Lerp(cameraRotCurrent.y, cameraRotCurrent.y + cameraRotDelta.y, rotationLerpPct),
            //                                    Mathf.Lerp(cameraRotCurrent.z, cameraRotCurrent.z + cameraRotDelta.z, rotationLerpPct));
            //
            ////we don't want any z rotation
            //cameraRotLerp.z = 0;
            //
            ////get rid of dead-zones
            //if(cameraRotLerp.x > 260.0f && cameraRotLerp.x < 275.0f)
            //{
            //    cameraRotLerp.x = 275.01f;
            //}
            //else if (cameraRotLerp.x > 60.0f && cameraRotLerp.x < 70.0f)
            //{
            //    cameraRotLerp.x = 59.99f;
            //}
            //
            //camera.transform.eulerAngles = cameraRotLerp;
            //Vector3 failure = Vector3.zero;
            //failure.y = camera.transform.eulerAngles.y;
            //
            //if (m_MovementState != MovementState.WallRunning)
            //transform.rotation = Quaternion.Euler(failure);
            //
            //float temp = camera.transform.localEulerAngles.x;
            //failure = Vector3.zero;
            //failure.x = temp;
            //camera.transform.localEulerAngles = failure;

            // Alex: Simple camera rotation
            Vector2 MouseInput = Controller.GetLookInput();
            m_Rotation.y += MouseInput.y * Camera_Sensitivity; // Setting rotation values also based on sensitivity 
            m_Rotation.x += MouseInput.x * Camera_Sensitivity;

            m_Rotation.x = Mathf.Clamp(m_Rotation.x, min, max); // Creating a clamp for x, min max can be changed in inspector

            Quaternion rot = Quaternion.Euler(-m_Rotation.x, m_Rotation.y, 0);
            m_Camera.transform.rotation = rot;
        }
    }
    void HandleState()
    {
        Vector3 localMoveDir = Controller.GetMoveInput();
        //Vector3 rotation = Controller.GetLookInput();
        //rotation.x = 0.0f;
        //transform.Rotate(rotation, Time.fixedDeltaTime * 120);        

        switch (m_MovementState)
        {
            case MovementState.OnGround:
                UpdateOnGround(localMoveDir, false);
                //transform.Rotate(rotation, Time.fixedDeltaTime * 120);
                m_JumpUsed = false;
                break;
            case MovementState.InAir:
                UpdateInAir(localMoveDir);
                //transform.Rotate(rotation, Time.fixedDeltaTime * 120);
                break;
            case MovementState.Disable:
                break;
            case MovementState.Crouch:
                //transform.Rotate(rotation, Time.fixedDeltaTime * 120);
                break;
            case MovementState.WallRunning:
                break;
            default:
                DebugUtils.LogError("Invalid Movement State: (0)", m_MovementState);
                break;
        }
    }

    void HandleJumping()
    {
        if (m_IsJumpQueued)
        {
            if (m_MovementState == MovementState.OnGround || m_MovementState == MovementState.WallRunning)
            {
                m_JumpUsed = true;
                if (m_MovementState != MovementState.WallRunning)
                    ActivateJump();
            }
            else if(m_MovementState == MovementState.InAir && !m_DoubleJumpUsed)
            {
                ActivateJump();
                m_DoubleJumpUsed = true;
            }
            m_IsJumpQueued = false;
        }
    }

    public enum MovementState
    {
        OnGround,
        InAir,
        Crouch,
        WallRunning,
        Disable
    }

    void UpdateGroundInfo()
    {
        GroundAngularVelocity = Vector3.zero;
        GroundVelocity = Vector3.zero;
        GroundNormal.Set(0.0f, 0.0f, 1.0f);

        m_CenterHeight = transform.position.y;
        float footheight = FootLocationObj.transform.position.y;

        float halfCapsuleHeight = m_CenterHeight - footheight;

        Vector3 rayStart = transform.position;
        rayStart.y += GroundCheckStartOffsetY;

        Vector3 rayDir = Vector3.down;

        float rayDist = halfCapsuleHeight + GroundCheckStartOffsetY - CheckForGroundRadius;

        RaycastHit[] hitInfos = Physics.SphereCastAll(rayStart, CheckForGroundRadius, rayDir, rayDist, m_GroundCheckMask);

        RaycastHit groundHitInfo = new RaycastHit();
        bool validGroundFound = false;
        float minGroundDistance = float.MaxValue;

        foreach(RaycastHit hitInfo in hitInfos)
        {
            float surfaceAngle = MathUtils.CalcVerticalAngle(hitInfo.normal);
            if(surfaceAngle < MinAllowedSurfaceAngle || hitInfo.distance <= 0)
            {
                continue;
            }

            if(hitInfo.distance < minGroundDistance)
            {
                minGroundDistance = hitInfo.distance;

                groundHitInfo = hitInfo;
                validGroundFound = true;
            }
        }

        if(!validGroundFound)
        {
            if(m_MovementState == MovementState.OnGround)
            {
                SetMovementState(MovementState.InAir);
            }
             return;
        }

        GroundNormal = groundHitInfo.normal;
        if (m_MovementState == MovementState.InAir)
        {
            SetMovementState(MovementState.OnGround);
        }
    }

    void UpdateOnGround(Vector3 localMoveDir, bool isJumping)
    {

        if (localMoveDir.sqrMagnitude > MathUtils.CompareEpsilon)
        {
            Vector3 localVel = m_Velocity - GroundVelocity;
            Vector3 moveAccel = CalcMoveAccel(localMoveDir);

            Vector3 groundTangent = moveAccel - Vector3.Project(moveAccel, GroundNormal);
            groundTangent.Normalize();

            moveAccel = groundTangent;

            Vector3 velAlongMoveDir = Vector3.Project(localVel, moveAccel);

            if(Vector3.Dot(velAlongMoveDir, moveAccel) > 0.0f)
            {
                localVel = MathUtils.LerpTo(OnGroundStopEaseSpeed, localVel, velAlongMoveDir, Time.fixedDeltaTime);
            }
            else
            {
                localVel = MathUtils.LerpTo(OnGroundStopEaseSpeed, localVel, Vector3.zero, Time.fixedDeltaTime);
            }

            if (Controller.IsSprinting())
            {
                moveAccel *= (OnGroundMoveAccel * SprintMult);
                localVel += moveAccel * Time.fixedDeltaTime;

                localVel = Vector3.ClampMagnitude(localVel, OnGroundMaxSpeed * SprintMult);

            }
            else
            {
                moveAccel *= OnGroundMoveAccel;
                localVel += moveAccel * Time.fixedDeltaTime;

                localVel = Vector3.ClampMagnitude(localVel, OnGroundMaxSpeed);

            }

            m_Velocity = localVel + GroundVelocity;
        }
        else
        {
            UpdateStopping(OnGroundStopEaseSpeed);
        }

        ApplyVelocity(m_Velocity);
        //so that turret can access players speed
        GroundVelocity = m_Velocity;
    }

    void UpdateInAir(Vector3 localMoveDir)
    {
        //Vector3 localVel = m_Velocity - AirVelocity;
        //Vector3 moveAccel = CalcMoveAccel(localMoveDir);

        //Vector3 velAlongMoveDir = Vector3.Project(localVel, moveAccel);

        //localVel = MathUtils.LerpTo(OnGroundMaxSpeed, localVel, velAlongMoveDir, Time.fixedDeltaTime);

        //moveAccel *= OnGroundMoveAccel;

        //localVel += moveAccel * Time.fixedDeltaTime;

        //localVel = Vector3.ClampMagnitude(localVel, OnGroundMaxSpeed);
        //m_Velocity.x = localVel.x;
        //m_Velocity.z = localVel.z;

        //ApplyVelocity(m_Velocity);
    }

     public void UpdateStopping(float stopEaseSpeed)
    {
        m_Velocity = MathUtils.LerpTo(stopEaseSpeed, m_Velocity, GroundVelocity, Time.fixedDeltaTime);
    }

    public void ApplyVelocity(Vector3 velocity)
    {
        Vector3 velocityDiff = velocity - m_Rigidbody.velocity;
        m_Rigidbody.AddForce(velocityDiff, ForceMode.VelocityChange);
    }

    public void ActivateJump()
    {
        m_Rigidbody.velocity = Vector3.zero;

        Vector3 forward = Controller.GetMoveInput();

        if (forward.y == 0)
        {
            forward.y = 1;
        }

        if(Controller.IsSprinting())
        {
            forward.Scale(new Vector3(SprintMult, SprintMult * 0.5f, SprintMult));
        }

        forward *= JumpStrength;
        m_Rigidbody.AddRelativeForce(forward, ForceMode.Impulse);
    }

    //transforms your vector into world coordinates
    public Vector3 CalcMoveAccel(Vector3 localMoveDir)
    {
        return transform.TransformDirection(localMoveDir);
    }

    public void SetMovementState(MovementState movementState)
    {
        switch(movementState)
        {
            case MovementState.OnGround:
                m_DoubleJumpUsed = false;
                break;
            case MovementState.InAir:
                break;
            case MovementState.Disable:
                m_Velocity = Vector3.zero;
                ApplyVelocity(m_Velocity);
                break;
            case MovementState.Crouch:
                break;
            case MovementState.WallRunning:
                break;
            default:
                DebugUtils.LogError("Invalid Movement State: (0)", movementState);
                break;
        }

        m_MovementState = movementState;
    }

    //Sets our respawnpoint, deactivates previous respawn point if any
    public void SetRespawnPoint(RespawnPoint resp)
    {
        if (m_Respawnpoint != null)
        {
            m_Respawnpoint.SetInactive();
        }

        m_Respawnpoint = resp;
    }

    private void OnGUI()
    {
    }

    //Starts a coroutine to respawn the player after a certain time passes
    public void OnDeath()
    {
        if (!m_StopPlaying)
        {
            GetComponent<AudioSource>().Play(0);
            m_StopPlaying = true;
        }

        StartCoroutine(DeathDelay());
    }

    IEnumerator DeathDelay()
    {
        yield return new WaitForSeconds(m_RespawnDelay);

        Respawn();
    }

    //respawns the player
    private void Respawn()
    {

        GameObject temp = GameObject.Find("CanvasObj");
        m_Velocity = Vector3.zero;

        if (temp)
        {
            CanvasGroup can = temp.GetComponentInChildren<CanvasGroup>();
            can.alpha = 0;
        }

        //Move to wherever our respawn point is
        if (m_Respawnpoint)
        {
            gameObject.transform.position = m_Respawnpoint.transform.position;
        }
        else
        {
            //TODO Make this default to the level start position
            gameObject.transform.position = Vector3.zero;
        }

        //Set Health Back to normal cause we're respawning
        HealthComponent health = GetComponent<HealthComponent>();
        if (health != null)
        {
            health.Reset();
        }
    }

    bool SetupHumanPlayer()
    {
        if(LevelManager.Instance.GetPlayer() == null)
        {
            DontDestroyOnLoad(gameObject);
            LevelManager.Instance.RegisterPlayer(this);

            Controller = new MouseKeyPlayerController();
            return true;
        }
        else
        {
            Destroy(gameObject);
            return false;
        }
    }

    public MovementState m_MovementState;
    Rigidbody m_Rigidbody;
    public Vector3 m_Velocity;
    float m_CenterHeight;
    int m_GroundCheckMask;
    public bool m_IsJumpQueued = false;
    public bool m_JumpUsed = false;
    public bool m_DoubleJumpUsed = false;
    private RespawnPoint m_Respawnpoint = null;
    private HealthComponent m_HealthComp;
    public float m_LookSpeed = 2.0f;
    public float m_RotationLerpTime = 0.01f;
    public float m_RespawnDelay =5.0f;
    public bool m_MouseLookEnabled = true;

    bool m_StopPlaying = false;
}
