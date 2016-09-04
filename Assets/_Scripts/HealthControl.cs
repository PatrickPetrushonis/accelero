using UnityEngine;
using System.Collections;

public class HealthControl : MonoBehaviour 
{
    //reference to destroy effect
    private PoolControl pool;
    //effect upon destruction
    [SerializeField] int effectIndex = 5;
    //current health of player
    private float health;
    //maximum and starting health value 
    private float maxHealth;
    //health regained each second while below maximum
    private const float healthRegen = 5.0f;

    void Start()
    {
        pool = GameObject.Find("GameManager").GetComponent<PoolControl>();

        maxHealth = GameControl.control.GetMaxHealth();
        GameControl.control.ChangeHealth(maxHealth);
    }

    void Update()
    {
        if(GameControl.control.destroyed) Destroy();

        health = GameControl.control.GetHealth();

        if(health <= 0)
        {
            //prevent negative health then destroy player
            health = 0;
            GameControl.control.destroyed = true;
        }
        else if(health >= maxHealth) health = maxHealth;
        else if(health < maxHealth) health += healthRegen * Time.deltaTime;

        //update game control health with difference between local health value and reference           
        GameControl.control.ChangeHealth(health - GameControl.control.GetHealth());
    }

    //deactivate player and trigger game over
    private void Destroy()
    {

        pool.ActivateEffect(transform, effectIndex);

        //disabling vibration until able to control duration and intensity
        //Handheld.Vibrate();

        GameControl.control.gameOver = true;
        gameObject.SetActive(false);
    }
}