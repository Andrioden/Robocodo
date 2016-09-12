using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.GameLogic
{
    public class PlayerColorManager
    {
        private Dictionary<string, Color32> playerColors = new Dictionary<string, Color32>();
        private List<Color32> colorPool = new List<Color32>
        {
            Color.blue,
            Color.red,
            Color.green,
            Color.cyan,
            Color.yellow,
            Color.magenta,
            Color.grey
        };

        public Color32 GetPlayerColor(string owner)
        {
            if (!playerColors.ContainsKey(owner))
                playerColors[owner] = GetNextColor();

            return playerColors[owner];
        }

        private Color32 GetNextColor()
        {
            var color = colorPool.FirstOrDefault();

            if (color == Color.clear)
                color = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), 1.0f);
            else
                colorPool.Remove(color);

            return color;
        }
    }
}
