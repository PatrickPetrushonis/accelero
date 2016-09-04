using UnityEngine;
using System;
using System.Collections;

public class Deactivator : MonoBehaviour 
{
    //reference to destroy effect
    private PoolControl pool;
    //this object's tag for determining functionality
    private string thisTag;
    //effect upon collision with player
    [SerializeField] int effectIndex = 0;
    //index of points if collectible
    [SerializeField] int valueIndex = 0;
    //amount of points and heal or amount to damage
    private float value = 0.0f;
    //whether this will heal on contact (only applicable to collectible)
    [SerializeField] bool heal = false;
    //whether thus grant immunity to the player
    [SerializeField] bool immune = false;
    //whether object will move
    [SerializeField] bool mobile = false;
    //speed in which a mobile object moves
    [SerializeField] float moveSpeed = 3f;    

    void Start()
    {
        pool = GameObject.Find("GameManager").GetComponent<PoolControl>();

        thisTag = gameObject.tag;

        if(thisTag == "Enemy")
            value = GameControl.control.GetDamage();
        else
            value = GameControl.control.GetPointValue(valueIndex);
    }

    void OnEnable()
    {
        if(mobile)
        {
            float vertical, horizontal = UnityEngine.Random.Range(-1, 1);

            //ensure there is always vertical movement
            do { vertical = UnityEngine.Random.Range(-1, 1); }
            while(vertical == 0);

            transform.rigidbody2D.AddForce(new Vector2(horizontal, vertical) * moveSpeed, ForceMode2D.Impulse);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //tag of collided object
        string otherTag = other.gameObject.tag;

        if(otherTag == "Player")
        {
            if(thisTag == "Collectible")
            {  
                GameControl.control.IncreaseScore(value);
                if(heal) GameControl.control.ChangeHealth(value);
                if(immune) GameControl.control.ActivateImmunity();
            }
            else if(thisTag == "Enemy")
            {
                //only damage player if player is alive
                if(!GameControl.control.destroyed)
                {                                      
                    if(!GameControl.control.GetImmunity())
                    {
                        GameControl.control.ChangeHealth(-value);
                        GameControl.control.IsDamaged();
                    }
                    else
                    {
                        GameControl.control.IncreaseScore(value);  
                    }
                } 
            }

            //only create final effect (explosion) on contact with player
            pool.ActivateEffect(transform, effectIndex);
        }

        //prevent objects from destroying one another
        if(otherTag != "Collectible" & otherTag != "Enemy")
        {
            gameObject.SetActive(false);
        }        
    }
}
