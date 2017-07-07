using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GameControl : MonoBehaviour 
{
    //reference to self for static singleton
    public static GameControl control;
    //internal class references
    private ChallengeFactor factor = new ChallengeFactor();
    private GameData data = new GameData();
    private Record record = new Record();

    //value score will increase each second
    private const float scoreMultiplier = 10f;
    //record and format final score and time
    private bool finalize = false;

    //reference to player gameobject
    public GameObject player;
    //horizontal accelerometer input 
    public float direction;

    //exceeded the previous high score
    public bool newHighScore = false;
    //status of player
    public bool destroyed = false;
    public bool damagedFlash = false;
    public bool damagedShake = false;
    //status of game state
    public bool gameOver = false;
    public bool pause = false;    

    void Awake()
    {
        //psuedo-singleton design
        if(control == null)
        {
            DontDestroyOnLoad(gameObject);
            control = this;
        }
        else if(control != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {        
        //ensure device screen will not timeout while playing
        Screen.sleepTimeout = (int)SleepTimeout.NeverSleep;
        Application.runInBackground = true;
        data.moveSpeed = data.initialSpeed;
    }

    void OnEnable()
    {
        // Add OnLevelFinishedLoading to scene loaded delegate
        SceneManager.sceneLoaded += OnLevelFinishedLoading;

        //load all record data
        LoadData();

        //load player settings
        if(PlayerPrefs.HasKey("volume")) { data.volume = Convert.ToBoolean(PlayerPrefs.GetFloat("volume")); }
        if(PlayerPrefs.HasKey("sensitivity")) { SetSensitivity(PlayerPrefs.GetFloat("sensitivity")); }
        if(PlayerPrefs.HasKey("inversion")) { SetControlInversion(PlayerPrefs.GetInt("inversion")); }
    }

    void OnDisable()
    {
        // Unsubscribe sceneloaded delegate from OnLevelFinishedLoading
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        //ensure game is unpaused
        if(scene.name == Application.loadedLevelName) { SetPause(false); }
    }

    void FixedUpdate()
    {
        //continously update score while game is not over
        if(!gameOver)
        {
            data.score += Time.deltaTime * scoreMultiplier;
            data.time = Time.timeSinceLevelLoad;
            IncrementChallenge();
        }
        else if(!finalize)
        {            
            //only finalize end time once
            finalize = true;
            //save time and score data
            SaveData();
            //get time since start and format to string  
            data.timeFormat = FormatTime(data.time);            
        }
    }

    //pause or unpause game (true to pause)
    public void SetPause(bool toPause)
    {
        if(toPause)
        {
            pause = true;
            Input.gyro.enabled = false;
            Time.timeScale = 0.0f;
        }
        else
        {
            pause = false;
            Input.gyro.enabled = true;
            Time.timeScale = 1.0f;
            CalibrateAccelerometer();
        }
    }

    //load level respective to passed value (-1 for current level, -2 to quit application)
    public void LoadLevel(int index)
    {        
        destroyed = false;
        damagedFlash = false;
        damagedShake = false;
        gameOver = false;
        finalize = false;
        newHighScore = false;

        data.isImmune = false;
        data.score = 0.0f;
        data.time = 0.0f;
        data.mostRecent = 0.0f;
        data.moveSpeed = data.initialSpeed;

        factor.challengeLevel = 0;
        factor.isComplete = false;

        if(index == -1)
            Application.LoadLevel(Application.loadedLevel);
        else if(index == -2)
            Application.Quit();
        else
            Application.LoadLevel(index);
    }

    //toggle gameobject activity
    public void ToggleActivate(GameObject toToggle)
    {
        toToggle.SetActive(!toToggle.activeSelf);
    }

    //convert and format a span of time into  
    public string FormatTime(float time)
    {
        TimeSpan span = TimeSpan.FromSeconds(time);
        int fractional = span.Milliseconds / 100;
        string format;

        if(span.Seconds > 3600)
            format = string.Format("{0:D2}:{1:D2}:{2:D2}.{3}", span.Hours, span.Minutes, span.Seconds, fractional);
        else if(span.Seconds > 600)
            format = string.Format("{0:D2}:{1:D2}.{2}", span.Minutes, span.Seconds, fractional);
        else
            format = string.Format("{0:D1}:{1:D2}.{2}", span.Minutes, span.Seconds, fractional);

        return format;
    }

    public void ResetRecords()
    {
        data.score = 0.0f;
        data.time = 0.0f;
        record.scoreMax = 0.0f;
        record.timeMax = 0.0f;
        record.timeTotal = 0.0f;
        SaveData();
    }

    private void CalibrateAccelerometer()
    {
        //current device acceleration
        Vector3 wantedDeadZone = Input.acceleration;
        //difference between zero acceleration and current value        
        Quaternion rotateQuaternion = Quaternion.FromToRotation(new Vector3(0, 0, -1f), wantedDeadZone);
        //creates a translation, rotation, scaling matrix
        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, rotateQuaternion, new Vector3(1, 1, 1));
        //calibrate offset by inverting this matrix
        data.calibrationMatrix = matrix.inverse;
    }

    //increase level of challenge at set increments
    private void IncrementChallenge()
    {
        if(!factor.isComplete)
        {
            if(data.time > factor.challengeIncrements[factor.challengeLevel])
            {
                factor.challengeLevel++;
                //increase player movement speed
                data.moveSpeed += data.challengeBoost;

                //stop incrementation after final level
                if(factor.challengeLevel >= factor.challengeIncrements.Length) 
                    factor.isComplete = true;
            }
        }
    }

    //serialize relevant information via binary formatter for data persistence
    private void SaveData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/gameData.dat");

        if(data.score > record.scoreMax)
        {
            record.scoreMax = data.score;
            newHighScore = true;
        }
        if(data.time > record.timeMax) record.timeMax = data.time;
        record.timeTotal += data.time;

        bf.Serialize(file, record);
        file.Close();
    }

    //deserialize stored data from persistent data path via binary formatter
    private void LoadData()
    {
        if(File.Exists(Application.persistentDataPath + "/gameData.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/gameData.dat", FileMode.Open);
            record = (Record)bf.Deserialize(file);
            file.Close();
        }
    }

    //challenge factor data get
    public int GetChallengeLevel() { return factor.challengeLevel; }

    //player data
    public float GetMoveSpeed() { return data.moveSpeed; }
    public float GetHealth() { return data.health; }
    public float GetMaxHealth() { return data.maxHealth; }
    public void ChangeHealth(float value)
    {        
        data.health += value;
    }
    

    //immunity
    public void ActivateImmunity()
    {
        if(!data.isImmune)
        {
            data.moveSpeed += data.boostSpeed;
            data.isImmune = true;
        }        



        data.immuneDeactiveTime = data.time;
        StartCoroutine(ImmunityCountDown(data.immuneDuration));
    }

    /*
     * set deactivation time = current time + duration
     * check if current time is equal to or greater than deactivation time
     * then deactivate immunity
     * immunitySlider.value equal to deactivation time - current time
    */


    private IEnumerator ImmunityCountDown(float duration)
    {
        yield return new WaitForSeconds(duration);

        if(data.time >= data.immuneDeactiveTime + duration)
        {
            data.isImmune = false;
            data.moveSpeed -= data.boostSpeed;
        }

        yield return null;
    }

    public bool GetImmunity() { return data.isImmune; }

    //damage related data
    public float GetDamage() { return data.damage; }

    public void IsDamaged()
    {
        damagedFlash = true;
        damagedShake = true;
    }

    //score data gets
    public float GetScore() { return data.score; }
    public float GetScoreMax() { return record.scoreMax; }

    //get value for collectible points and healing
    public float GetPointValue(int index) 
    {
        float value = 0;
        if(index < data.points.Length) value = data.points[index];
        return value;
    }

    //time data gets    
    public float GetTimeMax() { return record.timeMax; }
    public float GetTimeTotal() { return record.timeTotal; }
    public string GetFormattedTime() { return data.timeFormat; }

    //calibration matrix for current accelerometer calibration
    public Matrix4x4 GetMatrix() { return data.calibrationMatrix; }

    //volume
    public bool GetVolume() { return data.volume; }
    public void ToggleVolume() 
    {
        data.volume = !data.volume;
        int value = Convert.ToInt32(data.volume);
        PlayerPrefs.SetFloat("volume", value); 
    }

    //sensitivity
    public float GetSensitivity() { return data.sensitivity; }
    public void SetSensitivity(float value)
    {
        data.sensitivity = value;
        PlayerPrefs.SetFloat("sensitivity", value);
    }

    //controls
    public bool GetControlInversion() { return data.controlsInverted; }

    public void SetControlInversion(int value)
    {
        if(value != 0) 
            data.controlsInverted = true;
        else 
            data.controlsInverted = false;
        PlayerPrefs.SetInt("inversion", value);
    }

    //increase score through Deactivator
    public void IncreaseScore(float value) 
    {
        data.areNewPoints = true;
        data.mostRecent = value;
        data.score += value; 
    }

    //most recent points added to score
    public float GetRecentPoints()
    {
        data.areNewPoints = false;
        return data.mostRecent;
    }

    //whether new points were collected
    public bool DetermineWhetherNewPoints() { return data.areNewPoints; }    
}

//data indicators for current difficulty in respective categories
class ChallengeFactor
{
    //time values in which a challenge factor can increase
    public int[] challengeIncrements = { 30, 60, 90, 120, 150, 180, 210, 240 };
    public int challengeLevel = 0;
    public bool isComplete = false;
}

//persistent records of game achievements
[Serializable]
class Record
{
    public float scoreMax = 0.0f;
    public float timeMax = 0.0f;
    public float timeTotal = 0.0f;
}

//primary data storage
[Serializable]
class GameData
{
    //player max health
    public float health = 0.0f;
    public float maxHealth = 100f;

    //player immunity
    public float immuneDuration = 3.0f;
    public float immuneDeactiveTime = 0.0f;
    public bool isImmune = false;

    //player movement speed
    public float moveSpeed = 0.0f;
    public float initialSpeed = 12.0f;
    public float boostSpeed = 2.0f;
    public float challengeBoost = 0.75f;

    //damage inflicted by enemies
    public float damage = 75.0f;

    //collectible points and heal values
    public float[] points = { 50f, 100f, 150f };
    public float mostRecent;
    public bool areNewPoints = false;

    //current game score and time
    public float score = 0.0f;
    public float time = 0.0f;

    //time formatted into <minutes>:<seconds>
    public string timeFormat;

    //matrix representing callibration of accelerometer
    public Matrix4x4 calibrationMatrix;

    //game volume
    public bool volume = true;
    //control sensitivity
    public float sensitivity = 0.5f;
    //whether controls are inverted
    public bool controlsInverted = false;
}
