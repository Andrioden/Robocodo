using System;

public static class MathUtils
{

    public static double Distance(int fromX, int fromZ, int toX, int toZ)
    {
        return Math.Sqrt(Math.Pow(Math.Abs(toX - fromX), 2) + Math.Pow(Math.Abs(toZ - fromZ), 2));
    }

    /// <summary>
    /// Linearly converts an value in int range to the same linearly converted value inanother int range. Consider if you have an potential value range of 0 to 10.
    /// This 0 - 10 range should scale an object size from 0 - 100. So this method converts has to convert these two ranges.
    /// 
    /// The fromValue value represent for example 4 in the 0 - 10 range.
    /// 
    /// Return value will be the 0 - 100 range value representing the 4, which should be 40.
    /// </summary>
    public static int LinearConversion(int fromMin, int fromMax, int toMin, int toMax, int fromValue)
    {
        int fromRange = (fromMax - fromMin);
        int toRange = (toMax - toMin);
        int toValue = (((fromValue - fromMin) * toRange) / fromRange) + toMin;
        return toValue;
    }

    public static int LinearConversionInverted(double actualFrom, double maxFrom, int maxTo)
    {
        return Convert.ToInt32(maxTo * (maxFrom - actualFrom) / maxFrom);
    }

}
