using UnityEngine;
using System.Collections;
using System;

public class WorldTickController : MonoBehaviour
{

    private bool GameStarted = false;
    private float startTime;

    private int Tick = 0;
    public delegate void NewTickEventHandle();
    public event NewTickEventHandle TickEvent = delegate { }; // add empty delegate so the TickEvent can be called without any observers. Source: http://stackoverflow.com/questions/340610/create-empty-c-sharp-event-handlers-automatically/340618#340618

    private int HalfTick = 0;
    public delegate void NewHalfTickEventHandle();
    public event NewHalfTickEventHandle HalfTickEvent = delegate { };

    public static WorldTickController instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
        {
            Debug.LogError("Tried to created another instance of " + GetType() + ". Destroying.");
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    private void Start ()
    {
	    
	}

    // Update is called once per frame
    private void Update ()
    {
        // TODO MIGHT WANT TO LIMIT HOW OFTEN THIS RUN
        //InvokeRepeating("UpdateTimeData", 0, 0.1f);
        if (GameStarted)
        {
            float ellapsedTimeSinceGameStart = Time.time - startTime;

            int newTick = Convert.ToInt32(ellapsedTimeSinceGameStart / Settings.World_IrlSecondsPerTick);
            if (newTick > Tick)
            {
                Tick = newTick;
                TickEvent();
            }

            int newHalfTick = Convert.ToInt32(ellapsedTimeSinceGameStart * 2.0f / Settings.World_IrlSecondsPerTick);
            if (newHalfTick > HalfTick)
            {
                HalfTick = newHalfTick;
                HalfTickEvent();
            }

        }
    }

    public void StartGame()
    {
        GameStarted = true;
        startTime = Time.time;
    }
}