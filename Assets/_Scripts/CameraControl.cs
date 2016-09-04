using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour 
{
    //a camera within the the scene
    private Camera thisCamera;
    //transform of this camera
    private Transform thisTransform;
    //look at target for this camera
    private Transform target;

    //multiplier scale for distance control
    private const float scale = 0.5f;
    //imported pixel size of sprites
    private const float pixelHeight = 64f;
    //distance from player along z-axis (must be greater than 1f)
    private const float distance = 10f;
    //distance offset from player along y-axis
    private const float offset = 5f;

    //constant orthographic size regardless of screen resolution
    private const float orthoSize = 18f;

    //camera shake amount and degradation
    private float shakeIntensity = 0.0f;
    private float shakeDecay = 0.0f;
    private const float shakeValue = 0.065f;
    private const float decayValue = 0.002f;
    private bool shaking = false;
    //original rotation to return to post shake
    private Quaternion originRotation;

    void Start()
    {
        //distinguish this camera as the main camera
        thisCamera = Camera.main;
        thisTransform = thisCamera.transform;
        originRotation = thisTransform.localRotation;

        //dynamically set orthographic size based on screen height to ensure pixel perfect sprites
        thisCamera.orthographicSize = orthoSize/*Screen.height / (pixelHeight * scale) / 2f*/;

        //set camera target to position of player
        if(GameControl.control.player) target = GameControl.control.player.transform;
    }

    void LateUpdate()
    {
        //follow target with specified offset
        if(target != null) thisTransform.position = new Vector3(target.position.x, target.position.y + offset, -distance);

        if(GameControl.control.damagedShake && !shaking)
        {
            GameControl.control.damagedShake = false;
            shaking = true;
            shakeIntensity = shakeValue;
            shakeDecay = decayValue;
        }

        //provide degrading shake over random range
        if(shaking)
        {
            if(shakeIntensity > 0.0f)
            {
                thisTransform.localRotation = new Quaternion(originRotation.x + Random.Range(-shakeIntensity, shakeIntensity) * 0.2f,
                                                             originRotation.y + Random.Range(-shakeIntensity, shakeIntensity) * 0.2f,
                                                             originRotation.z + Random.Range(-shakeIntensity, shakeIntensity) * 0.2f,
                                                             originRotation.w + Random.Range(-shakeIntensity, shakeIntensity) * 0.2f);

                shakeIntensity -= shakeDecay;
            }
            else
            {
                shaking = false;
                thisTransform.localRotation = originRotation;
            } 
        }        
    }
}