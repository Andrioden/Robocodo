using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Robocodo.AndreAI
{
    /// <summary>
    /// This AI class follows a SEEK-DO pattern:
    /// 
    ///  ---------- SUMMARY ----------
    /// SEEK
    ///     HAS
    ///     
    ///     CONDITION CHECK 1
    ///     CONDITION CHECK 2...
    ///     
    ///     SEEK 1 (subseek)
    ///     SEEK 2 (subseek)...
    ///     
    ///     DO
    ///     
    ///     SEEK (recursive)
    ///  
    /// 
    ///  ---------- DETAILS ----------
    /// Has:
    ///     - First we always check if the AI does not already have what it seeks
    /// 
    /// 
    /// Seek: 
    ///     - Something the AI wants to gain
    ///     - Returns true if it managed to Seek it
    ///     - Can be trigger within a Seek (recursively)
    ///     
    /// Condition: 
    ///     - Something that stops the robot from doing the action to get what the AI Seeks.
    ///     - Returns true if the condition is fulfilled
    /// 
    /// Do:
    ///     - An actual real world changing action that the AI does to get what he Seeks.
    ///         
    /// </summary>
    public partial class AndreAI : AI
    {
        public static int Setting_DelayedStart_Min = 2; // Should not be lower than 2, since the game need to initialize a bit before the AI starts. Typical setting Owner on objects.
        public static int Setting_DelayedStart_Max = 3;
        public static int Setting_ThinkingInterval = 2;

        protected override void StartAI()
        {
            activeHarvestersTracker = new ActiveHarvestersTracker();

            float delayedStart = Utils.RandomFloat(Setting_DelayedStart_Min, Setting_DelayedStart_Max);
            StartCoroutine(ThinkCoroutine(delayedStart));
        }

        private IEnumerator ThinkCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            while (true)
            {
                Think();
                yield return new WaitForSeconds(Setting_ThinkingInterval);
            }
        }

        private void Think()
        {
            if (Settings.Debug_EnableAiLogging)
                HumanCommunicator.ShowPopupForAllHumans("Thinking... " + Icons.Heart, player.transform.position, TextPopup.ColorType.DEFAULT);

            Seek_ActiveHarvesters<FoodController>(4, 30);
            Seek_ActiveHarvesters<IronController>(2, 30);
            Seek_ActiveHarvesters<CopperController>(2, 30);

            Seek_VictoryByTech();

            Do_ReprogramCompletedHarvesters();
        }

        private bool Do_ReprogramCompletedHarvesters()
        {
            activeHarvestersTracker.ReprogramCompletedHarvesters();
            return true;
        }

    }
}