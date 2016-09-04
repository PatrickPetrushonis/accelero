using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuControl : MonoBehaviour 
{
    //current saved high scores and times
    [SerializeField] Text record;
    //control sensitivity
    [SerializeField] Slider sensitivity;
    //game volume
    [SerializeField] Toggle volume;    
    //control inversion
    [SerializeField] Toggle invert;
    //formatted records for longest and total time
    private string timeMax;
    private string timeTotal;

    void Start()
    {
        DetermineRecords();
        SetVolume();

        sensitivity.value = GameControl.control.GetSensitivity();
        invert.isOn = GameControl.control.GetControlInversion();
    }

    private void DetermineRecords()
    {
        timeMax = GameControl.control.FormatTime(GameControl.control.GetTimeMax());
        timeTotal = GameControl.control.FormatTime(GameControl.control.GetTimeTotal());

        record.text = (int)GameControl.control.GetScoreMax() + "\n" + timeMax + "\n" + timeTotal;
    }

    private void SetVolume()
    {
        volume.isOn = GameControl.control.GetVolume();
    }

    public void Settings(bool toUpdate)
    {
        if(toUpdate)
        {
            GameControl.control.ToggleVolume();
            GameControl.control.SetSensitivity(sensitivity.value);
            GameControl.control.SetControlInversion(Convert.ToInt32(invert.isOn));
        }
        else
        {
            SetVolume();
            sensitivity.value = GameControl.control.GetSensitivity();
            invert.isOn = GameControl.control.GetControlInversion();
        }        
    }

    public void ResetRecords()
    {
        GameControl.control.ResetRecords();
        DetermineRecords();
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