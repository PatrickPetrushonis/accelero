using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIController : MonoBehaviour 
{
    //canvas of scene
    [SerializeField] GameObject canvas;
    //heads-up-display menu (child of canvas)
    [SerializeField] GameObject HUD;
    //game over menu (child of canvas)
    [SerializeField] GameObject gameOver;
    //current and continuously updated score value    
    [SerializeField] Text score;
    //final stats for current game instance (final score and time)
    [SerializeField] Text finalStat;
    //health display for current health out of maximum
    [SerializeField] Slider healthSlider;
    [SerializeField] Image health;
    //immunity display for remaining duration of immunity
    [SerializeField] Slider immunitySlider;
    [SerializeField] Image immunity;
    //damage flash indicator
    [SerializeField] Image damageFlash;
    private float flashSpeed = 3.0f;
    //animator component on canvas
    private Animator anim;

    void Awake()
    {
        anim = canvas.GetComponent<Animator>();
    }

    void Update()
    {
        if(GameControl.control.gameOver)
        {
            //set final stat to score and time of current game instance 
            finalStat.text = (int)GameControl.control.GetScore() + "\n" + GameControl.control.GetFormattedTime();

            //activate game over screen and animation
            gameOver.SetActive(true);
            anim.SetTrigger("GameOver");

            if(GameControl.control.newHighScore) anim.SetTrigger("HighScore");
        }
        else
        {
            //update score while game is active
            score.text = ((int)GameControl.control.GetScore()).ToString();

            if(GameControl.control.damagedFlash)
            {
                GameControl.control.damagedFlash = false;
                damageFlash.color = ColorControl.color.flash;                
            }
            else
            {
                damageFlash.color = Color.Lerp(damageFlash.color, Color.clear, flashSpeed * Time.deltaTime);
            }

            healthSlider.value = GameControl.control.GetHealth();

            if(GameControl.control.GetImmunity())
                health.color = ColorControl.color.immunity;
            else if(healthSlider.value <= GameControl.control.GetDamage())
                health.color = ColorControl.color.danger;
            else
                health.color = ColorControl.color.health;
        }
    }

    //local calls to game control functions
    public void LoadLevel(int index)
    {
        GameControl.control.LoadLevel(index);
    }

    public void ToggleActivate(GameObject toToggle)
    {
        GameControl.control.ToggleActivate(toToggle);
    }

    public void TogglePause(bool toPause)
    {
        GameControl.control.SetPause(toPause);
    }
}