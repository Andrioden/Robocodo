using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.GameLogic
{
    public class PlayerColorManager
    {
        private Queue<Color32> colors = new Queue<Color32>
        (
            new List<Color32>()
            {
                Color.blue,
                Color.red,
                Color.green,
                Color.yellow,
                Color.magenta,
                Color.grey
            }
        );

        public Color32 GetNextColor()
        {
            if (colors.Count == 0)
                return new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), 1.0f);
            else
                return colors.Dequeue();
        }
    }
}
