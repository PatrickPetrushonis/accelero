using UnityEngine;
using System.Collections;

public class FlightControl : MonoBehaviour 
{
    private Gyroscope gyro;
    //current acceleration of device
    private Vector2 currentAcc;
    //current horizontal axes input       
    private float direction = 0f;
    //whether orientation is inverted in relation to accelerometer input
    private float inversion = 1f;
    //sensitivity of horizontal axis    
    private const float sensitivityH = 5f;
    //maximum turn limit
    private const float threshold = 0.65f;  

    void Start()
    {
        if(SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;
            gyro.enabled = true;
            currentAcc = GetAccelerometer(currentAcc);            
        }        
    }

    void OnLevelWasLoaded(int level)
    {
        if(level == Application.loadedLevel)
        {
            if(GameControl.control.GetControlInversion()) 
                inversion = 1f;
            else 
                inversion = -1f;
        }
    }

    void FixedUpdate()
    {
        DetermineDirection();
        LimitDirection();
        GameControl.control.direction = direction;
    }

    private void DetermineDirection()
    {
        //device acceleration over time
        Vector3 acceleration = Input.acceleration;
        //acceleration adjusted in respect to callibration
        currentAcc = GetAccelerometer(acceleration);
        //horizontal input clamped within range
        direction = Mathf.Clamp(currentAcc.x * sensitivityH * inversion, -1, 1);
    }

    private Vector3 GetAccelerometer(Vector3 accelerator)
    {
        Vector3 accel = GameControl.control.GetMatrix().MultiplyVector(accelerator);
        return accel;
    }

    private void LimitDirection()
    {        
        //prevent turns exceeding specified threshold
        if(direction > threshold) direction = threshold;
        else if(direction < -threshold) direction = -threshold;
    }
}