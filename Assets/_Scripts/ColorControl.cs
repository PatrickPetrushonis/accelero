using UnityEngine;
using System.Collections;

public class ColorControl : MonoBehaviour 
{
    //reference to self for static singleton
    public static ColorControl color;

    public Color32 standard = new Color32(225, 150, 50, 255);
    public Color32 health = new Color32(100, 175, 225, 255);
    public Color32 immunity = new Color32(255, 255, 255, 200);
    public Color32 danger = new Color32(245, 100, 100, 255);
    public Color32 flash = new Color32(200, 0, 0, 50);

    void Awake()
    {
        //psuedo-singleton design
        if(color == null)
        {
            DontDestroyOnLoad(gameObject);
            color = this;
        }
        else if(color != this)
        {
            Destroy(gameObject);
        }
    }
}