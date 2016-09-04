using UnityEngine;
using System.Collections;

public class Destroyer : MonoBehaviour 
{
    //time before object is destroyed
    public float time = 1.5f;

    void OnEnable()
    {
        //deactivate this gameobject after specified amount of time
        Invoke("Deactivate", time);
    }

    void OnDisable()
    {
        CancelInvoke("Deactivate");
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }
}