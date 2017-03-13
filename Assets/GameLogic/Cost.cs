using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Cost
{
    public int Copper;
    public int Iron;
    public int Food;

    public override string ToString()
    {
        return string.Format("Cost: Copper-{0}, Iron-{1}, Food-{2}", Copper, Iron, Food);
    }
}