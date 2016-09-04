using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerPoints : MonoBehaviour 
{
    //area of point display
    [SerializeField] Canvas pointDisplay;
    //value of point to display
    [SerializeField] Text pointValue;
    //animator on canvas of point display
    private Animator anim;

    void Start()
    {
        anim = pointDisplay.GetComponent<Animator>();
    }

    void Update()
    {
        //display point value on the addition of points
        if(GameControl.control.DetermineWhetherNewPoints())
        {
            float points = GameControl.control.GetRecentPoints();

            if(points == 50)
                pointValue.color = ColorControl.color.health;
            else if(points == 75)
                pointValue.color = ColorControl.color.danger;
            else if(points == 100)
                pointValue.color = ColorControl.color.immunity;
            else
                pointValue.color = ColorControl.color.standard;

            pointValue.text = points.ToString();
            anim.SetTrigger("DisplayPoints");
        }
    }
}