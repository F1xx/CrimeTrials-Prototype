using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public GameObject Player;
    public TurretProjectile Projectile;
    public GameObject ProjectileSpawnPoint;
    Vector3 PlayerPos;
    float MaxDistFromPlayer = 20.0f;
    float DistFromPlayer;
    Vector3 CurrentPlayerSpeed;
    GameObject shootTimerObject;
    ShootTimer shootTimer;
    LayerMask PlayerLayer;
    Vector3 TurretPos;
    public Vector3 TurretDir { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.Find("Player");
        PlayerPos = Vector3.zero;
        PlayerLayer = 1 << 9;

        shootTimerObject = GameObject.Find("ShootTimer");
        shootTimer = shootTimerObject.GetComponent<ShootTimer>();

        m_bIsFiring = false;
        m_bIsPlayerStopped = false;

        TurretPos = transform.position;
        TurretDir = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerPos = Player.transform.position;
        CurrentPlayerSpeed = Player.GetComponent<Rigidbody>().velocity;
        TurretDir = PlayerPos - TurretPos;
        TurretDir.Normalize();
        //print("TurretDir = " + TurretDir);
        IsPlayerStopped();

        if (IsPlayerClose() && Player.GetComponent<HealthComponent>().IsAlive())
        {
            if(m_bIsPlayerStopped == false)
            {
                RotateTurret(PlayerPos);
                shootTimer.StopTimerEarly();
                m_bIsFiring = false;
                //print("tracking player");
            }
            else
            {
                //dont fire if not already firing dummy
                if(m_bIsFiring == false && CanSeePlayer())
                {
                    Fire();
                    //print("FIRE");
                }
                else
                {
                    if (shootTimer.bHasTimerEnded == true)
                    {
                        RaycastHit hit;
                        Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, PlayerLayer);
                        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 10.0f, Color.red);

                        TurretProjectile bullet = Instantiate(Projectile, ProjectileSpawnPoint.transform.position, transform.rotation) as TurretProjectile;
                        Rigidbody bulletrigidbody = bullet.GetComponent<Rigidbody>();
                        bulletrigidbody.AddForce(TurretDir * 500);
                        shootTimer.StopTimer();
                        m_bIsFiring = false;
                    }
                }
            }

        }
    }

    void Fire()
    {
        //print("FIRE");

        m_bIsFiring = true;
        shootTimer.StartTimer();
    }

    bool IsPlayerClose()
    {
        DistFromPlayer = Vector3.Distance(PlayerPos, transform.position);
        //print("Distance to other: " + DistFromPlayer);
        if (DistFromPlayer <= MaxDistFromPlayer)
        {
            return true;
        }

        return false;
    }

    bool CanSeePlayer()
    {
        Vector3 dir = (Player.transform.position - transform.position).normalized;

        LayerMask mask = ~LayerMask.GetMask("Player");

        return Physics.Raycast(transform.position, dir, Mathf.Infinity, mask);
    }

    void IsPlayerStopped()
    {
        if(CurrentPlayerSpeed != Vector3.zero)
        {
            m_bIsPlayerStopped = false;
        }
        else
        {
            m_bIsPlayerStopped = true;
        }
    }

    void RotateTurret(Vector3 playerPos)
    {
        transform.LookAt(playerPos, Vector3.up);
    }


    bool m_bIsFiring;
    bool m_bIsPlayerStopped;
}
