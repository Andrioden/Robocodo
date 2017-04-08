using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ValueTracker
{
    private double smoothing;

    private double changePerTick = 0;
    public double ChangePerTick { get { return changePerTick; } }

    private int lastTick = 0;
    private double lastValue;

    /// <summary>
    /// Smoothing defines how quickly the average changes. Higher = faster change
    /// </summary>
    public ValueTracker(double smoothing, double initialValue)
    {
        this.smoothing = smoothing;
        lastValue = initialValue;
    }

    public void AddDataPoint(int tick, double newValue)
    {
        if (lastTick == tick)
            throw new Exception("Cant add a data point twice for the same tick");
        else if (lastTick > tick)
            throw new Exception("Cant add a data point older than the last data point");

        int ticksSinceLastDataPoint = tick - lastTick;
        double changeSinceLastDataPoint = newValue - lastValue;

        double newChangePerTick = changeSinceLastDataPoint / ticksSinceLastDataPoint;
        changePerTick = (newChangePerTick * smoothing) + (changePerTick * (1.0 - smoothing));

        lastTick = tick;
        lastValue = newValue;
    }

}