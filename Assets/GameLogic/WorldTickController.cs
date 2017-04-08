using UnityEngine;
using System.Collections;
using System;

public class WorldTickController : MonoBehaviour
{

    private float startTime;

    private int tick = 0;
    public int Tick { get { return tick; } }
    public event Action OnTick = delegate { };
    public event Action OnAfterTick = delegate { };

    private int halfTick = 0;
    public int HalfTick { get { return halfTick; } }
    public event Action OnHalfTick = delegate { };
    public event Action OnAfterHalfTick = delegate { };

    public static WorldTickController instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
        {
            Debug.LogError("Tried to create another instance of " + GetType() + ". Destroying.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    private void Update()
    {
        // TODO MIGHT WANT TO LIMIT HOW OFTEN THIS RUN
        //InvokeRepeating("UpdateTimeData", 0, 0.1f);
        float ellapsedTimeSinceGameStart = Time.time - startTime;

        int newTick = Convert.ToInt32(ellapsedTimeSinceGameStart / Settings.World_IrlSecondsPerTick);
        if (newTick > tick)
        {
            tick = newTick;
            OnTick();
            OnAfterTick();
        }

        int newHalfTick = Convert.ToInt32(ellapsedTimeSinceGameStart * 2.0f / Settings.World_IrlSecondsPerTick);
        if (newHalfTick > halfTick)
        {
            halfTick = newHalfTick;
            OnHalfTick();
            OnAfterHalfTick();
        }
    }
}