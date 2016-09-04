using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Thruster
{
    public ParticleSystem thruster;
    public GameObject trail;
}

[System.Serializable]
public class Axillary
{
    public Thruster primary;
    public Thruster secondary;
}

[System.Serializable]
public class ThrusterGroup
{
    public Thruster main;
    public Axillary right;
    public Axillary left;
}

public class PlayerControl : MonoBehaviour 
{
    //speed of reorientation            
    private float sensitivity = 0.5f;
    //minimum change before reorientation
    private const float orientThreshold = 0.025f;
    //minimum change before reorientation
    private const float thrustThreshold = 0.045f;
    //currently active thruster group
    private ThrusterGroup current;
    private ThrusterGroup recent;
    //thruster groups based on player status
    [SerializeField] ThrusterGroup standard;
    [SerializeField] ThrusterGroup immunity;
    [SerializeField] ThrusterGroup critical;
    //whether player is boosting
    private bool boosting = false;
    private bool damaged = false;
    //whether orientation is altering thrusters
    private bool canThrust = true;    

    void Start()
    {
        GameControl.control.player = gameObject;
        sensitivity = GameControl.control.GetSensitivity();

        //ensure only standard thruster group is active
        SetGroup(immunity, critical);        
        SetGroup(standard, immunity);
        current = standard;
    }
    
    void FixedUpdate()
    {
        //continuous forward movement
        transform.rigidbody2D.velocity = transform.up * GameControl.control.GetMoveSpeed();
        //reorientation of player in relation to accelerometer input
        OrientPlayer();

        if(GameControl.control.GetHealth() <= GameControl.control.GetDamage())
        {
            if(!damaged)
            {
                damaged = true;
                recent = current;
                current = critical;
                SetGroup(current, recent);
            }
        }
        else
        { 
            if(damaged)
            {
                damaged = false;
                recent = current;
                current = standard;
                SetGroup(current, recent);
            }
        }

        //boost animation upon gaining immunity
        if(GameControl.control.GetImmunity())
        {
            if(!boosting)
            {
                boosting = true;
                recent = current;
                current = immunity;
                SetGroup(current, recent);              
            }
        }
        else
        {
            if(boosting)
            {
                boosting = false;
                recent = current;
                current = standard;
                SetGroup(current, recent);               
            }
        }
    }

    private void OrientPlayer()
    {
        //desired direction of player movement
        Vector2 direction = new Vector2(GameControl.control.direction, -1);

        //calculate the difference between global up and desired direction
        float angle = Vector3.Angle(Vector3.up, -direction);

        //invert angle when gravity along x-axis is negative
        if(direction.x < 0) angle = -angle;  
        
        //convert required angle into Quaternion for transforming rotation
        Quaternion orientation = Quaternion.Euler(0, 0, angle);

        //calculate difference between current and desired rotation
        float change = transform.rotation.z - orientation.z;

        if(Mathf.Abs(change) - orientThreshold > 0)
        {
            //rotate player from current rotation to new orientation
            transform.rotation = Quaternion.Lerp(transform.rotation, orientation, sensitivity);
        }

        if(canThrust)
        {
            if(Mathf.Abs(change) - thrustThreshold > 0)
            {
                //provide visual feedback on reorientation direction
                StartCoroutine(DetermineAxillaryState(change));
            }
            else
            {
                SetThruster(current.right.primary, current.right.secondary);
                SetThruster(current.left.primary, current.left.secondary);
            }
        }
    }

    private IEnumerator DetermineAxillaryState(float change)
    {
        canThrust = false;

        if(change < 0)
        {
            SetThruster(current.right.secondary, current.right.primary);
            SetThruster(current.left.primary, current.left.secondary);
        }
        else if(change > 0)
        {
            SetThruster(current.left.secondary, current.left.primary);
            SetThruster(current.right.primary, current.right.secondary);
        }

        yield return new WaitForSeconds(0.33f);

        canThrust = true;

        yield return null;
    }

    private void SetGroup(ThrusterGroup active, ThrusterGroup inactive)
    {
        //toggle main thrusters
        active.main.thruster.Play(true);
        active.main.trail.SetActive(true);

        inactive.main.thruster.Stop(true);
        inactive.main.trail.SetActive(false);

        //deactivate all by primary axillary of current active
        SetThruster(inactive.right.primary, inactive.right.secondary);
        SetThruster(active.right.secondary, inactive.right.primary);
        SetThruster(active.right.primary, active.right.secondary);

        SetThruster(inactive.left.primary, inactive.left.secondary);
        SetThruster(active.left.secondary, inactive.left.primary);
        SetThruster(active.left.primary, active.left.secondary);
    }

    private void SetThruster(Thruster active, Thruster inactive)
    {
        active.thruster.Play(true);
        inactive.thruster.Stop(true);

        active.trail.SetActive(true);
        inactive.trail.SetActive(false);
    }
}